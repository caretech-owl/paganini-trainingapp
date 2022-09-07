using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LandmarkIcon : MonoBehaviour
{
    public enum LandmarkType {
        Placeholder = -1,
        Train = 1,
        Coffee = 2,
        Work = 3,
        Home = 4
    }
    public LandmarkType selectedLandmarkType = LandmarkType.Placeholder;
    private LandmarkType activeLandmarkType;

    // Start is called before the first frame update
    void Start()
    {
        displayLandmarkType(activeLandmarkType);
    }

    // Update is called once per frame
    void Update()
    {
        // If a new selected landmark has been set, we activate it
        if (activeLandmarkType != selectedLandmarkType)
        {
            displayLandmarkType(selectedLandmarkType);
        }

    }

    // activates the selected landmark type
    void displayLandmarkType(LandmarkType selected)
    {
        Debug.Log(gameObject.transform.childCount);
        for (int i=0; i< gameObject.transform.childCount; i++)
        {
            Transform ch = gameObject.transform.GetChild(i);
            Debug.Log(ch.name);

            ch.gameObject.SetActive(ch.name == selected.ToString());
            Debug.Log(ch.name + "--" + selected.ToString() + "=== " + (ch.name == selected.ToString()));

        }

        activeLandmarkType = selected;
    }
}
