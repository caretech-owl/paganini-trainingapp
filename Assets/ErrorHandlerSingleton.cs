using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

        public void AddNewError(string title, string description, bool showError = true)
        {
            errors.Add(new DetailedError
            {
                Time = DateTime.Now,
                Title = title,
                Description = description
            });

            if (showError)
            {
                errorUI.SetActive(true);
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
