using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using UnityEngine;

namespace Assets
{
    sealed class ErrorHandlerSingleton
    {
        // Static members are 'eagerly initialized', that is, 
        // immediately when class is loaded for the first time.
        // .NET guarantees thread safety for static initialization
        private static readonly ErrorHandlerSingleton instance = new ErrorHandlerSingleton();

        private List<DetailedError> errors;
        private GameObject errorUI;

        private ErrorHandlerSingleton()
        {
            errors = new List<DetailedError>();
        }

        internal DetailedError GetLastError()
        {
            return errors.Last();
        }

        public static ErrorHandlerSingleton GetErrorHandler()
        {
            return instance;
        }

        public void SetupNewErrorUI( GameObject go)
        {
            errorUI = go;
            Debug.Log("SetupNewErrorUI");
        }

        public void AddNewError(string title, string description, bool showError = true, bool writeErrorToFile = true)
        {
            errors.Add(new DetailedError
            {
                Time = DateTime.Now,
                Title = title,
                Description = description
            });

            if (showError && errorUI != null)
            {
                errorUI.SetActive(true);
            }

            if (writeErrorToFile)
            {
                SerializeAsXML();

                // Write to plain text file
                StreamWriter file = new StreamWriter(Application.persistentDataPath + "/DetailedErrors.txt", append: true);
                file.WriteLine(Time.time.ToString() + "\t" + title + "\t" + description);
            }
        }

        private void SerializeAsXML()
        {
            var objType = errors.GetType();

            try
            {
                using (var xmlwriter = new XmlTextWriter(Application.persistentDataPath + "/DetailedErrors.xml", Encoding.UTF8))
                {
                    xmlwriter.Indentation = 2;
                    xmlwriter.IndentChar = ' ';
                    xmlwriter.Formatting = Formatting.Indented;
                    var xmlSerializer = new XmlSerializer(objType);
                    xmlSerializer.Serialize(xmlwriter, errors);
                }
            }
            catch (System.IO.IOException ex)
            {
                AddNewError("Could nót write to file!", ex.Message, true, false);
            }
        }
    }

    public class DetailedError
    {
        public DateTime Time { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }
}
