using Assets;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

public class SyncProcessHandler : MonoBehaviour
{
    public FileTransferServer fileTransferServer;

    private List<Way> ways;
    private List<ExploratoryRouteWalk> begehungen;
    private List<DetailedWayExport> wayExports;
    private List<DetailedWayExportFiles> exportFilesForWay;
    private string currentFolderForCopies;

    // Start is called before the first frame update
    void Start()
    {
        // fileTransferServer._sharedFolder;
        // We have a user session
        if (AppState.currentUser != null)
        {
            DBConnector.Instance.Startup();
            GetWaysFromLocalDatabase();
            begehungen = DBConnector.Instance.GetConnection().Query<ExploratoryRouteWalk>("Select * FROM ExploratoryRouteWalk where Status =" + ((int)Way.WayStatus.Local) );
        }

        ExportWaysToSharedFolder();
    }

    private void ExportWaysToSharedFolder()
    {
        if (ways != null)
        {
            wayExports = new List<DetailedWayExport>();

            foreach (var way in ways)
            {
                DetailedWayExport detailedWayExport = FilledOutRecordingReport(way);

                //// Get GPS coordinates
                //List<Pathpoint> points = DBConnector.Instance.GetConnection().Query<Pathpoint>("SELECT * FROM Pathpoint where beg_id=?", way.Id);

                //if (points != null)
                //{
                //    detailedWayExport.Points = points;
                //}
                //else
                //{
                //    ErrorHandlerSingleton.GetErrorHandler().AddNewError("ExportWaysToSharedFolder", "No exploratory route walk available for way!");
                //}

                // Get files
                detailedWayExport.Folder = way.Name;
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

    // Update is called once per frame
    void Update()
    {
        
    }

    /// <summary>
    /// Reads back in the Wege List from SQLite and Saves it in the Object
    /// </summary>
    void GetWaysFromLocalDatabase()
    {
        string q = "Select w.* FROM way w join exploratoryroutewalk erw ON" +
            " w.Id = erw.Way_id " +
            " WHERE (w.Status = ?) or (erw.Status = ?)";

        List<Way> wege = DBConnector.Instance.GetConnection().Query<Way>(q, new object[] { (int)Way.WayStatus.Local, (int)Way.WayStatus.Local } );
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
        if (erw.Count > 0 )
        {
            detailedWayExport.RecordingName = erw[0].Name;
            detailedWayExport.RecordingDate = erw[0].Date;
        } else
        {
            Debug.LogError("There is no ERW for Way with Id = " + way.Id);
        }


        return detailedWayExport;
    }



    public class DetailedWayExport
    {
        public string Folder { get; set; }
        public List<DetailedWayExportFiles> Files { get; set; }
        public List<Pathpoint> Points { get; set; }
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
