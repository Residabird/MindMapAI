using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Media;

namespace MindMapAI
{
    public class TextInputDialog : Window
    {
        public string? InputText { get; private set; }

        public TextInputDialog(string title, string prompt, string defaultValue = "")
        {
            Width = 400;
            Height = 180;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            WindowStyle = WindowStyle.ToolWindow;
            Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1E1E2E")!);

            var grid = new Grid { Margin = new Thickness(15) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var promptText = new TextBlock
            {
                Text = prompt,
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CDD6F4")!),
                Margin = new Thickness(0, 0, 0, 10)
            };
            Grid.SetRow(promptText, 0);

            var textBox = new TextBox
            {
                Text = defaultValue,
                Margin = new Thickness(0, 0, 0, 15),
                Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#242434")!),
                Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#CDD6F4")!),
                BorderBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#313244")!),
                Padding = new Thickness(8, 6, 8, 6)
            };
            AutomationProperties.SetName(textBox, "Новое название тега");
            AutomationProperties.SetHelpText(textBox, "Введите новое название для тега");
            Grid.SetRow(textBox, 1);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okButton = new Button { Content = "OK", Width = 70, Margin = new Thickness(0, 0, 10, 0) };
            AutomationProperties.SetName(okButton, "Подтвердить");
            AutomationProperties.SetHelpText(okButton, "Сохранить новое название тега");
            var cancelButton = new Button { Content = "Отмена", Width = 70 };
            AutomationProperties.SetName(cancelButton, "Отмена");
            AutomationProperties.SetHelpText(cancelButton, "Отменить редактирование тега");

            okButton.Click += (s, e) => { InputText = textBox.Text; DialogResult = true; Close(); };
            cancelButton.Click += (s, e) => { DialogResult = false; Close(); };

            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 2);

            grid.Children.Add(promptText);
            grid.Children.Add(textBox);
            grid.Children.Add(buttonPanel);

            Content = grid;
            Title = title;
        }
    }
}
