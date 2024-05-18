using CodeMechanic.Diagnostics;
using CodeMechanic.Types;
using MySql.Data.MySqlClient;

namespace CodeMechanic.Systemd.Daemons;

public static class MySQLExceptionLogger
{
    public static async Task<int> LogInfo(string message,
        string application_name = nameof(CodeMechanic.Systemd.Daemons))
    {
        Console.WriteLine("logging message :>> " + message);

        try
        {
            var connectionString = SQLConnections.GetMySQLConnectionString();
            using var connection = new MySqlConnection(connectionString);
            string insert_query =
                @"insert into logs (exception_message, exception_text, application_name) values (@exception_message, @exception_text, @application_name)";

            var results = await Dapper.SqlMapper
                .QueryAsync(connection, insert_query,
                    new
                    {
                        application_name,
                        exception_text = message,
                        exception_message = "INFO"
                    });
            
            int affected = results.ToList().Count;

            Console.WriteLine($"logged {affected} log records.");
            return affected;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public static async Task<int> LogException(Exception exception)
    {
        Console.WriteLine("logging exception :>> " + exception.Message);

        try
        {
            var connectionString = SQLConnections.GetMySQLConnectionString();
            using var connection = new MySqlConnection(connectionString);
            string insert_query =
                @"insert into logs (exception_message, exception_text, application_name) values (@exception_message, @exception_text, @application_name)";

            var results = await Dapper.SqlMapper
                .QueryAsync(connection, insert_query,
                    new
                    {
                        application_name = nameof(CodeMechanic.Systemd.Daemons),
                        exception_text = exception.ToString(),
                        exception_message = exception.Message
                    });
            int affected = results.ToList().Count;

            Console.WriteLine($"logged {affected} log records.");

            return affected;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }
}

public static class SQLConnections
{
    public static string GetMySQLConnectionString()
    {
        var connectionString = new MySqlConnectionStringBuilder()
        {
            Database = Environment.GetEnvironmentVariable("MYSQLDATABASE"),
            Server = Environment.GetEnvironmentVariable("MYSQLHOST"),
            Password = Environment.GetEnvironmentVariable("MYSQLPASSWORD"),
            UserID = Environment.GetEnvironmentVariable("MYSQLUSER"),
            Port = (uint)Environment.GetEnvironmentVariable("MYSQLPORT").ToInt()
        }.ToString();

        if (connectionString.IsEmpty()) throw new ArgumentNullException(nameof(connectionString));
        return connectionString;
    }
}