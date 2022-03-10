using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
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
using UnityEngine.UI;

#if !UNITY_WEBGL

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
    //PairSocket server;

    TcpClient myClient;
    TcpListener myServer;
    private void ListenerWork()
    {
        AsyncIO.ForceDotNet.Force();
        //using (myClient = new TcpListener(IPAddress.Parse(Configuration.ipAddress), Configuration.portNumber))
        //using (server = new PairSocket())
        //{
            UnityEngine.Debug.Log("binding");
        myServer = new TcpListener(IPAddress.Parse(Configuration.ipAddress), Configuration.portNumber);

            //server.Connect("tcp://"+Configuration.ipAddress+":" + Configuration.portNumber.ToString());

            UnityEngine.Debug.Log("binded");
            Connected = true;
            while (!_listenerCancelled)
            {

                //check message
                  SendMessages();

                //Receive messages
                  ReceiveMessages();

                //var response = _messageDelegate(message);

                //send message, if any


            }
        //}
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

            //server.SendFrame(msg);
            UnityEngine.Debug.Log("sent message " + msg);
        }
    }

    private void TCPSendMessage(string msg)
    {

    }

    public void SendMessage(string messageToSend)
    {
        messageQueue.Enqueue(messageToSend);
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

    public void WaitForHeartbeat(bool isLatencyCheck)
    {
        waitingForHeartbeat = true;
        string receivedMessage = "";
        string targetStr = "HEARTBEAT_OK";
        int timeoutMS = 0;

        if (isLatencyCheck)
            timeoutMS = Configuration.elememHeartbeatTimeoutMS;//20ms timeout
        else
            timeoutMS = Configuration.elememTimeoutMS; //1000ms timeout

        TimeSpan heartbeatTimeout = new TimeSpan(0, 0, 0, 0, timeoutMS); 

        server.TryReceiveFrameString(heartbeatTimeout, out receivedMessage);

            if (receivedMessage != "" && receivedMessage != null)
            {
                string messageString = receivedMessage.ToString();
                UnityEngine.Debug.Log("received heartbeat successfully");
                waitingForHeartbeat = false;
                ReportMessage(messageString);
                ResetMissedHeartbeats();
            }
            else
            {
                UnityEngine.Debug.Log("received invalid HEARTBEAT or timed out");
                unreceivedHeartbeats++;
                string errorString = "MISSED HEARTBEAT COUNT " + unreceivedHeartbeats.ToString();
                waitingForHeartbeat = false;
                ReportMessage(errorString);
                return;
            }
        
    }

    public void WaitForMessage(string containingString, string errorMessage, int timeoutMS)
    {
        waitingForMessage = true;
        string receivedMessage = "";


        TimeSpan timeout = new TimeSpan(0, 0, 0, 0, timeoutMS); //1000ms timeout
        UnityEngine.Debug.Log("about to receive");
        server.TryReceiveFrameString(timeout, out receivedMessage);
        UnityEngine.Debug.Log("finished receiving");

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
            ReportMessage("received " + receivedMessage);
        }
        else
        {
            UnityEngine.Debug.Log("received invalid message " + receivedMessage);
            WaitForMessage(containingString,errorMessage,timeoutMS); //repeat the wait for message
        }

    
        return;
    }

    private void ReportMessage(string msg)
    {
        AltInterface.PrintReport(msg);
    }

    public void ResetMissedHeartbeats()
    {
        unreceivedHeartbeats = 0;
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

    private ElememWorker elememWorker;


    //public InterfaceManager manager;

    private void Start()
    {

        //elememWorker = new ElememWorker();
        //StartThread();
        //InitiateConnectionForSession(1);
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



    public static void PrintReport(string message)
    {
        UnityEngine.Debug.Log("REPORT: " + message);
    }

    //this coroutine connects to ramulator and communicates how ramulator expects it to
    //in order to start the experiment session.  follow it up with BeginNewTrial and
    //SetState calls
    public IEnumerator BeginNewSession(int sessionNumber)
    {
        yield return new WaitForSeconds(1f);
        while (!elememWorker.Connected)
        {
            yield return 0;
        }

        UnityEngine.Debug.Log("sending connected message");

#if ELEMEM_DEBUG
        ElememTestRunner.Instance.DisplayStatusText("Sending CONNECTED...");
#else
        Experiment.Instance.uiController.SetElememInstructions("Sending CONNECTED...");
#endif
        //send connected message to host
        DataPoint connected = new DataPoint("CONNECTED", HighResolutionDateTime.UtcNow, new Dictionary<string, object>());
        elememWorker.SendMessageToRamulator(connected.ToJSON());
#if ELEMEM_DEBUG
        ElememTestRunner.Instance.DisplayStatusText("Waiting for CONNECTED_OK");
#endif
        //wait for confirmation
        UnityEngine.Debug.Log("waiting for CONNECTED_OK");
        elememWorker.WaitForMessage("CONNECTED_OK", "Did not receive confirmation",Configuration.elememTimeoutMS);

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
        //send configure message
        System.Collections.Generic.Dictionary<string, object> configureData = new Dictionary<string, object>();
        configureData.Add("stim_mode", Configuration.stimMode.ToString());
        configureData.Add("experiment", Experiment.ExpName);
        configureData.Add("subject", "test_subj");
        DataPoint configureDataPoint = new DataPoint("CONFIGURE", HighResolutionDateTime.UtcNow, configureData);
        UnityEngine.Debug.Log("sending CONFIGURE message");
#if ELEMEM_DEBUG
        ElememTestRunner.Instance.DisplayStatusText("Sending CONFIGURE");
#endif
        elememWorker.SendMessageToRamulator(configureDataPoint.ToJSON());


        //wait for confirmation
#if ELEMEM_DEBUG
        ElememTestRunner.Instance.DisplayStatusText("Waiting for CONFIGURE_OK");
#endif
        UnityEngine.Debug.Log("waiting for CONFIGURE_OK");
        elememWorker.WaitForMessage("CONFIGURE_OK", "Did not receive confirmation", Configuration.elememTimeoutMS);

        resendTimer = 0f;
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

#if ELEMEM_DEBUG
        ElememTestRunner.Instance.DisplayStatusText("Performing latency check...");
#else
        Experiment.Instance.uiController.SetElememInstructions("Performing latency check...");
#endif

        //wait while doing latency check
        UnityEngine.Debug.Log("performing latency check");
        yield return StartCoroutine(PerformLatencyCheck());





        UnityEngine.Debug.Log("sending READY");
        DataPoint ready = new DataPoint("READY", HighResolutionDateTime.UtcNow, new Dictionary<string, object>());
        elememWorker.SendMessageToRamulator(ready.ToJSON());
        yield return null;



#if ELEMEM_DEBUG
        ElememTestRunner.Instance.DisplayStatusText("waiting for START...");
#else
        Experiment.Instance.uiController.SetElememInstructions("Finishing up...");
#endif
        UnityEngine.Debug.Log("waiting for START");
        elememWorker.WaitForMessage("START", "Start signal not received", Configuration.elememTimeoutMS);
        while (elememWorker.CheckIfWaitingForMessage())
        {
            yield return 0;
        }

          InvokeRepeating("SendHeartbeat", 0, 1); //repeat every 1 second



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
            yield return StartCoroutine(Heartbeat(true));
            sw.Stop();

            delay[i] = sw.ElapsedTicks * (1000f / Stopwatch.Frequency);
            UnityEngine.Debug.Log("delay  " + delay[i].ToString());
#if !UNITY_EDITOR

            if (delay[i] > 20)
            {
                UnityEngine.Debug.Log("break");
                break;
            }
            yield return new WaitForSeconds(0.05f - delay[i]);
#endif
            //Thread.Sleep(50 - (int)delay[i]);
        }

        float max = delay.Max();
        float mean = delay.Sum() / delay.Length;
        float acc = (1000L * 1000L * 1000L) / Stopwatch.Frequency;

        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("max_latency", max);
        dict.Add("mean_latency", mean);
        dict.Add("accuracy", acc);

        UnityEngine.Debug.Log("LATENCY MEAN " + mean.ToString());
        UnityEngine.Debug.Log("LATENCY MAX " + max.ToString());
#if !UNITY_EDITOR
        if (max > 20f)
        {
            UnityEngine.Debug.Log("exceeded max latency threshold");
            yield return null;
        }
#endif
        UnityEngine.Debug.Log("passed latency check");
        elememWorker.ResetMissedHeartbeats(); //reset missed heartbeat count if we've passed the latency check
        yield return null;
    }



    private IEnumerator Heartbeat(bool isLatencyCheck)
    {
        // UnityEngine.Debug.Log("sending heartbeat");
        var data = new Dictionary<string, object>();
        data.Add("count", elememWorker.ReturnHeartbeatCount());
        elememWorker.IncrementHeartbeatCount();
        DataPoint heartbeatDatapoint = new DataPoint("HEARTBEAT", HighResolutionDateTime.UtcNow, data);
        elememWorker.SendMessageToRamulator(heartbeatDatapoint.ToJSON());

        elememWorker.WaitForHeartbeat(isLatencyCheck);
        //elememWorker.WaitForMessage("HEARTBEAT_OK","Did not receive heartbeat confirmation");

        while (elememWorker.CheckIfWaitingForHeartbeat())
        {
            yield return 0;
        }
         UnityEngine.Debug.Log("finished waiting for heartbeat");

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

        StartCoroutine("Heartbeat",false);
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
#else
    public class AltInterface : MonoBehaviour
{

}
#endif