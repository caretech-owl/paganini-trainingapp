using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaysDataHandler : MonoBehaviour
{
    /// <summary>
    /// Game Object to Display Wege in
    /// </summary>

    //TODO: get from parent
    public LoginManager LoginHandler;

    [Header(@"List Configuration")]
    public GameObject WayListView;
    private WayList WayListHandler;


    private List<Way> ways;
    private Way selectedWay;

    [Header(@"Creation form Configuration")]
    public TMPro.TMP_InputField WegName;
    public TMPro.TMP_InputField WegStart;
    public TMPro.TMP_InputField WegZiel;

    public GameObject StartIconsToggleGroup;
    public GameObject DestinationIconsToggleGroup;

    [Header(@"Start UI Configuration")]
    public TMPro.TMP_Text StartPanelWegStart;
    public TMPro.TMP_Text StartPanelWegZiel;

    public GameObject StartPanelIconStart;
    public GameObject StartPanelIconZiel;

    public GameObject StartPanelOverwrite;
    public GameObject StartPanelNew;

    private bool StatusOverwriteERW;


    // Start is called before the first frame update
    void Start()
    {
        //LoginHandler = gameObject.transform.GetComponentInParent<LoginManager>();

        WayListHandler = WayListView.GetComponent<WayList>();


        Debug.Log(Application.persistentDataPath);
        AppState.SelectedWeg = -1;


        // We have a user session
        //if (AppState.CurrentUser != null)
        //{
        //    DBConnector.Instance.Startup();
        //    LoadWaysFromAPI();
        //}

    }

    public void CancelNewERW()
    {
        // If we are cancelling the overwriting, let's not delete the existing ERW.
        if (StatusOverwriteERW) return;

        //DBConnector.Instance.GetConnection().Execute("Delete from Way where Status =" + ((int)Way.WayStatus.Local) + " and Id =" + selectedWay.Id);

        // The ERW creation was cancelled, so we delete the Way if it was new
        var way = Way.Get(selectedWay.Id);
        if (way.FromAPI)
        {
            Way.Delete(selectedWay.Id);
        }


        AppState.currentBegehung = null;
        AppState.SelectedBegehung = -1;
        AppState.SelectedWeg = -1;
        selectedWay = null;

        LoadWaysFromLocalStorage();

    }


    public void LoadWaysFromAPI()
    {
        PaganiniRestAPI.Way.GetAll(GetWaysSucceed, GetWaysFailed);
    }


    /// <summary>
    /// Reads back in the Wege List from SQLite and Saves it in the Object
    /// </summary>
    void LoadWaysFromLocalStorage()
    {
        List<Way> ways = Way.GetAll(); //DBConnector.Instance.GetConnection().Query<Way>("Select * FROM Way");
        Debug.Log("Restorewege -> Capacity: " + ways.Count);

        if (ways.Count > 0)
            this.ways = ways;
        else
            this.ways = null;

        //List<Way> wege = DBConnector.Instance.GetConnection().Query<Way>("Select * FROM Way");
        //Debug.Log("Restorewege -> Capacity: " + wege.Count);

        //if (wege.Count > 0)
        //    this.ways = wege;
        //else
        //    this.ways = null;

        DisplayWege();
    }

    /// <summary>
    /// Deletes ways Data
    /// </summary>
    public void DeleteLocalData()
    {
        // We delete everything except what hasn't been synchronised yet
        //DBConnector.Instance.GetConnection().DeleteAll<Way>();

        //DBConnector.Instance.GetConnection().Execute("Delete from Way where Status =" + ((int)Way.WayStatus.FromAPI));
        Way.DeleteNonDirtyCopies();

        
        LoadWaysFromLocalStorage();
    }

    /// <summary>
    /// Saves ways to SQLite
    /// </summary>
    void SaveWaysToLocalStorage()
    {
        if (ways != null)
        {
            foreach (Way w in ways)
            {
                //DBConnector.Instance.GetConnection().InsertOrReplace(w);
                w.Insert();
            }

        }

    }


    /// <summary>
    /// Create new way
    /// </summary>
    public void AddWay()
    {

        LandmarkToggleList startIcon = StartIconsToggleGroup.GetComponent<LandmarkToggleList>();
        LandmarkToggleList destinationIcon = DestinationIconsToggleGroup.GetComponent<LandmarkToggleList>();

        var w = new Way
        {
            Name = WegName.text,
            Start = WegStart.text,
            StartType = ((int) startIcon.SelectedLandMarkType).ToString(),
            Destination = WegZiel.text,
            DestinationType = ((int)destinationIcon.SelectedLandMarkType).ToString(),
            Description = WegStart.text + "->" + WegZiel.text,
            FromAPI = false,
            IsDirty = true
        };

        if (ways == null) {
            ways = new List<Way>();
        }

        // safely create an ID, as the seconds since 2010        
        TimeSpan t = (DateTime.UtcNow - new DateTime(2010, 1, 1));
        w.Id = - (int)t.TotalSeconds;

        ways.Add(w);
        SaveWaysToLocalStorage();
        LoadWaysFromLocalStorage();

        AppState.SelectedWeg = w.Id;
        AppState.currentBegehung = w.Name;

        SetSelectedWay(w);        

    }
    
    public void SetSelectedWay(Way w)
    {
        SessionData.Instance.SaveData("SelectedWay", w);
        selectedWay = w;
    }


    /// <summary>
    /// Set up start panel with given information
    /// </summary>
    public void SetUpStartPanel()
    {
        
        // set text 
        StartPanelWegStart.text = selectedWay.Start;
        StartPanelWegZiel.text = selectedWay.Destination;

        StartPanelIconStart.GetComponent<LandmarkIcon>().SetSelectedLandmark(Int32.Parse(selectedWay.StartType));
        StartPanelIconZiel.GetComponent<LandmarkIcon>().SetSelectedLandmark(Int32.Parse(selectedWay.DestinationType));

        
        CheckIfLocalERW();

    }

    /// <summary>
    /// Request was successful
    /// </summary>
    /// <param name="ways">List of WegAPI objects</param>
    private void GetWaysSucceed(WayAPIList list)
    {
        // Delete local cache
        DeleteLocalData();

        this.ways = new List<Way>();
        foreach (var w in list.ways)
        {
            this.ways.Add(new Way(w));
            Debug.Log(new Way(w));
        }

        // Save API results, and then load it for use
        SaveWaysToLocalStorage();
        LoadWaysFromLocalStorage();
    }

    /// <summary>
    /// There were some problems with request.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    private void GetWaysFailed(string errorMessage)
    {
        Debug.LogError(errorMessage);


        Assets.ErrorHandlerSingleton.GetErrorHandler().AddNewError("Error Loading Ways", errorMessage);


        // TODO: Implement scene switching with "messages" displayed on the login (e.g., pin no valid)

        if (errorMessage == "Unauthorised")
        {
            LoginHandler.Logout();
            LoginHandler.RecheckLogin();
        }

        // Load it from the database if there are problems (e.g., connection problems)
        LoadWaysFromLocalStorage();

    }

    /// <summary>
    /// Prints the Begehungen to the GUI
    /// </summary>

    private void DisplayWege()
    {

        if (this.ways == null || this.ways.Capacity == 0)
        {
            WayListHandler.ShowBlankState();        
        }
        else
        { 
            WayListHandler.ClearList();

            foreach (Way w in ways)
            {
                WayListHandler.AddWayItem(w);
            }

            WayListHandler.ShowWayList();
        }
    }


    public void CreateNewERW()
    {
        Route.Delete(selectedWay.Id);
        Pathpoint.DeleteFromRoute(selectedWay.Id, null, null);

        //DBConnector.Instance.GetConnection().Execute("DELETE FROM ExploratoryRouteWalk where Id=" + selectedWay.Id);
        //DBConnector.Instance.GetConnection().Execute("DELETE FROM Pathpoint where RouteId=" + selectedWay.Id);


        Route route = new Route();
        route.Id = selectedWay.Id;
        route.WayId = selectedWay.Id;
        route.Date = DateTime.Now.ToUniversalTime();
        route.Name = selectedWay.Name;
        route.FromAPI = false;
        route.IsDirty = true;
       

        // We set the current exploratory walk
        AppState.currentBegehung = route.Name;
        AppState.SelectedBegehung = selectedWay.Id;
        AppState.SelectedWeg = selectedWay.Id;

        //DBConnector.Instance.GetConnection().InsertOrReplace(route);
        route.Insert();

    }



    private void CheckIfLocalERW()
    {
        //List<Route> erws = DBConnector.Instance.GetConnection().Query<Route>("Select * FROM ExploratoryRouteWalk where Id=" + selectedWay.Id);
        //Debug.Log("CheckIfLocalERW -> Capacity: " + erws.Count);

        var routeExists = Route.CheckIfExists(r => r.Id == selectedWay.Id);
       

        StartPanelOverwrite.SetActive(routeExists);
        StartPanelNew.SetActive(!routeExists);

        StatusOverwriteERW = routeExists;

    }


}
