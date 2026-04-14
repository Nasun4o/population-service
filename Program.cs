using Backend;
using System;
using System.Data.Common;

Console.WriteLine("Started");
Console.WriteLine("Getting DB Connection...");

IDbManager db = new SqliteDbManager();
DbConnection conn = db.GetConnection();

if (conn == null)
{
    Console.WriteLine("Failed to get connection");
}
else
{
    // Temporary: inspect schema
    using var cmd = conn.CreateCommand();
    cmd.CommandText = "SELECT sql FROM sqlite_master WHERE type='table';";
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        Console.WriteLine(reader.GetString(0));
    }
    conn.Close();
}