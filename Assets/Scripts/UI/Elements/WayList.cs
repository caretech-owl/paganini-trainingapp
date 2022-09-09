using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;



public class WayList : MonoBehaviour
{

    public GameObject scrollView;

    public GameObject content;

    public GameObject wayItemPrefab;

    public GameObject blankState;

    public WayEvent onWaySelected;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void ShowBlankState()
    {
        blankState.SetActive(true);
        scrollView.SetActive(false);
    }

    public void ShowWayList()
    {
        blankState.SetActive(false);
        scrollView.SetActive(true);
    }

    public void AddWayItem(Way w)
    {

        var neu = Instantiate(wayItemPrefab, content.transform);

        WayItem item = neu.GetComponent<WayItem>();
        item.fillWayItem(w);
        item.OnSelected = onWaySelected;

    }

    public void ClearList()
    {
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
    }


    //public void CreateWayEntry(Way way, Vector3 position)
    //{
    //    WayItem item = this.CreateWayItem(this.wayItemPrefab, way);
    //    item.transform.SetParent(this.content.transform, false);
    //}

    //private WayItem CreateWayItem(GameObject wayItemPrefab, Way way)
    //{
    //    WayItem item = Instantiate(wayItemPrefab).GetComponent<WayItem>();
    //    item.name = way.Id.ToString();

    //    //set Info Text
    //    item.info.text = way.Start.ToString() + " bis " + way.Destination.ToString();

    //    // store wege id in gameobject
    //    item.infoContainer.name = way.Id.ToString();

    //    // onClick
    //    item.infoContainer.onClick.AddListener(() => item.ListElementOnClick(item.infoContainer));
    //    item.startButton.onClick.AddListener(() => item.StartButtonOnClick(item.startButton));
    //    return item;
    //}
}