using FirstWeb.DTOS;
using FirstWeb.Models;
using FirstWeb.Services.Interfaces;
using System.Data.SQLite;
using static System.Reflection.Metadata.BlobBuilder;
using System.Reflection.PortableExecutable;
using System.Xml.Linq;
using System.Net;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FirstWeb.Services;

public class BookService : IBookService
{
    private const string connectionString = "Data source = mydata.db; Version =3";

    private void CreateTables()
    {
        List<string> createTableQueries = new()
        {
            "CREATE TABLE IF NOT EXISTS Books (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Price REAL, AuthorName TEXT)",
            "CREATE TABLE IF NOT EXISTS Genres(Id INTEGER PRIMARY KEY AUTOINCREMENT,Name NVARCHAR(100) NOT NULL);",
            "CREATE TABLE IF NOT EXISTS BookGenres (BookId INTEGER NOT NULL,GenreId INTEGER NOT NULL,PRIMARY KEY (BookId, GenreId),FOREIGN KEY (BookId) " +
            "REFERENCES Books (Id),FOREIGN KEY (GenreId) REFERENCES Genres (Id));"
        };

        using SQLiteConnection conn = new(connectionString);

        conn.Open();

        foreach (var query in createTableQueries)
        {
            using SQLiteCommand command = new(query, conn);
            command.ExecuteNonQuery();
        }
    }

    public BookModel Create(CreateBookModel createBookModel)
    {
        CreateTables();       

        using SQLiteConnection conn = new(connectionString);
        conn.Open();

        string findBookQuery = "SELECT * FROM Books WHERE Name = @name and Price = @price and AuthorName = @authorName";
        using SQLiteCommand findBookCommand = new(findBookQuery, conn);
        findBookCommand.Parameters.AddWithValue("@name", createBookModel.Name);
        findBookCommand.Parameters.AddWithValue("@price", createBookModel.Price);
        findBookCommand.Parameters.AddWithValue("@authorName", createBookModel.AuthorName);

        using SQLiteDataReader reader = findBookCommand.ExecuteReader();
        if (reader.HasRows)
        {
            return null;
        }
        
        string insertBookQuery = "INSERT INTO Books (Name, Price, AuthorName) " +
                                    "VALUES (@name, @price, @author)";
        using (SQLiteCommand bookCommand = new(insertBookQuery, conn))
        {
            bookCommand.Parameters.AddWithValue("@name", $"{createBookModel.Name}");
            bookCommand.Parameters.AddWithValue("@price", createBookModel.Price);
            bookCommand.Parameters.AddWithValue("@author", $"{createBookModel.AuthorName}");
            bookCommand.ExecuteNonQuery();
        }

        long bookId;
        using (SQLiteCommand getLastInsertRowIdCommand = new("SELECT last_insert_rowid();", conn))
        {
            bookId = Convert.ToInt64(getLastInsertRowIdCommand.ExecuteScalar());
        }

        foreach (var genre in createBookModel.Genres)
        {
            var genreId = GetOrCreateGenre(genre, conn);
            GetOrCreateBookGenre(bookId, genreId, conn);
        }

        WriterService writerService = new WriterService();
        CreateWriterModel writerModel = new CreateWriterModel()
        {
            FullName = createBookModel.AuthorName
        };
        long writerId = GetOrCreateWriter(writerModel, conn);
        GetOrCreateWriterBooks(writerId, bookId, conn);
        
        return GetById(bookId);
    }

    private static long GetOrCreateWriter(CreateWriterModel writer, SQLiteConnection conn)
    {
        string selectWriterQuery = "SELECT Id FROM Writers WHERE FullName = @name";
        long writerId;
        using SQLiteCommand selectWriterCommand = new(selectWriterQuery, conn);
        selectWriterCommand.Parameters.AddWithValue("@name", writer.FullName);
        var result = selectWriterCommand.ExecuteScalar();

        if (result != null)
        {
            writerId = (long)(result);
        }
        else
        {
            string insertWriterQuery = "INSERT INTO Writers (FullName) VALUES (@name)";
            using (SQLiteCommand writerCommand = new(insertWriterQuery, conn))
            {
                writerCommand.Parameters.AddWithValue("@name", writer.FullName);
                writerCommand.ExecuteNonQuery();
            }

            using SQLiteCommand getLastInsertRowIdCommand = new("SELECT last_insert_rowid();", conn);
            writerId = (long)getLastInsertRowIdCommand.ExecuteScalar();
        }        

        return writerId;
    }

    private static void GetOrCreateWriterBooks(long writerId, long bookId, SQLiteConnection connection)
    {
        string selectWriterBooksQuery = "SELECT WriterId, BookId FROM WriterBooks WHERE WriterId = @writerId and BookId = @bookId";
        using SQLiteCommand selectWriterBooksCommand = new(selectWriterBooksQuery, connection);
        selectWriterBooksCommand.Parameters.AddWithValue("@writerId", writerId);
        selectWriterBooksCommand.Parameters.AddWithValue("@bookId", bookId);

        var result = selectWriterBooksCommand.ExecuteScalar();

        if (result is not null)
        {
            return;
        }

        string insertWriterBooksQuery = "INSERT INTO WriterBooks (WriterId, BookId) VALUES (@writerId, @bookId)";
        using SQLiteCommand writerBooksCommand = new(insertWriterBooksQuery, connection);
        writerBooksCommand.Parameters.AddWithValue("@writerId", writerId);
        writerBooksCommand.Parameters.AddWithValue("@bookId", bookId);
        writerBooksCommand.ExecuteNonQuery();
    }

    public bool Delete(long id)
    {
        CreateTables();

        using var connection = new SQLiteConnection(connectionString);
        
        connection.Open();

        string deleteQuery = "DELETE FROM Books WHERE Id = @id";
        using var command = new SQLiteCommand(deleteQuery, connection);
        command.Parameters.AddWithValue("@id", id);

        int rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected > 0)
        {
            return true;
        }
        
        return false;
    }

    public List<BookModel> GetAll()
    {
        CreateTables();

        using SQLiteConnection conn = new(connectionString);
        conn.Open();

        var books = new List<Book>();
        using (var command = new SQLiteCommand("SELECT * FROM Books", conn))

        using (SQLiteDataReader reader = command.ExecuteReader())
        {
            if (reader.HasRows)
            {
                while (reader.Read())
                {

                    var book = new Book
                    {
                        Id = Convert.ToInt64(reader["Id"]),
                        Name = (string)reader["Name"],
                        AuthorName = (string)reader["AuthorName"],
                        Price = Convert.ToSingle(reader["Price"]),
                    };

                    book.Genres = GetGenres(book.Id, conn);

                    books.Add(book);
                }
            }

        }

        return books.Select(b => new BookModel
        {
            AuthorName = b.AuthorName,
            Price = b.Price,
            Genres = b.Genres.Select(g => new GenreModel
            {
                Id = g.Id,
                Name = g.Name,
            }).ToList(),
            Id = b.Id,
            Name = b.Name
        }).ToList();

    }

    public BookModel GetById(long id)
    {
        CreateTables();

        using SQLiteConnection conn = new(connectionString);
        conn.Open();

        Book book = new();
        string selectBookQuery = "SELECT * FROM Books WHERE Id = @bookId";
        using SQLiteCommand selectBookCommand = new(selectBookQuery, conn);
        selectBookCommand.Parameters.AddWithValue("@bookId", id);

        using SQLiteDataReader reader = selectBookCommand.ExecuteReader();
        if (reader.HasRows)
        {
            reader.Read();
            book = new Book
            {
                Id = Convert.ToInt64(reader["Id"]),
                Name = (string)reader["Name"],
                AuthorName = (string)reader["AuthorName"],
                Price = Convert.ToSingle(reader["Price"]),
                Genres = GetGenres(id, conn)
            };

            return new BookModel
            {
                AuthorName = book.AuthorName,
                Price = book.Price,
                Genres = book.Genres.Select(g=>new GenreModel 
                { 
                    Id=g.Id,
                    Name=g.Name,
                }).ToList(),
                Id = book.Id,
                Name = book.Name
            };
        }

        return null;

    }

    public BookModel Update(long id, CreateBookModel updateBookModel)
    {
        CreateTables();

        using SQLiteConnection connection = new(connectionString);
        connection.Open();

        string updateQuery = $"UPDATE Books SET Name = @name, Price = @price, AuthorName = @author WHERE Id = {id}";

        using SQLiteCommand command = new(updateQuery, connection);
        command.Parameters.AddWithValue("@name", updateBookModel.Name);
        command.Parameters.AddWithValue("@price", updateBookModel.Price);
        command.Parameters.AddWithValue("@author", updateBookModel.AuthorName);

        int rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected > 0)
        {
            string selectQuery = $"SELECT * FROM Books WHERE Id = {id}";

            using SQLiteCommand selectCommand = new(selectQuery, connection);
            using SQLiteDataReader reader = selectCommand.ExecuteReader();

            foreach (var genre in updateBookModel.Genres)
            {
                var genreId = GetOrCreateGenre(genre, connection);
                GetOrCreateBookGenre(id, genreId, connection);
            }

            if (reader.Read())
            {
                Book updatedModel = new()
                {
                    Id = (long)reader["Id"],
                    Name = (string)reader["Name"],
                    Price = (double)reader["Price"],
                    AuthorName = (string)reader["AuthorName"]
                };

                var genres = GetGenres(id, connection);

                return new BookModel
                {
                    AuthorName = updatedModel.AuthorName,
                    Price = updatedModel.Price,
                    Genres = genres.Select(g => new GenreModel
                    {
                        Id = g.Id,
                        Name = g.Name,
                    }).ToList(),

                    Id = updatedModel.Id,
                    Name = updatedModel.Name
                };
            }
        }

        return null;
    }

    private static List<Genre> GetGenres(long bookId, SQLiteConnection conn)
    {
        var genres = new List<Genre>();

        using var command = new SQLiteCommand($"SELECT Genres.Id, Genres.Name AS GenreName " +
                                              $"FROM Genres INNER JOIN BookGenres ON Genres.Id = BookGenres.GenreId " +
                                              $"WHERE BookGenres.BookId = @bookId;", conn);
        command.Parameters.AddWithValue("@bookId", bookId);

        using var reader = command.ExecuteReader();

        if (reader.HasRows)
        {
            while (reader.Read())
            {
                genres.Add(new Genre
                {
                    Id = Convert.ToInt64(reader["Id"]),
                    Name = (string)reader["GenreName"],
                });
            }
        }

        return genres;
    }

    private static long GetOrCreateGenre(CreateGenreModel genre, SQLiteConnection conn)
    {
        string selectGenreQuery = "SELECT Id FROM Genres WHERE Name = @name";
        long genreId;
        using (SQLiteCommand selectGenreCommand = new(selectGenreQuery, conn))
        {
            selectGenreCommand.Parameters.AddWithValue("@name", genre.Name);
            var result = selectGenreCommand.ExecuteScalar();

            if (result != null)
            {

                genreId = (long)(result);
            }
            else
            {

                string insertGenreQuery = "INSERT INTO Genres (Name) VALUES (@name)";
                using (SQLiteCommand genreCommand = new(insertGenreQuery, conn))
                {
                    genreCommand.Parameters.AddWithValue("@name", genre.Name);
                    genreCommand.ExecuteNonQuery();
                }


                using SQLiteCommand getLastInsertRowIdCommand = new("SELECT last_insert_rowid();", conn);
                genreId = (long)getLastInsertRowIdCommand.ExecuteScalar();
            }
        }

        return genreId;

    }

    private static void GetOrCreateBookGenre(long bookId, long genreId, SQLiteConnection connection)
    {
        string selectBookGenresQuery = "SELECT BookId, GenreId FROM BookGenres WHERE BookId = @bookId and GenreId = @genreId";
        using SQLiteCommand selectGenreCommand = new(selectBookGenresQuery, connection);
        selectGenreCommand.Parameters.AddWithValue("@bookId", bookId);
        selectGenreCommand.Parameters.AddWithValue("@genreId", genreId);

        var result = selectGenreCommand.ExecuteScalar();

        if (result is not null)
        {
            return;
        }
        
        string insertBookGenreQuery = "INSERT INTO BookGenres (BookId, GenreId) VALUES (@bookId, @genreId)";
        using SQLiteCommand bookGenreCommand = new(insertBookGenreQuery, connection);
        bookGenreCommand.Parameters.AddWithValue("@bookId", bookId);
        bookGenreCommand.Parameters.AddWithValue("@genreId", genreId);
        bookGenreCommand.ExecuteNonQuery();
    }

}