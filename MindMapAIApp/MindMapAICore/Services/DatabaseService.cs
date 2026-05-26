using Microsoft.Data.Sqlite;
using MindMapAICore.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

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

        // Конструктор для тестов (принимает строку подключения)
        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public void Initialize() // Инициализация , создание таблиц
        {
            using var connection = new SqliteConnection(_connectionString); // Подключение к БД
            connection.Open();
            // Создание таблицы заметок
            string sqlNotes = @"                    
                CREATE TABLE IF NOT EXISTS Notes(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Title TEXT NOT NULL,
                Content TEXT NOT NULL,
                CreatedAt TEXT NOT NULL,
                UpdatedAt TEXT NOT NULL
                )";

            using var cmdNotes = new SqliteCommand(sqlNotes, connection);
            cmdNotes.ExecuteNonQuery();
            // Создание таблицы тегов 
            string sqlTags = @"
            CREATE TABLE IF NOT EXISTS Tags(
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Name TEXT NOT NULL UNIQUE
                )";

            using var cmdTags = new SqliteCommand(sqlTags, connection);
            cmdTags.ExecuteNonQuery();
            // Создание таблицы связей (многие ко многим)
            string sqlNoteTags = @"
             CREATE TABLE IF NOT EXISTS NoteTags (
                NoteId INTEGER NOT NULL,
                TagId INTEGER NOT NULL,
                PRIMARY KEY (NoteId, TagId),
                FOREIGN KEY (NoteId) REFERENCES Notes(Id) ON DELETE CASCADE,
                FOREIGN KEY (TagId) REFERENCES Tags(Id) ON DELETE CASCADE
                )";

            using var cmdNoteTags = new SqliteCommand(sqlNoteTags, connection);
            cmdNoteTags.ExecuteNonQuery();
        }

        public List<Note> GetAllNotes() // Получение всех заметок 
        {
            var notes = new List<Note>();   // Создание списка для заметок

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT Id, Title, Content, CreatedAt, UpdatedAt FROM Notes ORDER BY UpdatedAt DESC";  // Выбираем поля

            using var command = new SqliteCommand(sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())   // Присваиваем полям позиции в таблице и добавляем их в notes
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

        public void AddNote(Note note)  // Добавление заметок 
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            // Добавляем в поля значения @title, @content, @createdAt, @updatedAt
            string sql = @"
                  INSERT INTO Notes (Title, Content, CreatedAt, UpdatedAt) 
                  VALUES (@title, @content, @createdAt, @updatedAt);
                  SELECT last_insert_rowid()";

            using var command = new SqliteCommand(sql, connection);
            // Добавляем параметры 
            command.Parameters.AddWithValue("@title", note.Title);
            command.Parameters.AddWithValue("@content", note.Content);
            command.Parameters.AddWithValue("@createdAt", note.CreatedAt.ToString("o"));
            command.Parameters.AddWithValue("@updatedAt", note.UpdatedAt.ToString("o"));

            var newId = Convert.ToInt32(command.ExecuteScalar());
            note.Id = newId;
        }

        public void UpdateNote(Note note)   // Обновление заметки
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();
            // Обновляем заметки и ставим названия  Title , Content, UpdatedAt где Id = @id
            string sql = "UPDATE Notes SET Title = @title, Content = @content, UpdatedAt = @updatedAt WHERE Id = @id";

            using var command = new SqliteCommand(sql, connection);

            command.Parameters.AddWithValue("@title", note.Title);
            command.Parameters.AddWithValue("@content", note.Content);
            command.Parameters.AddWithValue("@updatedAt", note.UpdatedAt.ToString("o"));
            command.Parameters.AddWithValue("@id", note.Id);

            command.ExecuteNonQuery();

        }

        public void DeleteNote(int id)  // Удаление заметки по id
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "DELETE FROM Notes WHERE Id = @id";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@id",id);

            int rows = command.ExecuteNonQuery();

            Debug.WriteLine($"Deleted {rows} rows");
        }

        public List<Tag> GetAllTags()   // Получение всех тегов
        {
            var tags = new List<Tag>(); // Создание списка для тегов

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT Id, Name FROM Tags ORDER BY Name"; // Выбираем поля
            using var command = new SqliteCommand( sql, connection);
            using var reader = command.ExecuteReader();

            while (reader.Read())   // Присваиваем полям позиции и добавляем их в tags
            {
                var tag = new Tag
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                };
                tags.Add(tag);
            }

            return tags;
        }

        public void AddTag(Tag tag)     // Добавление тегов
        {
            using var connection = new SqliteConnection( _connectionString);
            connection.Open();
            // Добавляем значение @name
            string sql = @"
            INSERT INTO Tags (Name) 
            VALUES (@name);
            SELECT last_insert_rowid()";

            using var command = new SqliteCommand(sql, connection);

            command.Parameters.AddWithValue("@name", tag.Name);

            var newTag = Convert.ToInt32(command.ExecuteScalar());
            tag.Id = newTag;
        }

        public void DeleteTag(int id)   // Удаление тега
        {
            using var connection = new SqliteConnection( _connectionString);
            connection.Open();

            string sql = "DELETE FROM Tags WHERE Id = @id";

            using var command = new SqliteCommand(sql,connection);
            command.Parameters.AddWithValue("@id", id);
            command.ExecuteNonQuery();
        }

        public Tag? GetTagByName(string name)   // Получение тега по имени
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "SELECT Id, Name FROM Tags WHERE Name = @name";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@name", name);

            using var reader = command.ExecuteReader();

            if (reader.Read())   // Если имя не пустое, то возвращаем Tag
            {
                return new Tag
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                };
            }
                return null;
        }

        public void AddTagToNote(int noteId, int tagId)     // Добавление тегов к заметке
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = @"
                         INSERT OR IGNORE INTO NoteTags (NoteId, TagId)
                            VALUES (@noteId, @tagId)";

            using var command = new SqliteCommand( sql, connection);
            command.Parameters.AddWithValue("@noteId", noteId);
            command.Parameters.AddWithValue("@tagId", tagId);

            command.ExecuteNonQuery();
        }

        public void RemoveTagFromNote(int noteId, int tagId)    // Удалить тег из заметки
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = @"DELETE FROM NoteTags WHERE NoteId = @noteId AND TagId = @tagId";

            using var command = new SqliteCommand(sql,connection);

            command.Parameters.AddWithValue("@noteId", noteId);
            command.Parameters.AddWithValue("@tagId", tagId);

            command.ExecuteNonQuery();
        }

        public List<Tag> GetTagsForNote(int noteId) // Получить тег по заметке
        {
            var tags = new List<Tag>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = @"
                            SELECT t.Id, t.Name FROM Tags t
                            JOIN NoteTags nt ON t.Id = nt.TagId
                            WHERE nt.NoteId = @noteId
                            ORDER BY t.Name";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@noteId", noteId);

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var tag = new Tag
                {
                    Id = reader.GetInt32(0),
                    Name = reader.GetString(1)
                };

                tags.Add(tag);
            }
            return tags;
        }
        

        public List<Note> GetNotesByTag(int tagId)   // Получить заметкут по тегу
        {
            var notes = new List<Note>();

            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = @"
                            SELECT n.Id, n.Title, n.Content, n.CreatedAt, n.UpdatedAt FROM Notes n
                            JOIN NoteTags nt ON n.Id = nt.NoteId
                            WHERE nt.TagId = @tagId
                            ORDER BY n.UpdatedAt DESC";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@tagId", tagId);

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

        public void UpdateTag(Tag tag)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            string sql = "UPDATE Tags SET Name = @name WHERE Id = @id";

            using var command = new SqliteCommand(sql, connection);
            command.Parameters.AddWithValue("@name", tag.Name);
            command.Parameters.AddWithValue("@id", tag.Id);

            command.ExecuteNonQuery();
        }

    }
}
