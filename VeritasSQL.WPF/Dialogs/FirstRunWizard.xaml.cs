using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VeritasSQL.Core.Models;
using VeritasSQL.Core.Services;

namespace VeritasSQL.WPF.Dialogs;

public partial class FirstRunWizard : Window
{
    private readonly SettingsService _settingsService;
    private readonly ConnectionManager _connectionManager;
    private int _currentStep = 1;
    private const int TotalSteps = 4;

    public string? ApiKey { get; private set; }
    public ConnectionProfile? CreatedConnection { get; private set; }

    public FirstRunWizard(SettingsService settingsService, ConnectionManager connectionManager)
    {
        InitializeComponent();
        _settingsService = settingsService;
        _connectionManager = connectionManager;
        
        // Add converter for InverseBoolConverter
        Resources.Add("InverseBoolConverter", new InverseBoolConverter());
        
        ShowStep(1);
    }

    private void ShowStep(int step)
    {
        _currentStep = step;

        // Hide all steps
        WelcomeStep.Visibility = Visibility.Collapsed;
        ApiKeyStep.Visibility = Visibility.Collapsed;
        ConnectionStep.Visibility = Visibility.Collapsed;
        CompleteStep.Visibility = Visibility.Collapsed;

        // Update step indicators
        UpdateStepIndicators(step);

        // Show current step
        switch (step)
        {
            case 1:
                WelcomeStep.Visibility = Visibility.Visible;
                StepTitleTextBlock.Text = "Step 1 of 4: Welcome";
                BackButton.IsEnabled = false;
                NextButton.Visibility = Visibility.Visible;
                SkipButton.Visibility = Visibility.Collapsed;
                FinishButton.Visibility = Visibility.Collapsed;
                break;

            case 2:
                ApiKeyStep.Visibility = Visibility.Visible;
                StepTitleTextBlock.Text = "Step 2 of 4: OpenAI API Key";
                BackButton.IsEnabled = true;
                NextButton.Visibility = Visibility.Visible;
                SkipButton.Visibility = Visibility.Visible;
                FinishButton.Visibility = Visibility.Collapsed;
                break;

            case 3:
                ConnectionStep.Visibility = Visibility.Visible;
                StepTitleTextBlock.Text = "Step 3 of 4: Database Connection";
                BackButton.IsEnabled = true;
                NextButton.Visibility = Visibility.Visible;
                SkipButton.Visibility = Visibility.Visible;
                FinishButton.Visibility = Visibility.Collapsed;
                break;

            case 4:
                CompleteStep.Visibility = Visibility.Visible;
                StepTitleTextBlock.Text = "Step 4 of 4: Complete";
                BackButton.IsEnabled = true;
                NextButton.Visibility = Visibility.Collapsed;
                SkipButton.Visibility = Visibility.Collapsed;
                FinishButton.Visibility = Visibility.Visible;
                break;
        }
    }

    private void UpdateStepIndicators(int currentStep)
    {
        var steps = new[] { Step1Indicator, Step2Indicator, Step3Indicator, Step4Indicator };
        for (int i = 0; i < steps.Length; i++)
        {
            if (i < currentStep)
            {
                // Completed step - blue
                steps[i].Fill = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(52, 152, 219)); // #3498DB
            }
            else if (i == currentStep - 1)
            {
                // Current step - blue
                steps[i].Fill = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(52, 152, 219)); // #3498DB
            }
            else
            {
                // Future step - gray
                steps[i].Fill = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(189, 195, 199)); // #BDC3C7
            }
        }
    }

    private void NextButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentStep == 2)
        {
            // Save API key if provided
            var apiKey = ApiKeyPasswordBox.Password;
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                ApiKey = apiKey;
            }
        }
        else if (_currentStep == 3)
        {
            // Create connection if not skipped
            if (SkipConnectionCheckBox.IsChecked != true)
            {
                try
                {
                    var connection = new ConnectionProfile
                    {
                        Name = ConnectionNameTextBox.Text,
                        Server = ServerTextBox.Text,
                        Database = DatabaseTextBox.Text,
                        AuthType = (AuthTypeComboBox.SelectedItem as ComboBoxItem)?.Tag?.ToString() == "SqlServer"
                            ? AuthenticationType.SqlServer
                            : AuthenticationType.Windows,
                        ConnectionTimeout = 30
                    };

                    if (string.IsNullOrWhiteSpace(connection.Name) ||
                        string.IsNullOrWhiteSpace(connection.Server) ||
                        string.IsNullOrWhiteSpace(connection.Database))
                    {
                        MessageBox.Show("Please fill in all connection fields or skip this step.",
                            "Incomplete Information", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    CreatedConnection = connection;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error creating connection: {ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
        }

        if (_currentStep < TotalSteps)
        {
            ShowStep(_currentStep + 1);
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentStep > 1)
        {
            ShowStep(_currentStep - 1);
        }
    }

    private void SkipButton_Click(object sender, RoutedEventArgs e)
    {
        if (_currentStep == 2)
        {
            // Skip API key - move to connection step
            ShowStep(3);
        }
        else if (_currentStep == 3)
        {
            // Skip connection - move to complete
            ShowStep(4);
        }
    }

    private async void FinishButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Save API key if provided
            if (!string.IsNullOrWhiteSpace(ApiKey))
            {
                var settings = await _settingsService.GetSettingsAsync();
                settings.SetOpenAIApiKey(ApiKey);
                await _settingsService.SaveSettingsAsync(settings);
            }

            // Save connection if created
            if (CreatedConnection != null)
            {
                await _connectionManager.SaveProfileAsync(CreatedConnection);
            }

            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving settings: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void GetApiKeyButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://platform.openai.com/api-keys",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Could not open browser: {ex.Message}\n\nPlease visit: https://platform.openai.com/api-keys",
                "Open Browser", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}

