using MindMapAI.ViewModels;
using MindMapAICore.Models;
using MindMapAICore.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media;

namespace MindMapAI
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private IDatabaseService _databaseService;
        private Point _lastContextMenuPosition;
            
        public MainWindow()
        {
            InitializeComponent();

            _databaseService = new DatabaseService();
            _databaseService.Initialize();

            _viewModel = new MainViewModel(_databaseService);
            DataContext = _viewModel;

            this.Closing += MainWindow_Closing;

        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            // TODO: открыть окно настроек
        }

        private void Graph_Click(object sender, RoutedEventArgs e)
        {
            // TODO: граф связей 
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            // TODO: навигация назад
        }

        private void Forward_Click(object sender, RoutedEventArgs e)
        {
            // TODO: навигация вперёд
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
            MessageBox.Show("Функция создания папок будет доступна в следующих версиях", "В разработке", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            while (child != null)
            {
                if (child is T parent)
                    return parent;
                child = VisualTreeHelper.GetParent(child);
            }
            return null;
        }

        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var listBoxItem = FindParent<ListBoxItem>(button);
            var note = listBoxItem?.DataContext as Note;

            if (note == null) return;

            // Временно выбираем заметку
            _viewModel.SelectedNote = note;

            var contextMenu = new ContextMenu();

            var deleteItem = new MenuItem { Header = "🗑 Удалить заметку" };
            var addTagItem = new MenuItem { Header = "🏷 Добавить теги" };
            var viewTagsItem = new MenuItem { Header = "👁 Посмотреть теги" };

            deleteItem.Click += (s, args) => {
                if (_viewModel.DeleteNoteCommand.CanExecute(null))
                    _viewModel.DeleteNoteCommand.Execute(null);
            };

            addTagItem.Click += (s, args) => {
                var tagWindow = new TagManagerWindow(
                    _databaseService, 
                    note.Id,
                    new ObservableCollection<Tag>(_viewModel.TagsForSelectedNote)
                );
                tagWindow.Owner = this;
                tagWindow.WindowStartupLocation = WindowStartupLocation.CenterOwner;
                tagWindow.ShowDialog();
                _viewModel.LoadTagsForSelectedNote();
            };

            viewTagsItem.Click += (s, args) => {
                var tags = _viewModel.TagsForSelectedNote;
                if (tags.Count == 0)
                {
                    MessageBox.Show("У этой заметки нет тегов", "Теги", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var tagsList = string.Join(", ", tags.Select(t => t.Name));
                    MessageBox.Show($"Теги заметки: {tagsList}", "Теги", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            };

            contextMenu.Items.Add(deleteItem);
            contextMenu.Items.Add(addTagItem);
            contextMenu.Items.Add(viewTagsItem);

            button.ContextMenu = contextMenu;
            button.ContextMenu.IsOpen = true;
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

            MessageBox.Show("ИИ-ассистент поможет анализировать заметки и генерировать теги\n\n(функция будет доступна в версии 2.0)", "MindMap AI Assistant", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Сохраняем текущую заметку при закрытии
            if (_viewModel.SelectedNote != null)
            {
                _viewModel.SaveNoteCommand.Execute(null);
            }
        }
    }
}