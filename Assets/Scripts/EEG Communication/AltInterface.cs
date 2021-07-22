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


public class AltWrapper : IHostPC
{


    // InterfaceManager manager;

    public AltWrapper()
    {
        // manager = _manager; 
        //  manager.ramulator.manager = manager;
        Start();
        Do(new EventBase(Connect));
    }

    public override void Connect()
    {
        // CoroutineToEvent.StartCoroutine(manager.ramulator.BeginNewSession((int)manager.GetSetting("session")), manager);
        UnityEngine.Debug.Log("TODO: CONNECT ");
    }

    public override void SendMessage(string type, Dictionary<string, object> data)
    {
        DataPoint point = new DataPoint(type, System.DateTime.UtcNow, data);
        string message = point.ToJSON();

        UnityEngine.Debug.Log("TODO: send message");
        //manager.Do(new EventBase(() => manager.ramulator.SendMessageToRamulator(message)));
    }

    public override void HandleMessage(string message, DateTime time)
    {
        throw new NotImplementedException("Ramulator does not expect responses to be handled externally");
    }
    public override JObject WaitForMessage(string type, int timeout)
    {
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
    const float timeoutDelay = 1f;
    const int unreceivedHeartbeatsToQuit = 8;


    private int heartbeatCount = 0;
    int messageTimeout = 1000;
    int heartbeatTimeout = 8000; // TODO: configuration
    private int unreceivedHeartbeats = 0;


    private bool waitingForMessage = false;
    private bool waitingForHeartbeat = false;
    private bool performingLatencyCheck = true;

    public bool Connected;
    PairSocket server;
    private void ListenerWork()
    {
        AsyncIO.ForceDotNet.Force();

        using (server = new PairSocket())
        {
            UnityEngine.Debug.Log("binding");
            server.Connect("tcp://192.168.0.8:5555");

            UnityEngine.Debug.Log("binded");
            Connected = true;
            while (!_listenerCancelled)
            {

                //check message
                //  SendMessages();

                //Receive messages
                //  ReceiveMessages();

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
            UnityEngine.Debug.Log("received message is " + message);
        }
        // _contactWatch.Restart();
    }

    private void SendMessages()
    {

        if (messageQueue.Count > 0)
        {
            string msg = messageQueue.Dequeue();
            server.SendFrame(msg);
            UnityEngine.Debug.Log("sent message " + msg);
        }
    }

    public void SendMessage(string messageToSend)
    {
        //UnityEngine.Debug.Log("added message to queue");
        messageQueue.Enqueue(messageToSend);
        //UnityEngine.Debug.Log("message queue " + messageQueue.Count.ToString());
    }



    public bool CheckIfWaitingForHeartbeat()
    {
        return waitingForHeartbeat;
    }

    public bool CheckIfWaitingForMessage()
    {
        return waitingForMessage;
    }

    public int ReturnHeartbeatCount()
    {
        return heartbeatCount;
    }

    public void IncrementHeartbeatCount()
    {
        heartbeatCount++;
    }

    public void WaitForHeartbeat()
    {
        waitingForHeartbeat = true;
        string receivedMessage = "";
        float startTime = 0f;
        string targetStr = "HEARTBEAT_OK";
        //  while (receivedMessage == null || !receivedMessage.Contains(targetStr))
        //  {
        startTime += Time.deltaTime;
        server.TryReceiveFrameString(out receivedMessage);
        if (receivedMessage != "" && receivedMessage != null)
        {
            string messageString = receivedMessage.ToString();
            // UnityEngine.Debug.Log("received: " + messageString);

            waitingForHeartbeat = false;
            //  UnityEngine.Debug.Log("TODO: Implement report back to mono thread");
            ReportMessage(messageString);

            //reset the unreceived heartbeats counter if we receive a response message from the host
            unreceivedHeartbeats = 0;
            //ReportMessage(messageString, false);
        }

        //if we have exceeded the timeout time, show warning and stop trying to connect
        //   UnityEngine.Debug.Log("TIME " + startTime.ToString());
        if (startTime > timeoutDelay)
        {
            unreceivedHeartbeats++;
            string errorString = "MISSED HEARTBEAT COUNT " + unreceivedHeartbeats.ToString();
            waitingForHeartbeat = false;
            ReportMessage(errorString);
            // waitingForMessage = false;
            return;
            //    }
            //return;
        }
    }

    public void WaitForMessage(string containingString, string errorMessage)
    {
        waitingForMessage = true;
        string receivedMessage = "";
        float startTime = 0f;
        // while (receivedMessage == null || !receivedMessage.Contains(containingString))
        //{
        startTime += Time.deltaTime;
        server.TryReceiveFrameString(out receivedMessage);


        if (receivedMessage != "" && receivedMessage != null)
        {
            JObject json = JObject.Parse(receivedMessage);
            string type = json.GetValue("type").Value<string>();
            UnityEngine.Debug.Log("receiving " + type + " when needing "  + containingString);
            if (type.Contains(containingString))
            {
                ReportMessage("RETURNED CORRECT MESSAGE");
                waitingForMessage = false;
            }
            else
            {
                ReportMessage("Got incorrect message");
            }
            //  UnityEngine.Debug.Log("received: " + messageString);

          //  waitingForMessage = false;
            //  UnityEngine.Debug.Log("TODO: Implement report back to mono thread");
            ReportMessage("received " + receivedMessage);
            //ReportMessage(messageString, false);
        }

        //if we have exceeded the timeout time, show warning and stop trying to connect
        if (startTime > timeoutDelay)
        {
            ReportMessage("BREAKING due to timeout");
            return;
            //    break;
        }
        return;
        //  }
    }

    private void ReportMessage(string msg)
    {
        AltInterface.PrintReport(msg);
    }

    public bool DidExceedHeartbeatMissedThreshold()
    {

        if (unreceivedHeartbeats > unreceivedHeartbeatsToQuit)
            return true;
        else
            return false;
    }




    public void SendMessageToRamulator(string message)
    {
        bool wouldNotHaveBlocked = server.TrySendFrame(message, more: false);
        //UnityEngine.Debug.Log("Tried to send a message: " + message + " \nWouldNotHaveBlocked: " + wouldNotHaveBlocked.ToString());
        //UnityEngine.Debug.Log("TODO: Implement report back to mono thread");
        ReportMessage(message);
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


public class AltInterface : MonoBehaviour
{

    private NetMQ.Sockets.PairSocket zmqSocket;
    private const string address = "tcp://*:8889";

    private ElememWorker elememWorker;

    //public InterfaceManager manager;

    private void Start()
    {

        elememWorker = new ElememWorker();
        StartThread();
        InitiateConnectionForSession(1);
    }

    void OnApplicationQuit()
    {
        if (zmqSocket != null)
        {
            zmqSocket.Close();
            NetMQConfig.Cleanup();
        }
    }

    public void StartThread()
    {
        elememWorker.Start();

    }

    public void InitiateConnectionForSession(int sessNum)
    {

        StartCoroutine("BeginNewSession", sessNum);
    }


    void Update()
    {
        /*
        if (Input.GetKeyDown(KeyCode.C))
        {
            StartThread();
        }
        if (Input.GetKeyDown(KeyCode.B))
        {
            StartCoroutine("BeginNewSession", 0);
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            StartThread();
        }

        if(Input.GetKeyDown(KeyCode.N))
        {
         //   StartThread();
            InitiateConnectionForSession(0);
            /*
            UnityEngine.Debug.Log("sending connected message");
            DataPoint connected = new DataPoint("CONNECTED", HighResolutionDateTime.UtcNow, new Dictionary<string, object>());
            elememWorker.SendMessageToRamulator(connected.ToJSON());
           
        }
    */

    }

    public static void PrintReport(string message)
    {
        UnityEngine.Debug.Log("REPORT: " + message);
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

        /*
        elememWorker.WaitForMessage("CONNECTED", "Ramulated not connected.");

        while(elememWorker.CheckIfWaiting())
        {
            yield return 0;
        }
        */

        //  StartThread();
        yield return new WaitForSeconds(1f);
        while (!elememWorker.Connected)
        {
           // UnityEngine.Debug.Log("waiting for Elemem Server to bind");
            yield return 0;
        }

        UnityEngine.Debug.Log("sending connected message");
        //send connected message to host
        DataPoint connected = new DataPoint("CONNECTED", HighResolutionDateTime.UtcNow, new Dictionary<string, object>());
        elememWorker.SendMessageToRamulator(connected.ToJSON());

        //wait for confirmation
        UnityEngine.Debug.Log("waiting for CONNECTED_OK");
        elememWorker.WaitForMessage("CONNECTED_OK", "Did not receive confirmation");

        float resendTimer = 0f;
        while (elememWorker.CheckIfWaitingForMessage())
        {
            resendTimer += Time.deltaTime;
            if (resendTimer > 1.5f)
            {
                connected = new DataPoint("CONNECTED", HighResolutionDateTime.UtcNow, new Dictionary<string, object>());
                elememWorker.SendMessageToRamulator(connected.ToJSON());
                resendTimer = 0f;
            }


            yield return 0;
        }

        //SendSessionEvent//////////////////////////////////////////////////////////////////////
        /*
        System.Collections.Generic.Dictionary<string, object> sessionData = new Dictionary<string, object>();
        sessionData.Add("name", Experiment.ExpName);
        sessionData.Add("version", Application.version);
        sessionData.Add("subject", "test_subj");
        //sessionData.Add("subject", Experiment.currentSubject.name);
        sessionData.Add("session_number", Experiment.sessionID);
        DataPoint sessionDataPoint = new DataPoint("SESSION", System.DateTime.UtcNow, sessionData);
        elememWorker.SendMessageToRamulator(sessionDataPoint.ToJSON());
        */

        //send configure message
        System.Collections.Generic.Dictionary<string, object> configureData = new Dictionary<string, object>();
        configureData.Add("stim_mode", Configuration.stimMode.ToString());
        configureData.Add("experiment", Experiment.ExpName);
        configureData.Add("subject", "test_subj");
        DataPoint configureDataPoint = new DataPoint("CONFIGURE", HighResolutionDateTime.UtcNow, configureData);
        elememWorker.SendMessageToRamulator(configureDataPoint.ToJSON());
   

        //wait for confirmation
        elememWorker.WaitForMessage("CONFIGURE_OK", "Did not receive confirmation");

        resendTimer = 0f;
        UnityEngine.Debug.Log("waiting for CONFIGURE_OK");
        while (elememWorker.CheckIfWaitingForMessage())
        {
            resendTimer += Time.deltaTime;
            if (resendTimer > 1.5f)
            {
                UnityEngine.Debug.Log("sending another message");
                configureDataPoint = new DataPoint("CONFIGURE", HighResolutionDateTime.UtcNow, configureData);
                elememWorker.SendMessageToRamulator(configureDataPoint.ToJSON());
                resendTimer = 0f;
            }
            yield return 0;
        }

        //wait while doing latency check
        UnityEngine.Debug.Log("performing latency check");
        yield return StartCoroutine(PerformLatencyCheck());



        //Begin Heartbeats///////////////////////////////////////////////////////////////////////
      //  InvokeRepeating("SendHeartbeat", 0, 1);


        UnityEngine.Debug.Log("sending READY");
        //SendReadyEvent////////////////////////////////////////////////////////////////////
        DataPoint ready = new DataPoint("READY", HighResolutionDateTime.UtcNow, new Dictionary<string, object>());
        elememWorker.SendMessageToRamulator(ready.ToJSON());
        yield return null;


        UnityEngine.Debug.Log("waiting for START");
        elememWorker.WaitForMessage("START", "Start signal not received");
        while (elememWorker.CheckIfWaitingForMessage())
        {
            yield return 0;
        }


        //   UnityEngine.Debug.Log("TODO: Implement receiving heartbeats (?)");
        // InvokeRepeating("ReceiveHeartbeat", 0, 1);


    }

    private IEnumerator PerformLatencyCheck()
    {

        //throw exception if latency is unacceptable
        Stopwatch sw = new Stopwatch();
        float[] delay = new float[20];

        for (int i = 0; i < 20; i++)
        {
            UnityEngine.Debug.Log("sending latency heartbeat " + i.ToString());
            sw.Restart();
            yield return StartCoroutine(Heartbeat());
            sw.Stop();

            delay[i] = sw.ElapsedTicks * (1000f / Stopwatch.Frequency);
            if (delay[i] > 20)
            {
                UnityEngine.Debug.Log("break");
                break;
            }
            yield return new WaitForSeconds(0.05f - delay[i]);
            //Thread.Sleep(50 - (int)delay[i]);
        }

        float max = delay.Max();
        float mean = delay.Sum() / delay.Length;
        float acc = (1000L * 1000L * 1000L) / Stopwatch.Frequency;

        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("max_latency", max);
        dict.Add("mean_latency", mean);
        dict.Add("accuracy", acc);

        UnityEngine.Debug.Log("LATENCY MAX " + max.ToString());
#if !UNITY_EDITOR
        if (max > 20f)
        {
            UnityEngine.Debug.Log("exceeded max latency threshold");
            yield return null;
        }
#endif
        UnityEngine.Debug.Log("passed latency check");

        yield return null;
    }



    private IEnumerator Heartbeat()
    {
        // UnityEngine.Debug.Log("sending heartbeat");
        var data = new Dictionary<string, object>();
        data.Add("count", elememWorker.ReturnHeartbeatCount());
        elememWorker.IncrementHeartbeatCount();
        DataPoint heartbeatDatapoint = new DataPoint("HEARTBEAT", HighResolutionDateTime.UtcNow, data);
        elememWorker.SendMessageToRamulator(heartbeatDatapoint.ToJSON());

        //UnityEngine.Debug.Log("waiting for heartbeat");
        elememWorker.WaitForHeartbeat();
        //elememWorker.WaitForMessage("HEARTBEAT_OK","Did not receive heartbeat confirmation");

        while (elememWorker.CheckIfWaitingForHeartbeat())
        {
            yield return 0;
        }

        // UnityEngine.Debug.Log("finished waiting for heartbeat");

        yield return null;
    }

    //ramulator expects this before the beginning of a new list
    public void BeginNewTrial(int trialNumber)
    {
        if (zmqSocket == null)
            throw new Exception("Please begin a session before beginning trials");
        System.Collections.Generic.Dictionary<string, object> sessionData = new Dictionary<string, object>();
        sessionData.Add("trial", trialNumber.ToString());
        DataPoint sessionDataPoint = new DataPoint("TRIAL", HighResolutionDateTime.UtcNow, sessionData);
        elememWorker.SendMessageToRamulator(sessionDataPoint.ToJSON());
    }

    //ramulator expects this when you display words to the subject.
    //for words, stateName is "WORD"
    public void SetState(string stateName, bool stateToggle, System.Collections.Generic.Dictionary<string, object> sessionData)
    {
        sessionData.Add("name", stateName);
        sessionData.Add("value", stateToggle.ToString());
        DataPoint sessionDataPoint = new DataPoint("STATE", HighResolutionDateTime.UtcNow, sessionData);
        elememWorker.SendMessageToRamulator(sessionDataPoint.ToJSON());
    }

    public void SendMathMessage(string problem, string response, int responseTimeMs, bool correct)
    {
        Dictionary<string, object> mathData = new Dictionary<string, object>();
        mathData.Add("problem", problem);
        mathData.Add("response", response);
        mathData.Add("response_time_ms", responseTimeMs.ToString());
        mathData.Add("correct", correct.ToString());
        DataPoint mathDataPoint = new DataPoint("MATH", HighResolutionDateTime.UtcNow, mathData);
        elememWorker.SendMessageToRamulator(mathDataPoint.ToJSON());
    }

    //checks to see if the missed heartbeats have crossed the threshold which should result in application quitting with an error message
    private void PerformHeartbeatCheck()
    {

        if (elememWorker.DidExceedHeartbeatMissedThreshold())
        {
            UnityEngine.Debug.Log("QUITTING DUE TO EXCEEDING MISSED HEARTBEATS");
            Application.Quit();
        }

    }


    private void SendHeartbeat()
    {
        //performing heartbeat check
        PerformHeartbeatCheck();

        StartCoroutine("Heartbeat");
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