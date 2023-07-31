using FirstWeb.DTOS;
using FirstWeb.Models;
using FirstWeb.Services.Interfaces;
using System.Data.SQLite;

namespace FirstWeb.Services;

public class BookService : IBookService
{
    private const string connectionString = "Data source = mydata.db; Version =3";

    public BookModel Create(CreateBookModel createBookModel)
    {
        CreateTables();

        using SQLiteConnection conn = new(connectionString);
        conn.Open();

        string insertBookQuery = "INSERT INTO Books (Name, Price, AuthorName, WriterId) " +
                                    "VALUES (@name, @price, @author, @writerId)";
        using (SQLiteCommand bookCommand = new(insertBookQuery, conn))
        {
            bookCommand.Parameters.AddWithValue("@name", $"{createBookModel.Name}");
            bookCommand.Parameters.AddWithValue("@price", createBookModel.Price);
            bookCommand.Parameters.AddWithValue("@author", $"{createBookModel.AuthorName}");
            bookCommand.Parameters.AddWithValue("@writerId", $"{createBookModel.WriterId}");
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
            CreateBookGenre(bookId, genreId, conn);
        }

        return GetById(bookId);

    }

    public bool Delete(long id)
    {
        throw new NotImplementedException();
    }

    public List<BookModel> GetAll()
    {
        throw new NotImplementedException();
    }

    public BookModel GetById(long id)
    {
        throw new NotImplementedException();
    }

    public BookModel Update(long id, CreateBookModel updateBookModel)
    {
        throw new NotImplementedException();
    }


    private void CreateTables()
    {
        List<string> createTableQueries = new()
        {
            "CREATE TABLE IF NOT EXISTS Books (Id INTEGER PRIMARY KEY AUTOINCREMENT, Name TEXT, Price REAL, AuthorName TEXT, WriterId INTEGER)",
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

    private static void CreateBookGenre(long bookId, long genreId, SQLiteConnection connection)
    {
        string insertBookGenreQuery = "INSERT INTO BookGenres (BookId, GenreId) VALUES (@bookId, @genreId)";
        using SQLiteCommand bookGenreCommand = new(insertBookGenreQuery, connection);
        bookGenreCommand.Parameters.AddWithValue("@bookId", bookId);
        bookGenreCommand.Parameters.AddWithValue("@genreId", genreId);
        bookGenreCommand.ExecuteNonQuery();
    }

}
