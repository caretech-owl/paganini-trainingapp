using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;



public class RouteWalkList : MonoBehaviour
{

    public GameObject ScrollView;

    public GameObject Content;

    public GameObject ItemPrefab;

    public GameObject BlankState;

    public RouteWalkEvent OnSelected;

    public TMPro.TMP_Text SelectedIdText;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }


    public void ShowBlankState()
    {
        BlankState.SetActive(true);
        ScrollView.SetActive(false);
    }

    public void ShowList()
    {
        BlankState.SetActive(false);
        ScrollView.SetActive(true);
    }

    public void RenderSelectedStatus(RouteWalk item)
    {
        if (SelectedIdText!= null)
            SelectedIdText.text = $"Route Walk: { item.Id}";
    }

    public void AddItem(RouteWalk routeWalk)
    {

        var neu = Instantiate(ItemPrefab, Content.transform);

        RouteWalkItem item = neu.GetComponent<RouteWalkItem>();
        item.FillRouteWalkItem(routeWalk);
        item.OnSelected = OnSelected;

    }

    public void ClearList()
    {
        foreach (Transform child in Content.transform)
        {
            Destroy(child.gameObject);
        }
    }

}