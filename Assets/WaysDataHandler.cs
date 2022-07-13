using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaysDataHandler : MonoBehaviour
{
    /// <summary>
    /// Game Object to Display Wege in
    /// </summary>
    public GameObject WegeList;

    public GameObject WegePrefab;

    private List<Weg> wege;

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
        AppState.SelectedWeg = -1;
        ///debug
        if (AppState.authtoken == "")
        {
            AppState.authtoken = "1234";
        }
        DBConnector.Instance.Startup();
        Restorewege();
    }


    /// <summary>
    /// Reads back in the Wege List from SQLite and Saves it in the Object
    /// </summary>
    void Restorewege()
    {
        List<Weg> wege = DBConnector.Instance.GetConnection().Query<Weg>("Select * FROM Weg");
        Debug.Log("Restorewege -> Capacity: " + wege.Count);

        if (wege.Count > 0)
            this.wege = wege;
        else
            this.wege = null;

        DisplayWege();
    }

    /// <summary>
    /// Deletes Wege Data
    /// </summary>
    public void DeleteLocalData()
    {
        DBConnector.Instance.GetConnection().DeleteAll<Weg>();
        wege = null;
        Restorewege();
    }

    /// <summary>
    /// Saves Wege to SQLite
    /// </summary>
    void SaveUserData()
    {
        if (wege != null)
            foreach (Weg w in wege)
            {
                DBConnector.Instance.GetConnection().InsertOrReplace(w);
            }

    }
    /// <summary>
    /// Calls Rest API to get user wege
    /// </summary>
    public void GetWege()
    {

        ServerCommunication.Instance.GetUserWege(GetWegeSucceed, GetWegeFailed, AppState.authtoken);
    }

    /// <summary>
    /// Create new way
    /// </summary>
    public void AddWay()
    {
        var w = new Weg();

        w.weg_name = WegName.text;
        w.start = WegStartType;
        w.weg_beschreibung = WegStart.text + "->" + WegZiel.text;
        w.ziel = WegZielType;

        if (wege == null) { 
            wege = new List<Weg>();
            w.weg_id = 1;
        }
        else
        {
            w.weg_id = LastWegeId + 1;
        }

        wege.Add(w);
        SaveUserData();
        Restorewege();

        AppState.currentBegehung = w.weg_name;
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
        this.wege = new List<Weg>();
        foreach (WegAPI w in wege.wege)
        {
            this.wege.Add(new Weg(w));
            Debug.Log(new Weg(w));
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
    }

    /// <summary>
    /// Prints the Begehungen to the GUI
    /// </summary>
    private void DisplayWege()
    {
        if (this.wege != null)
        {
            //deletes list
            foreach (Transform child in WegeList.transform)
            {
                Destroy(child.gameObject);
            }

            if (WegePrefab != null)
            {
                foreach (Weg w in wege)
                {
                    var neu = Instantiate(WegePrefab);
                    if (WegeList != null)
                    {
                        var list = neu.GetComponentsInChildren<Text>();
                        foreach (var df in list)
                        {
                            switch (df.name)
                            {
                                case "Name":
                                    df.text = w.weg_name;
                                    break;
                                case "Start":
                                    df.text = w.start.ToString();
                                    break;
                                case "Ziel":
                                    df.text = w.ziel.ToString();
                                    break;
                                case "Beschreibung":
                                    df.text = w.weg_beschreibung;
                                    break;
                                case "ID":
                                    df.text = w.weg_id.ToString();
                                    break;

                            }
                        }
                        neu.transform.SetParent(WegeList.transform);
                        neu.transform.localScale = new Vector3(1, 1, 1);
                        neu.transform.localPosition = new Vector3(neu.transform.localPosition.x, neu.transform.localPosition.y, 1);
                    }

                    LastWegeId = w.weg_id;
                }
            }
        }
    }
}
