using System.Linq;
using MindMapAICore.Models;
using MindMapAICore.Services;
using Xunit;
using System;
using System.IO;

namespace MindMap.Tests.Tests
{
    public class DatabaseServiceTests : IDisposable
    {
        private readonly DatabaseService _dbService;

        public DatabaseServiceTests()
        {
            var tempDb = Path.GetTempFileName();
            _dbService = new DatabaseService($"Data Source={tempDb}");
            _dbService.Initialize();
        }

        [Fact]
        public void AddNote_ShouldAddNoteAndAssignId()
        {
            var note = new Note
            {
                Title = "Тестовая заметка",
                Content = "Тестовое содержание",
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _dbService.AddNote(note);

            Assert.True(note.Id > 0, "Id должен быть присвоен после добавления");
        }

        [Fact]
        public void GetAllNotes_ShouldReturnAllNotes()
        {
            var note1 = new Note { Title = "Заметка 1", Content = "", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now };
            var note2 = new Note { Title = "Заметка 2", Content = "", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now };
            _dbService.AddNote(note1);
            _dbService.AddNote(note2);

            var notes = _dbService.GetAllNotes();

            Assert.Equal(2, notes.Count);
        }

        [Fact]
        public void UpdateNote_ShouldUpdateNoteContent()
        {
            var note = new Note { Title = "Старый заголовок", Content = "", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now };
            _dbService.AddNote(note);
            note.Title = "Новый заголовок";
            note.UpdatedAt = DateTime.Now;

            _dbService.UpdateNote(note);
            var updatedNote = _dbService.GetAllNotes().First();

            Assert.Equal("Новый заголовок", updatedNote.Title);
        }

        [Fact]
        public void DeleteNote_ShouldRemoveNote()
        {
            var note = new Note { Title = "Удаляемая заметка", Content = "", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now };
            _dbService.AddNote(note);

            _dbService.DeleteNote(note.Id);
            var notes = _dbService.GetAllNotes();

            Assert.Empty(notes);
        }

        [Fact]
        public void AddTag_ShouldAddTagAndAssignId()
        {
            var tag = new Tag { Name = "Тестовый тег" };

            _dbService.AddTag(tag);

            Assert.True(tag.Id > 0);
        }

        [Fact]
        public void GetTagByName_ShouldReturnCorrectTag()
        {
            var tag = new Tag { Name = "УникальныйТег" };
            _dbService.AddTag(tag);

            var foundTag = _dbService.GetTagByName("УникальныйТег");

            Assert.NotNull(foundTag);
            Assert.Equal(tag.Id, foundTag.Id);
        }

        [Fact]
        public void AddTagToNote_ShouldCreateRelation()
        {
            var note = new Note { Title = "Заметка", Content = "", CreatedAt = DateTime.Now, UpdatedAt = DateTime.Now };
            var tag = new Tag { Name = "Важный тег" };
            _dbService.AddNote(note);
            _dbService.AddTag(tag);

            _dbService.AddTagToNote(note.Id, tag.Id);
            var tagsForNote = _dbService.GetTagsForNote(note.Id);

            Assert.Single(tagsForNote);
            Assert.Equal(tag.Name, tagsForNote[0].Name);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
