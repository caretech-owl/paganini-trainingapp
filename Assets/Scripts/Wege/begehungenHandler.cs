using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
/// <summary>
/// This class is responsible for handling The Local Begehungen 
/// </summary>
public class begehungenHandler : MonoBehaviour
{
    /// <summary>
    /// Game Object to Display Profile in
    /// </summary>
    public GameObject BegehungList;

    public GameObject BegehungPrefab;

    private List<Begehung> begehungen;

    // Start is called before the first frame update
    void Start()
    {
        AppState.SelectedBegehung = -1;
        ///debug
        if (AppState.authtoken == "")
        {
            AppState.authtoken = "1234";
        }
        DBConnector.Instance.Startup();
        Restorebegehungen();
    }


    /// <summary>
    /// Reads back in the Begehungen List from SQLite and Saves it in the Object
    /// </summary>
    void Restorebegehungen()
    {
        List<Begehung> begehungen = DBConnector.Instance.GetConnection().Query<Begehung>("Select * FROM Begehung");
        if (begehungen.Capacity > 0)
            this.begehungen = begehungen;
        else
            this.begehungen = null;

        DisplayBegehungen();
    }

    /// <summary>
    /// Deletes Profile Data
    /// </summary>
    public void DeleteLocalData()
    {
        DBConnector.Instance.GetConnection().DeleteAll<Begehung>();
        begehungen = null;
        Restorebegehungen();
    }

    /// <summary>
    /// Saves Begehungen to SQLite
    /// </summary>
    void SaveUserData()
    {
        if (begehungen != null)
            foreach (Begehung b in begehungen)
            {
                DBConnector.Instance.GetConnection().InsertOrReplace(b);
            }

    }
    /// <summary>
    /// Calls Rest API to get user begehungen
    /// </summary>
    public void getBegehungen()
    {

        ServerCommunication.Instance.GetUserBegehungen(getBegehungenSucceed, getBegehungenFailed, AppState.authtoken, AppState.SelectedWeg);
    }

    /// <summary>
    /// Request was successful
    /// </summary>
    /// <param name="userProfile">UserProfile Object</param>
    private void getBegehungenSucceed(BegehungAPIList begehungen)
    {
        this.begehungen = new List<Begehung>();
        foreach (BegehungAPI b in begehungen.begehungen)
        {
            this.begehungen.Add(new Begehung(b));
        }
        SaveUserData();
        Restorebegehungen();
    }

    /// <summary>
    /// There were some problems with request.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    private void getBegehungenFailed(string errorMessage)
    {
        Debug.LogError(errorMessage);
    }

    /// <summary>
    /// Prints the Begehungen to the GUI
    /// </summary>
    private void DisplayBegehungen()
    {
        if (this.begehungen != null)
        {
            //deletes list
            foreach (Transform child in BegehungList.transform)
            {
                Destroy(child.gameObject);
            }

            if (BegehungPrefab != null)
            {
                foreach (Begehung b in begehungen)
                {
                    var neu = Instantiate(BegehungPrefab);
                    if (BegehungList != null)
                    {
                        var list = neu.GetComponentsInChildren<Text>();
                        foreach (var df in list)
                        {
                            switch (df.name)
                            {
                                case "Name":
                                    df.text = b.beg_name;
                                    break;
                                case "Datum":
                                    var d = b.beg_datum;
                                    df.text = d.Day + "." + d.Month + "." + d.Year;
                                    break;
                                case "ID":
                                    df.text = b.beg_id.ToString();
                                    break;

                            }
                        }
                        neu.transform.SetParent(BegehungList.transform);
                    }
                }
            }
        }
    }
}
