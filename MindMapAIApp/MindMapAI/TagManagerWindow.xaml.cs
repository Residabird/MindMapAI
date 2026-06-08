using MindMapAI.ViewModels;
using MindMapAICore.Models;
using MindMapAICore.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MindMapAI
{
    public partial class TagManagerWindow : Window
    {
        private readonly TagManagerViewModel _viewModel;

        public TagManagerWindow(IDatabaseService databaseService, int noteId, ObservableCollection<Tag> existingTags)
        {
            InitializeComponent();

            _viewModel = new TagManagerViewModel(databaseService, noteId, existingTags);
            DataContext = _viewModel;

            TagsListBox.ItemsSource = _viewModel.Tags;
        }

        private void NewTagBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                _viewModel.AddTagCommand.Execute(NewTagBox.Text?.Trim());
                CancelNewTag();
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
            if (sender is Button button && button.DataContext is Tag tag)
            {
                _viewModel.RenameTagCommand.Execute(tag);
            }
        }

        private void DeleteTag_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.DataContext is Tag tag)
            {
                _viewModel.DeleteTagCommand.Execute(tag);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void NewTagButton_Click(object sender, RoutedEventArgs e)
        {
            NewTagBox.Visibility = Visibility.Visible;
            NewTagButton.Visibility = Visibility.Collapsed;
            NewTagBox.Focus();
        }
    }
}
