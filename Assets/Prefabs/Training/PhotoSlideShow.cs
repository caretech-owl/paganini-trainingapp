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
    public GameObject SlidePrefab;
    public float dragThreshold = 30f;

    private int currentSlideIndex;
    public bool interactable = true;
    private Vector2 startPosition;
    private bool isDragging;
    private List<PathpointPhoto> pathpointPhotos;
    private List<GameObject> slides;
    private List<Texture2D> texturesToFreeUp;

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

    //private void GenerateSlides()
    //{
    //    slides = new List<Image>();

    //    for (int i = 0; i < pathpointPhotos.Count; i++)
    //    {
    //        GameObject newSlide = new GameObject("Slide" + i);
    //        newSlide.transform.SetParent(slideContainer, false);

    //        Image slideImage = newSlide.AddComponent<Image>();
    //        Texture2D texture = new Texture2D(2, 2);
    //        texture.LoadImage(pathpointPhotos[i].Data.Photo);
    //        slideImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

    //        // Set Preserve Aspect to true
    //        slideImage.preserveAspect = true;

    //        // Set anchors and pivot point
    //        slideImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
    //        slideImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
    //        slideImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
    //        slideImage.rectTransform.sizeDelta = new Vector2(slideContainer.rect.width, slideContainer.rect.height);
    //        slideImage.rectTransform.anchoredPosition = new Vector2(0, 0);

    //        slides.Add(slideImage);
    //    }
    //}

    private void GenerateSlides()
    {
        slides = new List<GameObject>(); // Use GameObject as the list type
        texturesToFreeUp = new List<Texture2D>();

        for (int i = 0; i < pathpointPhotos.Count; i++)
        {
            // Create the parent GameObject for each slide
            GameObject newSlide = new GameObject("Slide" + i);
            newSlide.transform.SetParent(slideContainer, false);

            // Add Image component to the parent GameObject
            Image background = newSlide.AddComponent<Image>();
            background.color = Color.white; // Set the background color to white
            background.raycastTarget = false; // Make the background non-interactable

            // Set the RectTransform's size to match slideContainer
            RectTransform newSlideRectTransform = background.GetComponent<RectTransform>();
            newSlideRectTransform.sizeDelta = slideContainer.rect.size;


            Mask mask = newSlide.AddComponent<Mask>();
            mask.showMaskGraphic = false;

            // Create a child GameObject for the image
            GameObject imageContainer = new GameObject("ImageContainer");
            imageContainer.transform.SetParent(newSlide.transform, false);

            Image slideImage = imageContainer.AddComponent<Image>();
            Texture2D texture = new Texture2D(2, 2);
            texture.LoadImage(pathpointPhotos[i].Data.Photo);
            slideImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));

            // Set Preserve Aspect to true
            slideImage.preserveAspect = true;

            // Set anchors and pivot point for the child Image
            slideImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            slideImage.rectTransform.anchorMin = new Vector2(0f, 0f);
            slideImage.rectTransform.anchorMax = new Vector2(1f, 1f);

            // Container aspect ratio
            float contAspectRatio = slideContainer.rect.width / (float)slideContainer.rect.height;

            // Calculate the aspect ratio of the image
            float aspectRatio = texture.width / (float)texture.height;

            // Set the sizeDelta based on the aspect ratio for the child Image
            if (contAspectRatio > aspectRatio) // Fit to width
            {
                //slideImage.rectTransform.sizeDelta = new Vector2(slideContainer.rect.width, slideContainer.rect.width / aspectRatio);
                slideImage.rectTransform.sizeDelta = new Vector2(0, slideContainer.rect.width / aspectRatio);
            }
            else // Fit to height
            {
                slideImage.rectTransform.sizeDelta = new Vector2(slideContainer.rect.height * aspectRatio, 0);
            }

            slides.Add(newSlide);
            texturesToFreeUp.Add(texture);
        }
    }


    public void ResetSlides()
    {
        if (slides != null)
        {
            // Destroy textures first
            foreach (var texture in texturesToFreeUp)
            {
                DestroyImmediate(texture);
            }

            // Then destroy the slides
            foreach (var slide in slides)
            {
                Destroy(slide);
            }

            slides.Clear();
            texturesToFreeUp.Clear();
        }

        currentSlideIndex = 0;
        isDragging = false;
    }



    public void CleanUpView()
    {
        ResetSlides();
    }

}
