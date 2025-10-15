using System.Windows;
using System.Windows.Controls;
using VeritasSQL.Core.Models;
using VeritasSQL.Core.Services;

namespace VeritasSQL.WPF.Dialogs;

public partial class ConnectionDialog : Window
{
    private readonly ConnectionManager _connectionManager;
    public ConnectionProfile? Profile { get; private set; }

    public ConnectionDialog(ConnectionManager connectionManager, ConnectionProfile? profile = null)
    {
        InitializeComponent();
        _connectionManager = connectionManager;

        if (profile != null)
        {
            Profile = profile;
            LoadProfile(profile);
        }
        else
        {
            Profile = new ConnectionProfile();
        }
    }

    private void LoadProfile(ConnectionProfile profile)
    {
        NameTextBox.Text = profile.Name;
        ServerTextBox.Text = profile.Server;
        DatabaseTextBox.Text = profile.Database;
        TimeoutTextBox.Text = profile.ConnectionTimeout.ToString();

        if (profile.AuthType == AuthenticationType.SqlServer)
        {
            AuthTypeComboBox.SelectedIndex = 1;
            UsernameTextBox.Text = profile.Username ?? string.Empty;
        }
        else
        {
            AuthTypeComboBox.SelectedIndex = 0;
        }
    }

    private void AuthTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (AuthTypeComboBox.SelectedItem is ComboBoxItem item)
        {
            var isSqlAuth = item.Tag?.ToString() == "SqlServer";
            
            UsernameLabel.Visibility = isSqlAuth ? Visibility.Visible : Visibility.Collapsed;
            UsernameTextBox.Visibility = isSqlAuth ? Visibility.Visible : Visibility.Collapsed;
            PasswordLabel.Visibility = isSqlAuth ? Visibility.Visible : Visibility.Collapsed;
            PasswordBox.Visibility = isSqlAuth ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private async void TestButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ValidateAndSave())
            return;

        var testButton = (Button)sender;
        testButton.IsEnabled = false;
        testButton.Content = "Teste...";

        try
        {
            var success = await _connectionManager.TestConnectionAsync(Profile!);
            
            if (success)
            {
                MessageBox.Show("Connection successful!", "Connection Test", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Connection failed. Please check the settings.", 
                    "Connection Test", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error testing connection: {ex.Message}", 
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            testButton.IsEnabled = true;
            testButton.Content = "Test Connection";
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        if (ValidateAndSave())
        {
            DialogResult = true;
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    private bool ValidateAndSave()
    {
        if (string.IsNullOrWhiteSpace(NameTextBox.Text))
        {
            MessageBox.Show("Please enter a name.", "Validation", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(ServerTextBox.Text))
        {
            MessageBox.Show("Please enter a server.", "Validation", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        if (string.IsNullOrWhiteSpace(DatabaseTextBox.Text))
        {
            MessageBox.Show("Please enter a database.", "Validation", 
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }

        Profile!.Name = NameTextBox.Text;
        Profile.Server = ServerTextBox.Text;
        Profile.Database = DatabaseTextBox.Text;
        
        if (AuthTypeComboBox.SelectedItem is ComboBoxItem item)
        {
            Profile.AuthType = item.Tag?.ToString() == "SqlServer" 
                ? AuthenticationType.SqlServer 
                : AuthenticationType.Windows;
        }

        if (Profile.AuthType == AuthenticationType.SqlServer)
        {
            Profile.Username = UsernameTextBox.Text;
            if (!string.IsNullOrEmpty(PasswordBox.Password))
            {
                Profile.EncryptedPassword = ConnectionProfile.EncryptPassword(PasswordBox.Password);
            }
        }

        if (int.TryParse(TimeoutTextBox.Text, out var timeout))
        {
            Profile.ConnectionTimeout = timeout;
        }

        return true;
    }
}

