using MindMapAI.ViewModels;
using MindMapAICore.Services;
using System.Windows;
using System.Windows.Controls;

namespace MindMapAI
{
    public partial class MainWindow : Window
    {
        private MainViewModel _viewModel;
        private Point _lastContextMenuPosition;

        public MainWindow()
        {
            InitializeComponent();

            var databaseService = new DatabaseService();
            databaseService.Initialize();

            _viewModel = new MainViewModel(databaseService);
            DataContext = _viewModel;
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
        }

        private void NewFolder_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция создания папок будет доступна в следующих версиях", "В разработке", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            var contextMenu = new ContextMenu();

            var deleteItem = new MenuItem { Header = "🗑 Удалить заметку" };
            var addTagItem = new MenuItem { Header = "🏷 Добавить теги" };
            var viewTagsItem = new MenuItem { Header = "👁 Посмотреть теги" };

            deleteItem.Click += (s, args) => {
                if (_viewModel.DeleteNoteCommand.CanExecute(null))
                    _viewModel.DeleteNoteCommand.Execute(null);
            };

            addTagItem.Click += (s, args) => {

                MessageBox.Show("Окно добавления тегов будет здесь", "Добавление тегов", MessageBoxButton.OK, MessageBoxImage.Information);
            };

            viewTagsItem.Click += (s, args) => {

                if (_viewModel.SelectedNote != null)
                    MessageBox.Show($"Теги заметки '{_viewModel.SelectedNote.Title}':\n\n(пока не реализовано)", "Теги заметки", MessageBoxButton.OK, MessageBoxImage.Information);
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
    }
}