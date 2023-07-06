using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Android;

public class LocationAccuracyDisplay : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI _gpsAccuracyText;
    [SerializeField] private SpriteRenderer _accuracyCircle;
    [SerializeField] private RectTransform _accuracyCircleTransform;

    [Header("Settings")]
    [SerializeField] private float _threshold = 5f;
    [SerializeField] private float _circleScaleFactor = 0.1f;
    [SerializeField] private Color _goodAccuracyColor = Color.green;
    [SerializeField] private Color _defaultAccuracyColor = Color.white;

    private bool _isAccuracyGood;

    private void Start()
    {
        
    }

    /// <summary>
    /// Triggers the location check service
    /// </summary>
    public void StartLocationCheck()
    {
        StartCoroutine(StartLocationService());
        //ResizeCircle(10);
    }

    /// <summary>
    /// Starts the location service and waits for the initialization to complete.
    /// </summary>
    private IEnumerator StartLocationService()
    {
        if (!Permission.HasUserAuthorizedPermission(Permission.FineLocation))
        {
            Permission.RequestUserPermission(Permission.FineLocation);
        }

        if (!Input.location.isEnabledByUser)
        {
            Debug.LogError("User has not enabled GPS");
            yield break;
        }

        Input.location.Start();

        int maxWait = 20;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }

        if (maxWait <= 0)
        {
            Debug.LogError("Timed out");
            yield break;
        }

        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("Unable to determine device location");
            yield break;
        }

    }

    private void Update()
    {        
        UpdateGPSAccuracyDisplay();
    }

    /// <summary>
    /// Updates the GPS accuracy display based on the location service's horizontal accuracy.
    /// </summary>
    private void UpdateGPSAccuracyDisplay()
    {
        if (Input.location.status == LocationServiceStatus.Running)
        {
            float accuracy = Input.location.lastData.horizontalAccuracy;

            ResizeCircle(accuracy);
        }
    }


    private void ResizeCircle(float accuracy)
    {

        _gpsAccuracyText.text = $"GPS Accuracy: {accuracy} meters";

        if (accuracy <= _threshold && !_isAccuracyGood)
        {
            _accuracyCircle.color = _goodAccuracyColor;
            _isAccuracyGood = true;
        }
        else if (accuracy > _threshold && _isAccuracyGood)
        {
            _accuracyCircle.color = _defaultAccuracyColor;
            _isAccuracyGood = false;
        }

        if (!_isAccuracyGood)
        {
            // Calculate the circle scale based on accuracy and the circle scale factor
            float circleScale = accuracy * _circleScaleFactor;

            // Get the parent RectTransform
            RectTransform parentTransform = _accuracyCircleTransform.parent.GetComponent<RectTransform>();

            // Calculate the maximum allowed scale based on the parent size
            float maxScale = Mathf.Min(parentTransform.rect.width, parentTransform.rect.height) * 0.5f;

            // Clamp the circle scale between 0 and the maximum allowed scale
            circleScale = Mathf.Clamp(circleScale, 0, maxScale);

            // Apply the clamped scale to the circle RectTransform
            _accuracyCircleTransform.localScale = new Vector3(circleScale, circleScale, 1);
        }

    }


}
