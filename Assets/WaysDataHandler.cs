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


    public GameObject WayListView;
    private WayList WayListHandler;


    private List<Way> ways;

    private int LastWegeId;

    public TMPro.TMP_InputField WegName;
    public TMPro.TMP_InputField WegStart;
    public TMPro.TMP_InputField WegZiel;
    public int WegStartType;
    public int WegZielType;



    public TMPro.TMP_Text StartPanelWegStart;
    public TMPro.TMP_Text StartPanelWegZiel;

    public GameObject StartPanelIconStartTrain;
    public GameObject StartPanelIconStartCoffee;
    public GameObject StartPanelIconStartWork;
    public GameObject StartPanelIconStartHome;

    public GameObject StartPanelIconZielTrain;
    public GameObject StartPanelIconZielCoffee;
    public GameObject StartPanelIconZielWork;
    public GameObject StartPanelIconZielHome;


    // Start is called before the first frame update
    void Start()
    {

        WayListHandler = WayListView.GetComponent<WayList>();


        Debug.Log(Application.persistentDataPath);
        AppState.SelectedWeg = -1;
        ///debug
        if (AppState.currentUser == null)
        {

        }
        DBConnector.Instance.Startup();
        getWaysfromAPI();
    }


    void getWaysfromAPI()
    {
        GetWege();
        Restorewege();
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
        var w = new Way
        {
            Name = WegName.text,
            Start = WegStart.text,
            StartType = WegStartType.ToString(),
            Destination = WegZiel.text,
            DestinationType = WegZielType.ToString(),
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
    }

    /// <summary>
    /// Set type of start
    /// </summary>
    public void SetWayStartType(int type)
    {
        WegStartType = type;
    }

    /// <summary>
    /// Set type of destination
    /// </summary>
    public void SetWayDestinationType(int type)
    {
        WegZielType = type;
    }

    /// <summary>
    /// Set up start panel with given information
    /// </summary>
    public void SetUpStartPanel()
    {
        // set text 
        StartPanelWegStart.text = WegStart.text;
        StartPanelWegZiel.text = WegZiel.text;

        // set icons
        StartPanelIconStartTrain.SetActive(false);
        StartPanelIconStartCoffee.SetActive(false);
        StartPanelIconStartWork.SetActive(false);
        StartPanelIconStartHome.SetActive(false);

        if (WegStartType == 1)
            StartPanelIconStartTrain.SetActive(true);
        else if (WegStartType == 2)
            StartPanelIconStartCoffee.SetActive(true);
        else if (WegStartType == 3)
            StartPanelIconStartWork.SetActive(true);
        else if (WegStartType == 4)
            StartPanelIconStartHome.SetActive(true);

        StartPanelIconZielTrain.SetActive(false);
        StartPanelIconZielCoffee.SetActive(false);
        StartPanelIconZielWork.SetActive(false);
        StartPanelIconZielHome.SetActive(false);

        if (WegZielType == 1)
            StartPanelIconZielTrain.SetActive(true);
        else if (WegZielType == 2)
            StartPanelIconZielCoffee.SetActive(true);
        else if (WegZielType == 3)
            StartPanelIconZielWork.SetActive(true);
        else if (WegZielType == 4)
            StartPanelIconZielHome.SetActive(true);


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

        // TODO: Either we have no connection, or the token is no longer valid.
        // If no longer valid, we should forward the user to the login screen
        Assets.ErrorHandlerSingleton.GetErrorHandler().AddNewError("Error Loading Ways", errorMessage);
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
                LastWegeId = w.Id;
            }

            WayListHandler.ShowWayList();
        }
    }

}
