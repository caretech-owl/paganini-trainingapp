using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class DirectionIcon : MonoBehaviour
{
    [SerializeField] private GameObject WrongDirection;
    [SerializeField] private GameObject CorrectDirection;
    [SerializeField] private GameObject Straight;
    [SerializeField] private GameObject RightTurn;
    [SerializeField] private GameObject LeftTurn;

    public enum DirectionType
    {
        [Description("You're going the wrong way.")]
        WrongDirection,

        [Description("You're on the correct path.")]
        CorrectDirection,

        [Description("Continue straight ahead.")]
        Straight,

        [Description("Make a right turn.")]
        RightTurn,

        [Description("Make a left turn.")]
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
        WrongDirection.SetActive(directionType == DirectionType.WrongDirection);
        CorrectDirection.SetActive(directionType == DirectionType.CorrectDirection);
        Straight.SetActive(directionType == DirectionType.Straight);
        RightTurn.SetActive(directionType == DirectionType.RightTurn);
        LeftTurn.SetActive(directionType == DirectionType.LeftTurn);
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
