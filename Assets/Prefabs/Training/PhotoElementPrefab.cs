using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[System.Serializable]
public class PhotoElementEvent : UnityEvent<PhotoElementPrefab>
{
}

public class PhotoElementPrefab : MonoBehaviour
{
    public RawImage POIPhoto;
    public Toggle SelectedToggle;

    public bool IsSelected
    {
        get
        {
            return SelectedToggle == null ? false : SelectedToggle.isOn;
        }
    }

    public PhotoElementEvent OnSelectedChanged;


    // Start is called before the first frame update
    void Start()
    {
        SelectedToggle.onValueChanged.AddListener(ToggleSelectedChanged);
    }

    // Update is called once per frame
    void Update()
    {

        
    }

    public void FillPhoto(PathpointPhoto p)
    {
        RenderPicture(p.Data.Photo);
    }

    private void RenderPicture(byte[] imageBytes)
    {
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);

        POIPhoto.texture = texture;
    }

    public void ToggleSelectedChanged(bool isActive)
    {
        if (OnSelectedChanged != null)
        {
            OnSelectedChanged.Invoke(this);
        }

        Debug.Log("Photo selected: " +isActive);
    }
}
