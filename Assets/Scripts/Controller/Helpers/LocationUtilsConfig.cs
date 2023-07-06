using UnityEngine;

public class LocationUtilsConfig : MonoBehaviour
{

    [SerializeField] public int searchRadius = 5;
    [SerializeField] public int locationHistorySize = 5;
    [SerializeField] public double offTrackThreshold = 20;
    [SerializeField] public double deviationHeadingThreshold = 30;
    [SerializeField] public double wrongTurnHeadingThreshold = 120;
    [SerializeField] public double oppositeHeadingThreshold = 160;
    [SerializeField] public double oppositeDistanceThreshold = 10; // meters since devaited
    [SerializeField] public double deviationDistanceThreshold = 10; // meters since devaited
    [SerializeField] public double onTargetDistanceThreshold = 10;

}