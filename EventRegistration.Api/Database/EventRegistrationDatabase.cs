using MySqlConnector;
using EventRegistration.Api.Interfaces;
using System.Data;

namespace EventRegistration.Api.Database;

public class EventRegistrationDatabase : IEventRegistrationDatabase
{
    private readonly string _connectionString;

    public EventRegistrationDatabase(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("Default")
            ?? throw new InvalidOperationException("Connection string 'Default' is not configured.");
    }

    public MySqlConnection Open()
    {
        var connection = new MySqlConnection(_connectionString);
        connection.Open();
        return connection;
    }

    IDbConnection IEventRegistrationDatabase.Open()
    {
        return Open();
    }
}