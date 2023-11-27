using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LandmarkIcon : MonoBehaviour
{
    public Color BackgroundColor = Color.white;
    private Color DefaultColor = Color.white;

    public enum LandmarkType
    {
        Placeholder = -1,
        Train = 1,
        Coffee = 2,
        Work = 3,
        Home = 4,
        Bus = 5,
        Park = 6,
        Shopping = 7
    }

    public LandmarkType SelectedLandmarkType = LandmarkType.Placeholder;
    private LandmarkType activeLandmarkType;

    // Start is called before the first frame update
    void Start()
    {
        ApplyColorToBackground(BackgroundColor);
        displayLandmarkType(activeLandmarkType);
    }

    // Update is called once per frame
    void Update()
    {
        // If a new selected landmark has been set, we activate it
        if (activeLandmarkType != SelectedLandmarkType)
        {
            displayLandmarkType(SelectedLandmarkType);
        }

    }

    public void ApplyColor(Color color)
    {
        DefaultColor = BackgroundColor;
        BackgroundColor = color;
        ApplyColorToBackground(color);
    }

    public void ResetColor()
    {
        BackgroundColor = DefaultColor;
        ApplyColorToBackground(BackgroundColor);
    }

    // activates the selected landmark type
    void displayLandmarkType(LandmarkType selected)
    {
        for (int i=0; i< gameObject.transform.childCount; i++)
        {
            Transform ch = gameObject.transform.GetChild(i);
            ch.gameObject.SetActive(ch.name == selected.ToString());
        }

        activeLandmarkType = selected;
    }

    public void SetSelectedLandmark(int typeCode)
    {
        SelectedLandmarkType = (LandmarkType)typeCode;
    }

    public void SetSelectedLandmark(string typeCode)
    {
        SelectedLandmarkType = (LandmarkType) int.Parse(typeCode);
    }

    private void ApplyColorToBackground(Color color)
    {
        Image[] backgroundComponents = gameObject.GetComponentsInChildren<Image>(true);

        foreach (var component in backgroundComponents)
        {
            if (component.name == "Background")
            {
                component.color = color;
            }
        }
    }

}
