using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WayList : MonoBehaviour
{

    public GameObject scrollView;

    public GameObject content;

    public GameObject wayItemPrefab;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void CreateWayEntry(Way way, Vector3 position)
    {
        WayItem item = this.CreateWayItem(this.wayItemPrefab, way);
        item.transform.SetParent(this.content.transform, false);
    }

    private WayItem CreateWayItem(GameObject wayItemPrefab, Way way)
    {
        WayItem item = Instantiate(wayItemPrefab).GetComponent<WayItem>();
        item.name = way.Id.ToString();

        //set Info Text
        item.info.text = way.Start.ToString() + " bis " + way.Destination.ToString();

        // store wege id in gameobject
        item.infoContainer.name = way.Id.ToString();

        // onClick
        item.infoContainer.onClick.AddListener(() => item.ListElementOnClick(item.infoContainer));
        item.startButton.onClick.AddListener(() => item.StartButtonOnClick(item.startButton));
        return item;
    }
}