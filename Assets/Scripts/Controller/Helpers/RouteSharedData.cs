using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine;

public class RouteSharedData : PersistentLazySingleton<RouteSharedData>
{
    public Route CurrentRoute;
    public Way CurrentWay;
    public List<Pathpoint> PathpointList;
    public List<Pathpoint> POIList;

    public event EventHandler OnDataDownloaded;


    public RouteSharedData()
    {
    }


    public Pathpoint PreparePOIData(int i)
    {
        var item = POIList[i];
        if (item.Photos == null)
        {
            item.Photos = PathpointPhoto.GetPathpointPhotoListByPOI(item.Id);
        }

        return item;
    }


    public void DownloadRouteDefinition()
    {
        Debug.Log("Downloading Route Definition.");

        CurrentRoute = SessionData.Instance.GetData<Route>("SelectedRoute");

        if (CurrentRoute == null)
        {
            SceneManager.LoadScene(SceneSwitcher.UserLoginScene);
        }

        CurrentWay = Way.Get(CurrentRoute.WayId);

        DownloadPathpoints();
    }

    public void LoadRouteFromDatabase()
    {
        PathpointList = Pathpoint.GetPathpointListByRoute(CurrentRoute.Id)
                                    .OrderBy(p => p.Timestamp)
                                    .ToList();
        POIList = PathpointList.Where(item => item.POIType != Pathpoint.POIsType.Point)
                                    .OrderBy(p => p.Timestamp)
                                    .ToList();
    }


    /**********************
    *  Data processing  *
    **********************/


    private void DownloadPathpoints()
    {
        Debug.Log("Downloading Pathpoints.");
        PaganiniRestAPI.Pathpoint.GetAll(CurrentRoute.Id, GetPathpointsSucceeded, LoadFailed);
    }


    private void DownloadPathpointPhotos()
    {
        Debug.Log("Downloading Pathpoint photos.");
        PaganiniRestAPI.PathpointPOI.GetAll(CurrentRoute.Id, GetPathpointPOIsSucceeded, LoadFailed);
    }

    private void DownloadPhotoData()
    {
        Debug.Log("Downloading Photo Data.");

        var lastUpdate = PhotoData.GetLastUpdateByRoute(CurrentRoute.Id);

        Dictionary<string, string> query = new Dictionary<string, string> { };
        if (lastUpdate != null)
        {
            var sinceDate = DateUtils.ConvertMillisecondsToUTCString(lastUpdate, "yyyy-MM-dd'T'HH:mm:ss");
            query = new Dictionary<string, string>
            {
                { "sinceDate", sinceDate }
            };
        }

        PaganiniRestAPI.PhotoData.GetAll(CurrentRoute.Id, query, GetPhotoDataSucceeded, LoadFailed);
    }

    /**********************
     * Event handlers     *
     **********************/

    private void GetPathpointsSucceeded(PathpointAPIList res)
    {
        // Delete local cache
        Pathpoint.DeleteNonDirtyCopies();

        // Insert clean verion
        foreach (var point in res.pathpoints)
        {
            // Insert pathpoint only if it's not already here
            if (!CurrentRoute.FromAPI || !Pathpoint.CheckIfExists(p => p.IsDirty && p.Id == point.ppoint_id))
            {
                Pathpoint p = new Pathpoint(point);
                p.Insert();
            }

        }

        DownloadPathpointPhotos();
    }

    private void GetPathpointPOIsSucceeded(PathpointPOIAPIList res)
    {
        Debug.Log("Processing Photos.");
        PathpointPhoto.DeleteNonDirtyCopies();

        if (res != null && res.pois != null)
        {
            // Insert clean verion
            foreach (var poi in res.pois)
            {
                foreach (var photo in poi.photos)
                {
                    // Insert pathpoint only if it's not already here
                    if (!CurrentRoute.FromAPI || !PathpointPhoto.CheckIfExists(p => p.IsDirty && p.Id == photo.pphoto_id))
                    {
                        PathpointPhoto p = new PathpointPhoto(photo);
                        p.Insert();
                    }
                }

            }
        }
                
        DownloadPhotoData();
    }

    private void GetPhotoDataSucceeded(PhotoDataAPIList res)
    {
        int nUpdated = 0;
        if (res != null && res.data != null)
        {
            foreach (var photo in res.data)
            {
                if (!CurrentRoute.FromAPI || !PhotoData.CheckIfExists(p => p.IsDirty && p.Id == photo.photo_id))
                {
                    PhotoData data = new PhotoData(photo);
                    data.Insert();
                    nUpdated++;
                }
            }
        }

        Debug.Log($"Done Downloading Route Definition. (Updated = {nUpdated} photos)");
        // Load editor
        OnDataDownloaded?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// This function is called when the Load methods fail.
    /// </summary>
    /// <param name="errorMessage">The error message returned by the API.</param>
    private void LoadFailed(string errorMessage)
    {
        Debug.Log(errorMessage);
    }


}

