using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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
    public GameObject UI_WaitForTablet;
    public GameObject UI_WaitForConnectionWithTablet;
    public GameObject UI_WaitForSyncEnd;
    public GameObject UI_SyncFinshed;
    public Progressbar Progressbar;

    private List<Way> ways;
    private List<ExploratoryRouteWalk> begehungen;
    private List<DetailedWayExport> wayExports;
    private List<DetailedWayExportFiles> exportFilesForWay;
    private string currentFolderForCopies;
    private int CountOfFiles = 0;
    private int TransferedCountOfFiles = 0;

    // Next update in second
    private int nextUpdate = 1;
    bool isSearchForTablet = true;



    // Start is called before the first frame update
    void Start()
    {
        // fileTransferServer._sharedFolder;
        // We have a user session
        if (AppState.currentUser != null)
        {
            DBConnector.Instance.Startup();
            GetWaysFromLocalDatabase();
            begehungen = DBConnector.Instance.GetConnection().Query<ExploratoryRouteWalk>("Select * FROM ExploratoryRouteWalk where Status =" + ((int)Way.WayStatus.Local));
        }

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

    private void ExportWaysToSharedFolder()
    {
        if (ways != null)
        {
            CountOfFiles = 0;
            wayExports = new List<DetailedWayExport>();

            foreach (var way in ways)
            {
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
                    ProcessDirectoryMP4(FileManagement.persistentDataPath + "/" + detailedWayExport.Folder);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }


                try
                {
                    ProcessDirectoryJPG(FileManagement.persistentDataPath + "/" + detailedWayExport.Folder);
                }
                catch (Exception ex)
                {
                    Debug.LogError(ex.Message);
                }

                detailedWayExport.Files = exportFilesForWay;

                wayExports.Add(detailedWayExport);
            }

            // write down to xml
            SerializeAsXML();

            Debug.Log("CountOfFiles: " + CountOfFiles);
        }
        else
        {
            ErrorHandlerSingleton.GetErrorHandler().AddNewError("ExportWaysToSharedFolder", "No ways available!");
        }
    }

    // Process all files in the directory passed in, recurse on any directories
    // that are found, and process the files they contain.
    private void ProcessDirectoryMP4(string targetDirectory)
    {
        Debug.Log(targetDirectory);
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
        DetailedWayExportFiles dwef = new DetailedWayExportFiles();

        dwef.File = file;
        dwef.Checksum = ComputeChecksum(file);

        //Debug.Log("Processed file:" + file);
        //Debug.Log("validServerList.captionText.text: " + validServerList.captionText.text);
        exportFilesForWay.Add(dwef);

        File.Copy(file, currentFolderForCopies + "/" + new FileInfo(file).Name);

        CountOfFiles++;
    }

    private byte[] ComputeChecksum(string file)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = File.OpenRead(file))
            {
                return md5.ComputeHash(stream);
            }
        }
    }

    // write down to xml file in shared folder
    private void SerializeAsXML()
    {
        var objType = wayExports.GetType();

        try
        {
            using (var xmlwriter = new XmlTextWriter(FileManagement.persistentDataPath + "/" + fileTransferServer._sharedFolder + "/waysForExport.xml", Encoding.UTF8))
            {
                xmlwriter.Indentation = 2;
                xmlwriter.IndentChar = ' ';
                xmlwriter.Formatting = Formatting.Indented;
                var xmlSerializer = new XmlSerializer(objType);
                xmlSerializer.Serialize(xmlwriter, wayExports);
            }
        }
        catch (System.IO.IOException ex)
        {
            ErrorHandlerSingleton.GetErrorHandler().AddNewError("Could nót write to file!", ex.Message, true, false);
        }
    }

    // write down coordinates for specific way to xml file in shared folder
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
                ErrorHandlerSingleton.GetErrorHandler().AddNewError("Could nót write to file!", ex.Message, true, false);
            }

            return filename;
        }
        else
        {
            return String.Empty;
        }

    }

    // Update is called once per frame
    void Update()
    {

        // If the next update is reached
        if (isSearchForTablet && Time.time >= nextUpdate)
        {
            Debug.Log(Time.time + ">=" + nextUpdate);
            // Change the next update (current second+1)
            nextUpdate = Mathf.FloorToInt(Time.time) + 2;
            fileTransferServer.SendPollRequest();
        }
    }

    public void DevicesListUpdate()
    {

        List<string> list = fileTransferServer.GetDeviceNamesList();
        if (list.Count > 0)
        {
            Debug.Log("DevicesListUpdate:" + SmartphoneAskForConnectionText.GetComponent<TMP_Text>().text);
            SmartphoneAskForConnectionText.GetComponent<TMP_Text>().text = SmartphoneAskForConnectionText.GetComponent<TMP_Text>().text.Replace("[TABLETNAME]", list[0]);
            isSearchForTablet = false;
        }
    }

    public void AcceptConnection()
    {
        UI_AskForConnection.SetActive(false);
        UI_WaitForConnectionWithTablet.SetActive(true);

        // create and copy files to synchronize
        ExportWaysToSharedFolder();

        // inform tablet
        fileTransferServer.RequestFile(0, "CONNECTIONALLOWED");
    }

    public void UpdateProgressbar(FileUpload fileUpload)
    {
        Debug.Log("UpdateProgressbar:fileUpload: " + fileUpload.GetName());

        if (fileUpload.GetName().Equals("HANDSHAKE"))
        {
            UI_WaitForTablet.SetActive(false);
            UI_AskForConnection.SetActive(true); 
        }
        else if (fileUpload.GetName().Equals("ENDOFSYNC"))
        {
            UI_WaitForSyncEnd.SetActive(false);
            UI_SyncFinshed.SetActive(true);
        }

        else if (fileUpload.GetName().Equals("waysForExport.xml"))
        {
            // do nothing here
        }
        else
        {
            TransferedCountOfFiles++;
            Debug.Log("UpdateProgressbar: CountOfFiles: " + CountOfFiles + "TransferedCountOfFiles: " + TransferedCountOfFiles);

            if (TransferedCountOfFiles >= CountOfFiles)
            {
                UI_WaitForSyncEnd.SetActive(false);
                UI_SyncFinshed.SetActive(true);
            }
            else
            {
                Progressbar.SetProgressbar(TransferedCountOfFiles / CountOfFiles);
            }
        }
    }

    public void OnFileDownload(FileRequest fileRequest)
    {
        Debug.Log("OnFileDownload:fileRequest: " + fileRequest._sourceName);

        if (fileRequest._sourceName.Equals("CONNECTIONALLOWED"))
        {
            UI_WaitForConnectionWithTablet.SetActive(false);
            UI_WaitForSyncEnd.SetActive(true);
        }

        //if (fileUpload.GetName().Equals("HANDSHAKE"))
        //{
        //    UI_WaitForTablet.SetActive(false);
        //    UI_AskForConnection.SetActive(true);
        //}
    }

    public void OnFileNotFoundUpload(FileRequest fileRequest)
    {
        Debug.Log("OnFileUpload:fileRequest: " + fileRequest._sourceName);

        //if (fileUpload.GetName().Equals("HANDSHAKE"))
        //{
        //    UI_WaitForTablet.SetActive(false);
        //    UI_AskForConnection.SetActive(true);
        //}
    }

    public void OnBeginFileUpload(FileUpload fileUpload)
    {
        Debug.Log("OnBeginFileUpload:fileUpload: " + fileUpload.GetName());

        //if (fileUpload.GetName().Equals("HANDSHAKE"))
        //{
        //    UI_WaitForTablet.SetActive(false);
        //    UI_AskForConnection.SetActive(true);
        //}
    }


    /// <summary>
    /// Reads back in the Wege List from SQLite and Saves it in the Object
    /// </summary>
    void GetWaysFromLocalDatabase()
    {
        string q = "Select w.* FROM way w join exploratoryroutewalk erw ON" +
            " w.Id = erw.Way_id " +
            " WHERE (w.Status = ?) or (erw.Status = ?)";

        List<Way> wege = DBConnector.Instance.GetConnection().Query<Way>(q, new object[] { (int)Way.WayStatus.Local, (int)Way.WayStatus.Local });
        Debug.Log("Restorewege -> Capacity: " + wege.Count);




        if (wege.Count > 0)
            this.ways = wege;
        else
            this.ways = null;

        //DisplayWege();
    }

    DetailedWayExport FilledOutRecordingReport(Way way)
    {
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
            Debug.LogError("There is no ERW for Way with Id = " + way.Id);
        }


        return detailedWayExport;
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
    }
}
