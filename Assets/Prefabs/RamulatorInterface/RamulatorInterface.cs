using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using NetMQ;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Threading;
using NetMQ.Sockets;
using System.Linq;
using System.Collections.Concurrent;
using System.Net.Sockets;


public class RamulatorWrapper : IHostPC {

    
   // InterfaceManager manager;

    public RamulatorWrapper() {
      // manager = _manager; 
     //  manager.ramulator.manager = manager;
       Start();
       Do(new EventBase(Connect));
    } 

    public override void Connect() {
        // CoroutineToEvent.StartCoroutine(manager.ramulator.BeginNewSession((int)manager.GetSetting("session")), manager);
        UnityEngine.Debug.Log("TODO: CONNECT ");
    }

    public override void SendMessage(string type, Dictionary<string, object> data) {
        DataPoint point = new DataPoint(type,System.DateTime.UtcNow, data);
        string message = point.ToJSON();

        UnityEngine.Debug.Log("TODO: send message");
        //manager.Do(new EventBase(() => manager.ramulator.SendMessageToRamulator(message)));
    }

    public override void HandleMessage(string message, DateTime time) {
        throw new NotImplementedException("Ramulator does not expect responses to be handled externally");
    }
    public override JObject WaitForMessage(string type, int timeout) {
        throw new NotImplementedException("Ramulator does not expect responses to be handled externally");
    }
}



public class ElememWorker
{

    private readonly Thread _listenerWorker;

    private bool _listenerCancelled;

    //public delegate string MessageDelegate(string message);

  //  private readonly MessageDelegate _messageDelegate;

    private readonly Stopwatch _contactWatch;

    private Queue<string> messageQueue;

    private const long ContactThreshold = 1000;


    //variables
    //how long to wait for ramulator to connect
    const int timeoutDelay = 150;
    const int unreceivedHeartbeatsToQuit = 8;

    private int unreceivedHeartbeats = 0;

    private bool waitingForMessage = false;

    public bool Connected;
    PairSocket server;
    private void ListenerWork()
    {
        AsyncIO.ForceDotNet.Force();

        using (server = new PairSocket())
        {
            UnityEngine.Debug.Log("binding");
            server.Bind("tcp://*:5555");

            UnityEngine.Debug.Log("binded");
            while (!_listenerCancelled)
            {
                Connected = _contactWatch.ElapsedMilliseconds < ContactThreshold;

                //check message
                SendMessages();

                //Receive messages
                ReceiveMessages();

                //var response = _messageDelegate(message);

                //send message, if any


            }
        }
        NetMQConfig.Cleanup();
    }

    private void ReceiveMessages()
    {

        string message;
        if (server.TryReceiveFrameString(out message))
        {
           // UnityEngine.Debug.Log("received message is " + message);
        }
        // _contactWatch.Restart();
    }

    private void SendMessages()
    {

        if (messageQueue.Count > 0)
        {
            string msg = messageQueue.Dequeue();
            server.SendFrame(msg);
          //  UnityEngine.Debug.Log("sent message");
        }
    }

    public void SendMessage(string messageToSend)
    {
        //UnityEngine.Debug.Log("added message to queue");
        messageQueue.Enqueue(messageToSend);
        //UnityEngine.Debug.Log("message queue " + messageQueue.Count.ToString());
    }

    public bool CheckIfWaiting()
    {
            return waitingForMessage;
    }

    public void WaitForMessage(string containingString, string errorMessage)
    {
        string receivedMessage = "";
        float startTime = Time.time;
        while (receivedMessage == null || !receivedMessage.Contains(containingString))
        {
            server.TryReceiveFrameString(out receivedMessage);
            if (receivedMessage != "" && receivedMessage != null)
            {
                string messageString = receivedMessage.ToString();
                UnityEngine.Debug.Log("received: " + messageString);

                waitingForMessage = false;
                UnityEngine.Debug.Log("TODO: Implement report back to mono thread");
                //ReportMessage(messageString, false);
            }

            //if we have exceeded the timeout time, show warning and stop trying to connect
            if (Time.time > startTime + timeoutDelay)
            {
                break;
            }
            return;
        }
    }


    private void ReceiveHeartbeat()
    {
        unreceivedHeartbeats = unreceivedHeartbeats + 1;
        UnityEngine.Debug.Log("Unreceived heartbeats: " + unreceivedHeartbeats.ToString());

        if (unreceivedHeartbeats > unreceivedHeartbeatsToQuit)
        {
            UnityEngine.Debug.Log("TODO: Implement the cancelling of repeating function on the mono thread");
          //  CancelInvoke("ReceiveHeartbeat");
         //   CancelInvoke("SendHeartbeat");
            UnityEngine.Debug.Log("TODO: IMPLEMENT MISSED HEARTBEAT");
            // ErrorNotification.Notify(new Exception("Too many missed heartbeats."));
        }

        string receivedMessage = "";
        float startTime = Time.time;
        server.TryReceiveFrameString(out receivedMessage);
        if (receivedMessage != "" && receivedMessage != null)
        {
            string messageString = receivedMessage.ToString();
            UnityEngine.Debug.Log("heartbeat received: " + messageString);
            UnityEngine.Debug.Log("TODO: Implement report back to mono thread");
            // ReportMessage(messageString, false);
            unreceivedHeartbeats = 0;
        }
    }



    public void SendMessageToRamulator(string message)
    {
        bool wouldNotHaveBlocked = server.TrySendFrame(message, more: false);
        UnityEngine.Debug.Log("Tried to send a message: " + message + " \nWouldNotHaveBlocked: " + wouldNotHaveBlocked.ToString());
        UnityEngine.Debug.Log("TODO: Implement report back to mono thread");
        // ReportMessage(message, true);
    }

    public ElememWorker()
    {
        //_messageDelegate = messageDelegate;
        _contactWatch = new Stopwatch();
        _contactWatch.Start();
        _listenerWorker = new Thread(ListenerWork);
    }

    public void Start()
    {
        _listenerCancelled = false;
        _listenerWorker.Start();
        messageQueue = new Queue<string>();
    }

    public void Stop()
    {
        _listenerCancelled = true;
        _listenerWorker.Join();
    }
}


public class RamulatorInterface : MonoBehaviour
{

    private NetMQ.Sockets.PairSocket zmqSocket;
    private const string address = "tcp://*:8889";

    private ElememWorker elememWorker;

    //public InterfaceManager manager;

    private void Start()
    {

        elememWorker = new ElememWorker();
    }

    void OnApplicationQuit()
    {
        if (zmqSocket != null) {
            zmqSocket.Close();
            NetMQConfig.Cleanup();
        }
    }

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            elememWorker.Start();
          //  StartCoroutine("BeginNewSession",0);
        }
        if(Input.GetKeyDown(KeyCode.B))
        {
              StartCoroutine("BeginNewSession",0);
        }

    }
    
        //this coroutine connects to ramulator and communicates how ramulator expects it to
        //in order to start the experiment session.  follow it up with BeginNewTrial and
        //SetState calls
        public IEnumerator BeginNewSession(int sessionNumber)
    {
        //Connect to ramulator///////////////////////////////////////////////////////////////////
       // zmqSocket = new NetMQ.Sockets.PairSocket();
      //  zmqSocket.Bind(address);
      //  UnityEngine.Debug.Log ("socket bound");


        elememWorker.WaitForMessage("CONNECTED", "Ramulated not connected.");

        while(elememWorker.CheckIfWaiting())
        {
            yield return 0;
        }

        //SendSessionEvent//////////////////////////////////////////////////////////////////////
        System.Collections.Generic.Dictionary<string, object> sessionData = new Dictionary<string, object>();
        sessionData.Add("name", Experiment.ExpName);
        sessionData.Add("version", Application.version);
        sessionData.Add("subject", "test_subj");
        //sessionData.Add("subject", Experiment.currentSubject.name);
        sessionData.Add("session_number", Experiment.sessionID);
        DataPoint sessionDataPoint = new DataPoint("SESSION", System.DateTime.UtcNow, sessionData);
        elememWorker.SendMessageToRamulator(sessionDataPoint.ToJSON());
        yield return null;


        //Begin Heartbeats///////////////////////////////////////////////////////////////////////
        InvokeRepeating("SendHeartbeat", 0, 1);


        //SendReadyEvent////////////////////////////////////////////////////////////////////
        DataPoint ready = new DataPoint("READY", System.DateTime.UtcNow, new Dictionary<string, object>());
        elememWorker.SendMessageToRamulator(ready.ToJSON());
        yield return null;


        elememWorker.WaitForMessage("START", "Start signal not received");
        while (elememWorker.CheckIfWaiting())
        {
            yield return 0;
        }



        InvokeRepeating("ReceiveHeartbeat", 0, 1);

    }
    /*
    private IEnumerator WaitForMessage(string containingString, string errorMessage)
    {
        string receivedMessage = "";
        float startTime = Time.time;
        while (receivedMessage == null || !receivedMessage.Contains(containingString))
        {
            zmqSocket.TryReceiveFrameString(out receivedMessage);
            if (receivedMessage != "" && receivedMessage != null)
            {
                string messageString = receivedMessage.ToString();
                UnityEngine.Debug.Log("received: " + messageString);
                ReportMessage(messageString, false);
            }

            //if we have exceeded the timeout time, show warning and stop trying to connect
            if (Time.time > startTime + timeoutDelay)
            {
                yield break;
            }
            yield return null;
        }
    }
    */
    //ramulator expects this before the beginning of a new list
    public void BeginNewTrial(int trialNumber)
    {
        if (zmqSocket == null)
            throw new Exception("Please begin a session before beginning trials");
        System.Collections.Generic.Dictionary<string, object> sessionData = new Dictionary<string, object>();
        sessionData.Add("trial", trialNumber.ToString());
        DataPoint sessionDataPoint = new DataPoint("TRIAL", System.DateTime.UtcNow, sessionData);
        elememWorker.SendMessageToRamulator(sessionDataPoint.ToJSON());
    }

    //ramulator expects this when you display words to the subject.
    //for words, stateName is "WORD"
    public void SetState(string stateName, bool stateToggle, System.Collections.Generic.Dictionary<string, object> sessionData)
    {
        sessionData.Add("name", stateName);
        sessionData.Add("value", stateToggle.ToString());
        DataPoint sessionDataPoint = new DataPoint("STATE", System.DateTime.UtcNow, sessionData);
        elememWorker.SendMessageToRamulator(sessionDataPoint.ToJSON());
    }

    public void SendMathMessage(string problem, string response, int responseTimeMs, bool correct)
    {
        Dictionary<string, object> mathData = new Dictionary<string, object>();
        mathData.Add("problem", problem);
        mathData.Add("response", response);
        mathData.Add("response_time_ms", responseTimeMs.ToString());
        mathData.Add("correct", correct.ToString());
        DataPoint mathDataPoint = new DataPoint("MATH", System.DateTime.UtcNow, mathData);
        elememWorker.SendMessageToRamulator(mathDataPoint.ToJSON());
    }


    private void SendHeartbeat()
    {
        DataPoint sessionDataPoint = new DataPoint("HEARTBEAT", System.DateTime.UtcNow, null);
        elememWorker.SendMessageToRamulator(sessionDataPoint.ToJSON());
    }

    private void ReportMessage(string message, bool sent)
    {
        Dictionary<string, object> messageDataDict = new Dictionary<string, object>();
        messageDataDict.Add("message", message);
        messageDataDict.Add("sent", sent.ToString());
        UnityEngine.Debug.Log("TODO: implement report message");
        //manager.ReportEvent("network", messageDataDict);
    }


    private void OnDestroy()
    {
        elememWorker.Stop();
    }

}