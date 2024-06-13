using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Events;


[System.Serializable]
public class RouteWalkEvent : UnityEvent<RouteWalk>
{
}


public class RouteWalkItem : MonoBehaviour
{
        
    [Header(@"Text")]
    public TMPro.TMP_Text IdText;
    public TMPro.TMP_Text StartDateText;
    public TMPro.TMP_Text CompletePercentage;

    [Header(@"Other")]
    public Button selectionButton;
    public GameObject CompleteStatus;
    public GameObject IncompleteStatus;


    public RouteWalkEvent OnSelected;

    private RouteWalk routeWalk;



    // Start is called before the first frame update
    void Start()
    {
        selectionButton.onClick.AddListener(RouteWalkSelected);        
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void FillRouteWalkItem(RouteWalk routeWalk)
    {
        StartDateText.text = routeWalk.StartDateTime.ToString("MMMM dd, yyyy h:mmtt");
        IdText.text = routeWalk.Id.ToString();

        CompleteStatus.SetActive(routeWalk.WalkCompletion == RouteWalk.RouteWalkCompletion.Completed);
        IncompleteStatus.SetActive(routeWalk.WalkCompletion != RouteWalk.RouteWalkCompletion.Completed);

        CompletePercentage.text = Math.Round(routeWalk.WalkCompletionPercentage * 100) + "%";

        this.routeWalk = routeWalk;
    }


    private void RouteWalkSelected()
    {
        Debug.Log("Route walk selected, id: " + routeWalk.Id);
        if (OnSelected != null)
        {
            OnSelected.Invoke(routeWalk);
        }
    }

}
