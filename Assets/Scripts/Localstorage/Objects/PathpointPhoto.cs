using SQLite4Unity3d;
using System.Linq;
using System.Collections.Generic;
using System;
using static PaganiniRestAPI;

public class PathpointPhoto : BaseModel<PathpointPhoto>
{
    [PrimaryKey]
    public int Id { set; get; }
    public int PathpointId { set; get; }
    public string Description { set; get; }
    public PhotoFeedback CleaningFeedback { set; get; }
    public PhotoFeedback DiscussionFeedback { set; get; }
    public long? Timestamp { set; get; }

    //public byte[] Photo { set; get; }
    public int? PhotoId { set; get; }

    [Ignore]
    public PhotoData Data { get; set; }

    public enum PhotoFeedback
    {
        None = 0,
        Delete = 1,
        Keep = 2,
    }

    public PathpointPhoto() { }


    public PathpointPhoto(IPathpointPhotoAPI photoAPI)
    {
        Id = photoAPI.pphoto_id;
        PathpointId = photoAPI.ppoint_id;
        Description = photoAPI.pphoto_description;
        //Photo = Convert.FromBase64String(photoAPI.photo);

        CleaningFeedback = ((PhotoFeedback?)photoAPI.pphoto_cleaning_feedback) ?? PhotoFeedback.None;
        DiscussionFeedback = ((PhotoFeedback?)photoAPI.pphoto_discussion_feedback) ?? PhotoFeedback.None;

        PhotoId = photoAPI.photo_id;

        Timestamp = DateUtils.ConvertUTCStringToTsMilliseconds(photoAPI.pphoto_timestamp, "yyyy-MM-dd'T'HH:mm:ss");

        FromAPI = true;
    }


    public static List<PathpointPhoto> GetPathpointPhotoListByPOI(int pathpointId)
    {
        List<PathpointPhoto> photos;

        var conn = DBConnector.Instance.GetConnection();

        // Query all Ways and their related Routes using sqlite-net's built-in mapping functionality

        photos = conn.Table<PathpointPhoto>().Where(p => p.PathpointId == pathpointId)
            .OrderBy(item => item.Timestamp)
            .ThenByDescending(item => item.Id)
            .ToList();

        // get PhotoData

        foreach (var photo in photos)
        {
            photo.Data = PhotoData.Get(photo.PhotoId);
        }


        return photos;
    }

    public static PathpointPhoto GetDefaultPhoto(List<PathpointPhoto> photos)
    {
        if (photos == null) return null;

        foreach (var photo in photos)
        {
            if (photo.CleaningFeedback != PhotoFeedback.Delete && photo.DiscussionFeedback != PhotoFeedback.Delete)
            {
                return photo;
            }
        }

        // We use one of the discarded ones
        if (photos.Count > 0)
        {
            return photos[0];
        }

        return null;
    }

    public IPathpointPhotoAPI ToAPI()
    {

        IPathpointPhotoAPI photo;
        // For an update statement
        if (FromAPI)
        {
            photo = new PathpointPhotoAPIUpdate();
            photo.pphoto_id = Id;
            photo.IsNew = false;
        }
        // For a post statement
        else
        {
            photo = new PathpointPhotoAPI();
            photo.IsNew = true;
        }


        photo.pphoto_description = Description;
        //photo.photo = Convert.ToBase64String(Photo);

        photo.pphoto_timestamp = DateUtils.ConvertMillisecondsToUTCString(Timestamp);

        photo.pphoto_cleaning_feedback = (int)CleaningFeedback;
        photo.pphoto_discussion_feedback = (int)DiscussionFeedback;

        return photo;
    }

    public IPathpointPhotoAPI ToAPIBatchElement()
    {

        IPathpointPhotoAPI photo;
        // For an update statement
        if (FromAPI)
        {
            photo = new PathpointPhotoAPIBatchUpdateElement();
            photo.pphoto_id = Id;
            photo.IsNew = false;
        }
        // For a post statement
        else
        {
            photo = new PathpointPhotoAPIBatchCreateElement();
            photo.IsNew = true;
        }

        photo.ppoint_id = PathpointId;
        photo.pphoto_description = Description;
        //photo.photo = Convert.ToBase64String(Photo);

        photo.pphoto_timestamp = DateUtils.ConvertMillisecondsToUTCString(Timestamp);

        photo.pphoto_cleaning_feedback = (int)CleaningFeedback;
        photo.pphoto_discussion_feedback = (int)DiscussionFeedback;

        return photo;
    }


    /// <summary>
    /// Returns a list of PathpointPhotos for a given route, filtered by an optional predicate.
    /// </summary>
    /// <param name="routeId">The ID of the route to filter by.</param>
    /// <param name="pathpointFromAPI">Indicates whether to consider only photos of pathtpoint from the API.</param>
    /// <param name="wherePhoto">An optional predicate to further filter the photos.</param>
    /// <returns>A list of PathpointPhotos.</returns>
    public static List<PathpointPhoto> GetListByRoute(int routeId, bool pathpointFromAPI, Func<PathpointPhoto, bool> wherePhoto = null)
    {
        List<PathpointPhoto> photos;

        var conn = DBConnector.Instance.GetConnection();

        // Query all PathpointPhotos and their related Pathpoints using sqlite-net's raw query
        string cmdText = @"SELECT pp.* FROM PathpointPhoto pp
                       JOIN Pathpoint p ON pp.PathpointId = p.Id
                       WHERE p.RouteId = ? and p.FromAPI = ?";
        photos = conn.Query<PathpointPhoto>(cmdText, routeId, pathpointFromAPI);

        // Apply the additional filter, if provided
        if (wherePhoto != null)
        {
            photos = photos.Where(wherePhoto).ToList();
        }

        // Order the resulting photos by PathpointId
        photos = photos.OrderBy(p => p.PathpointId).ToList();

        return photos;
    }


    /// <summary>
    /// Returns a list of PathpointPhotos for a given route, filtered by an optional predicate.
    /// </summary>
    /// <param name="routeId">The ID of the route to filter by.</param>
    /// <param name="wherePhoto">An optional predicate to further filter the photos.</param>
    /// <returns>A list of PathpointPhotos.</returns>
    public static List<PathpointPhoto> GetListByRoute(int routeId, Func<PathpointPhoto, bool> wherePhoto = null)
    {
        List<PathpointPhoto> photos;

        var conn = DBConnector.Instance.GetConnection();

        // Query all PathpointPhotos and their related Pathpoints using sqlite-net's raw query
        string cmdText = @"SELECT pp.* FROM PathpointPhoto pp
                       JOIN Pathpoint p ON pp.PathpointId = p.Id
                       WHERE p.RouteId = ?";
        photos = conn.Query<PathpointPhoto>(cmdText, routeId);

        // Apply the additional filter, if provided
        if (wherePhoto != null)
        {
            photos = photos.Where(wherePhoto).ToList();
        }

        // Order the resulting photos by PathpointId
        photos = photos.OrderBy(p => p.PathpointId).ToList();

        return photos;
    }

    public static void DeleteFromPOIs(int routeId, bool pathpointFromAPI, bool pathphotoFromAPI)
    {
        //Pathpoint.DeleteFromRoute(CurrentRoute.Id, new bool[] { false }, new Pathpoint.POIsType[] { Pathpoint.POIsType.Landmark, Pathpoint.POIsType.Reassurance });

        //// Open the SQLliteConnection
        var conn = DBConnector.Instance.GetConnection();

        // Prepare the base DELETE command text
        string cmdText = "DELETE FROM PathpointPhoto " +
                         "WHERE PathpointId IN (" +
                         "    SELECT p.Id " +
                         "    FROM Pathpoint p " +
                         "    WHERE p.FromAPI = ? AND p.RouteId = ? " +
                         ") AND FromAPI = ? AND IsDirty = ? ";


        List<object> parameters = new List<object> { pathpointFromAPI, routeId, pathphotoFromAPI, true };

        //// Prepare the SQLiteCommand with the command text and parameters
        SQLiteCommand cmd = conn.CreateCommand(cmdText, parameters.ToArray());

        // Execute the command
        cmd.ExecuteNonQuery();

    }

    public static void DeleteFromPOIs(int routeId)
    {
        //Pathpoint.DeleteFromRoute(CurrentRoute.Id, new bool[] { false }, new Pathpoint.POIsType[] { Pathpoint.POIsType.Landmark, Pathpoint.POIsType.Reassurance });

        //// Open the SQLliteConnection
        var conn = DBConnector.Instance.GetConnection();

        // Prepare the base DELETE command text
        string cmdText = "DELETE FROM PathpointPhoto " +
                         "WHERE PathpointId IN (" +
                         "    SELECT p.Id " +
                         "    FROM Pathpoint p " +
                         "    WHERE p.RouteId = ? " +
                         ") ";


        List<object> parameters = new List<object> { routeId };

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
        string cmdText = "DELETE FROM PathpointPhoto " +
                         "WHERE PathpointId = ? ";

        List<object> parameters = new List<object> { poiId };

        //// Prepare the SQLiteCommand with the command text and parameters
        SQLiteCommand cmd = conn.CreateCommand(cmdText, parameters.ToArray());

        // Execute the command
        cmd.ExecuteNonQuery();

    }


}
