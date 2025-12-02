using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
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
    [NotifyCanExecuteChangedFor(nameof(GenerateQuerySuggestionsCommand), nameof(GenerateSmartFiltersCommand), nameof(ExecuteQueryCommand))]
    private bool _isConnected;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateSqlCommand), nameof(ExplainSchemaCommand), nameof(GenerateQuerySuggestionsCommand), nameof(OptimizeQueryCommand), nameof(GenerateSmartFiltersCommand))]
    private SchemaInfo? _currentSchema;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ProfileDataCommand), nameof(CalculateQualityScoreCommand))]
    private TableInfo? _selectedTable;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateSqlCommand))]
    private string _naturalLanguageQuery = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ExecuteQueryCommand), nameof(ExplainSqlCommand), nameof(OptimizeQueryCommand), nameof(EstimateQueryCostCommand))]
    private string _generatedSql = string.Empty;

    [ObservableProperty]
    private string _explanation = string.Empty;

    [ObservableProperty]
    private ValidationResult? _validationResult;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AnalyzeDataInsightsCommand), nameof(SummarizeResultsCommand), 
        nameof(DetectAnomaliesCommand), nameof(RecommendVisualizationsCommand), nameof(ProfileDataCommand),
        nameof(CalculateQualityScoreCommand), nameof(FindCorrelationsCommand), nameof(GenerateDataStoryCommand))]
    private DataTable? _queryResults;

    [ObservableProperty]
    private int _resultRowCount;

    [ObservableProperty]
    private string _executionTime = string.Empty;

    [ObservableProperty]
    private string _StatusMessage = "Ready";

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(GenerateSqlCommand), nameof(ExecuteQueryCommand), nameof(ExplainSqlCommand), nameof(OptimizeQueryCommand), nameof(EstimateQueryCostCommand), nameof(AnalyzeDataInsightsCommand), nameof(SummarizeResultsCommand), nameof(DetectAnomaliesCommand), nameof(RecommendVisualizationsCommand), nameof(ProfileDataCommand), nameof(CalculateQualityScoreCommand), nameof(FindCorrelationsCommand), nameof(GenerateDataStoryCommand))]
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

    // Phase 4-9: Advanced AI Features
    [ObservableProperty]
    private QueryCostEstimate? _queryCostEstimate;

    [ObservableProperty]
    private string _cloudProvider = "azure"; // azure|aws|on-premise

    [ObservableProperty]
    private CorrelationAnalysis? _correlationAnalysis;

    [ObservableProperty]
    private StatisticalTestRecommendation? _statisticalTestRecommendation;

    [ObservableProperty]
    private string _researchQuestion = string.Empty;

    [ObservableProperty]
    private DataStory? _dataStory;

    [ObservableProperty]
    private string _storyTone = "professional"; // professional|casual|technical

    [ObservableProperty]
    private VoiceTranscription? _lastTranscription;

    [ObservableProperty]
    private bool _isRecording = false;

    [ObservableProperty]
    private string _appVersion = string.Empty;

    [ObservableProperty]
    private string _buildDate = string.Empty;

    // Chart Properties
    [ObservableProperty]
    private int _selectedChartTypeIndex = 0;

    [ObservableProperty]
    private ObservableCollection<string> _chartColumnNames = new();

    [ObservableProperty]
    private ObservableCollection<string> _chartNumericColumns = new();

    [ObservableProperty]
    private string? _selectedXAxisColumn;

    [ObservableProperty]
    private string? _selectedYAxisColumn;

    [ObservableProperty]
    private bool _isBarChartVisible = true;

    [ObservableProperty]
    private bool _isPieChartVisible = false;

    [ObservableProperty]
    private bool _hasNoChartData = true;

    [ObservableProperty]
    private ObservableCollection<LiveChartsCore.ISeries> _chartSeries = new();

    [ObservableProperty]
    private ObservableCollection<LiveChartsCore.ISeries> _pieChartSeries = new();

    [ObservableProperty]
    private ObservableCollection<LiveChartsCore.SkiaSharpView.Axis> _chartXAxes = new();

    [ObservableProperty]
    private ObservableCollection<LiveChartsCore.SkiaSharpView.Axis> _chartYAxes = new();

    // Query Templates
    [ObservableProperty]
    private ObservableCollection<QueryTemplate> _queryTemplates = new();

    [ObservableProperty]
    private QueryTemplate? _selectedTemplate;

    [ObservableProperty]
    private string _templateFilter = string.Empty;

    // Advanced History Search
    [ObservableProperty]
    private string _historySearchText = string.Empty;

    [ObservableProperty]
    private ConnectionProfile? _historyFilterConnection;

    [ObservableProperty]
    private DateTime? _historyFilterDateFrom;

    [ObservableProperty]
    private DateTime? _historyFilterDateTo;

    [ObservableProperty]
    private bool _historyFilterSuccessOnly;

    [ObservableProperty]
    private int _historySortIndex = 0; // 0=Newest, 1=Oldest, 2=Fastest, 3=Slowest, 4=MostRows

    [ObservableProperty]
    private ObservableCollection<QueryHistoryEntry> _filteredHistory = new();

    [ObservableProperty]
    private QueryHistoryEntry? _selectedHistoryItem;

    // Database First Date Feature (Tinder-inspired)
    [ObservableProperty]
    private DatabaseFirstDateResult? _firstDateResult;

    [ObservableProperty]
    private bool _showFirstDatePanel;

    [ObservableProperty]
    private FirstDateQuery? _selectedFirstDateQuery;

    // Query Preview Feature (Netflix-inspired)
    [ObservableProperty]
    private DataTable? _previewResults;

    [ObservableProperty]
    private bool _showPreviewPanel;

    [ObservableProperty]
    private int _previewRowCount = 5;

    // Achievements Feature (Gaming-inspired)
    [ObservableProperty]
    private UserAchievements _userAchievements = new();

    [ObservableProperty]
    private Achievement? _latestAchievement;

    [ObservableProperty]
    private bool _showAchievementPopup;

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

        // Initialize version and build date
        AppVersion = GetAppVersion();
        BuildDate = GetBuildDate();

        // Initialization
        _ = InitializeAsync();
    }

    private async Task InitializeAsync()
    {
        try
        {
            // Load settings and configure OpenAI service
            var settings = await _settingsService.GetSettingsAsync();
            var apiKey = settings.GetOpenAIApiKey();
            if (!string.IsNullOrEmpty(apiKey))
            {
                _openAIService.Configure(apiKey, settings.OpenAIModel);
            }

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
    private async Task NewConnectionAsync()
    {
        var dialog = new Dialogs.ConnectionDialog(_connectionManager);
        if (dialog.ShowDialog() == true && dialog.Profile != null)
        {
            try
            {
                await _connectionManager.SaveProfileAsync(dialog.Profile);

                // Refresh the profiles list
                var profiles = await _connectionManager.GetProfilesAsync();
                ConnectionProfiles = new ObservableCollection<ConnectionProfile>(profiles);

                // Select the newly created profile
                SelectedConnectionProfile = ConnectionProfiles.FirstOrDefault(p => p.Id == dialog.Profile.Id);

                StatusMessage = $"Connection profile '{dialog.Profile.Name}' created successfully";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving connection profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private async Task EditConnectionAsync()
    {
        if (SelectedConnectionProfile == null)
        {
            MessageBox.Show("Please select a connection profile to edit.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var dialog = new Dialogs.ConnectionDialog(_connectionManager, SelectedConnectionProfile);
        if (dialog.ShowDialog() == true && dialog.Profile != null)
        {
            try
            {
                await _connectionManager.SaveProfileAsync(dialog.Profile);

                // Refresh the profiles list
                var profiles = await _connectionManager.GetProfilesAsync();
                ConnectionProfiles = new ObservableCollection<ConnectionProfile>(profiles);

                // Reselect the edited profile
                SelectedConnectionProfile = ConnectionProfiles.FirstOrDefault(p => p.Id == dialog.Profile.Id);

                StatusMessage = $"Connection profile '{dialog.Profile.Name}' updated successfully";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error updating connection profile: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    [RelayCommand]
    private async Task OpenSettingsAsync()
    {
        try
        {
            var currentSettings = await _settingsService.GetSettingsAsync();
            var dialog = new Dialogs.SettingsDialog(_settingsService, currentSettings);

            if (dialog.ShowDialog() == true)
            {
                // Reload settings and reconfigure OpenAI service immediately
                var updatedSettings = await _settingsService.GetSettingsAsync();
                var apiKey = updatedSettings.GetOpenAIApiKey();

                if (!string.IsNullOrEmpty(apiKey))
                {
                    _openAIService.Configure(apiKey, updatedSettings.OpenAIModel);
                    StatusMessage = "Settings saved and applied successfully";
                    MessageBox.Show("Settings have been saved and applied. OpenAI API Key is now active.",
                        "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    StatusMessage = "Settings saved successfully";
                    MessageBox.Show("Settings have been saved.",
                        "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening settings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
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

            // DEBUG: Log the state
            System.Diagnostics.Debug.WriteLine($"LoadSchemaAsync: CurrentSchema is {(CurrentSchema == null ? "NULL" : "SET")}");
            System.Diagnostics.Debug.WriteLine($"LoadSchemaAsync: IsBusy = {IsBusy}");
            System.Diagnostics.Debug.WriteLine($"LoadSchemaAsync: CanExplainSchema() = {CanExplainSchema()}");
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading schema: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Schema loading failed";
        }
        finally
        {
            IsBusy = false;

            // DEBUG: Log after IsBusy reset
            System.Diagnostics.Debug.WriteLine($"LoadSchemaAsync (finally): IsBusy = {IsBusy}");
            System.Diagnostics.Debug.WriteLine($"LoadSchemaAsync (finally): CanExplainSchema() = {CanExplainSchema()}");

            // Explicitly notify commands to re-evaluate their CanExecute on UI thread
            Application.Current.Dispatcher.Invoke(() =>
            {
                ExplainSchemaCommand.NotifyCanExecuteChanged();
                GenerateQuerySuggestionsCommand.NotifyCanExecuteChanged();
                OptimizeQueryCommand.NotifyCanExecuteChanged();
                GenerateSmartFiltersCommand.NotifyCanExecuteChanged();
            });
        }
    }

    [RelayCommand(CanExecute = nameof(CanGenerateSql))]
    private async Task GenerateSqlAsync()
    {
        if (string.IsNullOrWhiteSpace(NaturalLanguageQuery))
        {
            MessageBox.Show("Please enter a natural language query.", "Input Required", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        if (CurrentSchema == null)
        {
            MessageBox.Show("Please load the database schema first by clicking 'Load Schema'.", "Schema Required", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

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
            var errorDetails = ex.InnerException != null
                ? $"{ex.Message}\n\nDetails: {ex.InnerException.Message}"
                : ex.Message;

            MessageBox.Show(
                $"Error generating SQL:\n\n{errorDetails}\n\nPlease check:\n• OpenAI API Key is configured in Settings\n• Internet connection is available\n• Schema is loaded",
                "SQL Generation Error",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
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

    private bool CanGenerateSql() => !IsBusy && CurrentSchema != null && !string.IsNullOrWhiteSpace(NaturalLanguageQuery);

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

    /// <summary>
    /// Query Preview - See 5 rows before loading 5 million! (Netflix-inspired)
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanExecuteQuery))]
    private async Task PreviewQueryAsync()
    {
        if (string.IsNullOrWhiteSpace(GeneratedSql) || SelectedConnectionProfile == null)
            return;

        IsBusy = true;
        StatusMessage = "Loading preview (5 rows)...";
        PreviewResults = null;
        ShowPreviewPanel = false;

        try
        {
            var connectionString = SelectedConnectionProfile.GetConnectionString();
            var result = await _queryExecutor.ExecutePreviewAsync(connectionString, GeneratedSql, PreviewRowCount);

            if (result.Success && result.Data != null)
            {
                PreviewResults = result.Data;
                ShowPreviewPanel = true;
                StatusMessage = $"Preview loaded: {result.RowCount} sample rows in {result.ExecutionTime.TotalMilliseconds:F0}ms";

                // Show preview info
                MessageBox.Show(
                    $"📋 Query Preview\n\n" +
                    $"Showing {result.RowCount} sample rows.\n" +
                    $"Execution time: {result.ExecutionTime.TotalMilliseconds:F0}ms\n\n" +
                    $"Click 'Execute' to run the full query.",
                    "Preview Results",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Preview failed: {result.ErrorMessage}", "Preview Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                StatusMessage = "Preview failed";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading preview: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Preview failed";
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Database First Date - 5 questions to know your new database! (Tinder-inspired)
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGenerateFirstDate))]
    private async Task GenerateFirstDateAsync()
    {
        if (CurrentSchema == null)
            return;

        IsBusy = true;
        StatusMessage = "💕 Generating First Date queries...";
        FirstDateResult = null;
        ShowFirstDatePanel = false;

        try
        {
            FirstDateResult = await _openAIService.GenerateFirstDateQueriesAsync(CurrentSchema);
            ShowFirstDatePanel = true;
            StatusMessage = $"💕 First Date ready! Meet \"{FirstDateResult.DatabaseNickname}\"";

            // Auto-select first query
            if (FirstDateResult.Queries.Any())
            {
                SelectedFirstDateQuery = FirstDateResult.Queries.First();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating First Date queries: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "First Date generation failed";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanGenerateFirstDate() => !IsBusy && CurrentSchema != null;

    /// <summary>
    /// Execute a First Date query
    /// </summary>
    [RelayCommand]
    private async Task ExecuteFirstDateQueryAsync(FirstDateQuery? query)
    {
        if (query == null || SelectedConnectionProfile == null)
            return;

        // Load the query into the main editor
        NaturalLanguageQuery = $"{query.Emoji} {query.Title}: {query.Description}";
        GeneratedSql = query.Sql;
        Explanation = query.Insight;

        // Execute it
        await ExecuteQueryAsync();

        StatusMessage = $"✨ First Date Query executed: {query.Title}";
    }

    /// <summary>
    /// Dismiss the First Date panel
    /// </summary>
    [RelayCommand]
    private void DismissFirstDate()
    {
        ShowFirstDatePanel = false;
    }

    [RelayCommand]
    private async Task RefreshHistoryAsync()
    {
        try
        {
            var history = await _historyService.GetHistoryAsync(100); // Load more for filtering
            History = new ObservableCollection<QueryHistoryEntry>(history);
            FilteredHistory = new ObservableCollection<QueryHistoryEntry>(history);
            StatusMessage = $"Loaded {history.Count} history entries";
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

    private bool CanExplainSchema()
    {
        var result = !IsBusy && CurrentSchema != null;
        System.Diagnostics.Debug.WriteLine($"CanExplainSchema: IsBusy={IsBusy}, CurrentSchema={(CurrentSchema == null ? "NULL" : "SET")}, Result={result}");
        return result;
    }

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
                CurrentSchema,
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
                CurrentSchema,
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
                CurrentSchema);

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
                SelectedTable.Name,
                QueryResults,
                CurrentSchema);

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
                CurrentSchema);

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
                CurrentSchema,
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
                SelectedTable.Name,
                QueryResults,
                CurrentSchema);

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
                CurrentSchema,
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

    /// <summary>
    /// Voice-to-SQL: Start recording voice input
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanStartVoiceInput))]
    private async Task StartVoiceInputAsync(Stream audioStream)
    {
        if (audioStream == null) return;

        IsBusy = true;
        IsRecording = true;
        StatusMessage = "Transcribing audio...";

        try
        {
            var transcription = await _openAIService.TranscribeAudioAsync(audioStream);
            LastTranscription = transcription;
            NaturalLanguageQuery = transcription.TranscribedText;
            StatusMessage = $"Transcribed ({transcription.Language}): {transcription.TranscribedText}";

            // Automatically generate SQL from voice
            await GenerateSqlAsync();

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "VoiceToSQL",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                NaturalLanguageQuery = transcription.TranscribedText,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error transcribing audio: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Voice transcription failed";
        }
        finally
        {
            IsBusy = false;
            IsRecording = false;
        }
    }

    private bool CanStartVoiceInput() => !IsBusy && !IsRecording;

    /// <summary>
    /// AI Query Cost Estimator
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanEstimateQueryCost))]
    private async Task EstimateQueryCostAsync()
    {
        if (string.IsNullOrWhiteSpace(GeneratedSql))
            return;

        IsBusy = true;
        StatusMessage = "Estimating query cost and resource usage...";

        try
        {
            var estimate = await _openAIService.EstimateQueryCostAsync(
                GeneratedSql,
                CurrentSchema,
                CloudProvider);

            QueryCostEstimate = estimate;
            StatusMessage = $"Estimated cost: ${estimate.EstimatedCloudCostUsd:F4} - {estimate.ExecutionTimeCategory} ({estimate.EstimatedExecutionTimeSeconds:F2}s)";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "EstimateQueryCost",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                GeneratedSql = GeneratedSql,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error estimating query cost: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Failed to estimate query cost";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanEstimateQueryCost() => !IsBusy && !string.IsNullOrWhiteSpace(GeneratedSql);

    /// <summary>
    /// AI Correlation Finder
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanFindCorrelations))]
    private async Task FindCorrelationsAsync()
    {
        if (QueryResults == null || string.IsNullOrWhiteSpace(GeneratedSql))
            return;

        IsBusy = true;
        StatusMessage = "Analyzing correlations in data...";

        try
        {
            var analysis = await _openAIService.FindCorrelationsAsync(
                QueryResults,
                GeneratedSql);

            CorrelationAnalysis = analysis;
            StatusMessage = $"Found {analysis.Correlations.Count} correlations";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "FindCorrelations",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                GeneratedSql = GeneratedSql,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error finding correlations: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Failed to find correlations";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanFindCorrelations() => !IsBusy && QueryResults != null && QueryResults.Rows.Count > 0;

    /// <summary>
    /// AI Statistical Test Recommender
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanRecommendStatisticalTests))]
    private async Task RecommendStatisticalTestsAsync()
    {
        if (QueryResults == null || string.IsNullOrWhiteSpace(ResearchQuestion))
            return;

        IsBusy = true;
        StatusMessage = "Recommending statistical tests...";

        try
        {
            var recommendation = await _openAIService.RecommendStatisticalTestsAsync(
                QueryResults,
                ResearchQuestion);

            StatisticalTestRecommendation = recommendation;
            StatusMessage = $"Recommended {recommendation.RecommendedTests.Count} tests";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "RecommendStatisticalTests",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error recommending statistical tests: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Failed to recommend tests";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanRecommendStatisticalTests() => !IsBusy && QueryResults != null && !string.IsNullOrWhiteSpace(ResearchQuestion);

    /// <summary>
    /// AI Data Storytelling
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanGenerateDataStory))]
    private async Task GenerateDataStoryAsync()
    {
        if (QueryResults == null || string.IsNullOrWhiteSpace(GeneratedSql))
            return;

        IsBusy = true;
        StatusMessage = "Creating data story...";

        try
        {
            var story = await _openAIService.GenerateDataStoryAsync(
                QueryResults,
                GeneratedSql,
                NaturalLanguageQuery,
                StoryTone);

            DataStory = story;
            StatusMessage = $"Story created: {story.Title}";

            await _auditLogger.LogAsync(new AuditEntry
            {
                Action = "GenerateDataStory",
                ConnectionProfile = SelectedConnectionProfile?.Name,
                GeneratedSql = GeneratedSql,
                ExecutionStatus = "Success"
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error generating data story: {ex.Message}",
                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            StatusMessage = "Failed to generate story";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool CanGenerateDataStory() => !IsBusy && QueryResults != null && QueryResults.Rows.Count > 0;

    /// <summary>
    /// Gets the application version from assembly
    /// </summary>
    private string GetAppVersion()
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var version = assembly.GetName().Version;
            return version != null ? $"v{version.Major}.{version.Minor}.{version.Build}" : "v1.0.0";
        }
        catch
        {
            return "v1.0.0";
        }
    }

    /// <summary>
    /// Gets the build date from assembly
    /// </summary>
    private string GetBuildDate()
    {
        try
        {
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            var location = assembly.Location;
            if (!string.IsNullOrEmpty(location) && File.Exists(location))
            {
                var fileInfo = new FileInfo(location);
                return fileInfo.LastWriteTime.ToString("dd.MM.yyyy HH:mm");
            }
            return DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        }
        catch
        {
            return DateTime.Now.ToString("dd.MM.yyyy HH:mm");
        }
    }

    /// <summary>
    /// Export data story as Word document
    /// </summary>
    [RelayCommand]
    private void ExportDataStory()
    {
        if (DataStory == null) return;

        var saveDialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Word Document (*.docx)|*.docx|PDF Document (*.pdf)|*.pdf|Text File (*.txt)|*.txt",
            DefaultExt = ".txt",
            FileName = $"DataStory_{DateTime.Now:yyyyMMdd_HHmmss}"
        };

        if (saveDialog.ShowDialog() == true)
        {
            try
            {
                // Export as formatted text for now (Word/PDF would need additional library)
                var story = DataStory;
                var content = new StringBuilder();
                content.AppendLine(story.Title.ToUpper());
                content.AppendLine(new string('=', 80));
                content.AppendLine();
                content.AppendLine("EXECUTIVE SUMMARY");
                content.AppendLine(new string('-', 80));
                content.AppendLine(story.ExecutiveSummary);
                content.AppendLine();

                foreach (var chapter in story.Chapters)
                {
                    content.AppendLine(chapter.ChapterTitle.ToUpper());
                    content.AppendLine(new string('-', 80));
                    content.AppendLine(chapter.Content);
                    content.AppendLine();
                }

                content.AppendLine("CONCLUSION");
                content.AppendLine(new string('-', 80));
                content.AppendLine(story.Conclusion);
                content.AppendLine();
                content.AppendLine("KEY TAKEAWAYS");
                content.AppendLine(new string('-', 80));
                foreach (var takeaway in story.KeyTakeaways)
                {
                    content.AppendLine($"• {takeaway}");
                }

                File.WriteAllText(saveDialog.FileName, content.ToString());
                StatusMessage = $"Data story exported to {Path.GetFileName(saveDialog.FileName)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting data story: {ex.Message}",
                    "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    #region Advanced History Commands

    /// <summary>
    /// Searches and filters history based on current filter settings
    /// </summary>
    [RelayCommand]
    private void SearchHistory()
    {
        var filtered = History.AsEnumerable();

        // Text search
        if (!string.IsNullOrWhiteSpace(HistorySearchText))
        {
            var searchLower = HistorySearchText.ToLower();
            filtered = filtered.Where(h =>
                (h.NaturalLanguageQuery?.ToLower().Contains(searchLower) == true) ||
                (h.GeneratedSql?.ToLower().Contains(searchLower) == true) ||
                (h.ConnectionProfileName?.ToLower().Contains(searchLower) == true));
        }

        // Connection filter
        if (HistoryFilterConnection != null)
        {
            filtered = filtered.Where(h => h.ConnectionProfileId == HistoryFilterConnection.Id);
        }

        // Date range filter
        if (HistoryFilterDateFrom.HasValue)
        {
            filtered = filtered.Where(h => h.ExecutedAt >= HistoryFilterDateFrom.Value);
        }
        if (HistoryFilterDateTo.HasValue)
        {
            filtered = filtered.Where(h => h.ExecutedAt <= HistoryFilterDateTo.Value.AddDays(1));
        }

        // Success filter
        if (HistoryFilterSuccessOnly)
        {
            filtered = filtered.Where(h => h.Success);
        }

        // Sorting
        filtered = HistorySortIndex switch
        {
            0 => filtered.OrderByDescending(h => h.ExecutedAt), // Newest
            1 => filtered.OrderBy(h => h.ExecutedAt), // Oldest
            2 => filtered.OrderBy(h => h.ExecutionTimeMs), // Fastest
            3 => filtered.OrderByDescending(h => h.ExecutionTimeMs), // Slowest
            4 => filtered.OrderByDescending(h => h.RowCount), // Most Rows
            _ => filtered.OrderByDescending(h => h.ExecutedAt)
        };

        FilteredHistory = new ObservableCollection<QueryHistoryEntry>(filtered);
        StatusMessage = $"Found {FilteredHistory.Count} queries matching filters";
    }

    /// <summary>
    /// Clears all history filters
    /// </summary>
    [RelayCommand]
    private void ClearHistoryFilters()
    {
        HistorySearchText = string.Empty;
        HistoryFilterConnection = null;
        HistoryFilterDateFrom = null;
        HistoryFilterDateTo = null;
        HistoryFilterSuccessOnly = false;
        HistorySortIndex = 0;
        FilteredHistory = new ObservableCollection<QueryHistoryEntry>(History);
        StatusMessage = "Filters cleared";
    }

    /// <summary>
    /// Loads the selected history item into the query editor
    /// </summary>
    [RelayCommand]
    private void LoadSelectedHistory()
    {
        if (SelectedHistoryItem == null)
        {
            MessageBox.Show("Please select a history item first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        NaturalLanguageQuery = SelectedHistoryItem.NaturalLanguageQuery ?? string.Empty;
        GeneratedSql = SelectedHistoryItem.GeneratedSql ?? string.Empty;
        StatusMessage = $"Loaded query from {SelectedHistoryItem.ExecutedAt:g}";
    }

    /// <summary>
    /// Adds selected history item to favorites
    /// </summary>
    [RelayCommand]
    private async Task AddHistoryToFavoritesAsync()
    {
        if (SelectedHistoryItem == null)
        {
            MessageBox.Show("Please select a history item first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // Use a simple name based on the query
        var defaultName = SelectedHistoryItem.NaturalLanguageQuery?.Length > 50 
            ? SelectedHistoryItem.NaturalLanguageQuery.Substring(0, 47) + "..."
            : SelectedHistoryItem.NaturalLanguageQuery ?? "My Query";

        SelectedHistoryItem.IsFavorite = true;
        SelectedHistoryItem.FavoriteName = defaultName;
        SelectedHistoryItem.FavoriteDescription = $"Added from history on {DateTime.Now:g}";

        await _historyService.UpdateEntryAsync(SelectedHistoryItem);
        await RefreshFavoritesAsync();
        StatusMessage = $"Added to favorites: {defaultName}";
        MessageBox.Show($"Query added to favorites!\n\nName: {defaultName}", "Added to Favorites", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// Exports selected history items to CSV
    /// </summary>
    [RelayCommand]
    private void ExportSelectedHistory()
    {
        if (FilteredHistory.Count == 0)
        {
            MessageBox.Show("No history items to export.", "Nothing to Export", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var saveDialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "CSV File (*.csv)|*.csv",
            DefaultExt = ".csv",
            FileName = $"QueryHistory_{DateTime.Now:yyyyMMdd_HHmmss}"
        };

        if (saveDialog.ShowDialog() == true)
        {
            try
            {
                var sb = new StringBuilder();
                sb.AppendLine("ExecutedAt,NaturalLanguageQuery,GeneratedSql,ConnectionProfile,RowCount,ExecutionTimeMs,Success");

                foreach (var item in FilteredHistory)
                {
                    sb.AppendLine($"\"{item.ExecutedAt:yyyy-MM-dd HH:mm:ss}\",\"{EscapeCsv(item.NaturalLanguageQuery)}\",\"{EscapeCsv(item.GeneratedSql)}\",\"{item.ConnectionProfileName}\",{item.RowCount},{item.ExecutionTimeMs:F0},{item.Success}");
                }

                File.WriteAllText(saveDialog.FileName, sb.ToString());
                StatusMessage = $"Exported {FilteredHistory.Count} history items to {Path.GetFileName(saveDialog.FileName)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting history: {ex.Message}", "Export Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private string EscapeCsv(string? value)
    {
        if (string.IsNullOrEmpty(value)) return string.Empty;
        return value.Replace("\"", "\"\"").Replace("\n", " ").Replace("\r", " ");
    }

    /// <summary>
    /// Deletes selected history items
    /// </summary>
    [RelayCommand]
    private async Task DeleteSelectedHistoryAsync()
    {
        if (SelectedHistoryItem == null)
        {
            MessageBox.Show("Please select a history item first.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"Are you sure you want to delete this query from history?\n\n\"{SelectedHistoryItem.NaturalLanguageQuery?.Substring(0, Math.Min(100, SelectedHistoryItem.NaturalLanguageQuery?.Length ?? 0))}...\"",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            await _historyService.DeleteEntryAsync(SelectedHistoryItem.Id);
            History.Remove(SelectedHistoryItem);
            FilteredHistory.Remove(SelectedHistoryItem);
            StatusMessage = "History item deleted";
        }
    }

    #endregion

    #region Template Commands

    /// <summary>
    /// Loads the built-in query templates
    /// </summary>
    [RelayCommand]
    private void LoadTemplates()
    {
        var templates = BuiltInTemplates.GetAll();
        
        // Apply filter if set
        if (!string.IsNullOrWhiteSpace(TemplateFilter))
        {
            var filter = TemplateFilter.ToLower();
            templates = templates.Where(t => 
                t.Name.ToLower().Contains(filter) || 
                t.Category.ToLower().Contains(filter) ||
                t.Description.ToLower().Contains(filter)).ToList();
        }
        
        QueryTemplates = new ObservableCollection<QueryTemplate>(templates);
        StatusMessage = $"Loaded {QueryTemplates.Count} query templates";
    }

    /// <summary>
    /// Applies the selected template to the query input
    /// </summary>
    [RelayCommand]
    private void ApplyTemplate()
    {
        if (SelectedTemplate == null)
        {
            MessageBox.Show("Please select a template first.", "No Template Selected", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        // Use the natural language template
        NaturalLanguageQuery = SelectedTemplate.NaturalLanguageTemplate;
        StatusMessage = $"Applied template: {SelectedTemplate.Name}";
    }

    /// <summary>
    /// Applies template and generates SQL immediately
    /// </summary>
    [RelayCommand]
    private async Task ApplyAndGenerateTemplateAsync()
    {
        if (SelectedTemplate == null)
        {
            MessageBox.Show("Please select a template first.", "No Template Selected", 
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        NaturalLanguageQuery = SelectedTemplate.NaturalLanguageTemplate;
        
        if (CurrentSchema != null && !string.IsNullOrEmpty(NaturalLanguageQuery))
        {
            await GenerateSqlAsync();
        }
    }

    #endregion

    #region Chart Commands

    /// <summary>
    /// Refreshes the chart based on current data and settings
    /// </summary>
    [RelayCommand]
    private void RefreshChart()
    {
        if (QueryResults == null || QueryResults.Rows.Count == 0)
        {
            HasNoChartData = true;
            IsBarChartVisible = false;
            IsPieChartVisible = false;
            return;
        }

        HasNoChartData = false;
        UpdateChartColumns();

        if (string.IsNullOrEmpty(SelectedXAxisColumn) || string.IsNullOrEmpty(SelectedYAxisColumn))
        {
            // Auto-select first suitable columns
            if (ChartColumnNames.Count > 0)
                SelectedXAxisColumn = ChartColumnNames[0];
            if (ChartNumericColumns.Count > 0)
                SelectedYAxisColumn = ChartNumericColumns[0];
        }

        UpdateChart();
    }

    /// <summary>
    /// Auto-generates the best chart type based on data
    /// </summary>
    [RelayCommand]
    private void AutoGenerateChart()
    {
        if (QueryResults == null || QueryResults.Rows.Count == 0)
        {
            MessageBox.Show("No data to visualize. Execute a query first.", "No Data", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        UpdateChartColumns();

        // Auto-select columns
        if (ChartColumnNames.Count > 0)
            SelectedXAxisColumn = ChartColumnNames[0];
        if (ChartNumericColumns.Count > 0)
            SelectedYAxisColumn = ChartNumericColumns[0];

        // Auto-select chart type based on data characteristics
        var rowCount = QueryResults.Rows.Count;
        var numericCount = ChartNumericColumns.Count;
        var categoryCount = ChartColumnNames.Count - numericCount;

        if (rowCount <= 10 && categoryCount > 0)
        {
            // Few categories - pie chart works well
            SelectedChartTypeIndex = 2; // Pie
        }
        else if (rowCount > 20)
        {
            // Many data points - line chart
            SelectedChartTypeIndex = 1; // Line
        }
        else
        {
            // Default to bar chart
            SelectedChartTypeIndex = 0; // Bar
        }

        UpdateChart();
        StatusMessage = "Chart auto-generated based on data characteristics";
    }

    /// <summary>
    /// Updates the available columns for chart axes
    /// </summary>
    private void UpdateChartColumns()
    {
        ChartColumnNames.Clear();
        ChartNumericColumns.Clear();

        if (QueryResults == null) return;

        foreach (DataColumn column in QueryResults.Columns)
        {
            ChartColumnNames.Add(column.ColumnName);

            // Check if column is numeric
            if (IsNumericType(column.DataType))
            {
                ChartNumericColumns.Add(column.ColumnName);
            }
        }
    }

    /// <summary>
    /// Checks if a type is numeric
    /// </summary>
    private bool IsNumericType(Type type)
    {
        return type == typeof(int) || type == typeof(long) || type == typeof(short) ||
               type == typeof(decimal) || type == typeof(double) || type == typeof(float) ||
               type == typeof(byte) || type == typeof(sbyte) || type == typeof(uint) ||
               type == typeof(ulong) || type == typeof(ushort);
    }

    /// <summary>
    /// Updates the chart based on current settings
    /// </summary>
    public void UpdateChart()
    {
        if (QueryResults == null || string.IsNullOrEmpty(SelectedXAxisColumn) || string.IsNullOrEmpty(SelectedYAxisColumn))
        {
            return;
        }

        try
        {
            // Get data
            var labels = new List<string>();
            var values = new List<double>();

            foreach (DataRow row in QueryResults.Rows)
            {
                var label = row[SelectedXAxisColumn]?.ToString() ?? "N/A";
                labels.Add(label.Length > 20 ? label.Substring(0, 17) + "..." : label);

                if (double.TryParse(row[SelectedYAxisColumn]?.ToString(), out double value))
                {
                    values.Add(value);
                }
                else
                {
                    values.Add(0);
                }
            }

            // Limit to first 50 items for performance
            if (labels.Count > 50)
            {
                labels = labels.Take(50).ToList();
                values = values.Take(50).ToList();
            }

            // Update chart based on type
            switch (SelectedChartTypeIndex)
            {
                case 0: // Bar Chart
                case 3: // Column Chart
                    CreateBarChart(labels, values);
                    break;
                case 1: // Line Chart
                    CreateLineChart(labels, values);
                    break;
                case 2: // Pie Chart
                    CreatePieChart(labels, values);
                    break;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error creating chart: {ex.Message}";
        }
    }

    /// <summary>
    /// Creates a bar chart
    /// </summary>
    private void CreateBarChart(List<string> labels, List<double> values)
    {
        IsBarChartVisible = true;
        IsPieChartVisible = false;
        HasNoChartData = false;

        ChartSeries = new ObservableCollection<ISeries>
        {
            new ColumnSeries<double>
            {
                Values = values,
                Name = SelectedYAxisColumn,
                Fill = new SolidColorPaint(SKColors.DodgerBlue)
            }
        };

        ChartXAxes = new ObservableCollection<Axis>
        {
            new Axis
            {
                Labels = labels,
                LabelsRotation = labels.Count > 10 ? 45 : 0,
                Name = SelectedXAxisColumn
            }
        };

        ChartYAxes = new ObservableCollection<Axis>
        {
            new Axis
            {
                Name = SelectedYAxisColumn
            }
        };
    }

    /// <summary>
    /// Creates a line chart
    /// </summary>
    private void CreateLineChart(List<string> labels, List<double> values)
    {
        IsBarChartVisible = true;
        IsPieChartVisible = false;
        HasNoChartData = false;

        ChartSeries = new ObservableCollection<ISeries>
        {
            new LineSeries<double>
            {
                Values = values,
                Name = SelectedYAxisColumn,
                Stroke = new SolidColorPaint(SKColors.DodgerBlue) { StrokeThickness = 2 },
                GeometryStroke = new SolidColorPaint(SKColors.DodgerBlue) { StrokeThickness = 2 },
                GeometryFill = new SolidColorPaint(SKColors.White),
                GeometrySize = 8
            }
        };

        ChartXAxes = new ObservableCollection<Axis>
        {
            new Axis
            {
                Labels = labels,
                LabelsRotation = labels.Count > 10 ? 45 : 0,
                Name = SelectedXAxisColumn
            }
        };

        ChartYAxes = new ObservableCollection<Axis>
        {
            new Axis
            {
                Name = SelectedYAxisColumn
            }
        };
    }

    /// <summary>
    /// Creates a pie chart
    /// </summary>
    private void CreatePieChart(List<string> labels, List<double> values)
    {
        IsBarChartVisible = false;
        IsPieChartVisible = true;
        HasNoChartData = false;

        var colors = new SKColor[]
        {
            SKColors.DodgerBlue, SKColors.Coral, SKColors.MediumSeaGreen,
            SKColors.Gold, SKColors.MediumPurple, SKColors.Tomato,
            SKColors.Teal, SKColors.Orange, SKColors.SlateBlue, SKColors.LimeGreen
        };

        var pieSeries = new ObservableCollection<ISeries>();
        for (int i = 0; i < Math.Min(labels.Count, 10); i++) // Limit to 10 slices
        {
            pieSeries.Add(new PieSeries<double>
            {
                Values = new[] { values[i] },
                Name = labels[i],
                Fill = new SolidColorPaint(colors[i % colors.Length])
            });
        }

        PieChartSeries = pieSeries;
    }

    #endregion
}

