using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography;
using System.Text;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using TMPro;
using UnityEngine;
using static FTSCore;

public class SyncProcessHandler : MonoBehaviour
{
    public FileTransferServer fileTransferServer;
    public GameObject SmartphoneAskForConnectionText;
    public GameObject UI_AskForConnection;
    public GameObject UI_NoData;
    public GameObject UI_WaitForTablet;
    public GameObject UI_WaitForConnectionWithTablet;
    public GameObject UI_WaitForSyncEnd;
    public GameObject UI_SyncFinshed;
    public GameObject UI_SyncError;

    public Progressbar Progressbar;

    private List<Way> ways;
    private List<DetailedWayExport> wayExports;
    private List<DetailedWayExportFiles> exportFilesForWay;
    private string currentFolderForCopies;
    private int CountOfFiles = 0;
    private int TransferedCountOfFiles = 0;

    // Next update in second
    private int nextUpdate = 1;
    bool isSearchForTablet = true;


    private RemoteDevice serverDevice;
    private SyncStatus CurrentStatus;

    // Status of the connection with the remote device
    private bool isCurrentDeviceConnected;
    private int connectionAttemps = 0;
    private DateTime lastHeartbeat = System.DateTime.Now;

    // This are the internal protocol states. This is used to verify
    // that incoming (protocol) request arrive at the proper order
    // (i.e., respecting the state machine of the protocol).
    public enum SyncStatus
    {
        LISTEN = 0,
        WAIT_ACCEPT = 1,
        ACCEPT = 2,
        INFORM_ROUTES = 3,
        WAIT_ERW_SELECT = 4,
        ERW_PREPARE = 5,
        ERW_READY = 6,
        ERW_MANIFEST = 7,
        DOWNLOAD = 8,
        DOWNLOAD_READY = 9,
        FINISH = 10,
        CANCEL = 11
    }


    // Start is called before the first frame update
    void Start()
    {
        CurrentStatus = SyncStatus.LISTEN;

        // We have a user session
        if (AppState.currentUser != null)
        {
            DBConnector.Instance.Startup();
            GetWaysFromLocalDatabase();
        }

        // Set the name of the device to the user's mnemonic token
        fileTransferServer._deviceName = AppState.currentUser.Mnemonic_token;

        // Initialise synchronisation folder
        ResetSynchronisationFolders();
    }


    // Update is called once per frame
    void Update()
    {
        // If the next update is reached
        if (isSearchForTablet && Time.time >= nextUpdate)
        {
            //Debug.Log(Time.time + ">=" + nextUpdate);
            // Change the next update (current second+1)
            nextUpdate = Mathf.FloorToInt(Time.time) + 2;
            fileTransferServer.SendPollRequest();
        }
    }

    /**************************************
     *  Public UI events (and utilities)  *
     **************************************/

    /// <summary>
    /// Triggers a connection accept command, to start the actual sync process
    /// the user.
    /// </summary>
    public void AcceptConnection()
    {
        UI_AskForConnection.SetActive(false);
        UI_WaitForConnectionWithTablet.SetActive(true);

        // create and copy files to synchronize
        ExportWaysToSharedFolder();

        // inform tablet
        RequestConnectionAllowed();

        // We start a co-routine to check on the connection with
        // the current device
        lastHeartbeat = System.DateTime.Now;
        StartCoroutine(CheckDeviceConnection());
    }


    /****************************************
     *  Pre- and post- processing functions *
     ****************************************/

    /// <summary>
    /// Reads back in the Wege List from SQLite and Saves it in the Object
    /// </summary>
    private void GetWaysFromLocalDatabase()
    {
        string q = "Select w.* FROM way w join exploratoryroutewalk erw ON" +
            " w.Id = erw.Way_id " +
            " WHERE (w.Status = ?) or (erw.Status = ?)";

        List<Way> wege = DBConnector.Instance.GetConnection().Query<Way>(q, new object[] { (int)Way.WayStatus.Local, (int)Way.WayStatus.Local });
        Debug.Log("Restorewege -> Capacity: " + wege.Count);


        if (wege.Count > 0)
            this.ways = wege;
        else
        {
            this.ways = null;
            UI_NoData.SetActive(true);
            UI_WaitForTablet.SetActive(false);

            ResetOrDisposeProcessProtocol();
        }

    }

    /// <summary>
    ///  Generates a DetailedWayExport entry from a Way definition. 
    /// </summary>
    /// <param name="way">A Way definition </param>
    private DetailedWayExport FilledOutRecordingReport(Way way)
    {
        // TODO: Check if this is used
        // Only local ERW should be considered!
        DetailedWayExport detailedWayExport = new DetailedWayExport
        {
            Id = way.Id,
            Name = way.Name,
            Description = way.Description,
            Destination = way.Destination,
            DestinationType = way.DestinationType,
            Start = way.Start,
            StartType = way.StartType,
            Status = way.Status
        };

        List<ExploratoryRouteWalk> erw = (DBConnector.Instance.GetConnection().Query<ExploratoryRouteWalk>("Select * FROM ExploratoryRouteWalk where Way_id = ?", new object[] { way.Id }));
        if (erw.Count > 0)
        {
            detailedWayExport.RecordingName = erw[0].Name;
            detailedWayExport.RecordingDate = erw[0].Date;
        }
        else
        {
            LogError("There is no ERW for Way with Id = " + way.Id);
        }

        return detailedWayExport;
    }

    /// <summary>
    /// Exports the list of local ERWs that are available for synchronisation
    /// into a xml file. This xml will be used to inform the server about the
    /// available routes.
    /// </summary>
    private void ExportWaysToSharedFolder()
    {
        if (ways != null)
        {
            wayExports = new List<DetailedWayExport>();

            foreach (var way in ways)
            {
                DetailedWayExport detailedWayExport = FilledOutRecordingReport(way);

                // Create the dummy files, representing the request messages for the way
                File.Create(FileManagement.persistentDataPath + "/" + fileTransferServer._sharedFolder + "/REQUEST-ERW-" + way.Id).Close();

                wayExports.Add(detailedWayExport);
            }

            // write down to xml
            DetailedWayExportFiles.SerializeAsXML(wayExports, "waysForExport.xml", fileTransferServer._sharedFolder);
        }
        else
        {
            ErrorHandlerSingleton.GetErrorHandler().AddNewError("ExportWaysToSharedFolder", "No ways available!");
        }
    }

    /// <summary>
    /// Prepares the given ERW for the synchronisation process, which includes
    /// pre-processing files and copying them to the sharing folder, and generating
    /// the synchronisation manifest to be sent out to the server. 
    /// After the processing, the server is notified that the ERW is ready.
    /// </summary>
    /// <param name="id"> Id of the ERW </param>
    /// <param name="fileName">Name of manifest file to generate</param>
    private void PrepareERW(int id, string fileName)
    {
        CountOfFiles = 0;
        var way = ways.FirstOrDefault(w => w.Id == id);

        DetailedWayExport detailedWayExport = FilledOutRecordingReport(way);

        // set destination folder for tablet
        detailedWayExport.Folder = way.Name;

        // Get GPS coordinates
        List<Pathpoint> points = DBConnector.Instance.GetConnection().Query<Pathpoint>("SELECT * FROM Pathpoint where erw_id=?", way.Id);
        detailedWayExport.Points = SerializeGPSCoordinatesAsXML(points, detailedWayExport);
        CountOfFiles++;

        // Get files
        currentFolderForCopies = FileManagement.persistentDataPath + "/" + fileTransferServer._sharedFolder;
        exportFilesForWay = new List<DetailedWayExportFiles>();

        try
        {
            ProcessDirectory(FileManagement.persistentDataPath + "/" + detailedWayExport.Folder, "*.mp4");
        }
        catch (Exception ex)
        {
            LogError(ex.Message);
        }


        try
        {
            ProcessDirectory(FileManagement.persistentDataPath + "/" + detailedWayExport.Folder, "*.jpg");
        }
        catch (Exception ex)
        {
            LogError(ex.Message);
        }

        detailedWayExport.Files = exportFilesForWay;

        // export to xml
        List<DetailedWayExport> erwExport = new List<DetailedWayExport>();
        erwExport.Add(detailedWayExport);
        DetailedWayExportFiles.SerializeAsXML(erwExport, $"{detailedWayExport.Name}-manifest.xml", fileTransferServer._sharedFolder);

        // Inform the tablet that the ERW is ready to be downloaded
        RequestErwReady();

    }

    /// <summary>
    //  Writes down coordinates for specific way to xml file in shared folder
    /// </summary>
    private string SerializeGPSCoordinatesAsXML(List<Pathpoint> points, DetailedWayExport detailedWayExport)
    {
        if (points != null)
        {
            var objType = points.GetType();
            string filename = FileManagement.persistentDataPath + "/" + fileTransferServer._sharedFolder + "/" + detailedWayExport.Name + "-coordinates.xml";

            try
            {
                using (var xmlwriter = new XmlTextWriter(filename, Encoding.UTF8))
                {
                    xmlwriter.Indentation = 2;
                    xmlwriter.IndentChar = ' ';
                    xmlwriter.Formatting = Formatting.Indented;
                    var xmlSerializer = new XmlSerializer(objType);
                    xmlSerializer.Serialize(xmlwriter, points);
                }
            }
            catch (System.IO.IOException ex)
            {
                filename = String.Empty;
                ErrorHandlerSingleton.GetErrorHandler().AddNewError("Could not write to file!", ex.Message, true, false);
            }

            return filename;
        }
        else
        {
            return String.Empty;
        }

    }

    /// <summary>
    // Processes all files in the directory passed in, recurse on any directories
    // that are found, and process the files they contain.
    /// </summary>
    private void ProcessDirectory(string targetDirectory, string extension)
    {
        // Process the list of files found in the directory.
        string[] fileEntries = Directory.GetFiles(targetDirectory, extension);
        foreach (string fileName in fileEntries)
        {
            List<DetailedWayExportFiles> entries = DetailedWayExportFiles.
                PrepareFileForExport(fileName, currentFolderForCopies);

            CountOfFiles = CountOfFiles + entries.Count;

            exportFilesForWay = exportFilesForWay.Concat(entries).ToList();
        }

        // Recurse into subdirectories of this directory.
        string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
        foreach (string subdirectory in subdirectoryEntries)
            ProcessDirectory(subdirectory, extension);
    }

    /// <summary>
    /// Resets the synchronisation folders, clearing up contents and initialising
    /// the dummy files used as part of the transfer protocol
    /// </summary>
    private void ResetSynchronisationFolders()
    {
        // Check existence of shared folder
        if (!Directory.Exists(FileManagement.persistentDataPath + "/" + fileTransferServer._sharedFolder))
        {
            Directory.CreateDirectory(FileManagement.persistentDataPath + "/" + fileTransferServer._sharedFolder);
        }

        // Delete all files in sharing folder
        try
        {
            foreach (var file in Directory.GetFiles(FileManagement.persistentDataPath + "/" + fileTransferServer._sharedFolder))
            {
                File.Delete(file);
            }
        }
        catch (Exception ex)
        {
            ErrorHandlerSingleton.GetErrorHandler().AddNewError("SynchronizationHandler:Start(): Error in 'Delete all files in sharing folder'", ex.Message, false);
        }

        File.Create(FileManagement.persistentDataPath + "/" + fileTransferServer._sharedFolder + "/HANDSHAKE").Close();
        File.Create(FileManagement.persistentDataPath + "/" + fileTransferServer._sharedFolder + "/ENDOFSYNC").Close();
    }


    /// <summary>
    /// Resets the underlying FTS connections, related variables, and stops
    /// connection polling co-routines.
    /// </summary>
    private void ResetOrDisposeProcessProtocol()
    {
        Log("Performing: ResetOrDisposeProcessProtocol");
        StopAllCoroutines();
        fileTransferServer.Disconnect();
    }

    /**************************
     *  FTS event listeners   *
     **************************/

    /// <summary>
    ///  FTS event listener for updating the device list (On Device List Update ()).
    /// </summary>
    /// <param name="devices">A list of FTSCore.RemoteDevice representing the
    /// currently connected devices. </param>
    public void OnDevicesListUpdate(List<FTSCore.RemoteDevice> devices)
    {

        Log($"OnDevicesListUpdate: Status: {CurrentStatus} Device list: " + devices.Count);
        isCurrentDeviceConnected = true;
        connectionAttemps = 0;

        if (devices.Count > 0 && CurrentStatus == SyncStatus.LISTEN)
        {
            serverDevice = devices[0];
            //Debug.Log("DevicesListUpdate:" + SmartphoneAskForConnectionText.GetComponent<TMP_Text>().text);
            SmartphoneAskForConnectionText.GetComponent<TMP_Text>().text = SmartphoneAskForConnectionText.GetComponent<TMP_Text>().text.Replace("[TABLETNAME]", devices[0].name);
            isSearchForTablet = false;
        }
    }

    /// <summary>
    /// FTS event listener for file uploading (On Upload ()). This is
    /// a callback that informs us of incoming requests from the connected device.
    /// </summary>
    /// <param name="fileUpload">FileUpload representing incoming request. </param>
    public void OnUpload(FileUpload fileUpload)
    {
        Debug.Log("UpdateProgressbar:fileUpload: " + fileUpload.GetName());

        if (fileUpload.GetName().Equals("HANDSHAKE"))
        {
            if (CurrentStatus != SyncStatus.LISTEN)
            {
                LogError($"ENDOFSYNC: Process status is {CurrentStatus.ToString()} when LISTEN was expected.");
                return;
            }
            UI_WaitForTablet.SetActive(false);
            UI_AskForConnection.SetActive(true);

            CurrentStatus = SyncStatus.WAIT_ACCEPT;
        }
        else if (fileUpload.GetName().Equals("waysForExport.xml"))
        {
            if (CurrentStatus != SyncStatus.ACCEPT)
            {
                LogError($"ENDOFSYNC: Process status is {CurrentStatus.ToString()} when ACCEPT was expected.");
                return;
            }
            // do nothing here
            CurrentStatus = SyncStatus.WAIT_ERW_SELECT;
        }
        else if (fileUpload.GetName().StartsWith("REQUEST-ERW-"))
        {
            if (CurrentStatus != SyncStatus.WAIT_ERW_SELECT)
            {
                LogError($"ENDOFSYNC: Process status is {CurrentStatus.ToString()} when WAIT_ERW_SELECT was expected.");
                return;
            }

            CurrentStatus = SyncStatus.ERW_PREPARE;

            string msg = fileUpload.GetName().Replace("REQUEST-ERW-", "");
            PrepareERW(Int32.Parse(msg), msg);            
        }
        else if (fileUpload.GetName().EndsWith("-manifest.xml"))
        {
            if (CurrentStatus != SyncStatus.ERW_READY)
            {
                LogError($"ENDOFSYNC: Process status is {CurrentStatus.ToString()} when ERW_READY was expected.");
                return;
            }
            // do nothing here
            CurrentStatus = SyncStatus.DOWNLOAD;
        }
        else if (fileUpload.GetName().Equals("ENDOFSYNC"))
        {
            if (CurrentStatus == SyncStatus.LISTEN ||
                CurrentStatus == SyncStatus.CANCEL ||
                CurrentStatus == SyncStatus.FINISH)
            {
                LogError($"ENDOFSYNC: Process status is {CurrentStatus.ToString()}, and can only be  terminated with a valid ongoing connetion.");
                return;
            }

            UI_WaitForSyncEnd.SetActive(false);
            UI_SyncFinshed.SetActive(true);

            CurrentStatus = SyncStatus.FINISH;
        }
        else
        {
            TransferedCountOfFiles++;
            Debug.Log("UpdateProgressbar: CountOfFiles: " + CountOfFiles + " TransferedCountOfFiles: " + TransferedCountOfFiles);

            if (TransferedCountOfFiles >= CountOfFiles)
            {
                UI_WaitForSyncEnd.SetActive(false);
                UI_SyncFinshed.SetActive(true);
            }
            else
            {
                float progress = (float)TransferedCountOfFiles / (float)CountOfFiles;
                Progressbar.SetProgressbar(progress);
                Debug.Log("UpdateProgressbar: %: " + progress);
            }
        }
    }

    /// <summary>
    /// FTS event listener for file downloading (On File Download ()). This is
    /// the callback that informs us that a requested file has
    /// been completely downloaded, or a protocol message has been served.
    /// </summary>
    /// <param name="file">FileRequest representing the downloaded file</param>
    public void OnFileDownload(FileRequest fileRequest)
    {
        Debug.Log("OnFileDownload:fileRequest: " + fileRequest._sourceName);

        if (fileRequest._sourceName.Equals("CONNECTIONALLOWED"))
        {
            UI_WaitForConnectionWithTablet.SetActive(false);
            UI_WaitForSyncEnd.SetActive(true);

            CurrentStatus = SyncStatus.ACCEPT;

        } else if (fileRequest._sourceName.StartsWith("REQUEST-ERW-READY"))
        {
            // do nothing
            CurrentStatus = SyncStatus.ERW_READY;
        }
    }

    /// <summary>
    /// FTS event listener for file not downloading (On File Not Upload ()). 
    /// </summary>
    /// <param name="file">FileRequest representing the file requested for download</param>
    public void OnFileNotFoundDownload(FileRequest fileRequest)
    {
        Debug.Log("OnFileNotFoundUpload:fileRequest: " + fileRequest._sourceName);
    }

    /// <summary>
    /// FTS event listener for the begin of file uploading (On Begin File Upload ()). We perform
    /// basic access validations here.
    /// </summary>
    /// <param name="fileUpload">FileUpload representing incoming request. </param>
    public void OnBeginFileUpload(FileUpload fileUpload)
    {
        Debug.Log("OnBeginFileUpload:fileUpload: " + fileUpload.GetName());

        // Someone else is trying to get the file?
        if (!fileUpload.IsThis(serverDevice.ip, fileUpload.GetName()))
        {
            Debug.LogError($"Fatal security risk: Another device in [{string.Join("--", fileTransferServer.GetDeviceNamesList())}] is attempting to retrieve files.");
            // Terminate the process
            fileTransferServer.Disconnect();
        }
    }


    /**********************************
     *  Protocol (outgoing) Requests  *
     **********************************/

    /// <summary>
    /// Informs the connected device that the process has been cancelled
    /// so that it can terminate it with the proper status.
    /// </summary>
    private void RequestCancelProcess()
    {
        LogProcess("CancelSynchronisation: Requesting ENDOFSYNC");
        RequestFile("ENDOFSYNC");

        CurrentStatus = SyncStatus.CANCEL;
    }

    /// <summary>
    /// Informs the server that the connection requests has been accepted
    /// </summary>
    private void RequestConnectionAllowed()
    {
        LogProcess("Requesting CONNECTIONALLOWED");
        RequestFile("CONNECTIONALLOWED");
    }

    /// <summary>
    /// Informs the server that the ERW preparation is done,
    /// and the ERW ready for the synchronisation process
    /// </summary>
    private void RequestErwReady()
    {
        LogProcess("Requesting REQUEST-ERW-READY");
        RequestFile("REQUEST-ERW-READY");
    }

    /// <summary>
    /// Requests the specified file from the currently connected device.
    /// </summary>
    /// <param name="fileName">Name of the file to request.</param>
    private void RequestFile(string fileName)
    {
        fileTransferServer.RequestFile(serverDevice.ip, fileName);
    }


    /// <summary>
    ///  Checks whether the remote device is reponsive (connected). This is based
    ///  on delayed polling requests, as FTS does not have a primitive for
    ///  checking the status of a remote device.
    /// </summary>
    private IEnumerator CheckDeviceConnection()
    {
        while (isCurrentDeviceConnected || connectionAttemps < 3)
        {
            string deviceIp = serverDevice.ip;


            // We switch off the connected flag
            isCurrentDeviceConnected = false;
            connectionAttemps++;

            // We poll the connected device
            // If alive, it should set the deviceConnected to = true

            fileTransferServer.SendPollRequest(deviceIp);
            lastHeartbeat = System.DateTime.Now;

            
            LogProcess($"Pooling request sent to device {serverDevice.name} (attempt {connectionAttemps}). ");

            yield return new WaitForSeconds(10f);
        }


        LogProcess($"Device {serverDevice.name} is disconnected.");

        UI_WaitForSyncEnd.SetActive(false);
        UI_SyncError.SetActive(true);
        ResetOrDisposeProcessProtocol();
    }



    /* Log functions */

    private void Log(string text)
    {
        Debug.Log(text);
    }

    private void LogError(string text)
    {
        Debug.Log(text);
    }

    private void LogProcess(string text)
    {
        string entry = $"[{System.DateTime.Now.ToString("HH:mm:ss")}] Status: {CurrentStatus.ToString()} Remote Device: {serverDevice.name} == {text}";
        Debug.Log(entry);
    }

    public class DetailedWayExport
    {
        public string Folder { get; set; }
        public List<DetailedWayExportFiles> Files { get; set; }
        public string Points { get; set; }
        public int Id { set; get; }
        public string Start { set; get; }
        public string Destination { set; get; }
        public string StartType { set; get; }
        public string DestinationType { set; get; }
        public string Name { set; get; }
        public string Description { set; get; }

        public System.DateTime RecordingDate { set; get; }
        public string RecordingName { set; get; }

        public int Status { set; get; }
    }

    public class DetailedWayExportFiles
    {
        public string File { get; set; }
        public byte[] Checksum { get; set; }


        // write down to xml file in shared folder
        public static void SerializeAsXML(List<DetailedWayExport> list, string fileName, string folder)
        {
            var objType = list.GetType();

            try
            {
                using (var xmlwriter = new XmlTextWriter(FileManagement.persistentDataPath + "/" + folder + "/" + fileName, Encoding.UTF8))
                {
                    xmlwriter.Indentation = 2;
                    xmlwriter.IndentChar = ' ';
                    xmlwriter.Formatting = Formatting.Indented;
                    var xmlSerializer = new XmlSerializer(objType);
                    xmlSerializer.Serialize(xmlwriter, list);
                }
            }
            catch (System.IO.IOException ex)
            {
                ErrorHandlerSingleton.GetErrorHandler().AddNewError("Could not write to file!", ex.Message, true, false);
            }
        }

        // Insert logic for processing found files here.
        public static List<DetailedWayExportFiles> PrepareFileForExport(string file, string destFolder)
        {

            double ChunkSize = 50 * 1024 * 1024;
            // Get the file size
            long fileSize = new FileInfo(file).Length;

            if (fileSize > ChunkSize)
            {
                return SplitFileForExport(file, destFolder, ChunkSize);
            }

            DetailedWayExportFiles dwef = new DetailedWayExportFiles();

            dwef.File = file;
            dwef.Checksum = ComputeChecksum(file);

            //exportFilesForWay.Add(dwef);
            //CountOfFiles++;
            //currentFolderForCopies

            System.IO.File.Copy(file, destFolder + "/" + new FileInfo(file).Name);

            return new List<DetailedWayExportFiles> { dwef };
        }

        public static List<DetailedWayExportFiles> SplitFileForExport(string file, string destFolder, double ChunkSize)
        {
            List < DetailedWayExportFiles >  dweList = new List<DetailedWayExportFiles>();

            long fileSize = new FileInfo(file).Length;

            // Calculate the number of chunks to split the file into
            int chunkCount = (int)Math.Ceiling(fileSize / (double)ChunkSize);

            // Create a buffer for reading chunks
            byte[] buffer = new byte[(int)ChunkSize];

            // Read the file in chunks and write them to the destination folder
            using (FileStream fs = new FileStream(file, FileMode.Open))
            {
                for (int i = 0; i < chunkCount; i++)
                {
                    // Calculate the chunk size
                    int chunkSize = (int)Math.Min(ChunkSize, fileSize - (i * ChunkSize));

                    if (chunkSize < ChunkSize)
                    {
                        buffer = new byte[(int)chunkSize];
                    }

                    // Read the chunk into the buffer
                    fs.Read(buffer, 0, chunkSize);

                    // Write the chunk to the destination folder
                    string chunkFileName = $"{new FileInfo(file).Name}.part{i + 1}-{chunkCount}.chunk";
                    System.IO.File.WriteAllBytes(destFolder + "/" + chunkFileName, buffer);

                    // Update the DetailedWayExportFiles object
                    DetailedWayExportFiles dwef = new DetailedWayExportFiles();
                    dwef.File = destFolder + "/" + chunkFileName;
                    dwef.Checksum = ComputeChecksum(dwef.File);
                    dweList.Add(dwef);
                }
            }

            return dweList;
        }

        public static byte[] ComputeChecksum(string file)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = System.IO.File.OpenRead(file))
                {
                    return md5.ComputeHash(stream);
                }
            }
        }


    }
}
