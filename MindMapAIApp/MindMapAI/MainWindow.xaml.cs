using MindMapAI.ViewModels;
using MindMapAICore.Models;
using MindMapAICore.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace MindMapAI
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private readonly IDatabaseService _databaseService;
            
        public MainWindow()
        {
            InitializeComponent();

            ApplySettings();

            _databaseService = new DatabaseService();
            _databaseService.Initialize();

            _viewModel = new MainViewModel(_databaseService);
            DataContext = _viewModel;

            this.Closing += MainWindow_Closing;

        }

        private void Settings_Click(object sender, RoutedEventArgs e)  
        {
            var settingsWindow = new SettingsWindow();
            settingsWindow.Owner = this;
            settingsWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            if (settingsWindow.ShowDialog() == true)
            {
                ApplySettings();
            }
        }

        private void Graph_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
            "Функция «Граф связей» будет доступна в версии 2.0\n\n" +
            "Она позволит визуализировать связи между заметками через общие теги.",
            "В разработке",
            MessageBoxButton.OK,
            MessageBoxImage.Information
            );
        }

        private void NewNote_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.AddNoteCommand.CanExecute(null))
                _viewModel.AddNoteCommand.Execute(null);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                TitleBox.Focus();
                TitleBox.SelectAll();
            }), System.Windows.Threading.DispatcherPriority.Background);
        }

        private void NewFolder_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
        "Функция «Папки» позволит группировать заметки по категориям.\n\n" +
        "Ожидается в версии 1.5 или 2.0.",
        "В разработке",
        MessageBoxButton.OK,
        MessageBoxImage.Information
            );
        }

        private static T? FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                    return parent;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }

        /// <summary>
        /// Открывает контекстное меню для заметки (три точки).
        /// Позволяет удалить заметку или управлять тегами.
        /// </summary>
        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is not Button button) return;
            if (FindParent<ListBoxItem>(button)?.DataContext is not Note note) return;

            _viewModel.SelectedNote = note;

            var contextMenu = new ContextMenu();
            var deleteItem = new MenuItem { Header = "🗑 Удалить заметку" };
            var addTagItem = new MenuItem { Header = "🏷 Теги" };

            deleteItem.Click += (s, args) => _viewModel.DeleteNoteCommand.Execute(null);
            addTagItem.Click += (s, args) => OpenTagManagerForCurrentNote();

            contextMenu.Items.Add(deleteItem);
            contextMenu.Items.Add(addTagItem);

            button.ContextMenu = contextMenu;
            button.ContextMenu!.IsOpen = true;
        }

        private void SaveNote_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.SaveNoteCommand.CanExecute(null))
                _viewModel.SaveNoteCommand.Execute(null);
        }

        private void DeleteNote_Click(object sender, RoutedEventArgs e)
        {
            if (_viewModel.DeleteNoteCommand.CanExecute(null))
                _viewModel.DeleteNoteCommand.Execute(null);
        }

        private void AIBot_Click(object sender, RoutedEventArgs e)
        {

            MessageBox.Show(
                "ИИ-ассистент поможет автоматически анализировать заметки и генерировать теги.\n\n" +
                "Функция будет доступна в версии 2.0.",
                "В разработке",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }

        private void MainWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            // Сохраняем текущую заметку при закрытии
            if (_viewModel.SelectedNote != null)
            {
                _viewModel.SaveNoteCommand.Execute(null);
            }
        }

        private void TagButton_Click(object sender, RoutedEventArgs e)
        {
            OpenTagManagerForCurrentNote();
        }

        private void OpenTagManagerForCurrentNote()
        {
            if (_viewModel.SelectedNote == null) return;

            var tagWindow = new TagManagerWindow(
                _databaseService,
                _viewModel.SelectedNote.Id,
                new ObservableCollection<Tag>(_viewModel.TagsForSelectedNote)
            );
            tagWindow.Owner = this;
            tagWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            tagWindow.ShowDialog();
            _viewModel.LoadTagsForSelectedNote();
        }

        private void ApplySettings()
        {
            int fontSize = Properties.Settings.Default.EditorFontSize;
            ContentBox.FontSize = fontSize;

            // Если открыт предпросмотр, обновляем его
            if (PreviewViewer.Visibility == Visibility.Visible)
            {
                var markdownText = ContentBox.Text;
                var flowDocument = MarkdownToFlowDocumentConverter.Convert(markdownText);
                PreviewViewer.Document = flowDocument;
            }
        }

        private void PreviewToggle_Checked(object sender, RoutedEventArgs e)
        {
            // Переключаем видимость
            ContentBox.Visibility = Visibility.Collapsed;
            PreviewViewer.Visibility = Visibility.Visible;

            // Конвертируем текущий Markdown в FlowDocument
            var markdownText = ContentBox.Text;
            var flowDocument = MarkdownToFlowDocumentConverter.Convert(markdownText);
            PreviewViewer.Document = flowDocument;
        }

        private void PreviewToggle_Unchecked(object sender, RoutedEventArgs e)
        {
            ContentBox.Visibility = Visibility.Visible;
            PreviewViewer.Visibility = Visibility.Collapsed;
        }
    }
}