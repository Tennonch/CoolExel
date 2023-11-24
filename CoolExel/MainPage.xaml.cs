using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using System;
using System.Collections.Generic;

using LabCalculator;

using Grid = Microsoft.Maui.Controls.Grid;
using Microsoft.Maui;
using System.Xml;
using System.Diagnostics;


namespace CoolExel
{
    public partial class MainPage : ContentPage
    {

        public static int CountColumn = 20;
        public static int CountRow = 50;

        public MainPage()
        {
            InitializeComponent();
            CreateGrid();
            InitEntryCompleted();

        }


        private void CreateGrid()
        {
            AddColumnsAndColumnLabels();
            AddRowsAndCellEntries();
            InitializeVariables();
        }

        private void InitEntryCompleted()
        {
            //press Enter to go to the next cell in a column
            foreach (var entry in grid.Children.OfType<Entry>())
            {
                entry.Completed += (s, e) =>
                {
                    var row = Grid.GetRow(entry);
                    var col = Grid.GetColumn(entry);
                    var content = entry.Text;
                    var nextEntry = grid.Children
                        .OfType<Entry>()
                        .FirstOrDefault(e => Grid.GetRow(e) == (row + 1) &&
                        Grid.GetColumn(e) == col);
                    if (content.StartsWith("="))
                        CalculateButton_Clicked(entry, e);
                    nextEntry?.Focus();
                };

            }
        }

        private void AddColumnsAndColumnLabels()
        {
            for (int col = 0; col < CountColumn + 1; col++)
            {
                grid.ColumnDefinitions.Add(new ColumnDefinition());

                if (col > 0)
                {
                    var label = new Label()
                    {
                        Text = GetColumnName(col),
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center
                    };
                    Grid.SetRow(label, 0);
                    Grid.SetColumn(label, col);
                    grid.Children.Add(label);
                }
            }

        }

        private void AddRowsAndCellEntries()
        {
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            for (int row = 1; row <= CountRow; row++)
            {
                grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

                var label = new Label()
                {
                    Text = (row).ToString(),
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center
                };
                Grid.SetRow(label, row);
                Grid.SetColumn(label, 0);
                grid.Children.Add(label);

                for (int col = 0; col < CountColumn; col++)
                {
                    var entry = new Entry
                    {
                        Text = "",
                        VerticalOptions = LayoutOptions.Center,
                        HorizontalOptions = LayoutOptions.Center,
                        WidthRequest = 200,
                        HeightRequest = 50
                    };
                    entry.Unfocused += Entry_Unfocused;
                    entry.Focused += Entry_Focused;
                    Grid.SetRow(entry, row);
                    Grid.SetColumn(entry, col + 1);
                    grid.Children.Add(entry);
                }
            }
        }

        private string GetColumnName(int colIndex)
        {
            int divident = colIndex;
            string columnName = string.Empty;

            while (divident > 0)
            {
                int modulo = (divident - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                divident = (divident - modulo) / 26;
            }

            return columnName;
        }

        private void Entry_Focused(object sender, EventArgs e)
        {
            var entry = (Entry)sender;
            textInput.Text = entry.Text;
        }

        private async void Entry_Unfocused(object sender, FocusEventArgs e)
        {
            var entry = (Entry)sender;
            var row = Grid.GetRow(entry) - 1;
            var col = Grid.GetColumn(entry) - 1;
            textInput.Text = "";


            // =... tap on the cell you want to use in the formula
            if (entry.Text.StartsWith("="))
            {

                await Task.Delay(100);
                foreach (var anotherEntry in grid.Children.OfType<Entry>())
                {
                    if (anotherEntry.IsFocused && anotherEntry != entry)
                    {
                        var anotherRow = Grid.GetRow(anotherEntry);
                        var anotherCol = Grid.GetColumn(anotherEntry);
                        entry.Text += (GetColumnName(anotherCol) + anotherRow).ToString();
                        entry.Focus();
                    }
                }

            }
            else
            {
                double value = double.TryParse(entry.Text, out value) ? value : 0;
                LabCalculatorVisitor.tableIdentifier[GetColumnName(col + 1) + (row + 1)] = value;
            }
            entry.Unfocus();
        }

        private async void CalculateButton_Clicked(object sender, EventArgs e)
        {
            foreach (var entry in grid.Children.OfType<Entry>())
            {
                try
                {
                    string expression = entry.Text;
                    if (expression.StartsWith("="))
                    {
                        var result = LabCalculator.Calculator.Evaluate(expression.Substring(1));
                        entry.Text = result.ToString();
                        var row = Grid.GetRow(entry);
                        var col = Grid.GetColumn(entry);
                        LabCalculatorVisitor.tableIdentifier[GetColumnName(col) + row] = result;
                        return;
                    }
                }//Specified argument was out of the range of valid values. (Parameter 'i')   at System.Text.RegularExpressions.ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument arg)
                catch (Exception ex)
                {
                    entry.Text = ex.Message;
                    return;
                }
            }
            await DisplayAlert("Операція не може бути виконана.", "Вираз є константою.", "ОК");
        }

        private async void SaveButton_Clicked(object sender, EventArgs e)
        {
            List<List<string>> SaveRows = new List<List<string>>();
            List<string> cols = new List<string>();
            int countCols = 0;
            foreach (var entry in grid.Children.OfType<Entry>())
            {
                if (countCols != 0 && countCols % CountColumn == 0)
                {
                    SaveRows.Add(new List<string>(cols));
                    countCols = 0;
                    cols.Clear();
                    continue;
                }
                else
                {
                    cols.Add(entry.Text == null ? " " : entry.Text);
                    countCols++;
                }
            }
            SaveTable.DrawTable(SaveRows.Select(l => l.ToArray()).ToArray());
            await DisplayAlert("", "Ваша таблиця була збережена до папки проекту у файл table.txt", "ОК");
        }

        private async void ExitButton_Clicked(object sender, EventArgs e)
        {
            bool answer = await DisplayAlert("Підтвердження", "Ви дійсно хочете покинути застосунок?", "Так", "Ні");
            if (answer)
            {
                System.Environment.Exit(0);
            }
        }

        private async void HelpButton_Clicked(object sender, EventArgs e)
        {
            await DisplayAlert("Довідка", "ЛР1 Руденького Павла. Таблиця", "ОК");
        }

        private void AddRowButton_Clicked(Object sender, EventArgs e)
        {
            int newRow = grid.RowDefinitions.Count;

            grid.RowDefinitions.Add(new RowDefinition());

            var label = new Label
            {
                Text = newRow.ToString(),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            Grid.SetRow(label, newRow);
            Grid.SetColumn(label, 0);
            grid.Children.Add(label);

            for (int col = 0; col < CountColumn; col++)
            {
                var entry = new Entry
                {
                    Text = "",
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    WidthRequest = 200,
                    HeightRequest = 50
                };

                entry.Unfocused += Entry_Unfocused;

                Grid.SetRow(entry, newRow);
                Grid.SetColumn(entry, col + 1);
                grid.Children.Add(entry);
            }
            CountRow++;
            InitEntryCompleted();
        }

        private void DeleteRowButton_Clicked(Object sender, EventArgs e)
        {
            if (grid.RowDefinitions.Count > 1)
            {
                int lastRowIndex = grid.RowDefinitions.Count - 1;
                grid.RowDefinitions.RemoveAt(lastRowIndex);

                // Create a list to hold the children to be removed
                List<View> childrenToRemove = new List<View>();

                // Add all children in the last row to the list
                foreach (var child in grid.Children.OfType<Entry>())
                {
                    if (Grid.GetRow(child) == lastRowIndex)
                    {
                        childrenToRemove.Add(child);
                    }
                }
                // Remove the children from the grid
                foreach (var child in childrenToRemove)
                {
                    grid.Children.Remove(child);
                }

                // Find and remove the label at the beginning of the row
                foreach (var child in grid.Children)
                {
                    if (child is Label label && Grid.GetRow(label) == lastRowIndex && Grid.GetColumn(label) == 0)
                    {
                        grid.Children.Remove(label);
                        break;
                    }
                }
            }
        }


        private void AddColumnButton_Clicked(Object sender, EventArgs e)
        {
            int newColumn = grid.ColumnDefinitions.Count;

            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var label = new Label
            {
                Text = GetColumnName(newColumn),
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };
            Grid.SetRow(label, 0);
            Grid.SetColumn(label, newColumn);
            grid.Children.Add(label);

            for (int row = 0; row < CountRow; row++)
            {
                var entry = new Entry
                {
                    Text = "",
                    VerticalOptions = LayoutOptions.Center,
                    HorizontalOptions = LayoutOptions.Center,
                    WidthRequest = 200,
                    HeightRequest = 50
                };
                entry.Unfocused += Entry_Unfocused;
                Grid.SetRow(entry, row + 1);
                Grid.SetColumn(entry, newColumn);
                grid.Children.Add(entry);
            }
            CountColumn++;
            InitEntryCompleted();
        }

        private void DeleteColumnButton_Clicked(Object sender, EventArgs e)
        {
            if (grid.ColumnDefinitions.Count > 2)
            {
                int lastColumnIndex = grid.ColumnDefinitions.Count - 1;
                grid.ColumnDefinitions.RemoveAt(lastColumnIndex);
                // Create a list to hold the children to be removed
                List<View> childrenToRemove = new List<View>();

                // Add all children in the last row to the list
                foreach (var child in grid.Children.OfType<Entry>())
                {
                    if (Grid.GetColumn(child) == lastColumnIndex)
                    {
                        childrenToRemove.Add(child);
                    }
                }
                // Remove the children from the grid
                foreach (var child in childrenToRemove)
                {
                    grid.Children.Remove(child);
                }
                // Find and remove the label at the top of the column
                foreach (var child in grid.Children.OfType<Label>())
                {
                    if (Grid.GetColumn(child) == lastColumnIndex && Grid.GetRow(child) == 0)
                    {
                        grid.Children.Remove(child);
                        break;
                    }
                }
            }
        }

        private void InitializeVariables()
        {
            for (int row = 1; row <= CountRow; row++)
            {
                for (int col = 1; col <= CountColumn; col++)
                {
                    LabCalculatorVisitor.tableIdentifier.Add((GetColumnName(col) + row), 0);
                }
            }
        }
    }
}