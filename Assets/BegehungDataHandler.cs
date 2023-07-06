using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BegehungDataHandler : MonoBehaviour
{
    /// <summary>
    /// Game Object to display Begehungen in
    /// </summary>
    public GameObject BegehungList;

    public GameObject BegehungPrefab;

    public GameObject UI;

    public GameObject PinPopupPrefab;

    public TMPro.TMP_InputField BegehungName;
    public TMPro.TMP_InputField BegehungStart;
    public TMPro.TMP_InputField BegehungZiel;
    private GameObject popup = null;

    private int expectedPin = 0;

    private List<Route> begehungen;

    // Start is called before the first frame update
    void Start()
    {
        AppState.SelectedBegehung = -1;
        ///debug
        if (AppState.CurrentUser == null)
        {

        }
        DBConnector.Instance.Startup();
        Restorebegehungen();
    }


    /// <summary>
    /// Reads back in the Begehungen List from SQLite and Saves it in the Object
    /// </summary>
    void Restorebegehungen()
    {
        List<Route> begehungen = DBConnector.Instance.GetConnection().Query<Route>("Select * FROM ExploratoryRouteWalk");
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
        DBConnector.Instance.GetConnection().DeleteAll<Route>();
        begehungen = null;

        Restorebegehungen();
    }

    /// <summary>
    /// Saves Begehungen to SQLite
    /// </summary>
    void SaveUserData()
    {
        if (begehungen != null)
            foreach (Route b in begehungen)
            {
                DBConnector.Instance.GetConnection().InsertOrReplace(b);
            }

    }
    /// <summary>
    /// Calls Rest API to get user begehungen
    /// </summary>
    public void GetBegehungen()
    {

        //ServerCommunication.Instance.GetUserBegehungen(GetBegehungenSucceed, GetBegehungenFailed, AppState.CurrentUser.Apitoken, AppState.SelectedWeg);
    }

    /// <summary>
    /// Create new Begehung
    /// </summary>
    public void AddBegehungen()
    {
        var b = new Route
        {
            Date = DateTime.Now,
            Name = BegehungName.text,
            Pin = 1234
        };

        if (begehungen == null)
            begehungen = new List<Route>();

        this.begehungen.Add(b);
        SaveUserData();
        Restorebegehungen();

        AppState.currentBegehung = b.Name;
    }

    /// <summary>
    /// Request was successful
    /// </summary>
    /// <param name="userProfile">UserProfile Object</param>
    private void GetBegehungenSucceed(RouteAPIList begehungen)
    {
        DeleteLocalData();
        this.begehungen = new List<Route>();
        foreach (var b in begehungen.erw)
        {
            this.begehungen.Add(new Route(b));
            Debug.Log(new Route(b));
        }
        SaveUserData();
        Restorebegehungen();
    }

    /// <summary>
    /// There were some problems with request.
    /// </summary>
    /// <param name="errorMessage">Error message.</param>
    private void GetBegehungenFailed(string errorMessage)
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
                foreach (Route b in begehungen)
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
                                    df.text = b.Name;
                                    break;
                                case "Datum":
                                    var d = b.Date;
                                    df.text = d.Day + "." + d.Month + "." + d.Year;
                                    break;
                                case "ID":
                                    df.text = b.Id.ToString();
                                    break;
                                case "PIN":
                                    df.text = b.Pin.ToString();
                                    break;

                            }
                        }
                        Button button = neu.GetComponentInChildren<Button>();
                        button.onClick.AddListener(OpenPopup);
                        neu.transform.SetParent(BegehungList.transform);
                        neu.transform.localScale = new Vector3(1, 1, 1);

                        neu.transform.localPosition = new Vector3(neu.transform.localPosition.x, neu.transform.localPosition.y, 1);
                    }
                }
            }
        }
    }
    /// <summary>
    /// Opens PIN Popup
    /// </summary>
    private void ClosePopup()
    {
        if (this.popup != null)
        {
            Destroy(popup);
            this.popup = null;
        }
    }

    private void Checkpin()
    {
        var list = this.UI.GetComponentsInChildren<Text>();
        foreach (var df in list)
        {
            switch (df.name)
            {
                case "BegehungPinText":
                    Debug.Log(this.expectedPin);
                    Debug.Log(df.text);
                    if (int.Parse(df.text) == this.expectedPin)
                    {
                        SceneManager.LoadScene("DokumentierteErstbegehung");
                    }
                    else
                    {
                        df.text = "";
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// Opens PIN Popup
    /// </summary>
    private void OpenPopup()
    {
        //delete popup if exists
        ClosePopup();
        //open popup
        //get button id and pin
        var list = EventSystem.current.currentSelectedGameObject.GetComponentsInChildren<Text>();
        foreach (var df in list)
        {
            switch (df.name)
            {
                case "ID":
                    AppState.SelectedBegehung = int.Parse(df.text);
                    break;
                case "PIN":
                    this.expectedPin = int.Parse(df.text);
                    break;
            }
        }
        this.popup = Instantiate(PinPopupPrefab, UI.transform);

        //set button onclicks
        var Buttonlist = popup.GetComponentsInChildren<Button>();
        foreach (var b in Buttonlist)
        {
            switch (b.name)
            {
                case "PINabortButton":
                    b.onClick.AddListener(ClosePopup);
                    break;
                case "PINokButton":
                    b.onClick.AddListener(Checkpin);
                    break;

            }
        }
    }
}
