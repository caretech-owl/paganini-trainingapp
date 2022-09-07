using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

/// <summary>
/// This class is responsible for handling REST API requests to remote server.
/// To extend this class you just need to add new API methods.
/// </summary>
public class ServerCommunication : PersistentLazySingleton<ServerCommunication>
{
    #region [Request Type]
    public enum Requesttype
    {
        GET, POST, DELETE
    }
    public struct Header
    {
        public string name;
        public string value;
    }
    #endregion

    #region [Server Communication]

    /// <summary>
    /// This method is used to begin sending request process.
    /// </summary>
    /// <param name="url">API url.</param>
    /// <param name="callbackOnSuccess">Callback on success.</param>
    /// <param name="callbackOnFail">Callback on fail.</param>
    /// <param name="type">Type of Request to Send</param>
    /// <param name="payload">Payload to send with Post Request</param>
    /// <typeparam name="T">Data Model Type.</typeparam>
    private void SendRequest<T>(string url, UnityAction<T> callbackOnSuccess, UnityAction<string> callbackOnFail, Header[] header, Requesttype type = Requesttype.GET, string payload = "")
    {
        StartCoroutine(RequestCoroutine(url, callbackOnSuccess, callbackOnFail, header, type, payload));
    }

    /// <summary>
    /// Coroutine that handles communication with REST server.
    /// </summary>
    /// <returns>The coroutine.</returns>
    /// <param name="url">API url.</param>
    /// <param name="callbackOnSuccess">Callback on success.</param>
    /// <param name="callbackOnFail">Callback on fail.</param>
    /// <param name="type">Type of Request to Send</param>
    /// <param name="payload">Payload to send with Post Request</param>
    /// <typeparam name="T">Data Model Type.</typeparam>
    private IEnumerator RequestCoroutine<T>(string url, UnityAction<T> callbackOnSuccess, UnityAction<string> callbackOnFail, Header[] header, Requesttype type, string payload)
    {
        UnityWebRequest www;
        switch (type)
        {
            case Requesttype.GET:
                www = UnityWebRequest.Get(url);
                break;
            case Requesttype.POST:
                www = UnityWebRequest.Post(url, payload);
                break;
            case Requesttype.DELETE:
                www = UnityWebRequest.Delete(url);
                break;
            default:
                www = UnityWebRequest.Get(url);
                break;
        }
        //certificat workaround
        www.certificateHandler = new ForceAcceptAll();

        //set http header
        foreach (Header h in header)
        {
            www.SetRequestHeader(h.name, h.value);
        }

        yield return www.SendWebRequest();

        if (www.isNetworkError)
        {
            Debug.LogError(www.error);
            callbackOnFail?.Invoke("NetworkError");
        }
        else if (www.isHttpError && www.responseCode >= 500)
        {
            Debug.LogError(www.error);
            callbackOnFail?.Invoke("ServerError");
        }
        else if (www.isHttpError && www.responseCode == 401)
        {
            Debug.LogError(www.error);
            callbackOnFail?.Invoke("Unauthorised");
        }
        else if (www.isHttpError)
        {
            Debug.LogError(www.error);
            callbackOnFail?.Invoke(www.error);
        }
        else
        {
            ParseResponse(www.downloadHandler.text, callbackOnSuccess, callbackOnFail);
        }
    }
    //workaround bis certifikat trusted
    public class ForceAcceptAll : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }
    /// <summary>
    /// This method finishes request process as we have received answer from server.
    /// </summary>
    /// <param name="data">Data received from server in JSON format.</param>
    /// <param name="callbackOnSuccess">Callback on success.</param>
    /// <param name="callbackOnFail">Callback on fail.</param>
    /// <typeparam name="T">Data Model Type.</typeparam>
    private void ParseResponse<T>(string data, UnityAction<T> callbackOnSuccess, UnityAction<string> callbackOnFail)
    {
        var parsedData = JsonUtility.FromJson<T>(data);
        callbackOnSuccess?.Invoke(parsedData);
    }

    #endregion

    #region [API]

    /// <summary>
    /// This method call server API to get the userProfile
    /// </summary>
    /// <param name="callbackOnSuccess">Callback on success.</param>
    /// <param name="callbackOnFail">Callback on fail.</param>
    public void GetUserProfile(UnityAction<UserAPI> callbackOnSuccess, UnityAction<string> callbackOnFail, string apitoken)
    {
        Header[] header = new Header[1];
        header[0].name = "apitoken";
        header[0].value = apitoken;
        SendRequest(PaganiniRestAPI.getUserProfile, callbackOnSuccess, callbackOnFail, header);
    }

    /// <summary>
    /// This method call server API to login via PIN 
    /// </summary>
    /// <param name="callbackOnSuccess">Callback on success.</param>
    /// <param name="callbackOnFail">Callback on fail.</param>
    public void GetUserAuthentification(UnityAction<APIToken> callbackOnSuccess, UnityAction<string> callbackOnFail, int pin)
    {
        Header[] header = new Header[1];
        header[0].name = "pin";
        header[0].value = pin.ToString();
        SendRequest(PaganiniRestAPI.getAuthToken, callbackOnSuccess, callbackOnFail, header);
    }

    /// <summary>
    /// This method call server API to get the wege for the local user
    /// </summary>
    /// <param name="callbackOnSuccess">Callback on success.</param>
    /// <param name="callbackOnFail">Callback on fail.</param>
    public void GetUserWege(UnityAction<WegAPIList> callbackOnSuccess, UnityAction<string> callbackOnFail, string apitoken)
    {
        Header[] header = new Header[1];
        header[0].name = "apitoken";
        header[0].value = apitoken;
        Debug.Log(PaganiniRestAPI.getUserWege + " apitoken: " + apitoken);
        SendRequest(PaganiniRestAPI.getUserWege, callbackOnSuccess, callbackOnFail, header);
    }

    /// <summary>
    /// This method call server API to get the begehungen for a specific weg 
    /// </summary>
    /// <param name="callbackOnSuccess">Callback on success.</param>
    /// <param name="callbackOnFail">Callback on fail.</param>
    public void GetUserBegehungen(UnityAction<ExploratoryRouteWalkAPIList> callbackOnSuccess, UnityAction<string> callbackOnFail, string apitoken, int wegId)
    {
        Header[] header = new Header[1];
        header[0].name = "apitoken";
        header[0].value = apitoken;
        SendRequest(PaganiniRestAPI.GetUserBegehungen(wegId), callbackOnSuccess, callbackOnFail, header);
    }



    #endregion
}
