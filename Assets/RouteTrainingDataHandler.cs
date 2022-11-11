using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class RouteTrainingDataHandler : MonoBehaviour
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



    // Start is called before the first frame update
    void Start()
    {
        //LoginHandler = gameObject.transform.GetComponentInParent<LoginManager>();

        WayListHandler = WayListView.GetComponent<WayList>();


        Debug.Log(Application.persistentDataPath);
        AppState.SelectedWeg = -1;


    }

    public void LoadRouteTrainings()
    {
        // We have a user session
        if (AppState.currentUser != null)
        {
            DBConnector.Instance.Startup();
            GetWaysfromAPI();
        }
    }




    public void GetWaysfromAPI()
    {
        GetWege();
    }


    /// <summary>
    /// Reads back in the Wege List from SQLite and Saves it in the Object
    /// </summary>
    void Restorewege()
    {
        List<Way> wege = DBConnector.Instance.GetConnection().Query<Way>("Select * FROM Way where Status =" + ((int)Way.WayStatus.FromAPI));
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
    void DeleteLocalData()
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
    private void GetWege()
    {

        ServerCommunication.Instance.GetUserWege(GetWegeSucceed, GetWegeFailed, AppState.currentUser.Apitoken);
    }


    public void SetSelectedWay(Way w)
    {
        SessionData.Instance.SaveData("SelectedWay", w);
        selectedWay = w;
    }



    /// <summary>
    /// Request was successful
    /// </summary>
    /// <param name="wege">List of WegAPI objects</param>
    private void GetWegeSucceed(WegAPIList wege)
    {
        DeleteLocalData();
        this.ways = new List<Way>();
        foreach (WayAPI w in wege.ways)
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



}

