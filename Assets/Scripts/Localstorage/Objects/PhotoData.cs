using SQLite4Unity3d;
using System.Linq;
using System.Collections.Generic;
using System;
using System.Globalization;
using UnityEngine;

public class PhotoData : BaseModel<PhotoData>
{
    [PrimaryKey]
    public int Id { set; get; }
    public long? LastUpdate { set; get; }
    public byte[] Photo { set; get; }


    public PhotoData() { }


    public PhotoData(IPhotoDataAPI photoAPI)
    {
        Id = photoAPI.photo_id;
        if (photoAPI.photo != null && photoAPI.photo.Trim() != "")
            Photo = Convert.FromBase64String(photoAPI.photo);
        LastUpdate = DateUtils.ConvertUTCStringToTsMilliseconds(photoAPI.last_update, "yyyy-MM-dd'T'HH:mm:ss");

        FromAPI = true;
    }


    public IPhotoDataAPI ToAPI()
    {

        IPhotoDataAPI photo;
        // For an update statement
        if (FromAPI)
        {
            photo = new PhotoDataAPIUpdate();
            photo.photo_id = Id;
            photo.IsNew = false;
        }
        // For a post statement
        else
        {
            photo = new PhotoDataAPI();
            photo.IsNew = true;
        }

        return photo;
    }


    public static long? GetLastUpdateByRoute(int routeId)
    {

        var conn = DBConnector.Instance.GetConnection();

        // Query all PathpointPhotos and their related Pathpoints using sqlite-net's raw query
        //string cmdText = @"SELECT max(pd.LastUpdate) maxlastUpdate FROM PhotoData pd
        //                    JOIN PathpointPhoto pp ON pd.Id = pp.PhotoId
        //                    JOIN Pathpoint p ON pp.PathpointId = p.Id
        //                    WHERE p.RouteId = ?";

        string cmdText = @"SELECT pd.*
            FROM PhotoData pd
    JOIN PathpointPhoto pp ON pd.Id = pp.PhotoId
    JOIN Pathpoint p ON pp.PathpointId = p.Id
    WHERE p.RouteId = ?
    ORDER BY pd.LastUpdate DESC
    LIMIT 1";

        List<PhotoData> maxLastUpdate = conn.Query<PhotoData>(cmdText, routeId).ToList();

        if (maxLastUpdate.Count > 0)
        {
            return maxLastUpdate[0].LastUpdate;
        }

        return null;
    }

    /// <summary>
    /// Returns a list of PhotoData for a given route, filtered by an optional predicate.
    /// </summary>
    /// <param name="routeId">The ID of the route to filter by.</param>
    /// <param name="photosFromAPI">Indicates whether to consider only photos from the API.</param>
    /// <param name="wherePhoto">An optional predicate to further filter the photos.</param>
    /// <returns>A list of PhotoData.</returns>
    public static List<PhotoData> GetListByRoute(int routeId, bool pathphotosFromAPI, Func<PhotoData, bool> wherePhoto = null)
    {
        List<PhotoData> photos;

        var conn = DBConnector.Instance.GetConnection();

        // Query all PathpointPhotos and their related Pathpoints using sqlite-net's raw query
        string cmdText = @"SELECT pd.* FROM PhotoData pd
                            JOIN PathpointPhoto pp ON pd.Id = pp.PhotoId
                            JOIN Pathpoint p ON pp.PathpointId = p.Id
                            WHERE p.RouteId = ? AND pp.FromAPI = ?";
        photos = conn.Query<PhotoData>(cmdText, routeId, pathphotosFromAPI);

        // Apply the additional filter, if provided
        if (wherePhoto != null)
        {
            photos = photos.Where(wherePhoto).ToList();
        }

        // Order the resulting photos by PathpointId
        photos = photos.OrderBy(p => p.LastUpdate).ToList();

        return photos;
    }

    /// <summary>
    /// Returns a list of PhotoData for a given route, filtered by an optional predicate.
    /// </summary>
    /// <param name="routeId">The ID of the route to filter by.</param>
    /// <param name="wherePhoto">An optional predicate to further filter the photos.</param>
    /// <returns>A list of PhotoData.</returns>
    public static List<PhotoData> GetListByRoute(int routeId, Func<PhotoData, bool> wherePhoto = null)
    {
        List<PhotoData> photos;

        var conn = DBConnector.Instance.GetConnection();

        // Query all PathpointPhotos and their related Pathpoints using sqlite-net's raw query
        string cmdText = @"SELECT pd.* FROM PhotoData pd
                            JOIN PathpointPhoto pp ON pd.Id = pp.PhotoId
                            JOIN Pathpoint p ON pp.PathpointId = p.Id
                            WHERE p.RouteId = ?";
        photos = conn.Query<PhotoData>(cmdText, routeId);

        // Apply the additional filter, if provided
        if (wherePhoto != null)
        {
            photos = photos.Where(wherePhoto).ToList();
        }

        // Order the resulting photos by PathpointId
        photos = photos.OrderBy(p => p.LastUpdate).ToList();

        return photos;
    }


    public static void DeleteFromRoute(int routeId)
    {
        //Pathpoint.DeleteFromRoute(CurrentRoute.Id, new bool[] { false }, new Pathpoint.POIsType[] { Pathpoint.POIsType.Landmark, Pathpoint.POIsType.Reassurance });

        //// Open the SQLliteConnection
        var conn = DBConnector.Instance.GetConnection();

        // Prepare the base DELETE command text
        string cmdText = @"DELETE FROM PhotoData 
                         WHERE Id IN (
                             SELECT pp.PhotoId 
                             FROM PathpointPhoto pp
                                JOIN Pathpoint p
                                    ON pp.PathpointId = p.id
                             WHERE p.RouteId = ? 
                         ) ";


        List<object> parameters = new List<object> { routeId };

        //// Prepare the SQLiteCommand with the command text and parameters
        SQLiteCommand cmd = conn.CreateCommand(cmdText, parameters.ToArray());

        // Execute the command
        cmd.ExecuteNonQuery();

    }

    public static void DeleteFromRoute(int routeId, bool pathpointFromAPI, bool pathphotoFromAPI)
    {

        //// Open the SQLliteConnection
        var conn = DBConnector.Instance.GetConnection();

        // Prepare the base DELETE command text
        string cmdText = @"DELETE FROM PhotoData 
                         WHERE Id IN (
                             SELECT pp.PhotoId 
                             FROM PathpointPhoto pp
                                JOIN Pathpoint p
                                    ON pp.PathpointId = p.id
                             WHERE p.RouteId = ? AND p.FromAPI = ? AND pp.FromAPI = ?
                         ) ";


        List<object> parameters = new List<object> { routeId, pathpointFromAPI, pathphotoFromAPI };

        //// Prepare the SQLiteCommand with the command text and parameters
        SQLiteCommand cmd = conn.CreateCommand(cmdText, parameters.ToArray());

        // Execute the command
        cmd.ExecuteNonQuery();

    }

    public static void DeleteByPOI(int poiId)
    {

        //// Open the SQLliteConnection
        var conn = DBConnector.Instance.GetConnection();

        // Prepare the base DELETE command text
        string cmdText = @"DELETE FROM PhotoData 
                         WHERE Id IN (
                             SELECT pp.PhotoId 
                             FROM PathpointPhoto pp
                             WHERE pp.PathpointId = ?
                         ) ";


        List<object> parameters = new List<object> { poiId };

        //// Prepare the SQLiteCommand with the command text and parameters
        SQLiteCommand cmd = conn.CreateCommand(cmdText, parameters.ToArray());

        // Execute the command
        cmd.ExecuteNonQuery();

    }

}
