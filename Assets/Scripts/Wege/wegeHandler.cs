using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// This class is responsible for handling The Local Wege 
/// </summary>
public class WegeHandler : MonoBehaviour
{
    /// <summary>
    /// Game Object to Display Wege in
    /// </summary>
    public GameObject WegeList;

    public GameObject WegePrefab;

    private List<Way> wege;

    // Start is called before the first frame update
    void Start()
    {
        AppState.SelectedWeg = -1;
        ///debug
        if (AppState.currentUser == null) 
        {
        }
        DBConnector.Instance.Startup();
        Restorewege();
    }


    /// <summary>
    /// Reads back in the Wege List from SQLite and Saves it in the Object
    /// </summary>
    void Restorewege()
    {
        List<Way> wege = DBConnector.Instance.GetConnection().Query<Way>("Select * FROM Weg");
        if (wege.Capacity > 0)
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
        DBConnector.Instance.GetConnection().DeleteAll<Way>();
        wege = null;
        Restorewege();
    }

    /// <summary>
    /// Saves Wege to SQLite
    /// </summary>
    void SaveUserData()
    {
        if (wege != null)
            foreach(Way w in wege)
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
    /// Request was successful
    /// </summary>
    /// <param name="wege">List of WegAPI objects</param>
    private void GetWegeSucceed(WegAPIList wege)
    {
        DeleteLocalData();
        this.wege = new List<Way>();
        foreach (WayAPI w in wege.ways)
        {
            this.wege.Add(new Way(w));
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
                foreach (Way w in wege)
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
                                    df.text = w.Name;
                                    break;
                                case "Start":
                                    df.text = w.Start.ToString();
                                    break;
                                case "Ziel":
                                    df.text = w.Destination.ToString();
                                    break;
                                case "Beschreibung":
                                    df.text = w.Description;
                                    break;
                                case "ID":
                                    df.text = w.Id.ToString();
                                    break;

                            }
                        }
                        neu.transform.SetParent(WegeList.transform);
                    }
                }
            }
        }
    }
}
