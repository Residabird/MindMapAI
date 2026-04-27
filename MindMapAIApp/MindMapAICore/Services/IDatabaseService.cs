using MindMapAICore.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace MindMapAICore.Services
{
    public interface IDatabaseService
    {
        void Initialize();
        List<Note> GetAllNotes();
        void AddNote(Note note);
        void UpdateNote(Note note);
        void DeleteNote(int id);
        List<Tag> GetAllTags();
        void AddTag(Tag tag);
        void DeleteTag(int id);
        Tag? GetTagByName(string name);
        void AddTagToNote(int noteId, int tagId);
        void RemoveTagFromNote(int noteId, int tagId);
        List<Tag> GetTagsForNote(int noteId);
        List<Note> GetNotesByTag(int tagId);
    }
}
