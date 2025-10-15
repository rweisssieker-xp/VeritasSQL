using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace VeritasSQL.WPF.Dialogs;

public partial class ParameterDialog : Window
{
    public Dictionary<string, string> Parameters { get; private set; } = new();
    private readonly List<string> _parameterNames;
    private readonly Dictionary<string, TextBox> _textBoxes = new();

    public ParameterDialog(List<string> parameterNames)
    {
        InitializeComponent();
        _parameterNames = parameterNames;

        CreateParameterFields();
    }

    private void CreateParameterFields()
    {
        foreach (var paramName in _parameterNames)
        {
            var grid = new Grid { Margin = new Thickness(0, 5, 0, 5) };
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

            var label = new TextBlock
            {
                Text = $"{paramName}:",
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 0, 10, 0)
            };
            Grid.SetColumn(label, 0);
            grid.Children.Add(label);

            var textBox = new TextBox
            {
                Padding = new Thickness(5),
                ToolTip = $"Enter value for parameter '{paramName}'"
            };
            Grid.SetColumn(textBox, 1);
            grid.Children.Add(textBox);

            _textBoxes[paramName] = textBox;
            ParameterPanel.Children.Add(grid);
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        // Sammle alle Parameterwerte
        foreach (var kvp in _textBoxes)
        {
            if (!string.IsNullOrWhiteSpace(kvp.Value.Text))
            {
                Parameters[kvp.Key] = kvp.Value.Text;
            }
        }

        // Validation: Check if all parameters are filled
        if (Parameters.Count < _parameterNames.Count)
        {
            MessageBox.Show("Please fill in all parameters.", 
                "Validation", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        DialogResult = true;
        Close();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}

