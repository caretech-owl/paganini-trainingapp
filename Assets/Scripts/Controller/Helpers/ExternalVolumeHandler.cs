using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using System.Collections;
using SFB = SimpleFileBrowser;
using SimpleFileBrowser;

public class ExternalVolumeHandler : MonoBehaviour
{
    // Change this to the desired directory path on the external volume.

    private string DestinationFolderPath;
    private string DestinationFolderName;

    public SyncProcessHandler SyncProcess;

    [Header("UI Configuration")]

    [SerializeField] private GameObject VolumeLookupPanel;
    [SerializeField] private GameObject VolumeLSelectPanel;
    [SerializeField] private GameObject FinishSuccessPanel;
    [SerializeField] private GameObject ExportErrorPanel;

    [Header("Volume Data UI")]
    [SerializeField] private TMPro.TMP_Text VolumeNameText;
    [SerializeField] private TMPro.TMP_Text ErrorMessageText;

    void Start()
    {
        // Start a coroutine to detect volume insertion.
        
    }

    public void Initialise()
    {
        DestinationFolderName = null;
        DestinationFolderPath = null;

        StartCoroutine(CheckPathCoroutine());
        StartCoroutine(CheckVolumeInserted());

        try {
            SFB.FileBrowser.DirectNativeSAF();
        }
        catch(Exception e) {
            Debug.Log(e.StackTrace);
        }
    }

    IEnumerator CheckPathCoroutine()
    {

        Debug.Log("CheckPathCoroutine(): " + (SFB.FileBrowser.GetCurrentPath() == null));
        yield return new WaitForSeconds(1.0f);

        if (SFB.FileBrowser.GetCurrentPath() != null) {
            DestinationFolderName = SFB.FileBrowser.GetCurrentPathName();
            DestinationFolderPath = SFB.FileBrowser.GetCurrentPath();


            Debug.Log($"DestinationFolderName: {DestinationFolderName} | DestinationFolderPath: {DestinationFolderPath} ");

            VolumeNameText.text = DestinationFolderName;

            VolumeLookupPanel.SetActive(false);
            VolumeLSelectPanel.SetActive(true);            
        }
        else
        {
            yield return null;
        }

    }


    IEnumerator CheckVolumeInserted()
    {
        bool volumeDisconnected = false;
        // Wait for a short delay before checking if the volume is inserted.

        Debug.Log("CheckVolumeInserted(): " + (DestinationFolderPath == null));

        yield return new WaitForSeconds(1.0f);

        if (DestinationFolderPath != null)
        {
            volumeDisconnected = !SFB.FileBrowserHelpers.DirectoryExists(DestinationFolderPath);
        }

        if (volumeDisconnected) {

            ErrorMessageText.text = "The destination folder is no longer present. Is the USB stick still connected? ";

            VolumeLSelectPanel.SetActive(false);
            ExportErrorPanel.SetActive(true);

            yield return null;
        }
    }


    public void StartFilesExport()
    {       

        if (!SFB.FileBrowserHelpers.DirectoryExists(DestinationFolderPath))
        {
            ErrorMessageText.text = "The destination folder is no longer present. Is the USB stick still connected? ";

            VolumeLSelectPanel.SetActive(false);
            ExportErrorPanel.SetActive(true);
            return;
        }

        try
        {
            string exportFolder = AppState.CurrentUser.Mnemonic_token + "-export-" + DateTime.Now.ToLongDateString();
            string path = SyncProcess.SyncroniseByFiles(exportFolder);

            DestinationFolderPath = SFB.FileBrowserHelpers.CreateFolderInDirectory(DestinationFolderPath, exportFolder);
            SFB.FileBrowserHelpers.MoveDirectory(path, DestinationFolderPath);

            FinishSuccessPanel.SetActive(true);
            VolumeLSelectPanel.SetActive(false);
        }
        catch (Exception e)
        {
            ErrorMessageText.text = "Error copying files. ";
            Debug.Log(e.StackTrace);

            VolumeLSelectPanel.SetActive(false);
            ExportErrorPanel.SetActive(true);
        }
    }

}
