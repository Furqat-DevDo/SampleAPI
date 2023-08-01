using FirstWeb.DTOS;
using FirstWeb.Models;
using FirstWeb.Services.Interfaces;
using System.Data.SQLite;
using System.Net;

namespace FirstWeb.Services;

public class WriterService : IWriterService
{
    private const string connectionString = "Data source = mydata.db; Version =3";

    private void CreateTables()
    {
        List<string> createTableQueries = new()
        {
            "CREATE TABLE IF NOT EXISTS Writers (Id INTEGER PRIMARY KEY AUTOINCREMENT, FullName TEXT, BirthDate DATETIME)",
            "CREATE TABLE IF NOT EXISTS WriterBooks (WriterId INT NOT NULL, BookId INT NOT NULL, PRIMARY KEY (WriterId, BookId), FOREIGN KEY (WriterId) REFERENCES Writers (Id), FOREIGN KEY (BookId) REFERENCES Books (Id);"
        };

        using SQLiteConnection conn = new(connectionString);

        conn.Open();

        foreach (var query in createTableQueries)
        {
            using SQLiteCommand command = new(query, conn);
            command.ExecuteNonQuery();
        }
    }

    public WriterModel Create(CreateWriterModel writerModel)
    {
        using SQLiteConnection conn = new SQLiteConnection(connectionString);
        conn.Open();

        string findWriterQuery = "SELECT * FROM Writers WHERE FullName = @fullName and BirthDate = @birthDate";
        using SQLiteCommand findBookCommand = new(findWriterQuery, conn);
        findBookCommand.Parameters.AddWithValue("@name", writerModel.FullName);
        findBookCommand.Parameters.AddWithValue("@price", writerModel.BirthDate);
        using SQLiteDataReader reader = findBookCommand.ExecuteReader();
        if (reader.HasRows)
        {
            return null;
        }


        string insertQuery = "INSERT INTO Writers (FullName, BirthDate) VALUES (@fullname, @birthdate)";

        using (SQLiteCommand command = new SQLiteCommand(insertQuery, conn))
        {
            command.Parameters.AddWithValue("@fullname", $"{writerModel.FullName}");
            command.Parameters.AddWithValue("@birthdate", writerModel.BirthDate);

            command.ExecuteNonQuery();
        }
        
        long writerId;
        using (SQLiteCommand getLastInsertRowIdCommand = new("SELECT last_insert_rowid();", conn))
        {
            writerId = Convert.ToInt64(getLastInsertRowIdCommand.ExecuteScalar());
        }     

        return GetById(writerId);
    }    

    public bool Delete(long id)
    {
        CreateTables();

        using var connection = new SQLiteConnection(connectionString);
        
        connection.Open();

        string deleteQuery1 = "DELETE FROM Writers WHERE Id = @id";
        using var command1 = new SQLiteCommand(deleteQuery1, connection);
        command1.Parameters.AddWithValue("@id", id);
        int rowsAffected1 = command1.ExecuteNonQuery();

        

        if (rowsAffected1 > 0 )
        {
            string deleteQuery2 = "DELETE FROM WriterBooks WHERE WriterId = @id";
            using var command2 = new SQLiteCommand(deleteQuery2, connection);
            command2.Parameters.AddWithValue("@id", id);
            int rowsAffected2 = command2.ExecuteNonQuery();

            return true;
        }

        return false;
    }

    public List<WriterModel> GetAll()
    {
        CreateTables();

        using SQLiteConnection conn = new(connectionString);
        conn.Open();

        var writers = new List<WriterModel>();
        using var command = new SQLiteCommand("SELECT * FROM Writers", conn);

        using SQLiteDataReader reader = command.ExecuteReader();
        
        if (reader.HasRows)
        {
            while (reader.Read())
            {
                var writer = new WriterModel
                {
                    Id = Convert.ToInt64(reader["Id"]),
                    FullName = (string)reader["FullName"],
                    BirthDate = Convert.ToDateTime(reader["BirthDate"]),
                };

                writers.Add(writer);
            }
        }       

        return writers;
    }

    public WriterModel GetById(long id)
    {
        CreateTables();

        using SQLiteConnection conn = new(connectionString);
        conn.Open();

        WriterModel writer = new();
        string selectWriterQuery = "SELECT * FROM Writers WHERE Id = @writerId";
        using SQLiteCommand selectWriterCommand = new(selectWriterQuery, conn);
        selectWriterCommand.Parameters.AddWithValue("@writerId", id);

        using SQLiteDataReader reader = selectWriterCommand.ExecuteReader();
        if (reader.HasRows)
        {
            reader.Read();
            writer = new WriterModel
            {
                Id = Convert.ToInt64(reader["Id"]),
                FullName = (string)reader["FullName"],
                BirthDate = Convert.ToDateTime(reader["BirthDate"]),
            };

            return writer;
        }

        return null;
    }

    public WriterModel Update(long id, CreateWriterModel updateWriterModel)
    {
        CreateTables();

        using SQLiteConnection connection = new(connectionString);
        connection.Open();

        string updateQuery = $"UPDATE Writers SET FullName = @fullName, BirthDate = @birthDate WHERE Id = {id}";

        using SQLiteCommand command = new(updateQuery, connection);
        command.Parameters.AddWithValue("@name", updateWriterModel.FullName);
        command.Parameters.AddWithValue("@birthDate", updateWriterModel.BirthDate);

        int rowsAffected = command.ExecuteNonQuery();

        if (rowsAffected > 0)
        {
            string selectQuery = $"SELECT * FROM Writers WHERE Id = {id}";

            using SQLiteCommand selectCommand = new(selectQuery, connection);
            using SQLiteDataReader reader = selectCommand.ExecuteReader();

            if (reader.Read())
            {
                WriterModel updatedModel = new()
                {
                    Id = (long)reader["Id"],
                    FullName = (string)reader["FullName"],
                    BirthDate = (DateTime)reader["BirthDate"],
                };

                return updatedModel;
            }
        }

        return null;
    }

}

