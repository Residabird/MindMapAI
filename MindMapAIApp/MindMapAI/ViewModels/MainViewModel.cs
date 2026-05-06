using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using MindMapAICore.Models;
using System;
using MindMapAICore.Services;

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

        private void LoadData()
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

        private void AddNote()
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
    }
}
