using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExploringRouteManager : MonoBehaviour
{
    public GameObject LocationOverlay;
    public string BegehungName = "test";


    private bool isLocationOverlayVisible = false;
    // Start is called before the first frame update
    void Start()
    {
       Debug.Log(AppState.SelectedWeg); 
       Debug.Log(AppState.SelectedBegehung); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleLocationOverlay()
    {
        if (isLocationOverlayVisible)
            isLocationOverlayVisible = false;
        else
            isLocationOverlayVisible = true;

        LocationOverlay.SetActive(isLocationOverlayVisible);
    }
}
