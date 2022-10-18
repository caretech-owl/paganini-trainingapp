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
        validServerList = transform.parent.Find("PanelCheck").Find("DropdownServers").GetComponent<Dropdown>();

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
        //string file = sf.GetSelection();
        //if (file != "Please select a file...")
        //{
        //    Debug.Log("validServerList.captionText.text: " + validServerList.captionText.text);
        //    Debug.Log("file: " + file.ToString());
        //    Debug.Log("sf.IsFullPath(): " + sf.IsFullPath().ToString());
        //    _upload = fts.SendFile(validServerList.captionText.text, file, sf.IsFullPath());
        //}

        //foreach (string f in Directory.EnumerateFiles(FileManagement.persistentDataPath + "/rfh/Videos", "*.mp4"))
        //{
        //    Debug.Log("Send File: " + f.ToString());
        //    _upload = fts.SendFile("192.168.2.100", f, true);
        //}

        ProcessDirectoryMP4(FileManagement.persistentDataPath);
        ProcessDirectoryJPG(FileManagement.persistentDataPath);
    }

    // Process all files in the directory passed in, recurse on any directories
    // that are found, and process the files they contain.
    private void ProcessDirectoryMP4(string targetDirectory)
    {
        // Process the list of files found in the directory.
        string[] fileEntries = Directory.GetFiles(targetDirectory, "*.mp4");
        foreach (string fileName in fileEntries)
            ProcessFile(fileName);

        // Recurse into subdirectories of this directory.
        string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
        foreach (string subdirectory in subdirectoryEntries)
            ProcessDirectoryMP4(subdirectory);
    }

    // Process all files in the directory passed in, recurse on any directories
    // that are found, and process the files they contain.
    private void ProcessDirectoryJPG(string targetDirectory)
    {
        // Process the list of files found in the directory.
        string[] fileEntries = Directory.GetFiles(targetDirectory, "*.jpg");
        foreach (string fileName in fileEntries)
            ProcessFile(fileName);

        // Recurse into subdirectories of this directory.
        string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
        foreach (string subdirectory in subdirectoryEntries)
            ProcessDirectoryJPG(subdirectory);
    }

    // Insert logic for processing found files here.
    private void ProcessFile(string file)
    {
        //Debug.Log("Processed file:" + file);
        //Debug.Log("validServerList.captionText.text: " + validServerList.captionText.text);

        _upload = fts.SendFile(validServerList.captionText.text, file, true);
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
