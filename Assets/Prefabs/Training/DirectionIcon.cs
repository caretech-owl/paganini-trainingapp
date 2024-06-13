using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.UI;

public class DirectionIcon : MonoBehaviour
{
    [Header(@"Arrow Components")]
    [SerializeField] private Color NormalColor = Color.yellow;
    [SerializeField] private Color ActiveColor = Color.green;

    [Header(@"Arrow Components")]
    [SerializeField] private GameObject WrongDirection;
    //[SerializeField] private GameObject CorrectDirection;
    [SerializeField] private GameObject Straight;
    [SerializeField] private GameObject RightTurn;
    [SerializeField] private GameObject LeftTurn;

    public enum DirectionType
    {
        [Description("Das ist nicht der richtige Weg")]
        WrongDirection,

        //[Description("Du bist auf dem richtigen Weg")]
        //CorrectDirection,

        [Description("Immer <color=blue>geradeaus</color> bleiben")]
        Straight,

        [Description("Hier <color=blue>rechts</color> abbiegen")]
        RightTurn,

        [Description("Hier <color=blue>links</color> abbiegen")]
        LeftTurn
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void RenderDirection(DirectionType directionType)
    {
        ApplyColorToBackground(NormalColor);
        WrongDirection?.SetActive(directionType == DirectionType.WrongDirection);
        //CorrectDirection?.SetActive(directionType == DirectionType.CorrectDirection);
        Straight?.SetActive(directionType == DirectionType.Straight);
        RightTurn?.SetActive(directionType == DirectionType.RightTurn);
        LeftTurn?.SetActive(directionType == DirectionType.LeftTurn);
    }

    public void SetConfirmationMode(bool active)
    {
        ApplyColorToBackground(active? ActiveColor : NormalColor);
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

// This is an extension for the enum descriptions 
public static class EnumExtensions
{
    public static string GetDescription(this Enum value)
    {
        var fieldInfo = value.GetType().GetField(value.ToString());
        var attributes = fieldInfo.GetCustomAttributes(false);

        if (attributes.Length > 0 && attributes[0] is DescriptionAttribute descriptionAttribute)
            return descriptionAttribute.Description;

        return value.ToString();
    }
}
