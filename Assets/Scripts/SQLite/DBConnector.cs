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
            Debug.Log("DB Startup Completed. ");
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
        Debug.Log("ConnectToDatabase :" + dbPath);
        this.connection = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);
       
    }

    private void CreateTables()
    {
        this.connection.CreateTable<User>();
        this.connection.CreateTable<Pathpoint>();
        this.connection.CreateTable<AuthToken>();
        this.connection.CreateTable<ExploratoryRouteWalk>();
        this.connection.CreateTable<Way>();
        this.connection.CreateTable<Address>();
    }

    public void TruncateTables()
    {
        this.connection.DeleteAll<User>();
        this.connection.DeleteAll<AuthToken>();
        this.connection.DeleteAll<Pathpoint>();
        this.connection.DeleteAll<ExploratoryRouteWalk>();
        this.connection.DeleteAll<Way>();
        this.connection.DeleteAll<Address>();
    }

    public void DropTables()
    {
        try
        {
            this.connection.DropTable<User>();
        }
        catch { }
        try
        {
            this.connection.DropTable<AuthToken>();
    }
        catch { }
        try
        {
            this.connection.DropTable<Pathpoint>();
        }
        catch { }
        try
        {
            this.connection.DropTable<ExploratoryRouteWalk>();
        }
        catch { }
        try
        {
            this.connection.DropTable<Way>();
        }
        catch { }
        try
        {
            this.connection.DropTable<Address>();
        }
        catch { }
        }
    #endregion
}
