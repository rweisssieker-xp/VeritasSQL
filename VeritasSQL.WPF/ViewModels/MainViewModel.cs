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

    // AI Features
    [ObservableProperty]
    private ObservableCollection<QuerySuggestion> _querySuggestions = new();

    [ObservableProperty]
    private DataInsights? _dataInsights;

    [ObservableProperty]
    private QueryOptimization? _queryOptimization;

    [ObservableProperty]
    private string _schemaInsights = string.Empty;

    [ObservableProperty]
    private ObservableCollection<SmartFilter> _smartFilters = new();

    // NEW AI Features (Set 2)
    [ObservableProperty]
    private SqlErrorAnalysis? _sqlErrorAnalysis;

    [ObservableProperty]
    private string _resultSummary = string.Empty;

    [ObservableProperty]
    private AnomalyDetectionResult? _anomalyDetection;

    [ObservableProperty]
    private VisualizationRecommendations? _visualizationRecommendations;

    [ObservableProperty]
    private string _semanticSearchQuery = string.Empty;

    [ObservableProperty]
    private ObservableCollection<QueryHistoryEntry> _semanticSearchResults = new();

    // Phase 1: AI Co-Pilot Features
    [ObservableProperty]
    private ObservableCollection<QueryCopilotSuggestion> _copilotSuggestions = new();

    [ObservableProperty]
    private ObservableCollection<PredictiveQuerySuggestion> _predictiveQueries = new();

    [ObservableProperty]
    private JoinPathResult? _joinPath;

    [ObservableProperty]
    private string _joinSourceTable = string.Empty;

    [ObservableProperty]
    private string _joinTargetTable = string.Empty;

    // Phase 2: Enterprise Features
    [ObservableProperty]
    private DataProfilingResult? _dataProfilingResult;

    [ObservableProperty]
    private ConversationContext _conversationContext = new();

    [ObservableProperty]
    private string _chatMessage = string.Empty;

    [ObservableProperty]
    private ObservableCollection<ConversationTurn> _chatHistory = new();

    // Phase 3: Premium Features
    [ObservableProperty]
    private DashboardDefinition? _generatedDashboard;

    [ObservableProperty]
    private string _dashboardTopic = string.Empty;

    [ObservableProperty]
    private DataQualityScore? _dataQualityScore;

    [ObservableProperty]
    private BusinessImpactAnalysis? _impactAnalysis;

    [ObservableProperty]
    private string _schemaChangeDescription = string.Empty;

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

    // ===== NEW AI FEATURES =====

    /// <summary>
    /// Generates intelligent query suggestions based on the current schema
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGenerateQuerySuggestions))]
    private async Task GenerateQuerySuggestionsAsync()
    {
        if (CurrentSchema == null)
            return;

        IsBusy = true;
        StatusMessage = "Generating AI query suggestions...";

        try
        {
            var suggestions = await _openAIService.GenerateQuerySuggestionsAsync(CurrentSchema, 10);
            QuerySuggestions = new ObservableCollection<QuerySuggestion>(suggestions);
            StatusMessage = $"Generated {suggestions.Count} query suggestions";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "GenerateQuerySuggestions",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating query suggestions: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Failed to generate query suggestions";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanGenerateQuerySuggestions() => !IsBusy && IsConnected && CurrentSchema != null;

    /// <summary>
    /// Applies a query suggestion to the natural language query field
    /// </summary>
    [RelayCommand]
    private void ApplyQuerySuggestion(QuerySuggestion suggestion)
    {
        if (suggestion != null)
        {
            NaturalLanguageQuery = suggestion.NaturalLanguageQuery;
            StatusMessage = $"Applied suggestion: {suggestion.Title}";
        }
    }

    /// <summary>
    /// Analyzes query results and provides AI-powered insights
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAnalyzeDataInsights))]
    private async Task AnalyzeDataInsightsAsync()
    {
        if (QueryResults == null || string.IsNullOrWhiteSpace(GeneratedSql))
            return;

        IsBusy = true;
        StatusMessage = "Analyzing data with AI...";

        try
        {
            var insights = await _openAIService.AnalyzeDataInsightsAsync(QueryResults, GeneratedSql);
            DataInsights = insights;
            StatusMessage = "Data insights generated";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "AnalyzeDataInsights",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                GeneratedSql = GeneratedSql,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error analyzing data insights: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Failed to analyze data insights";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanAnalyzeDataInsights() => !IsBusy && QueryResults != null && QueryResults.Rows.Count > 0;

    /// <summary>
    /// Gets AI-powered query optimization recommendations
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanOptimizeQuery))]
    private async Task OptimizeQueryAsync()
    {
        if (string.IsNullOrWhiteSpace(GeneratedSql) || CurrentSchema == null)
            return;

        IsBusy = true;
        StatusMessage = "Getting AI optimization suggestions...";

        try
        {
            var optimization = await _openAIService.OptimizeQueryAsync(GeneratedSql, CurrentSchema);
            QueryOptimization = optimization;
            StatusMessage = $"Query optimization: {optimization.PerformanceRating}";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "OptimizeQuery",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                GeneratedSql = GeneratedSql,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error optimizing query: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Failed to optimize query";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanOptimizeQuery() => !IsBusy && !string.IsNullOrWhiteSpace(GeneratedSql) && CurrentSchema != null;

    /// <summary>
    /// Applies the AI-optimized SQL to the current query
    /// </summary>
    [RelayCommand]
    private async Task ApplyOptimizedQueryAsync()
    {
        if (QueryOptimization == null || string.IsNullOrWhiteSpace(QueryOptimization.OptimizedSql))
            return;

        GeneratedSql = QueryOptimization.OptimizedSql;

        // Re-validate
        var settings = await _settingsService.GetSettingsAsync();
        var validator = new QueryValidator(CurrentSchema!, settings.DefaultRowLimit);
        ValidationResult = validator.Validate(GeneratedSql);

        StatusMessage = "Applied optimized SQL";
    }

    /// <summary>
    /// Gets AI explanation of schema relationships
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExplainSchema))]
    private async Task ExplainSchemaAsync(string? focusTable = null)
    {
        if (CurrentSchema == null)
            return;

        IsBusy = true;
        StatusMessage = "Getting AI schema insights...";

        try
        {
            var insights = await _openAIService.ExplainSchemaRelationshipsAsync(CurrentSchema, focusTable);
            SchemaInsights = insights;
            StatusMessage = "Schema insights generated";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "ExplainSchema",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error explaining schema: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Failed to explain schema";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanExplainSchema() => !IsBusy && CurrentSchema != null;

    /// <summary>
    /// Generates smart filter suggestions based on sample data
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGenerateSmartFilters))]
    private async Task GenerateSmartFiltersAsync(string tableName)
    {
        if (string.IsNullOrWhiteSpace(tableName) || CurrentSchema == null || SelectedConnectionProfile == null)
            return;

        IsBusy = true;
        StatusMessage = "Generating smart filter suggestions...";

        try
        {
            // First, get sample data from the table
            var sampleSql = $"SELECT TOP 100 * FROM {tableName}";
            var connectionString = SelectedConnectionProfile.GetConnectionString();
            var result = await _queryExecutor.ExecuteQueryAsync(connectionString, sampleSql);

            if (result.Success && result.Data != null)
            {
                var filters = await _openAIService.GenerateSmartFiltersAsync(tableName, result.Data, CurrentSchema);
                SmartFilters = new ObservableCollection<SmartFilter>(filters);
                StatusMessage = $"Generated {filters.Count} smart filter suggestions";

                await _auditLogger.LogAsync(new AuditEntry
                {
                    Action = "GenerateSmartFilters",
                    ConnectionProfile = SelectedConnectionProfile.Name,
                    ExecutionStatus = "Success"
                });
            }
            else
            {
                MessageBox.Show($"Could not retrieve sample data: {result.ErrorMessage}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating smart filters: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Failed to generate smart filters";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanGenerateSmartFilters() => !IsBusy && IsConnected && CurrentSchema != null;

    /// <summary>
    /// Applies a smart filter to the natural language query
    /// </summary>
    [RelayCommand]
    private void ApplySmartFilter(SmartFilter filter)
    {
        if (filter != null)
        {
            // Append the filter suggestion to the natural language query
            var filterText = $"Filter by {filter.Column} {filter.FilterType} {filter.SuggestedValue}";
            if (string.IsNullOrWhiteSpace(NaturalLanguageQuery))
            {
                NaturalLanguageQuery = filterText;
            }
            else
            {
                NaturalLanguageQuery += $" and {filterText}";
            }

            StatusMessage = $"Applied filter: {filter.Column}";
        }
    }

    // ===== NEW AI FEATURES (SET 2) =====

    /// <summary>
    /// Analyzes SQL errors and provides fix suggestions
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAnalyzeSqlError))]
    private async Task AnalyzeSqlErrorAsync(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(GeneratedSql) || CurrentSchema == null || string.IsNullOrWhiteSpace(errorMessage))
            return;

        IsBusy = true;
        StatusMessage = "Analyzing SQL error with AI...";

        try
        {
            var analysis = await _openAIService.ExplainAndFixSqlErrorAsync(GeneratedSql, errorMessage, CurrentSchema);
            SqlErrorAnalysis = analysis;
            StatusMessage = $"Error analysis complete: {analysis.ErrorType}";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "AnalyzeSqlError",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                GeneratedSql = GeneratedSql,
                ErrorMessage = errorMessage,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error analyzing SQL error: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Failed to analyze SQL error";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanAnalyzeSqlError() => !IsBusy && !string.IsNullOrWhiteSpace(GeneratedSql) && CurrentSchema != null;

    /// <summary>
    /// Applies the AI-fixed SQL to the current query
    /// </summary>
    [RelayCommand]
    private async Task ApplyFixedSqlAsync()
    {
        if (SqlErrorAnalysis == null || string.IsNullOrWhiteSpace(SqlErrorAnalysis.FixedSql))
            return;

        GeneratedSql = SqlErrorAnalysis.FixedSql;

        // Re-validate
        var settings = await _settingsService.GetSettingsAsync();
        var validator = new QueryValidator(CurrentSchema!, settings.DefaultRowLimit);
        ValidationResult = validator.Validate(GeneratedSql);

        StatusMessage = "Applied fixed SQL from AI analysis";
        SqlErrorAnalysis = null; // Clear analysis after applying
    }

    /// <summary>
    /// Generates natural language summary of query results
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSummarizeResults))]
    private async Task SummarizeResultsAsync()
    {
        if (QueryResults == null || string.IsNullOrWhiteSpace(GeneratedSql) || string.IsNullOrWhiteSpace(NaturalLanguageQuery))
            return;

        IsBusy = true;
        StatusMessage = "Generating AI summary of results...";

        try
        {
            var summary = await _openAIService.SummarizeResultsAsync(QueryResults, GeneratedSql, NaturalLanguageQuery);
            ResultSummary = summary;
            StatusMessage = "Result summary generated";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "SummarizeResults",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                GeneratedSql = GeneratedSql,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error summarizing results: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Failed to summarize results";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSummarizeResults() => !IsBusy && QueryResults != null && QueryResults.Rows.Count > 0;

    /// <summary>
    /// Detects anomalies in query results
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanDetectAnomalies))]
    private async Task DetectAnomaliesAsync()
    {
        if (QueryResults == null || string.IsNullOrWhiteSpace(GeneratedSql))
            return;

        IsBusy = true;
        StatusMessage = "Detecting anomalies with AI...";

        try
        {
            var result = await _openAIService.DetectAnomaliesAsync(QueryResults, GeneratedSql);
            AnomalyDetection = result;
            StatusMessage = result.HasAnomalies
                ? $"Found {result.AnomalyCount} anomalies"
                : "No anomalies detected";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "DetectAnomalies",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                GeneratedSql = GeneratedSql,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error detecting anomalies: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Failed to detect anomalies";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanDetectAnomalies() => !IsBusy && QueryResults != null && QueryResults.Rows.Count > 0;

    /// <summary>
    /// Performs semantic search through query history
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSemanticSearch))]
    private async Task PerformSemanticSearchAsync()
    {
        if (string.IsNullOrWhiteSpace(SemanticSearchQuery))
            return;

        IsBusy = true;
        StatusMessage = "Performing AI semantic search...";

        try
        {
            var historyList = History.ToList();
            var matchingIndices = await _openAIService.SemanticSearchHistoryAsync(SemanticSearchQuery, historyList);

            var results = new List<QueryHistoryEntry>();
            foreach (var index in matchingIndices)
            {
                if (index >= 0 && index < historyList.Count)
                {
                    results.Add(historyList[index]);
                }
            }

            SemanticSearchResults = new ObservableCollection<QueryHistoryEntry>(results);
            StatusMessage = $"Found {results.Count} semantically similar queries";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "SemanticSearch",
                NaturalLanguageQuery = SemanticSearchQuery,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error in semantic search: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Semantic search failed";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSemanticSearch() => !IsBusy && !string.IsNullOrWhiteSpace(SemanticSearchQuery) && History.Count > 0;

    /// <summary>
    /// Gets AI visualization recommendations for query results
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRecommendVisualizations))]
    private async Task RecommendVisualizationsAsync()
    {
        if (QueryResults == null || string.IsNullOrWhiteSpace(GeneratedSql))
            return;

        IsBusy = true;
        StatusMessage = "Getting AI visualization recommendations...";

        try
        {
            var recommendations = await _openAIService.RecommendVisualizationsAsync(QueryResults, GeneratedSql);
            VisualizationRecommendations = recommendations;
            StatusMessage = "Visualization recommendations generated";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "RecommendVisualizations",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                GeneratedSql = GeneratedSql,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error getting visualization recommendations: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Failed to get visualization recommendations";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanRecommendVisualizations() => !IsBusy && QueryResults != null && QueryResults.Rows.Count > 0;

    /// <summary>
    /// Phase 1: AI Query Co-Pilot - Get real-time suggestions
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGetCopilotSuggestions))]
    private async Task GetCopilotSuggestionsAsync()
    {
        if (string.IsNullOrWhiteSpace(NaturalLanguageQuery) || NaturalLanguageQuery.Length < 3)
            return;

        try
        {
            var suggestions = await _openAIService.GetCopilotSuggestionsAsync(
                NaturalLanguageQuery,
                _currentSchema,
                History.ToList());

            CopilotSuggestions = new ObservableCollection<QueryCopilotSuggestion>(suggestions);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Co-pilot error: {ex.Message}";
        }
    }

    private bool CanGetCopilotSuggestions() => !IsBusy && !string.IsNullOrWhiteSpace(NaturalLanguageQuery);

    /// <summary>
    /// Phase 1: Predictive Next Query - Netflix-style recommendations
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanPredictNextQueries))]
    private async Task PredictNextQueriesAsync()
    {
        if (History.Count == 0)
            return;

        IsBusy = true;
        StatusMessage = "Predicting next queries...";

        try
        {
            var lastQuery = History.First();
            var predictions = await _openAIService.PredictNextQueriesAsync(
                lastQuery,
                History.ToList(),
                _currentSchema,
                QueryResults);

            PredictiveQueries = new ObservableCollection<PredictiveQuerySuggestion>(predictions);
            StatusMessage = $"Found {predictions.Count} predictive suggestions";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "PredictNextQueries",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error predicting queries: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Failed to predict next queries";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanPredictNextQueries() => !IsBusy && History.Count > 0;

    /// <summary>
    /// Phase 1: Smart JOIN Path Finder
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanFindJoinPath))]
    private async Task FindJoinPathAsync()
    {
        if (string.IsNullOrWhiteSpace(JoinSourceTable) || string.IsNullOrWhiteSpace(JoinTargetTable))
            return;

        IsBusy = true;
        StatusMessage = $"Finding JOIN path from {JoinSourceTable} to {JoinTargetTable}...";

        try
        {
            var path = await _openAIService.FindJoinPathAsync(
                JoinSourceTable,
                JoinTargetTable,
                _currentSchema);

            JoinPath = path;
            StatusMessage = $"Found path with {path.PathLength} steps";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "FindJoinPath",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                GeneratedSql = path.CompleteSql,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error finding JOIN path: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Failed to find JOIN path";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanFindJoinPath() => !IsBusy && !string.IsNullOrWhiteSpace(JoinSourceTable) && !string.IsNullOrWhiteSpace(JoinTargetTable);

    /// <summary>
    /// Phase 2: AI Data Profiling & PII Detection
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanProfileData))]
    private async Task ProfileDataAsync()
    {
        if (SelectedTable == null || QueryResults == null)
            return;

        IsBusy = true;
        StatusMessage = "Profiling data and detecting PII...";

        try
        {
            var profiling = await _openAIService.ProfileDataAsync(
                SelectedTable.TableName,
                QueryResults,
                _currentSchema);

            DataProfilingResult = profiling;
            StatusMessage = $"Profiling complete - Risk: {profiling.OverallRisk}";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "ProfileData",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error profiling data: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Failed to profile data";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanProfileData() => !IsBusy && QueryResults != null && QueryResults.Rows.Count > 0;

    /// <summary>
    /// Phase 2: Conversational Chat Interface
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanSendChatMessage))]
    private async Task SendChatMessageAsync()
    {
        if (string.IsNullOrWhiteSpace(ChatMessage))
            return;

        IsBusy = true;
        var userMessage = ChatMessage;
        ChatMessage = string.Empty; // Clear input

        try
        {
            var response = await _openAIService.ChatAsync(
                userMessage,
                ConversationContext,
                _currentSchema);

            ChatHistory = new ObservableCollection<ConversationTurn>(ConversationContext.Turns);

            // If SQL was generated, update the main query
            if (!string.IsNullOrEmpty(response.Sql))
            {
                GeneratedSql = response.Sql;
            }

            StatusMessage = "Chat response received";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "ChatMessage",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                GeneratedSql = response.Sql,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error in chat: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Chat failed";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanSendChatMessage() => !IsBusy && !string.IsNullOrWhiteSpace(ChatMessage);

    /// <summary>
    /// Phase 3: Automated Dashboard Generator
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGenerateDashboard))]
    private async Task GenerateDashboardAsync()
    {
        if (string.IsNullOrWhiteSpace(DashboardTopic))
            return;

        IsBusy = true;
        StatusMessage = $"Generating dashboard for: {DashboardTopic}...";

        try
        {
            var dashboard = await _openAIService.GenerateDashboardAsync(
                DashboardTopic,
                _currentSchema,
                History.ToList());

            GeneratedDashboard = dashboard;
            StatusMessage = $"Dashboard created with {dashboard.Widgets.Count} widgets";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "GenerateDashboard",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating dashboard: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Failed to generate dashboard";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanGenerateDashboard() => !IsBusy && !string.IsNullOrWhiteSpace(DashboardTopic);

    /// <summary>
    /// Phase 3: AI Data Quality Score
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanCalculateQualityScore))]
    private async Task CalculateQualityScoreAsync()
    {
        if (SelectedTable == null || QueryResults == null)
            return;

        IsBusy = true;
        StatusMessage = "Calculating data quality score...";

        try
        {
            var score = await _openAIService.CalculateDataQualityScoreAsync(
                SelectedTable.TableName,
                QueryResults,
                _currentSchema);

            DataQualityScore = score;
            StatusMessage = $"Quality Score: {score.OverallScore:F1}/100 (Grade: {score.Grade})";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "CalculateQualityScore",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error calculating quality score: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Failed to calculate quality score";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanCalculateQualityScore() => !IsBusy && QueryResults != null && QueryResults.Rows.Count > 0;

    /// <summary>
    /// Phase 3: Business Impact Analysis
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanAnalyzeImpact))]
    private async Task AnalyzeImpactAsync()
    {
        if (string.IsNullOrWhiteSpace(SchemaChangeDescription))
            return;

        IsBusy = true;
        StatusMessage = "Analyzing business impact...";

        try
        {
            var analysis = await _openAIService.AnalyzeSchemaChangeImpactAsync(
                SchemaChangeDescription,
                _currentSchema,
                History.ToList());

            ImpactAnalysis = analysis;
            StatusMessage = $"Impact: {analysis.ImpactLevel} - {analysis.AffectedQueries.Count} queries affected";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "AnalyzeImpact",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error analyzing impact: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Failed to analyze impact";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanAnalyzeImpact() => !IsBusy && !string.IsNullOrWhiteSpace(SchemaChangeDescription);

    /// <summary>
    /// Apply a predictive query suggestion
    /// </summary>
    [RelayCommand]
    private void ApplyPredictiveQuery(PredictiveQuerySuggestion suggestion)
    {
        if (suggestion == null) return;

        NaturalLanguageQuery = suggestion.NaturalLanguageQuery;
        GeneratedSql = suggestion.Sql;
        StatusMessage = $"Applied: {suggestion.Title}";
    }

    /// <summary>
    /// Apply a co-pilot suggestion
    /// </summary>
    [RelayCommand]
    private void ApplyCopilotSuggestion(QueryCopilotSuggestion suggestion)
    {
        if (suggestion == null) return;

        NaturalLanguageQuery = suggestion.CompletionText;
        GeneratedSql = suggestion.FullSql;
        StatusMessage = "Co-pilot suggestion applied";
    }

    /// <summary>
    /// Apply JOIN path SQL
    /// </summary>
    [RelayCommand]
    private void ApplyJoinPath()
    {
        if (JoinPath == null) return;

        GeneratedSql = JoinPath.CompleteSql;
        StatusMessage = "JOIN path SQL applied";
    }

    /// <summary>
    /// Execute dashboard widget query
    /// </summary>
    [RelayCommand]
    private async Task ExecuteDashboardWidgetAsync(DashboardWidget widget)
    {
        if (widget == null) return;

        GeneratedSql = widget.Sql;
        await ExecuteQueryAsync();
    }

    /// <summary>
    /// Clear conversation context
    /// </summary>
    [RelayCommand]
    private void ClearConversation()
    {
        ConversationContext = new ConversationContext();
        ChatHistory.Clear();
        StatusMessage = "Conversation cleared";
    }
}

