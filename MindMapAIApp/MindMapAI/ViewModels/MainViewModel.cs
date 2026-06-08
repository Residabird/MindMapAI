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
    public enum SortField { UpdatedAt, CreatedAt, Title }
    public enum SortDirection { Descending, Ascending }

    public class SortOption
    {
        public SortField Value { get; set; }
        public string DisplayName { get; set; } = string.Empty;
        public override string ToString() => DisplayName;
    }

    public class TagFilterItem
    {
        public int? TagId { get; set; }
        public string Name { get; set; } = string.Empty;
        public override string ToString() => Name;
    }

    public class MainViewModel : ViewModelBase
    {
        private readonly IDatabaseService _databaseService;

        private Note? _selectedNote;
        private string _newNoteTitle = string.Empty;
        private int? _currentNoteId = null;
        private string _newNoteContent = string.Empty;
        private Dictionary<int, List<Tag>> _noteTagsCache = new();

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

        private SortField _selectedSortField = SortField.UpdatedAt;
        private bool _isSortAscending;
        private int? _filterByTagId;

        public SortField SelectedSortField
        {
            get => _selectedSortField;
            set
            {
                SetField(ref _selectedSortField, value);
                OnPropertyChanged(nameof(FilteredNotes));
            }
        }

        public bool IsSortAscending
        {
            get => _isSortAscending;
            set
            {
                SetField(ref _isSortAscending, value);
                OnPropertyChanged(nameof(FilteredNotes));
            }
        }

        public int? FilterByTagId
        {
            get => _filterByTagId;
            set
            {
                SetField(ref _filterByTagId, value);
                OnPropertyChanged(nameof(FilteredNotes));
            }
        }

        public ObservableCollection<Tag> UsedTags { get; set; }
        public ObservableCollection<TagFilterItem> TagFilterItems { get; set; }
        public List<SortOption> SortOptions { get; } = new()
        {
            new SortOption { Value = SortField.UpdatedAt, DisplayName = "По дате изменения" },
            new SortOption { Value = SortField.CreatedAt, DisplayName = "По дате создания" },
            new SortOption { Value = SortField.Title, DisplayName = "По названию" },
        };

        public List<Note> FilteredNotes
        {
            get
            {
                var filtered = Notes.AsEnumerable();

                var filter = FilterTag?.ToLower();
                if (!string.IsNullOrWhiteSpace(filter))
                {
                    filtered = filtered.Where(n =>
                        n.Title.ToLower().Contains(filter) ||
                        GetCachedTags(n.Id).Any(t => t.Name.ToLower().Contains(filter))
                    );
                }

                if (FilterByTagId.HasValue)
                {
                    var tagId = FilterByTagId.Value;
                    filtered = filtered.Where(n =>
                        GetCachedTags(n.Id).Any(t => t.Id == tagId)
                    );
                }

                filtered = IsSortAscending
                    ? filtered.OrderBy(n => GetSortValue(n))
                    : filtered.OrderByDescending(n => GetSortValue(n));

                return filtered.ToList();
            }
        }

        private List<Tag> GetCachedTags(int noteId) =>
            _noteTagsCache.TryGetValue(noteId, out var tags) ? tags : new List<Tag>();

        private object GetSortValue(Note n) => SelectedSortField switch
        {
            SortField.Title => n.Title,
            SortField.CreatedAt => n.CreatedAt,
            SortField.UpdatedAt => n.UpdatedAt,
            _ => n.UpdatedAt
        };

        public MainViewModel(IDatabaseService databaseService)
        {
            _databaseService = databaseService;

            Notes = new ObservableCollection<Note>();
            TagsForSelectedNote = new ObservableCollection<Tag>();
            AllTags = new ObservableCollection<Tag>();
            UsedTags = new ObservableCollection<Tag>();
            TagFilterItems = new ObservableCollection<TagFilterItem>();

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

        public void LoadData()     // Загрузка данных 
        {
            var notesFromDb = _databaseService.GetAllNotes();
            Notes.Clear();

            foreach (var note in notesFromDb)
            {
                Notes.Add(note);
            }

            _noteTagsCache = _databaseService.GetAllNoteTags();

            var tagsFromDb = _databaseService.GetAllTags();

            AllTags.Clear();

            foreach (var tag in tagsFromDb)
            {
                AllTags.Add(tag);
            }

            var usedTags = _databaseService.GetUsedTags() ?? new List<Tag>();

            UsedTags.Clear();
            TagFilterItems.Clear();
            TagFilterItems.Add(new TagFilterItem { TagId = null, Name = "Все заметки" });

            foreach (var tag in usedTags)
            {
                UsedTags.Add(tag);
                TagFilterItems.Add(new TagFilterItem { TagId = tag.Id, Name = tag.Name });
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

        protected virtual MessageBoxResult ConfirmDelete() =>
            MessageBox.Show(
                $"Удалить заметку '{SelectedNote?.Title}'?",
                "Подтверждение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question
            );

        private void DeleteNote()
        {
            if (SelectedNote == null)
            {
                MessageBox.Show("Не выбрана заметка для удаления", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var result = ConfirmDelete();
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

            var tags = _databaseService.GetTagsForNote(SelectedNote.Id) ?? new List<Tag>();

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