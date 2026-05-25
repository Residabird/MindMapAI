using MindMapAICore.Models;
using MindMapAICore.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;

namespace MindMapAI
{
    public partial class TagManagerWindow : Window
    {
        private readonly IDatabaseService _databaseService;
        private readonly int _noteId;

        public ObservableCollection<Tag> Tags { get; set; }

        public TagManagerWindow(IDatabaseService databaseService, int noteId, ObservableCollection<Tag> existingTags)
        {
            InitializeComponent();

            _databaseService = databaseService;
            _noteId = noteId;

            Tags = new ObservableCollection<Tag>(existingTags);
            TagsListBox.ItemsSource = Tags;
        }

        private void NewTagBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;

                string tagName = NewTagBox.Text?.Trim() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(tagName))
                {
                    CancelNewTag();
                    return;
                }

                try
                {
                    if (_databaseService == null)
                    {
                        MessageBox.Show("Ошибка: сервис базы данных не инициализирован", "Критическая ошибка");
                        return;
                    }

                    var existingTag = _databaseService.GetTagByName(tagName);
                    if (existingTag == null)
                    {
                        existingTag = new Tag { Name = tagName };
                        _databaseService.AddTag(existingTag);
                    }

                    if (existingTag.Id == 0)
                    {
                        MessageBox.Show("Ошибка: тег не сохранился в базе", "Ошибка");
                        return;
                    }

                    _databaseService.AddTagToNote(_noteId, existingTag.Id);

                    if (!Tags.Any(t => t.Id == existingTag.Id))
                    {
                        Tags.Add(existingTag);
                    }

                    CancelNewTag();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}\n\n{ex.StackTrace}", "Детали");
                }
            }
            else if (e.Key == Key.Escape)
            {
                CancelNewTag();
            }
        }

        private void CancelNewTag()
        {
            NewTagBox.Text = "";
            NewTagBox.Visibility = Visibility.Collapsed;
            NewTagButton.Visibility = Visibility.Visible;
        }

        private void EditTag_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tag = button?.DataContext as Tag;
            if (tag == null) return;

            // Запрашиваем новое имя тега
            var inputDialog = new TextInputDialog("Редактирование тега", "Введите новое название:", tag.Name);
            inputDialog.Owner = this;
            if (inputDialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(inputDialog.InputText))
            {
                var newName = inputDialog.InputText.Trim();
                if (newName != tag.Name)
                {
                    // Проверяем, не существует ли уже тег с таким именем
                    var existingTag = _databaseService.GetTagByName(newName);
                    if (existingTag != null && existingTag.Id != tag.Id)
                    {
                        MessageBox.Show("Тег с таким именем уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }

                    // Обновляем тег
                    tag.Name = newName;
                    _databaseService.UpdateTag(tag); // Нужно добавить этот метод в DatabaseService

                    // Обновляем отображение
                    var index = Tags.IndexOf(tag);
                    Tags[index] = tag;
                }
            }
        }

        private void DeleteTag_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var tag = button?.DataContext as Tag;
            if (tag == null) return;

            var result = MessageBox.Show($"Удалить тег '{tag.Name}' из заметки?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _databaseService.RemoveTagFromNote(_noteId, tag.Id);
                Tags.Remove(tag);
            }
        }
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void NewTagButton_Click(object sender, RoutedEventArgs e)
        {
            NewTagBox.Visibility = Visibility.Visible;
            NewTagButton.Visibility = Visibility.Collapsed;
            NewTagBox.Focus();
        }

    }

    // Простое диалоговое окно для ввода текста
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
            Grid.SetRow(textBox, 1);

            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            var okButton = new Button { Content = "OK", Width = 70, Margin = new Thickness(0, 0, 10, 0) };
            var cancelButton = new Button { Content = "Отмена", Width = 70 };

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