using CommunityToolkit.Mvvm.Input;
using MindMapAICore.Models;
using MindMapAICore.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MindMapAI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IDatabaseService _databaseService;

        private Note? _selectedNote;
        private string _newNoteTitle = string.Empty;
        private int? _currentNoteId = null;
        private string _newNoteContent = string.Empty;

        public Note? SelectedNote
        {
            get => _selectedNote;
            set
            {
                if (_selectedNote == value) return;

                int? previousId = _selectedNote?.Id; 

                SetField(ref _selectedNote, value);

                if (value != null)
                {
                    LoadTagsForSelectedNote();
                }
                else
                {
                    TagsForSelectedNote.Clear();
                }
                CommandManager.InvalidateRequerySuggested();
            }
        }

        public string NewNoteTitle
        {
            get => _newNoteTitle;
            set => SetField(ref _newNoteTitle, value);
        }

        public string NewNoteContent
        {
            get => _newNoteContent;
            set => SetField(ref _newNoteContent, value);
        }

        private string _filterTag = string.Empty;
        public string FilterTag
        {
            get => _filterTag;
            set
            {
                SetField(ref _filterTag, value);
                OnPropertyChanged(nameof(FilteredNotes));
            }
        }

        private string _tagsInput = string.Empty;
        public string TagsInput
        {
            get => _tagsInput;
            set => SetField(ref _tagsInput, value);
        }

        public List<Note> FilteredNotes
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FilterTag))
                    return Notes.ToList();

                var filter = FilterTag.ToLower();

                return Notes.Where(n =>
                    n.Title.ToLower().Contains(filter) ||
                    _databaseService.GetTagsForNote(n.Id).Any(t => t.Name.ToLower().Contains(filter))
                ).ToList();
            }
        }

        public MainViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;

            Notes = new ObservableCollection<Note>();
            TagsForSelectedNote = new ObservableCollection<Tag>();
            AllTags = new ObservableCollection<Tag>();

            AddNoteCommand = new RelayCommand(AddNote);
            DeleteNoteCommand = new RelayCommand(DeleteNote, CanDeleteNote);
            SaveNoteCommand = new RelayCommand(SaveNote, CanSaveNote);
            ApplyFilterCommand = new RelayCommand(ApplyFilter);

            LoadData();

            CommandManager.InvalidateRequerySuggested();
        }

        public ObservableCollection<Note> Notes { get; set; }
        public ObservableCollection<Tag> TagsForSelectedNote { get; set; }
        public ObservableCollection<Tag> AllTags { get; set; }

        public ICommand AddNoteCommand { get; }
        public ICommand DeleteNoteCommand { get; }
        public ICommand SaveNoteCommand { get; }
        public ICommand ApplyFilterCommand { get; }

        private void LoadData()     // Загрузка данных 
        {
            var notesFromDb = _databaseService.GetAllNotes();
            Notes.Clear();

            foreach (var note in notesFromDb)
            {
                Notes.Add(note);
            }

            var tagsFromDb = _databaseService.GetAllTags();

            AllTags.Clear();

            foreach (var tag in tagsFromDb)
            {
                AllTags.Add(tag);
            }
        }

        private void AddNote()  // Добавление заметки
        {
            var newNote = new Note
            {
                Title = "Новая заметка",
                Content = "",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _databaseService.AddNote(newNote);
            Notes.Add(newNote);

            // Обновляем фильтрованный список
            OnPropertyChanged(nameof(FilteredNotes));

            SelectedNote = newNote;
        }

        private void DeleteNote()
        {
            if (SelectedNote == null)
            {
                MessageBox.Show("Не выбрана заметка для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = MessageBox.Show($"Удалить заметку '{SelectedNote.Title}'?", "Подтверждение", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                var noteToDelete = SelectedNote;
                var noteIdToDelete = noteToDelete.Id;
                var noteIndex = Notes.IndexOf(noteToDelete);

                _databaseService.DeleteNote(noteIdToDelete);

                _selectedNote = null;

                Notes.RemoveAt(noteIndex);

                // Обновляем список
                OnPropertyChanged(nameof(FilteredNotes));

                if (Notes.Count > 0)
                {
                    var newIndex = noteIndex >= Notes.Count ? Notes.Count - 1 : noteIndex;
                    SelectedNote = Notes[newIndex];
                }
                else
                {
                    SelectedNote = null;
                }

                MessageBox.Show("Заметка удалена!", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanDeleteNote() => SelectedNote != null;

        private void SaveNote()
        {
            if (SelectedNote == null) return;

            SelectedNote.UpdatedAt = DateTime.Now;
            _databaseService.UpdateNote(SelectedNote);

            // Обновляем только фильтрованный список
            OnPropertyChanged(nameof(FilteredNotes));
        }

        private bool CanSaveNote() => SelectedNote != null;  // Валидация сохранения заметки

        public void LoadTagsForSelectedNote()   // Загрузка тегов для заметок
        {
            TagsForSelectedNote.Clear();

            if (SelectedNote == null)
                return;

            var tags = _databaseService.GetTagsForNote(SelectedNote.Id);

            foreach (var tag in tags)
            {
                TagsForSelectedNote.Add(tag);
            }
        }

        private void ApplyFilter()
        {
            OnPropertyChanged(nameof(FilteredNotes));
        }
    }
}