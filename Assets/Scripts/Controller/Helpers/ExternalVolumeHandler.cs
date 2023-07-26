using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System;
using static PaganiniRestAPI;

public class ExternalVolumeHandler : MonoBehaviour
{
    // Change this to the desired directory path on the external volume.
    private string externalVolumePath = "/Android/data/com.your.package/files/";

    private bool volumeMounted = false;
    private bool checkingVolume = false;

    public SyncProcessHandler SyncProcess;
    public GameObject FinishSuccessPanel;
    public GameObject VolueLookupPanel;

    private void Start()
    {
        // Check if the volume is already mounted.
        //CheckVolumeMounted();

        // Start a coroutine to detect volume insertion.
        //StartCoroutine(CheckVolumeInserted()); // /storage?

        //var volumes = GetMountedExternalVolumes("/Volumes");

        //TODO: Get path from volume
        string path = "";

        StartFilesExport(path);
    }

    private void StartFilesExport(string path)
    {
        try
        {
            SyncProcess.SyncroniseByFiles(path);

            FinishSuccessPanel.SetActive(true);
            VolueLookupPanel.SetActive(false);
        }
        catch (Exception e)
        {
            //TODO: Errors in processing
        }
    }

    private void CheckVolumeMounted()
    {
        // Check if the external volume is mounted by verifying the directory's existence.
        if (Directory.Exists(externalVolumePath))
        {
            volumeMounted = true;
            Debug.Log("External volume is mounted.");
        }
        else
        {
            Debug.Log("External volume is not mounted.");
        }
    }

    private System.Collections.IEnumerator CheckVolumeInserted()
    {
        // Wait for a short delay before checking if the volume is inserted.
        yield return new WaitForSeconds(1.0f);

        checkingVolume = true;

        // Check if the external volume is inserted after the delay.
        if (!volumeMounted && Directory.Exists(externalVolumePath))
        {
            volumeMounted = true;
            Debug.Log("External volume inserted and mounted.");
        }

        checkingVolume = false;
    }

    public void ManualSelectVolume()
    {
        // You can implement a UI element here to allow the user to manually select the volume path.

        // For demonstration purposes, let's use the built-in Android file picker.
        // Note that this requires using Unity Android plugins.
        // Example plugin: https://github.com/yasirkula/UnityNativeFilePicker

        // After the user selects the volume, store the selected path in 'externalVolumePath'.
    }

    public void ListVolumeContents()
    {
        if (volumeMounted)
        {
            string[] files = Directory.GetFiles(externalVolumePath);
            string[] directories = Directory.GetDirectories(externalVolumePath);

            Debug.Log("Files in the external volume:");
            foreach (string file in files)
            {
                Debug.Log(file);
            }

            Debug.Log("Directories in the external volume:");
            foreach (string directory in directories)
            {
                Debug.Log(directory);
            }
        }
        else
        {
            Debug.Log("External volume is not mounted. Please mount or select the volume.");
        }
    }

    public static string[] GetMountedExternalVolumes(string volumesRoot)
    {
        string[] potentialExternalVolumes = Directory.GetDirectories(volumesRoot);

        // Filter out any directories that are not mounted volumes
        List<string> mountedVolumes = new List<string>();
        foreach (string volumePath in potentialExternalVolumes)
        {
            // Check if the directory exists and is accessible
            if (Directory.Exists(volumePath))
            {
                // Ensure it's not the primary internal storage
                if (!volumePath.Equals($"{volumesRoot}/emulated/0") && !volumePath.Equals($"{volumesRoot}/self"))
                {
                    mountedVolumes.Add(volumePath);
                }
            }
            Debug.Log(volumePath);
        }

        return mountedVolumes.ToArray();
    }

}
