using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PictureToggle : MonoBehaviour
{
    public Toggle ToggleItem;
    public Image Picture;
    public Outline ImageOutline;

    [Header(@"Feedback")]
    public GameObject FeedbackGroup;
    public GameObject FeedbackOK;
    public GameObject FeedbackWrong;


    public int ToggleIndex { get; set; }

    private Color? originalOutlineColor = null;

    // Start is called before the first frame update
    void Start()
    {
        ToggleItem.onValueChanged.AddListener(HandleOnValueChanged);
        originalOutlineColor = ImageOutline.effectColor;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void RenderPicture(byte[] rawImage)
    {
        CleanupView();

        FeedbackGroup.SetActive(false);

        // Convert raw image byte array to Texture2D
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(rawImage);

        // Set the Texture2D as the sprite for the Image component
        Picture.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);

        AspectRatioFitter ratioFitter = Picture.gameObject.GetComponent<AspectRatioFitter>();
        ratioFitter.aspectRatio = (float)texture.width / texture.height;

        // Adjust the size of the Image component to match the size of the texture
        //Picture.SetNativeSize();
    }

    public void CleanupView()
    {
        // Reset state
        ToggleItem.SetIsOnWithoutNotify(false);
        HandleOnValueChanged(false);

        // Destroy existing image texture
        if (Picture.sprite != null)
        {
            DestroyImmediate(Picture.sprite.texture, true);
        }

    }

    public void RenderFeedback(bool success)
    {
        FeedbackGroup.SetActive(true);
        FeedbackOK.SetActive(success);
        FeedbackWrong.SetActive(!success);
    }

    public void OnDestroy()
    {
        CleanupView();
    }

    public void HandleOnValueChanged(bool value)
    {
        if (originalOutlineColor == null)
            return;

        var color = (Color)originalOutlineColor;

        // Adjust the opacity of the outline color
        if (value)
        {
            ImageOutline.effectColor = new Color(color.r, color.g, color.b, 1f);
        }
        else
        {
            ImageOutline.effectColor = color;
        }
    }
}
