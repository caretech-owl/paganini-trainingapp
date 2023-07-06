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
        PaganiniRestAPI.Pathpoint.GetAll(CurrentRoute.Id, GetPathpointsSucceeded, LoadFailed);
    }

    private void DownloadPhotos()
    {
        PaganiniRestAPI.PathpointPOI.GetAll(CurrentRoute.Id, GetPathpointPOIsSucceeded, LoadFailed);
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

        DownloadPhotos();
    }

    private void GetPathpointPOIsSucceeded(PathpointPOIAPIList res)
    {
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

