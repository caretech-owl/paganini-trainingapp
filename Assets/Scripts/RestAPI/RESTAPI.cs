using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;



public class RESTAPI : PersistentLazySingleton<RESTAPI>
{
    JsonSerializerSettings settings = new JsonSerializerSettings
    {
        Formatting = Formatting.Indented,
        NullValueHandling = NullValueHandling.Ignore
    };


    public void Get<T>(string endpoint, UnityAction<T> successCallback, UnityAction<string> errorCallback, Dictionary<string, string> headers = null)
    {
        UnityWebRequest request = UnityWebRequest.Get(endpoint);
        SetHeaders(request, headers);        

        StartCoroutine(PerformRequest<T>(request, successCallback, errorCallback));
    }

    public void Post<TResult>(string endpoint, BaseAPI resource, UnityAction<TResult> successCallback, UnityAction<string> errorCallback, Dictionary<string, string> headers = null)
    {
        string jsonData = JsonConvert.SerializeObject(resource, settings);
        PostRaw(endpoint, jsonData, successCallback, errorCallback, "application/json", headers);
    }

    public void PostMultipart<TResult>(string endpoint, BaseAPI resource, Dictionary<string, byte[]> files, UnityAction<TResult> successCallback, UnityAction<string> errorCallback, Dictionary<string, string> headers = null)
    {
        string jsonData = JsonConvert.SerializeObject(resource, settings);

        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("data", jsonData));

        foreach (var file in files)
        {
            formData.Add(new MultipartFormFileSection(file.Key, file.Value, file.Key, "image/png"));
        }

        UnityWebRequest request = UnityWebRequest.Post(endpoint, formData);
        request.downloadHandler = new DownloadHandlerBuffer();

        SetHeaders(request, headers);

        StartCoroutine(PerformRequest<TResult>(request, successCallback, errorCallback));
    }

    public void PutMultipart<TResult>(string endpoint, BaseAPI resource, Dictionary<string, byte[]> files, UnityAction<TResult> successCallback, UnityAction<string> errorCallback, Dictionary<string, string> headers = null)
    {
        string jsonData = JsonConvert.SerializeObject(resource, settings);

        List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
        formData.Add(new MultipartFormDataSection("data", jsonData));

        foreach (var file in files)
        {
            formData.Add(new MultipartFormFileSection(file.Key, file.Value, file.Key, "image/png"));
        }

        byte[] boundary = UnityWebRequest.GenerateBoundary();
        byte[] formSections = UnityWebRequest.SerializeFormSections(formData, boundary);
        byte[] endingBoundary = Encoding.UTF8.GetBytes("--" + Encoding.UTF8.GetString(boundary) + "--");

        byte[] payload = Concatenate(formSections.ToList(), endingBoundary);

        UnityWebRequest request = new UnityWebRequest(endpoint, UnityWebRequest.kHttpVerbPUT);
        request.uploadHandler = new UploadHandlerRaw(payload);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "multipart/form-data; boundary=" + Encoding.UTF8.GetString(boundary));

        SetHeaders(request, headers);

        StartCoroutine(PerformRequest<TResult>(request, successCallback, errorCallback));
    }

    public void Put<TResult>(string endpoint, BaseAPI resource, UnityAction<TResult> successCallback, UnityAction<string> errorCallback, Dictionary<string, string> headers = null)
    {
        string jsonData = JsonConvert.SerializeObject(resource, settings);

        Debug.Log(jsonData);

        UnityWebRequest request = new UnityWebRequest(endpoint, "PUT");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        SetHeaders(request, headers);

        StartCoroutine(PerformRequest<TResult>(request, successCallback, errorCallback));
    }

    public void Delete<T>(string endpoint, UnityAction<T> successCallback, UnityAction<string> errorCallback, Dictionary<string, string> headers = null)
    {
        UnityWebRequest request = UnityWebRequest.Delete(endpoint);
        SetHeaders(request, headers);

        StartCoroutine(PerformRequest<T>(request, successCallback, errorCallback));
    }


    protected IEnumerator PerformRequest<TResult>(UnityWebRequest request, UnityAction<TResult> successCallback, UnityAction<string> errorCallback)
    {
        //certificat workaround
        request.certificateHandler = new ForceAcceptAll();

        yield return request.SendWebRequest();

        try
        {
            if (request.isNetworkError || (request.responseCode != 200 && request.responseCode != 201 && request.responseCode != 204))
            {
                Debug.LogWarning(request.error);
                errorCallback(request.error);
            }
            else
            {
                //TResult responseData = JsonUtility.FromJson<TResult>(request.downloadHandler.text);
                TResult responseData = JsonConvert.DeserializeObject<TResult>(request.downloadHandler.text);
                successCallback(responseData);
            }
        }
        finally
        {
            request.Dispose();
        }
    }



    /* Private functions */

    private void PostRaw<TResult>(string endpoint, string jsonData, UnityAction<TResult> successCallback, UnityAction<string> errorCallback, string contentType, Dictionary<string, string> headers = null)
    {

        Debug.Log(jsonData);

        UnityWebRequest request = new UnityWebRequest(endpoint, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", contentType);
        SetHeaders(request, headers);

        StartCoroutine(PerformRequest<TResult>(request, successCallback, errorCallback));

    }

    private byte[] Concatenate(List<byte> requestData, byte[] boundaryBytes)
    {
        List<byte> resultData = new List<byte>(requestData);
        resultData.AddRange(boundaryBytes);
        return resultData.ToArray();
    }


    private class ForceAcceptAll : CertificateHandler
    {
        protected override bool ValidateCertificate(byte[] certificateData)
        {
            return true;
        }
    }

    private void SetHeaders(UnityWebRequest request, Dictionary<string, string> headers)
    {
        if (headers != null)
        {
            foreach (var header in headers)
            {
                request.SetRequestHeader(header.Key, header.Value);
            }
        }
    }
}
