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
        if (AppState.currentUser != null)
        {
            DBConnector.Instance.Startup();
            getWaysfromAPI();
        }

    }


    void getWaysfromAPI()
    {
        GetWege();
        //Restorewege();
    }


    /// <summary>
    /// Reads back in the Wege List from SQLite and Saves it in the Object
    /// </summary>
    void Restorewege()
    {
        List<Way> wege = DBConnector.Instance.GetConnection().Query<Way>("Select * FROM Way");
        Debug.Log("Restorewege -> Capacity: " + wege.Count);

        if (wege.Count > 0)
            this.ways = wege;
        else
            this.ways = null;

        DisplayWege();
    }

    /// <summary>
    /// Deletes ways Data
    /// </summary>
    public void DeleteLocalData()
    {
        // We delete everything except what hasn't been synchronised yet
        //DBConnector.Instance.GetConnection().DeleteAll<Way>();
        DBConnector.Instance.GetConnection().Execute("Delete from Way where Status =" + ((int)Way.WayStatus.FromAPI));
        Restorewege();
    }

    /// <summary>
    /// Saves ways to SQLite
    /// </summary>
    void SaveUserData()
    {
        if (ways != null)
            foreach (Way w in ways)
            {
                DBConnector.Instance.GetConnection().InsertOrReplace(w);
            }

    }
    /// <summary>
    /// Calls Rest API to get user wege
    /// </summary>
    public void GetWege()
    {

        ServerCommunication.Instance.GetUserWege(GetWegeSucceed, GetWegeFailed, AppState.currentUser.Apitoken);
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
            Status = (int) Way.WayStatus.Local            
        };

        if (ways == null) {
            ways = new List<Way>();
        }

        // safely create an ID, as the seconds since 2010        
        TimeSpan t = (DateTime.UtcNow - new DateTime(2010, 1, 1));
        w.Id = - (int)t.TotalSeconds;

        ways.Add(w);
        SaveUserData();
        Restorewege();

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

        StartPanelIconStart.GetComponent<LandmarkIcon>().selectedLandmarkType = (LandmarkIcon.LandmarkType)Int32.Parse(selectedWay.StartType);
        StartPanelIconZiel.GetComponent<LandmarkIcon>().selectedLandmarkType = (LandmarkIcon.LandmarkType)Int32.Parse(selectedWay.DestinationType);

        
        CheckIfLocalERW();

    }

    /// <summary>
    /// Request was successful
    /// </summary>
    /// <param name="wege">List of WegAPI objects</param>
    private void GetWegeSucceed(WegAPIList wege)
    {
        DeleteLocalData();
        this.ways = new List<Way>();
        foreach (WayAPI w in wege.wege)
        {
            this.ways.Add(new Way(w));
            Debug.Log(new Way(w));
        }
        SaveUserData();
        Restorewege();
    }

    /// <summary>
    /// There were some problems with request.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    private void GetWegeFailed(string errorMessage)
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
        Restorewege();

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

        DBConnector.Instance.GetConnection().Execute("DELETE FROM ExploratoryRouteWalk where Id=" + selectedWay.Id);
        DBConnector.Instance.GetConnection().Execute("DELETE FROM Pathpoint where Erw_id=" + selectedWay.Id);


        ExploratoryRouteWalk walk = new ExploratoryRouteWalk();
        walk.Id = selectedWay.Id;
        walk.Way_id = selectedWay.Id;
        walk.Date = DateTime.Now;
        walk.Name = selectedWay.Name;
        walk.Status = (int)Way.WayStatus.Local;

        // We set the current exploratory walk
        AppState.currentBegehung = walk.Name;
        AppState.SelectedBegehung = selectedWay.Id;
        AppState.SelectedWeg = selectedWay.Id;

        DBConnector.Instance.GetConnection().InsertOrReplace(walk);

    }

    public void CancelNewERW()
    {
        // If we are cancelling the overwriting, let's not delete the existing ERW.
        if (StatusOverwriteERW) return;

        DBConnector.Instance.GetConnection().Execute("Delete from Way where Status =" + ((int)Way.WayStatus.Local) + " and Id =" + selectedWay.Id);
        AppState.currentBegehung = null;
        AppState.SelectedBegehung = -1;
        AppState.SelectedWeg = -1;
        selectedWay = null;

        Restorewege();

    }

        void CheckIfLocalERW()
    {
        List<ExploratoryRouteWalk> erws = DBConnector.Instance.GetConnection().Query<ExploratoryRouteWalk>("Select * FROM ExploratoryRouteWalk where Id=" + selectedWay.Id);
        Debug.Log("CheckIfLocalERW -> Capacity: " + erws.Count);


        StartPanelOverwrite.SetActive(erws.Count > 0);
        StartPanelNew.SetActive(erws.Count == 0);

        StatusOverwriteERW = erws.Count > 0;

    }



    //public void SetSelectedERW(ExploratoryRouteWalk erw)
    //{
    //    SessionData.Instance.SaveData("SelectedERW", erw);
    //}


}
