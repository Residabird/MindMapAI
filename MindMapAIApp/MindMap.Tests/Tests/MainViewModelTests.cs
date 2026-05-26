using System.Collections.Generic;
using System.Linq;
using MindMapAICore.Models;
using MindMapAICore.Services;
using MindMapAI.ViewModels;
using Moq;
using Xunit;

namespace MindMap.Tests.Tests
{
    public class MainViewModelTests
    {
        private readonly Mock<IDatabaseService> _mockDb;
        private readonly MainViewModel _viewModel;

        public MainViewModelTests()
        {
            _mockDb = new Mock<IDatabaseService>();

            _mockDb.Setup(db => db.GetAllNotes()).Returns(new List<Note>());
            _mockDb.Setup(db => db.GetAllTags()).Returns(new List<Tag>());

            _viewModel = new MainViewModel(_mockDb.Object);
        }

        [Fact]
        public void AddNoteCommand_ShouldCreateNewNoteWithDefaultTitle()
        {
            Note? addedNote = null;
            _mockDb.Setup(db => db.AddNote(It.IsAny<Note>()))
                   .Callback<Note>(note => addedNote = note);

            _viewModel.AddNoteCommand.Execute(null);

            Assert.NotNull(addedNote);
            Assert.Equal("Новая заметка", addedNote.Title);
            Assert.Equal("", addedNote.Content);
            _mockDb.Verify(db => db.AddNote(It.IsAny<Note>()), Times.Once);
        }

        [Fact]
        public void AddNoteCommand_ShouldAddNoteToNotesCollection()
        {
            int initialCount = _viewModel.Notes.Count;

            _viewModel.AddNoteCommand.Execute(null);

            Assert.Equal(initialCount + 1, _viewModel.Notes.Count);
        }

        [Fact]
        public void AddNoteCommand_ShouldSelectNewlyCreatedNote()
        {
            _viewModel.AddNoteCommand.Execute(null);

            Assert.NotNull(_viewModel.SelectedNote);
            Assert.Equal("Новая заметка", _viewModel.SelectedNote.Title);
        }

        [Fact]
        public void DeleteNoteCommand_WhenNoteSelected_ShouldDeleteNote()
        {
            _viewModel.AddNoteCommand.Execute(null);
            var noteToDelete = _viewModel.SelectedNote;
            int initialCount = _viewModel.Notes.Count;

            _viewModel.DeleteNoteCommand.Execute(null);

            Assert.Equal(initialCount - 1, _viewModel.Notes.Count);
            _mockDb.Verify(db => db.DeleteNote(noteToDelete.Id), Times.Once);
        }

        [Fact]
        public void DeleteNoteCommand_WhenNoNoteSelected_ShouldNotDelete()
        {
            _viewModel.SelectedNote = null;
            int initialCount = _viewModel.Notes.Count;

            _viewModel.DeleteNoteCommand.Execute(null);

            Assert.Equal(initialCount, _viewModel.Notes.Count);
            _mockDb.Verify(db => db.DeleteNote(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public void SaveNoteCommand_WhenNoteSelected_ShouldUpdateNote()
        {
            _viewModel.AddNoteCommand.Execute(null);
            var note = _viewModel.SelectedNote;
            note.Title = "Изменённый заголовок";

            _viewModel.SaveNoteCommand.Execute(null);

            _mockDb.Verify(db => db.UpdateNote(It.Is<Note>(n => n.Title == "Изменённый заголовок")), Times.Once);
        }

        [Fact]
        public void SaveNoteCommand_WhenNoNoteSelected_ShouldNotUpdate()
        {
            _viewModel.SelectedNote = null;

            _viewModel.SaveNoteCommand.Execute(null);

            _mockDb.Verify(db => db.UpdateNote(It.IsAny<Note>()), Times.Never);
        }

        [Fact]
        public void FilteredNotes_ShouldFilterByTitle()
        {
            _viewModel.AddNoteCommand.Execute(null);
            var note = _viewModel.SelectedNote;
            note.Title = "Уникальное название для поиска";
            _viewModel.SaveNoteCommand.Execute(null);

            _viewModel.AddNoteCommand.Execute(null);
            _viewModel.SelectedNote.Title = "Обычная заметка";
            _viewModel.SaveNoteCommand.Execute(null);

            _viewModel.FilterTag = "Уникальное";

            Assert.Single(_viewModel.FilteredNotes);
            Assert.Contains("Уникальное", _viewModel.FilteredNotes[0].Title);
        }

        [Fact]
        public void FilteredNotes_WhenFilterEmpty_ShouldReturnAllNotes()
        {
            _viewModel.AddNoteCommand.Execute(null);
            _viewModel.AddNoteCommand.Execute(null);
            _viewModel.AddNoteCommand.Execute(null);
            int allNotesCount = _viewModel.Notes.Count;

            _viewModel.FilterTag = "";

            Assert.Equal(allNotesCount, _viewModel.FilteredNotes.Count);
        }
    }
}