using MySqlConnector;

public static class DbConnection
{

    private static MySqlConnection connectionConfig;

    static DbConnection()
    {
        var builder = new MySqlConnectionStringBuilder
        {
            Server = "127.0.0.1",
            UserID = "AdminUser",
            Password = "Passwort5678",
            Database = "mmo_project_reaper",
        };

        connectionConfig = new MySqlConnection(builder.ConnectionString);
    }

    public static MySqlConnection GetConfiguredConnection()
    {
        return connectionConfig.Clone();
    }

}
