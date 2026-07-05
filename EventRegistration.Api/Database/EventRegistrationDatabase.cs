using System.Data;
using EventRegistration.Api.Interfaces;
using MySqlConnector;

namespace EventRegistration.Api.Database;

public class EventRegistrationDatabase : IEventRegistrationDatabase
{
    private readonly string _connectionString;
    public EventRegistrationDatabase(string connectionString) => _connectionString = connectionString;

    public IDbConnection Open()
    {
        var connection = new MySqlConnection(_connectionString);
        connection.Open();
        return connection;
    }
}