using System;
using UnityEngine;
using Newtonsoft.Json;
using WebSocketSharp;
using UnityEngine.Events;

public class SocketsAPI : MonoBehaviour
{
    private WebSocket ws;
    private Route CurrentRoute;

    [SerializeField] private string ServerURL = "ws://localhost:8080";
    [SerializeField] private bool EnableLocationSharing = false;


    public UnityEvent OnConnectionSuccessful;

    private void Start()
    {
       
        

        //ConnectToServer();

    }

    public void ConnectToServer()
    {
        CurrentRoute = SessionData.Instance.GetData<Route>("SelectedRoute");

        if (!EnableLocationSharing) return;
        try
        {
            ws = new WebSocket(ServerURL);

            ws.OnMessage += (sender, e) =>
            {
                HandleMessage(e.Data);
            };

            ws.OnOpen += (sender, e) =>
            {
                Debug.Log("Connected to server.");

                var connObj = new
                {
                    type = "connect",
                    userId = AppState.CurrentUser.Id,
                    sessionId = CurrentRoute.Id
                };

                SendMessage(connObj);
                
            };

            ws.ConnectAsync();
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void SendPathpointTrace(PathpointTraceMessage message, string eventType = null)
    {
        if (!EnableLocationSharing) return;

        message.type = "pathpoint";
        message.eventType = eventType;
        SendMessage(message);
    }

    public void SendRouteTrace(PathpointTraceMessage message)
    {
        if (!EnableLocationSharing) return;

        message.type = "route";
        SendMessage(message);
    }

    private void SendMessage(object message)
    {
        if (!EnableLocationSharing) return;

        if (ws == null || !ws.IsAlive)
        {
            Debug.Log("Not connected to server.");
            return;
        }

        try
        {
            string jsonMessage = JsonConvert.SerializeObject(message);
            ws.Send(jsonMessage);
            //Debug.Log("Client sent message: " + jsonMessage);
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }
    }

    public void HandleMessage(string message)
    {
        //try
        //{
        //    dynamic data = JsonConvert.DeserializeObject(message);

        //    if (data.type == "pathpoint")
        //    {
        //        Pathpoint receivedPathpoint = JsonConvert.DeserializeObject<Pathpoint>(data.pathpoint.ToString());
        //        Debug.Log("Received pathpoint: " + receivedPathpoint.Id);
        //    }
        //}
        //catch (Exception e)
        //{
        //    Debug.Log(e.Message);
        //}
    }
}
