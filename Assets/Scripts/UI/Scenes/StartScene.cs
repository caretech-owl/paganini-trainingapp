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
//        this.DisplayWayList(GetListOfWays());

        // Set current scene to last scene for navigation
        AppState.lastScene = AppState.currentScene;
        AppState.currentScene = AppState.startScene;
    }

    // Update is called once per frame
    void Update()
    {

    }

    //TODO: Check where this is used
    //private void DisplayWayList(List<Way> wayObjectList)
    //{
    //    float count = 0;
    //    foreach (Way wayObject in wayObjectList)
    //    {
    //        float spacing = -2;
    //        Vector3 pos = new Vector3(spacing * count, 0, 0);
    //        wayList.CreateWayEntry(wayObject, pos);
    //        count++;
    //    }
    //}

    //private List<Way> GetListOfWays()
    //{
    //    // TEMP Placeholder for sqlite querey
    //    List<Way> wayObjectList = new List<Way>();

    //    wayObjectList.Add(new Way() { Id = 1, Start = 1, Destination = 2, Name = "Test Name", Description = "Dies ist eine super Beschreibung" });
    //    wayObjectList.Add(new Way() { Id = 2, Start = 3, Destination = 4, Name = "Test Name", Description = "Dies ist eine super Beschreibung" });
    //    wayObjectList.Add(new Way() { Id = 3, Start = 5, Destination = 6, Name = "Test Name", Description = "Dies ist eine super Beschreibung" });
    //    wayObjectList.Add(new Way() { Id = 4, Start = 7, Destination = 8, Name = "Test Name", Description = "Dies ist eine super Beschreibung" });


    //    return wayObjectList;
    //}
}
