using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using TMPro;

public class getWege : MonoBehaviour
{

    public List<GameObject> wegeListe;

    public GameObject wegItem;

    private List<Way> wege;

    // Start is called before the first frame update
    void Start()
    {
        //WegItem = transform.GetChild(0).gameObject;
        //WegItem = Resources.Load("Prefabs/Wege/WegItem") as GameObject;
        //GameObject newItem = Instantiate(Resources.Load("Prefabs/Wege/WegItem")) as GameObject;
        //GameObject newWegItem;

        this.wege = this.GetWege();

        //List<GameObject> wegeList = new List<GameObject>();

        foreach (Way weg in this.wege)
        {
            //Create Item
            var item = Instantiate(wegItem, transform);
            item.name = weg.Id.ToString();
            var buttons = item.GetComponentsInChildren(typeof(Button));
            var infoContainer = buttons[0] as Button;
            var infoButton = buttons[1] as Button;
            var startButton = buttons[2] as Button;
            var infoText = infoContainer.GetComponentInChildren<Text>() as Text;

            //set Info Text
            infoText.text = weg.Start.ToString() + " bis " + weg.Destination.ToString();

            // store wege id in gameobject
            infoContainer.name = weg.Id.ToString();

            // onClick
            infoButton.onClick.AddListener(() => this.ListElementOnClick(infoButton));
            startButton.onClick.AddListener(() => this.StartButtonOnClick(startButton));
            // var itemButton = item.GetComponents(typeof(Button))[0] as Button;
            // itemButton.onClick.AddListener(this.ListElementOnClick);
            // remember List
            wegeListe.Add(item);

        }


    }

    // Update is called once per frame
    void Update()
    {

    }

    private void StartButtonOnClick(Button selectedGameObject)
    {
        var selectedItem = selectedGameObject.transform.parent;
        this.OnClick(int.Parse(selectedItem.name));

    }
    private void ListElementOnClick(Button selectedGameObject)
    {
        var selectedItem = selectedGameObject.transform.parent;
        this.OnClick(int.Parse(selectedItem.name));
    }

    private void OnClick(int wegeId)
    {
        AppState.SelectedWeg = wegeId;
        SceneManager.LoadScene(AppState.allOkScene);
    }

    private List<Way> GetWege()
    {
        // TEMP Placeholder for sqlite querey
        List<Way> wegeList = new List<Way>();

        //wegeList.Add(new Weg() { weg_id = "Test Id1", start = "Bielefeld", ziel = "Paris", weg_name = "Test Name", weg_beschreibung = "Dies ist eine super Beschreibung" });
        //wegeList.Add(new Weg() { weg_id = "Test Id2", start = "Bielefeld", ziel = "Berlin", weg_name = "Test Name", weg_beschreibung = "Dies ist eine super Beschreibung" });
        //wegeList.Add(new Weg() { weg_id = "Test Id3", start = "Paris", ziel = "Bielefeld", weg_name = "Test Name", weg_beschreibung = "Dies ist eine super Beschreibung" });
        //wegeList.Add(new Weg() { weg_id = "Test Id4", start = "Berlin", ziel = "Bielefeld", weg_name = "Test Name", weg_beschreibung = "Dies ist eine super Beschreibung" });

        wegeList.Add(new Way() { Id = 1, Start = 1, Destination = 2, Name = "Test Name", Description = "Dies ist eine super Beschreibung" });
        wegeList.Add(new Way() { Id = 2, Start = 3, Destination = 4, Name = "Test Name", Description = "Dies ist eine super Beschreibung" });
        wegeList.Add(new Way() { Id = 3, Start = 5, Destination = 6, Name = "Test Name", Description = "Dies ist eine super Beschreibung" });
        wegeList.Add(new Way() { Id = 4, Start = 7, Destination = 8, Name = "Test Name", Description = "Dies ist eine super Beschreibung" });


        return wegeList;
    }
}
