using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTimed : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The Button component to be timed.")]
    private Button buttonObject;

    [SerializeField]
    [Tooltip("The Image component representing the background of the button.")]
    private Image fillImage;

    [SerializeField]
    [Tooltip("The color to fill the background of the button as a progress bar.")]
    private Color fillColor = Color.blue;

    [SerializeField]
    [Tooltip("Original text of the button.")]
    private GameObject ButtonOriginalText;

    [SerializeField]
    [Tooltip("Destination for the modified text when filled out.")]
    private GameObject FilledOutTextContainer;

    [SerializeField]
    [Tooltip("Color of the text when filling out.")]
    private Color FilledOutTextColor = Color.white;

    [SerializeField]
    [Tooltip("The duration in seconds until the button triggers itself.")]
    public int Timeout = 5;

    private float currentTime = 0f;
    private bool isTiming = false;

    // Start is called before the first frame update
    void Start()
    {
        // Initialize the fill color
        fillImage.color = fillColor;

        // Add listener to the button
        //buttonObject.onClick.AddListener(TriggerButton);

        CloneButtonTextForFillout();

        // Start timing
        StartTiming();
    }

    // Update is called once per frame
    void Update()
    {
        if (isTiming)
        {
            currentTime += Time.deltaTime;

            // Update fill amount based on current time and timeout
            fillImage.fillAmount = currentTime / Timeout;

            // If timeout is reached, trigger the button
            if (currentTime >= Timeout)
            {
                TriggerButton();
            }
        }
    }

    public void CancelTrigger()
    {
        isTiming = false;
        FilledOutTextContainer.transform.parent.gameObject.SetActive(false);
    }

    private void TriggerButton()
    {
        FilledOutTextContainer.transform.parent.gameObject.SetActive(false);
        buttonObject.onClick.Invoke();

        // Reset timer and stop timing
        currentTime = 0f;
        isTiming = false;

        //Debug.Log("Triggered Button");
    }

    public void StartTiming()
    {
        fillImage.fillAmount = 0;
        isTiming = true;
        currentTime = 0;
        FilledOutTextContainer.transform.parent.gameObject.SetActive(true);
    }

    private void CloneButtonTextForFillout()
    {
        // Clone ButtonOriginalText and set it as a child of FilledOutTextContainer
        GameObject cloneObj = Instantiate(ButtonOriginalText, FilledOutTextContainer.transform);
        cloneObj.name = "FilledOutText";

        // Get the Text component from the clone
        TMPro.TMP_Text filledText = cloneObj.GetComponent<TMPro.TMP_Text>();

        // Change the color of the text to FilledOutTextColor
        filledText.color = FilledOutTextColor;

        // Find the children images of ButtonOriginalText and change their color to FilledOutTextColor
        SVGImage[] images = cloneObj.GetComponentsInChildren<SVGImage>();
        foreach (SVGImage image in images)
        {
            image.color = FilledOutTextColor;
        }
    }


}
