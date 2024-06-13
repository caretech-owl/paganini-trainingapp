using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class RouteWalkSimulation : MonoBehaviour
{
    public RouteWalkList WalkList;
    public RouteTrainingTracking RouteTracking;

    [Header(@"Status bar")]
    public GameObject StatusBar;
    public Button PauseButton;
    public Button PlayButton;

    public int simulationUpdateInternval = 1; // seconds

    private void Start()
    {
        RenderRouteWalks();
    }

    public void RenderRouteWalks()
    {
        WalkList.ClearList();

        var walks = RouteWalk.GetAll( r=> r.RouteId == RouteSharedData.Instance.CurrentRoute.Id);

        foreach ( var walk in walks)
        {
            WalkList.AddItem(walk);
        }

        if (walks.Count == 0)
        {
            WalkList.ShowBlankState();
        }
        else
        {
            WalkList.ShowList();
        }
        
    }

    public void SimulateRouteWalk(RouteWalk routeWalk)
    {
        RouteTracking.StartSimulationTracking(routeWalk.Id, simulationUpdateInternval);

        if (StatusBar!= null) StatusBar?.SetActive(true);
        if (PauseButton != null) PauseButton?.gameObject.SetActive(true);
        if (PlayButton != null) PlayButton?.gameObject.SetActive(false);
    }

    public void ShowSimulationStatus()
    {

    }

    public void PauseSimulation(bool pause)
    {
        RouteTracking.PauseSimulationTracking(pause);
    }

}
