using System.Collections.ObjectModel;
using System.Data;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VeritasSQL.Core.Data;
using VeritasSQL.Core.Models;
using VeritasSQL.Core.Services;
using VeritasSQL.Core.Validation;

namespace VeritasSQL.WPF.ViewModels;

/// <summary>
/// Main ViewModel for the application
/// </summary>
public partial class MainViewModel : ObservableObject
{
    private readonly ConnectionManager _connectionManager;
    private readonly SettingsService _settingsService;
    private readonly SchemaService _schemaService;
    private readonly OpenAIService _openAIService;
    private readonly QueryExecutor _queryExecutor;
    private readonly HistoryService _historyService;
    private readonly AuditLogger _auditLogger;
    private readonly DomainDictionaryService _dictionaryService;

    [ObservableProperty]
    private ObservableCollection<ConnectionProfile> _connectionProfiles = new();

    [ObservableProperty]
    private ConnectionProfile? _selectedConnectionProfile;

    [ObservableProperty]
    private bool _isConnected;

    [ObservableProperty]
    private SchemaInfo? _currentSchema;

    [ObservableProperty]
    private string _naturalLanguageQuery = string.Empty;

    [ObservableProperty]
    private string _generatedSql = string.Empty;

    [ObservableProperty]
    private string _explanation = string.Empty;

    [ObservableProperty]
    private ValidationResult? _validationResult;

    [ObservableProperty]
    private DataTable? _queryResults;

    [ObservableProperty]
    private int _resultRowCount;

    [ObservableProperty]
    private string _executionTime = string.Empty;

    [ObservableProperty]
    private string _StatusMessage = "Ready";

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private ObservableCollection<QueryHistoryEntry> _history = new();

    [ObservableProperty]
    private ObservableCollection<QueryHistoryEntry> _favorites = new();

    [ObservableProperty]
    private int _selectedTabIndex;

    public MainViewModel(
        ConnectionManager connectionManager,
        SettingsService settingsService,
        SchemaService schemaService,
        OpenAIService openAIService,
        QueryExecutor queryExecutor,
        HistoryService historyService,
        AuditLogger auditLogger,
        DomainDictionaryService dictionaryService)
    {
        _connectionManager = connectionManager;
        _settingsService = settingsService;
        _schemaService = schemaService;
        _openAIService = openAIService;
        _queryExecutor = queryExecutor;
        _historyService = historyService;
        _auditLogger = auditLogger;
        _dictionaryService = dictionaryService;

        // Initialization
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            var profiles = await _connectionManager.GetProfilesAsync();
            ConnectionProfiles = new ObservableCollection<ConnectionProfile>(profiles);

            var history = await _historyService.GetHistoryAsync(50);
            History = new ObservableCollection<QueryHistoryEntry>(history);

            var favorites = await _historyService.GetFavoritesAsync();
            Favorites = new ObservableCollection<QueryHistoryEntry>(favorites);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error during initialization: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task ConnectAsync()
    {
        if (SelectedConnectionProfile == null)
        {
            MessageBox.Show("Please select a connection profile.", "No Connection", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        IsBusy = true;
        StatusMessage = "Connecting...";

        try
        {
            // Teste Verbindung
            var success = await _connectionManager.TestConnectionAsync(SelectedConnectionProfile);
            if (!success)
            {
                MessageBox.Show("Connection failed. Please check the connection settings.", 
                    "Connection Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            IsConnected = true;
            SelectedConnectionProfile.LastUsed = DateTime.Now;
            await _connectionManager.SaveProfileAsync(SelectedConnectionProfile);

            // Lade Schema
            await LoadSchemaAsync();

            StatusMessage = $"Connected to {SelectedConnectionProfile.Name}";

            // Audit-Log
            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "Connect",
                ConnectionProfile = SelectedConnectionProfile.Name,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error connecting: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Connection failed";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "Connect",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                ExecutionStatus = "Failed",
                ErrorMessage = ex.Message
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task LoadSchemaAsync()
    {
        if (!IsConnected || SelectedConnectionProfile == null)
            return;

        IsBusy = true;
        StatusMessage = "Loading database schema...";

        try
        {
            var connectionString = SelectedConnectionProfile.GetConnectionString();
            CurrentSchema = await _schemaService.LoadSchemaAsync(connectionString);

            StatusMessage = $"Schema loaded: {CurrentSchema.Tables.Count} tables, {CurrentSchema.Views.Count} views";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading schema: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Schema loading failed";
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand(CanExecute = nameof(CanGenerateSql))]
    private async Task GenerateSqlAsync()
    {
        if (string.IsNullOrWhiteSpace(NaturalLanguageQuery) || CurrentSchema == null)
            return;

        IsBusy = true;
        StatusMessage = "Generating SQL...";
        GeneratedSql = string.Empty;
        Explanation = string.Empty;
        ValidationResult = null;

        try
        {
            // OpenAI Aufruf mit Dictionary
            var dictionary = await _dictionaryService.GetDictionaryAsync();
            var response = await _openAIService.GenerateSqlAsync(NaturalLanguageQuery, CurrentSchema, dictionary);

            GeneratedSql = response.Sql;
            Explanation = response.Explanation;

            // Validierung
            var settings = await _settingsService.GetSettingsAsync();
            var validator = new QueryValidator(CurrentSchema, settings.DefaultRowLimit);
            ValidationResult = validator.Validate(GeneratedSql);

            if (ValidationResult.ModifiedSql != null)
            {
                GeneratedSql = ValidationResult.ModifiedSql;
            }

            StatusMessage = ValidationResult.IsValid ? "SQL generiert und validiert" : "SQL generiert - Validierungsfehler vorhanden";

            // Audit-Log
            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "GenerateSQL",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                NaturalLanguageQuery = NaturalLanguageQuery,
                GeneratedSql = GeneratedSql,
                ValidationStatus = ValidationResult.IsValid ? "Valid" : "Invalid",
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating SQL: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "SQL generation failed";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "GenerateSQL",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                NaturalLanguageQuery = NaturalLanguageQuery,
                ExecutionStatus = "Failed",
                ErrorMessage = ex.Message
            });
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanGenerateSql() => !IsBusy && IsConnected && !string.IsNullOrWhiteSpace(NaturalLanguageQuery);

    [RelayCommand(CanExecute = nameof(CanExecuteQuery))]
    private async Task ExecuteQueryAsync()
    {
        if (string.IsNullOrWhiteSpace(GeneratedSql) || SelectedConnectionProfile == null)
            return;

        // Prüfe Validierung
        if (ValidationResult == null || !ValidationResult.IsValid)
        {
            var result = MessageBox.Show(
                "The SQL query has validation errors. Do you want to continue anyway?",
                "Validation Error",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;
        }

        IsBusy = true;
        StatusMessage = "Executing query...";
        QueryResults = null;
        ResultRowCount = 0;
        ExecutionTime = string.Empty;

        try
        {
            var connectionString = SelectedConnectionProfile.GetConnectionString();
            var result = await _queryExecutor.ExecuteQueryAsync(connectionString, GeneratedSql);

            if (result.Success && result.Data != null)
            {
                QueryResults = result.Data;
                ResultRowCount = result.RowCount;
                ExecutionTime = $"{result.ExecutionTime.TotalMilliseconds:F0} ms";
                StatusMessage = $"Query successful: {result.RowCount} rows in {ExecutionTime}";

                // Zur Ergebnis-Tab wechseln
                SelectedTabIndex = 0;

                // Historie speichern
                var historyEntry = new QueryHistoryEntry
                {
                    NaturalLanguageQuery = NaturalLanguageQuery,
                    GeneratedSql = GeneratedSql,
                    ConnectionProfileId = SelectedConnectionProfile.Id,
                    ConnectionProfileName = SelectedConnectionProfile.Name,
                    RowCount = result.RowCount,
                    ExecutionTimeMs = result.ExecutionTime.TotalMilliseconds,
                    Success = true
                };

                await _historyService.AddEntryAsync(historyEntry);
                await RefreshHistoryAsync();

                // Audit-Log
                await _auditLogger.LogAsync(new AuditEntry
                {
                    Action = "ExecuteQuery",
                    ConnectionProfile = SelectedConnectionProfile.Name,
                    NaturalLanguageQuery = NaturalLanguageQuery,
                    GeneratedSql = GeneratedSql,
                    ExecutionStatus = "Success",
                    RowCount = result.RowCount,
                    ExecutionTimeMs = result.ExecutionTime.TotalMilliseconds
                });
            }
            else
            {
                MessageBox.Show($"Query failed: {result.ErrorMessage}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "Query failed";

                // Save history with error
                var historyEntry = new QueryHistoryEntry
                {
                    NaturalLanguageQuery = NaturalLanguageQuery,
                    GeneratedSql = GeneratedSql,
                    ConnectionProfileId = SelectedConnectionProfile.Id,
                    ConnectionProfileName = SelectedConnectionProfile.Name,
                    Success = false,
                    ErrorMessage = result.ErrorMessage
                };

                await _historyService.AddEntryAsync(historyEntry);
                await RefreshHistoryAsync();

                // Audit-Log
                await _auditLogger.LogAsync(new AuditEntry
                {
                    Action = "ExecuteQuery",
                    ConnectionProfile = SelectedConnectionProfile.Name,
                    NaturalLanguageQuery = NaturalLanguageQuery,
                    GeneratedSql = GeneratedSql,
                    ExecutionStatus = "Failed",
                    ErrorMessage = result.ErrorMessage
                });
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error executing query: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Query execution failed";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanExecuteQuery() => !IsBusy && IsConnected && !string.IsNullOrWhiteSpace(GeneratedSql);

    [RelayCommand]
    private async Task RefreshHistoryAsync()
    {
        try
        {
            var history = await _historyService.GetHistoryAsync(50);
            History = new ObservableCollection<QueryHistoryEntry>(history);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading history: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshFavoritesAsync()
    {
        try
        {
            var favorites = await _historyService.GetFavoritesAsync();
            Favorites = new ObservableCollection<QueryHistoryEntry>(favorites);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error loading favorites: {ex.Message}";
        }
    }

    [RelayCommand]
    private void LoadHistoryEntry(QueryHistoryEntry entry)
    {
        NaturalLanguageQuery = entry.NaturalLanguageQuery;
        GeneratedSql = entry.GeneratedSql;
        Explanation = string.Empty;
        StatusMessage = "History entry loaded";
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportToCsvAsync()
    {
        if (QueryResults == null || SelectedConnectionProfile == null)
            return;

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "CSV-Dateien (*.csv)|*.csv",
            FileName = $"export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
        };

        if (dialog.ShowDialog() == true)
        {
            IsBusy = true;
            StatusMessage = "Exporting to CSV...";

            try
            {
                var exporter = new Core.Export.CsvExporter();
                await exporter.ExportAsync(
                    QueryResults,
                    dialog.FileName,
                    includeMetadata: true,
                    sqlQuery: GeneratedSql,
                    connectionProfile: SelectedConnectionProfile.Name,
                    rowCount: ResultRowCount);

                StatusMessage = $"Export successful: {dialog.FileName}";
                MessageBox.Show($"Data successfully exported to:\n{dialog.FileName}",
                    "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting CSV: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "CSV export failed";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    [RelayCommand(CanExecute = nameof(CanExport))]
    private async Task ExportToExcelAsync()
    {
        if (QueryResults == null || SelectedConnectionProfile == null)
            return;

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Excel-Dateien (*.xlsx)|*.xlsx",
            FileName = $"export_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        };

        if (dialog.ShowDialog() == true)
        {
            IsBusy = true;
            StatusMessage = "Exporting to Excel...";

            try
            {
                var exporter = new Core.Export.ExcelExporter();
                await exporter.ExportAsync(
                    QueryResults,
                    dialog.FileName,
                    includeMetadata: true,
                    sqlQuery: GeneratedSql,
                    connectionProfile: SelectedConnectionProfile.Name,
                    rowCount: ResultRowCount);

                StatusMessage = $"Export successful: {dialog.FileName}";
                MessageBox.Show($"Data successfully exported to:\n{dialog.FileName}",
                    "Export Successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting Excel: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                StatusMessage = "Excel export failed";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }

    private bool CanExport() => !IsBusy && QueryResults != null && QueryResults.Rows.Count > 0;

    [RelayCommand(CanExecute = nameof(CanExplainSql))]
    private async Task ExplainSqlAsync()
    {
        if (string.IsNullOrWhiteSpace(GeneratedSql))
            return;

        IsBusy = true;
        StatusMessage = "Explaining SQL...";

        try
        {
            var explanation = await _openAIService.ExplainSqlAsync(GeneratedSql);
            Explanation = explanation;
            StatusMessage = "SQL explanation generated";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "ExplainSQL",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                GeneratedSql = GeneratedSql,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error explaining SQL: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "SQL explanation failed";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanExplainSql() => !IsBusy && !string.IsNullOrWhiteSpace(GeneratedSql);

    [RelayCommand(CanExecute = nameof(CanRefine))]
    private async Task RefineSqlAsync(string refinementRequest)
    {
        if (string.IsNullOrWhiteSpace(GeneratedSql) || CurrentSchema == null)
            return;

        IsBusy = true;
        StatusMessage = "Refining SQL...";

        try
        {
            var response = await _openAIService.RefineSqlAsync(GeneratedSql, refinementRequest, CurrentSchema);
            
            GeneratedSql = response.Sql;
            Explanation = response.Explanation;

            // Re-validierung
            var settings = await _settingsService.GetSettingsAsync();
            var validator = new QueryValidator(CurrentSchema, settings.DefaultRowLimit);
            ValidationResult = validator.Validate(GeneratedSql);

            if (ValidationResult.ModifiedSql != null)
            {
                GeneratedSql = ValidationResult.ModifiedSql;
            }

            StatusMessage = "SQL refined";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error refining SQL: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "SQL refinement failed";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanRefine() => !IsBusy && !string.IsNullOrWhiteSpace(GeneratedSql) && CurrentSchema != null;
}

