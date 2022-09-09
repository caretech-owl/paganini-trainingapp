using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;

[System.Serializable]
public class WayEvent : UnityEvent<Way>
{
}


public class WayItem : MonoBehaviour
{

    public LandmarkIcon startLandmark;
    public LandmarkIcon destinationLandmark;

    public TMPro.TMP_Text startName;
    public TMPro.TMP_Text destinationName;

    public Button selectionButton;



    public WayEvent OnSelected;

    private Way way;

    // Start is called before the first frame update
    void Start()
    {

        selectionButton.onClick.AddListener(waySelected);

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void fillWayItem(Way w)
    {
        startName.text = w.Start;
        destinationName.text = w.Destination;

        startLandmark.selectedLandmarkType = (LandmarkIcon.LandmarkType) Int32.Parse(w.StartType);
        destinationLandmark.selectedLandmarkType = (LandmarkIcon.LandmarkType)Int32.Parse(w.DestinationType);

        this.way = w;
    }


    private void waySelected()
    {
        Debug.Log("Way selected, id: " + way.Id);
        if (OnSelected != null)
        {
            OnSelected.Invoke(way);
        }
    }

    //public void StartButtonOnClick(Button selectedGameObject)
    //{
    //    var selectedItem = selectedGameObject.transform.parent;
    //    this.OnClick(int.Parse(selectedItem.name));

    //}
    //public void ListElementOnClick(Button selectedGameObject)
    //{
    //    var selectedItem = selectedGameObject.transform.parent;
    //    this.OnClick(int.Parse(selectedItem.name));
    //}

    //public void OnClick(int wegeId)
    //{
    //    AppState.SelectedWeg = wegeId;
    //    SceneManager.LoadScene(AppState.allOkScene);
    //}
}
