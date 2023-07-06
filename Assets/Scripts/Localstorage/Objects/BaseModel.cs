using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using SQLite4Unity3d;


/// <summary>
/// Represents the base class for all models that require basic SQLite operations.
/// </summary>
/// <typeparam name="T">The type of the derived model class.</typeparam>
public class BaseModel<T> where T : BaseModel<T>, new()
{
    /// <summary>
    /// Gets or sets a value indicating whether the model instance is dirty (modified).
    /// </summary>
    public bool IsDirty { set; get; } = false;

    /// <summary>
    /// Gets or sets a value indicating whether the model instance is from the API.
    /// </summary>
    public bool FromAPI { set; get; } = false;

    /// <summary>
    /// Marks the model instance as clean (not modified).
    /// </summary>
    public void MarkAsClean()
    {
        IsDirty = false;
    }

    /// <summary>
    /// Inserts or replaces the current model instance in the database.
    /// </summary>
    public void Insert()
    {
        var conn = DBConnector.Instance.GetConnection();
        conn.InsertOrReplace(this);
    }

    /// <summary>
    /// Inserts all model instances in the provided list into the database.
    /// </summary>
    /// <param name="list">The list of model instances to insert.</param>
    /// <returns>The number of inserted rows.</returns>
    public static int InsertAll(List<T> list)
    {
        var conn = DBConnector.Instance.GetConnection();
        return conn.InsertAll(list.AsEnumerable());
    }

    /// <summary>
    /// Retrieves all model instances from the database, optionally filtered by the provided predicate.
    /// </summary>
    /// <param name="predicate">An optional filter expression.</param>
    /// <returns>A list of model instances.</returns>
    public static List<T> GetAll(Expression<Func<T, bool>> predicate = null)
    {
        var conn = DBConnector.Instance.GetConnection();
        var query = conn.Table<T>();

        if (predicate != null)
        {
            query = query.Where(predicate);
        }

        return query.ToList();
    }

    /// <summary>
    /// Checks if any model instances exist in the database that satisfy the provided predicate.
    /// </summary>
    /// <param name="predicate">A filter expression.</param>
    /// <returns>True a instances exist that satisfy the predicate, otherwise false.</returns>
    public static bool CheckIfExists(Expression<Func<T, bool>> predicate)
    {
        List<T> list = GetAll(predicate);
        return (list != null && list.Count > 0);
    }

    /// <summary>
    /// Retrieves a model instance from the database by its primary key.
    /// </summary>
    /// <param name="pk">The primary key value.</param>
    /// <returns>The model instance, if found; otherwise, null.</returns>
    public static T Get(object pk)
    {
        var conn = DBConnector.Instance.GetConnection();
        return conn.Get<T>(pk);
    }

    /// <summary>
    /// Deletes a model instance from the database by its primary key.
    /// </summary>
    /// <param name="objId">The primary key value of the model instance to delete.</param>
    public static void Delete(object objId)
    {
        var conn = DBConnector.Instance.GetConnection();
        conn.Delete<T>(objId);
    }

    /// <summary>
    /// Deletes all model instances from the database, optionally filtered by the provided predicate.
    /// </summary>
    public static void DeleteAll()
    {
        var conn = DBConnector.Instance.GetConnection();
        conn.DeleteAll<T>();
    }

    /// <summary>
    /// Deletes all non-dirty (unmodified) model instances from the database.
    /// </summary>
    public static void DeleteNonDirtyCopies()
    {
        var conn = DBConnector.Instance.GetConnection();
        string query = String.Format("DELETE from {0} where IsDirty = 0",
                                      conn.Table<T>().Table.TableName);
        conn.Execute(query);
    }

    /// <summary>
    /// Gets the model instance with the minimum Id.
    /// </summary>
    /// <param name="idSelector">A function to select the Id property from a model instance.</param>
    /// <returns>The model instance with the minimum Id, if found; otherwise, null.</returns>
    public static T GetWithMinId(Func<T, int> idSelector)
    {
        var conn = DBConnector.Instance.GetConnection();
        var query = conn.Table<T>();

        // The query will be null if the table is empty
        if (query.Count() == 0)
            return null;

        return query.OrderBy(idSelector).FirstOrDefault();
    }


}
