using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class SendFile : MonoBehaviour
{
    FileTransferServer fts;
    SelectFile sf;
    Dropdown validServerList;

    Image _bar;
    Text _progress;
    FTSCore.FileUpload _upload;

    // Use this for initialization
    void Start ()
    {
        fts = GameObject.Find("FileTransferServer").GetComponent<FileTransferServer>();
        sf = transform.parent.Find("PanelSelect").GetComponent<SelectFile>();
        validServerList = transform.parent.Find("PanelSelect").Find("DropdownDevices").GetComponent<Dropdown>();

        _bar = transform.Find("UploadBar").Find("Bar").GetComponent<Image>();
        _bar.fillAmount = 0f;
        _progress = transform.Find("UploadBar").Find("Text").GetComponent<Text>();
        _progress.text = "0%";
    }

    void Update()
    {
        if(_upload != null)
        {
            _progress.text = Mathf.FloorToInt(_upload.GetProgress() * 100f) + "%";
            _bar.fillAmount = _upload.GetProgress();
        }
    }

    public void SendSelectedFile()
    {
        foreach (string file in Directory.EnumerateFiles(FileManagement.persistentDataPath + "/rfh/Videos", "*.mp4"))
        {
            Debug.Log("Send File: " + file.ToString());
           _upload = fts.SendFile("192.168.2.100", file.ToString(), false);
        }
    }

    public void BroadcastSelectedFile()
    {
        string file = sf.GetSelection();
        if (file != "Please select a file...")
        {
            fts.SendFile("", file, sf.IsFullPath());
            _bar.fillAmount = 0f;
            _progress.text = "Broadcast";
        }
    }

    public void TX_Event(FTSCore.FileUpload upload)
    {
        if(_upload != null && _upload.IsThis(upload))
        {
            _bar.fillAmount = 1f;
            _progress.text = "100%";
            _upload = null;
        }
    }

    public void TxTimeout_Event(FTSCore.FileUpload upload)
    {
        if (_upload != null && _upload.IsThis(upload))
        {
            _bar.fillAmount = 0f;
            _progress.text = "Timeout";
            _upload = null;
        }
    }
}
