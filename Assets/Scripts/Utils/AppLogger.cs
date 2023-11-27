using System;
using System.IO;
using System.Linq;
using UnityEngine;

public class AppLogger : PersistentLazySingleton<AppLogger>
{
    private string logFolderPath;
    private StreamWriter logWriter = null;
    private int minStringName = 15;

    public AppLogger() { }

    private void InitialiseLog()
    {
        // Create a log folder within the persistent data path
        logFolderPath = Path.Combine(Application.persistentDataPath, "Logs");
        Directory.CreateDirectory(logFolderPath);

        // Keep only the last 10 log sessions
        CleanUpOldLogs();

        // Generate a unique log file name using a timestamp
        string logFileName = "session_" + DateTime.Now.ToString("yyyyMMddHHmmss") + ".log";
        string logFilePath = Path.Combine(logFolderPath, logFileName);

        // Open the log file for writing
        logWriter = File.AppendText(logFilePath);

        // Log the app's version number
        LogMessage("App Version: " + Application.version);
    }

    public void LogError(string message)
    {
        Log("Error: " + message);
    }

    public void LogWarning(string message)
    {
        Log("Warning: " + message);
    }

    public void LogMessage(string message)
    {
        Log("Message: " + message);
    }

    public void LogFromMethod(string className, string methodName, string message)
    {
        Log("[PAGAnInI]" + className.PadRight(minStringName) + "\t" + methodName.PadRight(minStringName) + " \t LOG\t" + message);
    }

    public void ErrorFromMethod(string className, string methodName, string message)
    {
        Log("[PAGAnInI][" + className.PadRight(minStringName) + "\t" + methodName.PadRight(minStringName) + " \t ERROR\t" + message);
    }

    private void Log(string message)
    {
        if (logWriter == null)
        {
            InitialiseLog();
        }

        // Write the message to the log file
        logWriter.WriteLine(DateTime.Now + " " + message);
        logWriter.Flush(); // Ensure the message is written immediately
    }

    new void OnDestroy()
    {
        // Close the log file when the session ends        
        if (logWriter != null)
            logWriter.Close();
        base.OnDestroy();
    }

    private void CleanUpOldLogs()
    {
        string[] logFiles = Directory.GetFiles(logFolderPath, "session_*.log");
        if (logFiles.Length >= 10)
        {
            // Sort the log files by creation date and keep the last 10
            var orderedLogs = logFiles.Select(f => new FileInfo(f))
                .OrderByDescending(f => f.CreationTime)
                .Skip(10);

            // Delete the older log files
            foreach (var file in orderedLogs)
            {
                File.Delete(file.FullName);
            }
        }
    }
}
