using CommunityToolkit.Mvvm.Input;
using MindMapAICore.Models;
using MindMapAICore.Services;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MindMapAI.ViewModels
{
    public class TagManagerViewModel : ViewModelBase
    {
        private readonly IDatabaseService _databaseService;
        private readonly int _noteId;

        public ObservableCollection<Tag> Tags { get; set; }

        public TagManagerViewModel(IDatabaseService databaseService, int noteId, ObservableCollection<Tag> existingTags)
        {
            _databaseService = databaseService;
            _noteId = noteId;
            Tags = new ObservableCollection<Tag>(existingTags);

            AddTagCommand = new RelayCommand<string>(AddTag);
            RenameTagCommand = new RelayCommand<Tag>(RenameTag);
            UnlinkTagCommand = new RelayCommand<Tag>(UnlinkTag);
            DeleteTagCommand = new RelayCommand<Tag>(DeleteTag);
        }

        public ICommand AddTagCommand { get; }
        public ICommand RenameTagCommand { get; }
        public ICommand UnlinkTagCommand { get; }
        public ICommand DeleteTagCommand { get; }

        private void AddTag(string? tagName)
        {
            if (string.IsNullOrWhiteSpace(tagName))
                return;

            try
            {
                var existingTag = _databaseService.GetTagByName(tagName);
                if (existingTag == null)
                {
                    existingTag = new Tag { Name = tagName };
                    _databaseService.AddTag(existingTag);
                }

                if (existingTag.Id == 0)
                {
                    MessageBox.Show("Ошибка: тег не сохранился в базе", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                _databaseService.AddTagToNote(_noteId, existingTag.Id);

                if (!Tags.Any(t => t.Id == existingTag.Id))
                {
                    Tags.Add(existingTag);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RenameTag(Tag? tag)
        {
            if (tag == null) return;

            var dialog = new TextInputDialog("Редактирование тега", "Введите новое название:", tag.Name);
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputText))
            {
                var newName = dialog.InputText.Trim();
                if (newName == tag.Name) return;

                var existingTag = _databaseService.GetTagByName(newName);
                if (existingTag != null && existingTag.Id != tag.Id)
                {
                    MessageBox.Show("Тег с таким именем уже существует", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                tag.Name = newName;
                _databaseService.UpdateTag(tag);

                var index = Tags.IndexOf(tag);
                Tags[index] = tag;
            }
        }

        private void UnlinkTag(Tag? tag)
        {
            if (tag == null) return;

            var result = MessageBox.Show($"Отвязать тег '{tag.Name}' от текущей заметки?", "Подтверждение",
                MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                _databaseService.RemoveTagFromNote(_noteId, tag.Id);
                Tags.Remove(tag);
            }
        }

        private void DeleteTag(Tag? tag)
        {
            if (tag == null) return;

            var result = MessageBox.Show(
                $"Удалить тег '{tag.Name}' из системы?\nОн будет отвязан от всех заметок.",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

            if (result == MessageBoxResult.Yes)
            {
                _databaseService.DeleteTag(tag.Id);
                Tags.Remove(tag);
            }
        }
    }
}
