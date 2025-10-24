using System.Windows;
using VeritasSQL.Core.Models;
using VeritasSQL.Core.Services;

namespace VeritasSQL.WPF.Dialogs;

public partial class SettingsDialog : Window
{
    private readonly SettingsService _settingsService;
    public AppSettings? Settings { get; private set; }

    public SettingsDialog(SettingsService settingsService, AppSettings? currentSettings = null)
    {
        InitializeComponent();
        _settingsService = settingsService;

        if (currentSettings != null)
        {
            LoadSettings(currentSettings);
        }
    }

    private void LoadSettings(AppSettings settings)
    {
        // Set API Key if available (decrypt first)
        var apiKey = settings.GetOpenAIApiKey();
        if (!string.IsNullOrEmpty(apiKey))
        {
            ApiKeyPasswordBox.Password = apiKey;
        }

        // Set Model
        foreach (System.Windows.Controls.ComboBoxItem item in ModelComboBox.Items)
        {
            if (item.Tag?.ToString() == settings.OpenAIModel)
            {
                ModelComboBox.SelectedItem = item;
                break;
            }
        }

        // Set limits
        DefaultRowLimitTextBox.Text = settings.DefaultRowLimit.ToString();
        MaxRowLimitTextBox.Text = settings.MaxRowLimit.ToString();
        QueryTimeoutTextBox.Text = settings.QueryTimeoutSeconds.ToString();
    }

    private async void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            // Validate inputs
            if (!int.TryParse(DefaultRowLimitTextBox.Text, out int defaultLimit) || defaultLimit <= 0)
            {
                MessageBox.Show("Default Row Limit must be a positive number.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(MaxRowLimitTextBox.Text, out int maxLimit) || maxLimit <= 0)
            {
                MessageBox.Show("Max Row Limit must be a positive number.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(QueryTimeoutTextBox.Text, out int timeout) || timeout <= 0)
            {
                MessageBox.Show("Query Timeout must be a positive number.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (defaultLimit > maxLimit)
            {
                MessageBox.Show("Default Row Limit cannot be greater than Max Row Limit.", "Validation Error",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Create settings object
            var settings = new AppSettings
            {
                DefaultRowLimit = defaultLimit,
                MaxRowLimit = maxLimit,
                QueryTimeoutSeconds = timeout,
                OpenAIModel = (ModelComboBox.SelectedItem as System.Windows.Controls.ComboBoxItem)?.Tag?.ToString() ?? "gpt-4"
            };

            // Encrypt and set API key if provided
            var apiKey = ApiKeyPasswordBox.Password;
            if (!string.IsNullOrWhiteSpace(apiKey))
            {
                settings.SetOpenAIApiKey(apiKey);
            }

            // Save settings
            await _settingsService.SaveSettingsAsync(settings);

            Settings = settings;
            DialogResult = true;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving settings: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
