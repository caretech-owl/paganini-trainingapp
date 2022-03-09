using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StartScene : MonoBehaviour
{

    // public SettingButton settingButton;

    public WayList wayList;

    // Start is called before the first frame update
    void Start()
    {
        this.DisplayWayList(GetListOfWays());

        // Set current scene to last scene for navigation
        AppState.lastScene = AppState.currentScene;
        AppState.currentScene = AppState.startScene;
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void DisplayWayList(List<Weg> wayObjectList)
    {
        float count = 0;
        foreach (Weg wayObject in wayObjectList)
        {
            float spacing = -2;
            Vector3 pos = new Vector3(spacing * count, 0, 0);
            wayList.CreateWayEntry(wayObject, pos);
            count++;
        }
    }

    private List<Weg> GetListOfWays()
    {
        // TEMP Placeholder for sqlite querey
        List<Weg> wayObjectList = new List<Weg>();

        wayObjectList.Add(new Weg() { weg_id = 1, start = 1, ziel = 2, weg_name = "Test Name", weg_beschreibung = "Dies ist eine super Beschreibung" });
        wayObjectList.Add(new Weg() { weg_id = 2, start = 3, ziel = 4, weg_name = "Test Name", weg_beschreibung = "Dies ist eine super Beschreibung" });
        wayObjectList.Add(new Weg() { weg_id = 3, start = 5, ziel = 6, weg_name = "Test Name", weg_beschreibung = "Dies ist eine super Beschreibung" });
        wayObjectList.Add(new Weg() { weg_id = 4, start = 7, ziel = 8, weg_name = "Test Name", weg_beschreibung = "Dies ist eine super Beschreibung" });


        return wayObjectList;
    }
}
