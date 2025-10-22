using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VeritasSQL.Core.Models;
using VeritasSQL.WPF.ViewModels;

namespace VeritasSQL.WPF;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        _viewModel.PropertyChanged += ViewModel_PropertyChanged;
        
        // SQL Editor Binding
        SqlEditor.TextChanged += SqlEditor_TextChanged;
    }

    private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.GeneratedSql))
        {
            SqlEditor.Text = _viewModel.GeneratedSql;
        }
        else if (e.PropertyName == nameof(MainViewModel.CurrentSchema))
        {
            UpdateSchemaTree();
        }
    }

    private void SqlEditor_TextChanged(object? sender, EventArgs e)
    {
        if (_viewModel != null && SqlEditor.Text != _viewModel.GeneratedSql)
        {
            _viewModel.GeneratedSql = SqlEditor.Text;
        }
    }

    private void UpdateSchemaTree()
    {
        SchemaTreeView.Items.Clear();

        if (_viewModel.CurrentSchema == null)
            return;

        // Tables
        var tablesNode = new TreeViewItem { Header = $"Tables ({_viewModel.CurrentSchema.Tables.Count})" };
        foreach (var table in _viewModel.CurrentSchema.Tables.OrderBy(t => t.Schema).ThenBy(t => t.Name))
        {
            var tableNode = new TreeViewItem { Header = table.FullName };
            foreach (var column in table.Columns)
            {
                var columnHeader = $"{column.Name} ({column.DataType})";
                if (column.IsPrimaryKey) columnHeader += " [PK]";
                tableNode.Items.Add(new TreeViewItem { Header = columnHeader });
            }
            tablesNode.Items.Add(tableNode);
        }
        SchemaTreeView.Items.Add(tablesNode);

        // Views
        var viewsNode = new TreeViewItem { Header = $"Views ({_viewModel.CurrentSchema.Views.Count})" };
        foreach (var view in _viewModel.CurrentSchema.Views.OrderBy(v => v.Schema).ThenBy(v => v.Name))
        {
            var viewNode = new TreeViewItem { Header = view.FullName };
            foreach (var column in view.Columns)
            {
                var columnHeader = $"{column.Name} ({column.DataType})";
                viewNode.Items.Add(new TreeViewItem { Header = columnHeader });
            }
            viewsNode.Items.Add(viewNode);
        }
        SchemaTreeView.Items.Add(viewsNode);

        tablesNode.IsExpanded = true;
        viewsNode.IsExpanded = true;
    }

    private void SchemaFilterBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        // Schema filter logic (simplified)
        UpdateSchemaTree();
    }

    private void HistoryItem_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListViewItem item && item.Content is QueryHistoryEntry entry)
        {
            _viewModel.LoadHistoryEntryCommand.Execute(entry);
        }
    }

    private void FavoriteItem_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is ListViewItem item && item.Content is QueryHistoryEntry entry)
        {
            _viewModel.LoadHistoryEntryCommand.Execute(entry);
        }
    }

    /// <summary>
    /// Voice Input Button Click Handler
    /// Note: Full implementation requires audio recording library (e.g., NAudio)
    /// This is a placeholder showing the concept
    /// </summary>
    private async void VoiceInputButton_Click(object sender, RoutedEventArgs e)
    {
        // Placeholder implementation - in production, would use NAudio or similar
        var result = MessageBox.Show(
            "Voice Input Feature\n\n" +
            "To use voice input:\n" +
            "1. Install NAudio NuGet package\n" +
            "2. Record audio from microphone\n" +
            "3. Save as WAV file\n" +
            "4. Pass to OpenAI Whisper API\n\n" +
            "For now, you can manually upload an audio file instead.\n\n" +
            "Would you like to select an audio file?",
            "Voice Input",
            MessageBoxButton.YesNo,
            MessageBoxImage.Information);

        if (result != MessageBoxResult.Yes)
            return;

        // Demo: Allow user to select existing audio file
        var openDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Audio Files (*.wav;*.mp3;*.m4a)|*.wav;*.mp3;*.m4a|All Files (*.*)|*.*",
            Title = "Select Audio File for Transcription"
        };

        if (openDialog.ShowDialog() == true)
        {
            try
            {
                using var fileStream = System.IO.File.OpenRead(openDialog.FileName);
                await _viewModel.StartVoiceInputCommand.ExecuteAsync(fileStream);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error processing audio file: {ex.Message}",
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}