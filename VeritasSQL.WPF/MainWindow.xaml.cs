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
}