﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

using UnityEngine.UI;
using static LandmarkIcon;

public class LandmarkToggleList : MonoBehaviour
{
    public GameObject TogglePrefab;
    public GameObject Content;

    ToggleGroup toggleGroup;

    public LandmarkType SelectedLandMarkType;

    private readonly List<LandmarkIcon.LandmarkType> displayOrder = new List<LandmarkIcon.LandmarkType>
    {
        LandmarkIcon.LandmarkType.Home,
        LandmarkIcon.LandmarkType.Work,
        LandmarkIcon.LandmarkType.Park,
        LandmarkIcon.LandmarkType.Shopping,
        LandmarkIcon.LandmarkType.Train,
        LandmarkIcon.LandmarkType.Bus,
        LandmarkIcon.LandmarkType.Coffee
    };

    // Start is called before the first frame update
    void Start()
    {
        toggleGroup = GetComponent<ToggleGroup>();

        //LandmarkIcon.LandmarkType[]  icons = (LandmarkIcon.LandmarkType[]) Enum.GetValues(typeof(LandmarkIcon.LandmarkType));
        //Debug.Log("Landmark icons: " + icons.Length);

        foreach(LandmarkIcon.LandmarkType icon in displayOrder)
        {
            if (icon != LandmarkIcon.LandmarkType.Placeholder) {
                GameObject obj = Instantiate(TogglePrefab, Content.transform);
                LandmarkIconToggle toggle = obj.GetComponent<LandmarkIconToggle>();

                toggle.LandmarkType = icon;
                toggle.Group = toggleGroup;
                //toggle.AlphaWhenInactive = 0.2f;

                Toggle t = toggle.GetComponent<Toggle>();
                t.onValueChanged.AddListener(delegate
                {
                    if (t.isOn) {
                        SelectedLandMarkType = GetSelectedLandmarkType();
                    }
                    else if (!toggleGroup.AnyTogglesOn())
                    {
                        SelectedLandMarkType = LandmarkIcon.LandmarkType.Placeholder;
                    }
                });
            }

        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }


    LandmarkType GetSelectedLandmarkType()
    {

        foreach (Toggle toggle in toggleGroup.ActiveToggles())
        {
            Debug.Log(toggle.GetComponent<LandmarkIconToggle>().LandmarkType);

            return (toggle.GetComponent<LandmarkIconToggle>().LandmarkType);
        }

        return LandmarkType.Placeholder;


    }
}
