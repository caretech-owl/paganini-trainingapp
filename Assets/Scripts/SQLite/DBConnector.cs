using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using SQLite4Unity3d;
public class DBConnector : PersistentLazySingleton<DBConnector>
{
    private SQLiteConnection connection;
    public string databaseName = "wegetraining.db";


    #region [Databaseoperations]

    public void Startup()
    {
        if (this.connection == null)
        {
            string dbPath = Application.persistentDataPath + "/" + databaseName;
            ConnectToDatabase(dbPath);
            CreateTables();
        }
    }

    public SQLiteConnection GetConnection()
    {
        return this.connection;
    }

    public void TruncateTable<t>()
    {
        this.connection.DeleteAll<t>();
    }

    private void ConnectToDatabase(string dbPath)
    {
        this.connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
    }

    private void CreateTables()
    {
        this.connection.CreateTable<UserProfile>();
        this.connection.CreateTable<Wegpunkt>();
        this.connection.CreateTable<AuthToken>();
        this.connection.CreateTable<Begehung>();

    }

    private void TruncateTables()
    {
        this.connection.DeleteAll<UserProfile>();
        this.connection.DeleteAll<AuthToken>();
        this.connection.DeleteAll<Wegpunkt>();
        this.connection.CreateTable<Begehung>();
    }

    private void DropTables()
    {
        this.connection.DeleteAll<UserProfile>();
        this.connection.DeleteAll<AuthToken>();
        this.connection.CreateTable<Wegpunkt>();
        this.connection.CreateTable<Begehung>();
    }
    #endregion
}
