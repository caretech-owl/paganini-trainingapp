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

    private List<Weg> wege;

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
            foreach(Weg w in wege)
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
                    }
                }
            }
        }
    }
}
