using CommunityToolkit.Mvvm.Input;
using MindMapAICore.Models;
using MindMapAICore.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace MindMapAI.ViewModels
{
    public enum NoteSortField
    {
        [Description("По дате изменения")]
        UpdatedAt,
        [Description("По дате создания")]
        CreatedAt,
        [Description("По названию")]
        Title
    }

    public class MainViewModel : ViewModelBase
    {
        private readonly IDatabaseService _databaseService;

        private Note? _selectedNote;
        private string _newNoteTitle = string.Empty;
        private int? _currentNoteId = null;
        private string _newNoteContent = string.Empty;

        private NoteSortField _selectedSortField = NoteSortField.UpdatedAt;
        private bool _sortAscending = false;
        private Tag? _filterByTag;
        private readonly Tag _allTagsPlaceholder;
        private ObservableCollection<Tag> _filterTagOptions = null!;

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

        public NoteSortField SelectedSortField
        {
            get => _selectedSortField;
            set
            {
                SetField(ref _selectedSortField, value);
                OnPropertyChanged(nameof(FilteredNotes));
            }
        }

        public bool SortAscending
        {
            get => _sortAscending;
            set
            {
                SetField(ref _sortAscending, value);
                OnPropertyChanged(nameof(FilteredNotes));
            }
        }

        public Tag? FilterByTag
        {
            get => _filterByTag;
            set
            {
                SetField(ref _filterByTag, value);
                OnPropertyChanged(nameof(FilteredNotes));
            }
        }

        public ObservableCollection<Tag> FilterTagOptions
        {
            get => _filterTagOptions;
            set => SetField(ref _filterTagOptions, value);
        }

        public string SortArrow => SortAscending ? "▲" : "▼";

        public List<KeyValuePair<NoteSortField, string>> SortFieldOptions => new()
        {
            new(NoteSortField.UpdatedAt, "По дате изм."),
            new(NoteSortField.CreatedAt, "По дате созд."),
            new(NoteSortField.Title, "По названию")
        };

        public List<Note> FilteredNotes
        {
            get
            {
                IEnumerable<Note> query = Notes;

                // Текстовый фильтр (по заголовку или тегам)
                if (!string.IsNullOrWhiteSpace(FilterTag))
                {
                    var filter = FilterTag.ToLower();
                    query = query.Where(n =>
                        n.Title.ToLower().Contains(filter) ||
                        _databaseService.GetTagsForNote(n.Id).Any(t => t.Name.ToLower().Contains(filter))
                    );
                }

                // Фильтр по конкретному тегу
                if (FilterByTag != null && FilterByTag.Id != _allTagsPlaceholder.Id)
                {
                    var tagId = FilterByTag.Id;
                    query = query.Where(n => _databaseService.GetTagsForNote(n.Id).Any(t => t.Id == tagId));
                }

                // Сортировка
                query = SelectedSortField switch
                {
                    NoteSortField.Title => SortAscending
                        ? query.OrderBy(n => n.Title)
                        : query.OrderByDescending(n => n.Title),
                    NoteSortField.CreatedAt => SortAscending
                        ? query.OrderBy(n => n.CreatedAt)
                        : query.OrderByDescending(n => n.CreatedAt),
                    NoteSortField.UpdatedAt => SortAscending
                        ? query.OrderBy(n => n.UpdatedAt)
                        : query.OrderByDescending(n => n.UpdatedAt),
                    _ => query.OrderByDescending(n => n.UpdatedAt)
                };

                return query.ToList();
            }
        }

        public MainViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;

            _allTagsPlaceholder = new Tag { Id = -1, Name = "Все теги" };
            _filterByTag = _allTagsPlaceholder;

            Notes = new ObservableCollection<Note>();
            TagsForSelectedNote = new ObservableCollection<Tag>();
            AllTags = new ObservableCollection<Tag>();
            FilterTagOptions = new ObservableCollection<Tag>();

            AddNoteCommand = new RelayCommand(AddNote);
            DeleteNoteCommand = new RelayCommand(DeleteNote, CanDeleteNote);
            SaveNoteCommand = new RelayCommand(SaveNote, CanSaveNote);
            ApplyFilterCommand = new RelayCommand(ApplyFilter);
            ToggleSortDirectionCommand = new RelayCommand(ToggleSortDirection);
            ClearTagFilterCommand = new RelayCommand(ClearTagFilter);

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
        public ICommand ToggleSortDirectionCommand { get; }
        public ICommand ClearTagFilterCommand { get; }

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
            FilterTagOptions.Clear();
            FilterTagOptions.Add(_allTagsPlaceholder);

            foreach (var tag in tagsFromDb)
            {
                AllTags.Add(tag);
                FilterTagOptions.Add(tag);
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

        private void ToggleSortDirection()
        {
            SortAscending = !SortAscending;
        }

        private void ClearTagFilter()
        {
            FilterByTag = _allTagsPlaceholder;
        }
    }
}