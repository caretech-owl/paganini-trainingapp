using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrainingHeader : MonoBehaviour
{
    [SerializeField] private TMPro.TMP_Text RouteNameText;

    private RouteSharedData SharedData; 
    // Start is called before the first frame update
    void Start()
    {
        SharedData = RouteSharedData.Instance;
        FillRouteName(SharedData.CurrentRoute);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void FillRouteName(Route route)
    {
        if (route == null) return;

        var text = route.Name;
        text = char.ToUpper(text[0]) + text.Substring(1);
        RouteNameText.text = text;
    }
}
