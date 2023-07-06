using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrientationHandler : MonoBehaviour
{
    public ScreenOrientation orientation;
    void Awake()
    {
        Screen.orientation = orientation;
    }
}
