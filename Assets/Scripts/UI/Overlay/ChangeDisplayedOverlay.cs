using UnityEngine;
using UnityEngine.UI;

public enum OverlayState
{
    None,
    Left,
    Right,
    Center
}

public class ChangeDisplayedOverlay : MonoBehaviour
{
    private RawImage overlayImage;
    private OverlayState state;
    public Texture default_texture;
    public Texture left_texture;
    public Texture right_texture;
    public Texture center_texture;

    void Start()
    {
        overlayImage = GetComponent<RawImage>();
        state= OverlayState.None;

    }

    public void NextOverlay()
    {
        switch(state)
        {
            case OverlayState.None:
                ChangeColorToOpaque();
                overlayImage.texture = left_texture;
                state = OverlayState.Left;
                break;
            case OverlayState.Left:
                ChangeColorToOpaque();
                overlayImage.texture = right_texture;
                state = OverlayState.Right;
                break;
            case OverlayState.Right:
                ChangeColorToOpaque();
                overlayImage.texture = center_texture;
                state = OverlayState.Center;
                break;
            case OverlayState.Center:
                ChangeColorToInvisible();
                overlayImage.texture = default_texture;
                state = OverlayState.None;
                break;
        }
    }

   void ChangeColorToInvisible()
    {
        overlayImage.color = new Color(1,1,1,0);
    }
   void ChangeColorToOpaque()
    {
        overlayImage.color = new Color(1, 1, 1, 0.3f);
    }

}