using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MindMapAICore.Models;
using System;
using MindMapAICore.Services;
using System.Linq;

namespace MindMapAI.ViewModels
{
    public class MainViewModel : ViewModelBase 
    {
        private readonly IDatabaseService _databaseService;

        private Note? _selectedNote;
        private string _newNoteTitle = string.Empty;
        private string _newNoteContent = string.Empty;

        public Note? SelectedNote  
        {
            get => _selectedNote;
            set
            {
                SetField(ref _selectedNote, value);
                LoadTagsForSelectedNote();
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
                if (string.IsNullOrEmpty(FilterTag)) return Notes.ToList();

                return Notes.Where(n => _databaseService.GetTagsForNote(n.Id).Any(t => t.Name.Contains(FilterTag, StringComparison.OrdinalIgnoreCase))).ToList();
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

            LoadData();
        }

        public ObservableCollection<Note> Notes { get; set; }   
        public ObservableCollection<Tag> TagsForSelectedNote { get; set; }
        public ObservableCollection<Tag> AllTags { get; set; }

        public ICommand AddNoteCommand { get;}  
        public ICommand DeleteNoteCommand { get;}
        public ICommand SaveNoteCommand { get; }

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
            if (string.IsNullOrWhiteSpace(NewNoteTitle))
                return;

                var note = new Note
                {
                    Title = NewNoteTitle,
                    Content = NewNoteContent,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                _databaseService.AddNote(note);
                Notes.Add(note);

                NewNoteTitle = string.Empty;
                NewNoteContent = string.Empty;
        }

        private void DeleteNote()   // Удаление заметки
        {
            if (SelectedNote == null)
                return;

            _databaseService.DeleteNote(SelectedNote.Id);
            Notes.Remove(SelectedNote);
            SelectedNote = null;
        }

        private bool CanDeleteNote() => SelectedNote != null;    // Валидация удаления заметки

        private void SaveNote() // Сохранение заметки
        {
            if (SelectedNote == null)
                return;

            SelectedNote.UpdatedAt = DateTime.Now;
            _databaseService.UpdateNote(SelectedNote);

            var index = Notes.IndexOf(SelectedNote);
            Notes[index] = SelectedNote;
        }

        private bool CanSaveNote() => SelectedNote != null;  // Валидация сохранения заметки 

        private void LoadTagsForSelectedNote()   // Загрузка тегов для заметок
        {
            TagsForSelectedNote.Clear();

            if (SelectedNote == null)
                return;

            var tags = _databaseService.GetTagsForNote(SelectedNote.Id);

            foreach ( var tag in tags)
            {
                TagsForSelectedNote.Add(tag);
            }
        }
    }
}