using UnityEngine;

public class LocationUtilsConfig : MonoBehaviour
{

    [SerializeField] public int searchRadius = 5;
    [SerializeField] public int locationHistorySize = 5;
    [SerializeField] public double adaptiveOfftrackThresholdScalingFactor = 0.2;

    [SerializeField] public double offTrackThreshold = 20;
    [SerializeField] public double offTrackThresholdEnd = 18;  // We recommend this value to be lower than the deviationDistanceThresholdEnd

    [SerializeField] public double deviationDistanceThreshold = 10; // meters since devaited
    [SerializeField] public double deviationDistanceThresholdEnd = 10; // meters since devaited

    [SerializeField] public double oppositeDistanceThreshold = 10; // meters since devaited
    [SerializeField] public double oppositeDistanceThresholdEnd = 10; // meters since devaited

    [SerializeField] public double deviationHeadingThreshold = 30;
    [SerializeField] public double wrongTurnHeadingThreshold = 120;
    [SerializeField] public double oppositeHeadingThreshold = 160;
    [SerializeField] public double maxCorrectHeadingThreshold = 30;
    [SerializeField] public double offTrackHeadingThresholdEnd = 90; // angle to come into the path to be considered back on track (+ offTrackThresholdEnd)

    [SerializeField] public double onTargetMinDistanceThreshold = 10;
    [SerializeField] public double onTargetMaxDistanceThreshold = 15;


    [Header(@"Walking")]
    [Tooltip("The minimum speed threshold (in meters per second) to consider someone as walking. Speeds below this threshold may indicate stationary or slow movement.")]
    public double MinWalkingSpeedThreshold = 0.4;

    [Tooltip("The max speed threshold (in meters per second) to consider a measurement to be valid.")]
    public double MaxWalkingSpeedThreshold = 10;

    [Tooltip("The max accuracy to consider the measurment to be valid.")]
    public double MaxValidGPSAccuracy = 30;

    [Tooltip("Number of measurements to calculate user speed.")]
    public int WalkingGPSHistorySize = 3;

    [Tooltip("Max valid time for GPS measurement history, in milliseconds.")]
    public long MaxWalkingGPSHistoryValidMilliseconds = 4 * 1000;

    [Tooltip("Expected GPS measurement accuracy, used as default values in the location smoothing function.")]
    public double ExpectedGPSAccuracy = 15;

}