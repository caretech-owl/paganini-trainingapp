using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PhotoSlideShow : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform slideContainer;    
    public GameObject visualIndicatorPrefab;
    public GameObject auditoryIndicatorPrefab;
    public float dragThreshold = 30f;

    private int currentSlideIndex;
    public bool interactable = true;
    private Vector2 startPosition;
    private bool isDragging;
    private List<PathpointPhoto> pathpointPhotos;
    private List<Image> slides;

    private void Start()
    {        
        //currentSlideIndex = 0;
        //isDragging = false;
        //UpdateVisualIndicators();
    }

    public void LoadSlideShow(List<PathpointPhoto> photos)
    {
        ResetSlides();
        pathpointPhotos = photos;

        if (photos.Count > 0)
        {
            GenerateSlides();
            interactable = true;
        }
        else
        {
            interactable = false;
        }
    }



    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!interactable) return;

        startPosition = eventData.position;
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!interactable) return;

        if (isDragging)
        {
            float deltaX = eventData.position.x - startPosition.x;
            slideContainer.anchoredPosition = new Vector2(-currentSlideIndex * Screen.width + deltaX, 0);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!interactable) return;

        isDragging = false;
        float deltaX = eventData.position.x - startPosition.x;

        if (Mathf.Abs(deltaX) > dragThreshold)
        {
            if (deltaX < 0 && currentSlideIndex < slides.Count - 1)
            {
                currentSlideIndex++;
            }
            else if (deltaX > 0 && currentSlideIndex > 0)
            {
                currentSlideIndex--;
            }
        }

        StartCoroutine(SmoothSlideTransition());
        //UpdateVisualIndicators();
    }

    private IEnumerator SmoothSlideTransition()
    {
        float targetX = -currentSlideIndex * Screen.width;
        float duration = 0.2f;
        float elapsedTime = 0;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float newX = Mathf.Lerp(slideContainer.anchoredPosition.x, targetX, elapsedTime / duration);
            slideContainer.anchoredPosition = new Vector2(newX, 0);
            yield return null;
        }
    }

    private void GenerateSlides()
    {
        slides = new List<Image>();

        for (int i = 0; i < pathpointPhotos.Count; i++)
        {
            GameObject newSlide = new GameObject("Slide" + i);
            newSlide.transform.SetParent(slideContainer, false);

            Image slideImage = newSlide.AddComponent<Image>();
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(pathpointPhotos[i].Data.Photo);
            slideImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // Set Preserve Aspect to true
            slideImage.preserveAspect = true;

            // Set anchors and pivot point
            slideImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            slideImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            slideImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            slideImage.rectTransform.sizeDelta = new Vector2(slideContainer.rect.width, slideContainer.rect.height);
            slideImage.rectTransform.anchoredPosition = new Vector2(0, 0);

            slides.Add(slideImage);
        }
    }


    public void ResetSlides()
    {
        if (slides != null)
        {
            for (int i = 0; i < slides.Count; i++)
            {
                Destroy(slides[i].gameObject);
            }
            slides.Clear();
        }
        
        currentSlideIndex = 0;
        isDragging = false;
        //UpdateVisualIndicators();
    }




    //private void UpdateVisualIndicators()
    //{
    //    foreach (Transform child in transform)
    //    {
    //        if (child.CompareTag("VisualIndicator"))
    //        {
    //            Destroy(child.gameObject);
    //        }
    //    }

    //    for (int i = 0; i < slides.Count; i++)
    //    {
    //        GameObject indicator = Instantiate(visualIndicatorPrefab, transform);
    //        if (i == currentSlideIndex)
    //        {
    //            indicator.GetComponent<Image>().color = Color.green; // Active slide indicator
    //        }
    //        else
    //        {
    //            indicator.GetComponent<Image>().color = Color.gray; // Inactive slide indicator
    //        }
    //    }

    //    if (auditoryIndicatorPrefab != null)
    //    {
    //        GameObject auditoryIndicator = Instantiate(auditoryIndicatorPrefab, transform);
    //        // Configure the auditory indicator based on the user's preferences and abilities.
    //    }
    //}
}
