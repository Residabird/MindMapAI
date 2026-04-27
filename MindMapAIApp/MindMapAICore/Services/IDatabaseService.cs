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
    }
}
