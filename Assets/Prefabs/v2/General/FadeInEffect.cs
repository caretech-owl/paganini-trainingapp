using System.Collections;
using UnityEngine;

public class FadeInEffect : MonoBehaviour
{
    public float fadeInDuration = 1.0f; // Duration of the fade-in effect
    public AnimationCurve fadeInCurve; // Animation curve for customizing the fade-in curve

    private CanvasGroup canvasGroup;
    private Coroutine fadeInCoroutine;

    private void Awake()
    {
        // Ensure the object has a CanvasGroup component
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            // If not, add one
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // Set the initial alpha to 0
        canvasGroup.alpha = 0f;
    }

    private void OnEnable()
    {
        // Reset the alpha to 0 when the GameObject is activated
        canvasGroup.alpha = 0f;

        // Start the fade-in effect
        fadeInCoroutine = StartCoroutine(FadeIn());
    }

    private void OnDisable()
    {
        // Stop the fade-in effect coroutine if it's running
        if (fadeInCoroutine != null)
        {
            StopCoroutine(fadeInCoroutine);
        }
    }

    private IEnumerator FadeIn()
    {
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            // Calculate the normalized progress of the fade-in effect
            float normalizedTime = elapsedTime / fadeInDuration;

            // Calculate the alpha value based on the animation curve
            float alpha = fadeInCurve.Evaluate(normalizedTime);

            // Apply the alpha to the CanvasGroup
            canvasGroup.alpha = alpha;

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            yield return null; // Wait for the next frame
        }

        // Ensure the alpha is set to 1 at the end of the fade-in effect
        canvasGroup.alpha = 1f;
    }
}
