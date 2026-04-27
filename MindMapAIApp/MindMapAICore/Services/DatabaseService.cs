using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using MindMapAICore.Models;
using Microsoft.Data.Sqlite;

namespace MindMapAICore.Services
{
    public class DatabaseService : IDatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            string appDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dbFolder = Path.Combine(appDataPath, "MindMapAI");
            Directory.CreateDirectory(dbFolder);
            string dbPath = Path.Combine(dbFolder, "notes.db");
            
            _connectionString = $"Data Source={dbPath};";
        }

        public void Initialize()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            string sql = @"CREATE TABLE IF NOT EXISTS Notes(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Content TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
                )";

            using var command = new SqliteCommand(sql, connection);
            command.ExecuteNonQuery();
        }

        public List<Note> GetAllNotes()
        {
            var notes = new List<Note>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT Id, Title, Content, CreatedAt, UpdatedAt FROM Notes ORDER BY UpdatedAt DESC";

            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                var note = new Note
                {
                    Id = reader.GetInt32(0),
                    Title = reader.GetString(1),
                    Content = reader.GetString(2),
                    CreatedAt = DateTime.Parse(reader.GetString(3)),
                    UpdatedAt = DateTime.Parse(reader.GetString(4))
                };
                notes.Add(note);
            }
            return notes;
        }

        public void AddNote(Note note)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = @"INSERT INTO Notes (Title, Content, CreatedAt, UpdatedAt)
                  VALUES (@title, @content, @createdAt, @updatedAt);
                  SELECT last_insert_rowid()";

            using var command = new SqliteCommand( sql, connection);

            command.Parameters.AddWithValue("@title", note.Title);
            command.Parameters.AddWithValue("@content", note.Content);
            command.Parameters.AddWithValue("@createdAt", note.CreatedAt.ToString("o"));
            command.Parameters.AddWithValue("@updatedAt", note.UpdatedAt.ToString("o"));

            var newId = Convert.ToInt32(command.ExecuteScalar());
            note.Id = newId;
        }

        public void UpdateNote(Note note)
        {
            using var connection = new SqliteConnection( _connectionString);
            connection.Open();

            string sql = "UPDATE Notes SET Title = @title, Content = @content, UpdatedAt = @updatedAt WHERE Id = @id";

            using var command = new SqliteCommand(sql, connection);

            command.Parameters.AddWithValue("@title", note.Title);
            command.Parameters.AddWithValue("@content", note.Content);
            command.Parameters.AddWithValue("@updatedAt", note.UpdatedAt.ToString("o"));
            command.Parameters.AddWithValue("@id", note.Id);

            command.ExecuteNonQuery();
            }
        }
    }
