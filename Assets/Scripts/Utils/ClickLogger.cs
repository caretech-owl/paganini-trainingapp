using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickLogger : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private bool debugMode = true;

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"Clicked on: {gameObject.name}", gameObject);
    }

    private void Start()
    {
        if (debugMode)
        {
            Debug.Log("ClickLogger:Attaching to all elements");
            AttachToAllUIElements();
        }
    }

    private void AttachToAllUIElements()
    {
        Button[] buttons = FindObjectsOfType<Button>();
        foreach (Button button in buttons)
        {
            AttachToElement(button.gameObject);
        }

        InputField[] inputFields = FindObjectsOfType<InputField>();
        foreach (InputField inputField in inputFields)
        {
            AttachToElement(inputField.gameObject);
        }

        Image[] images = FindObjectsOfType<Image>();
        foreach (Image image in images)
        {
            AttachToElement(image.gameObject);
        }

        RawImage[] rawImages = FindObjectsOfType<RawImage>();
        foreach (RawImage rawImage in rawImages)
        {
            AttachToElement(rawImage.gameObject);
        }

        Text[] texts = FindObjectsOfType<Text>();
        foreach (Text text in texts)
        {
            AttachToElement(text.gameObject);
        }

        RectTransform[] rectTransforms = FindObjectsOfType<RectTransform>();
        foreach (RectTransform rectTransform in rectTransforms)
        {
            AttachToElement(rectTransform.gameObject);
        }

        GraphicRaycaster[] graphicRaycasters = FindObjectsOfType<GraphicRaycaster>();
        foreach (GraphicRaycaster graphicRaycaster in graphicRaycasters)
        {
            AttachToElement(graphicRaycaster.gameObject);
        }
    }


    private void AttachToElement(GameObject uiElement)
    {
        uiElement.AddComponent<ClickLogger>();

        EventTrigger eventTrigger = uiElement.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerClick;
        entry.callback.AddListener((eventData) => OnPointerClick((PointerEventData)eventData));
        eventTrigger.triggers.Add(entry);
    }
}
