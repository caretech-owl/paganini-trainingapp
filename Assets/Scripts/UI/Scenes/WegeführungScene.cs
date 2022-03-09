using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WegeführungScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        // Set current scene to last scene for navigation
        AppState.lastScene = AppState.currentScene;
        AppState.currentScene = AppState.wegeFührungScene;
    }

    // Update is called once per frame
    void Update()
    {

    }
}
