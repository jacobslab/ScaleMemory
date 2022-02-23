using System;
using System.Linq;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using System.Net.Sockets;
using Newtonsoft.Json.Linq;
using NetMQ;
using NetMQ.Sockets;

// TODO: class containing constructors for elemem messages

public abstract class IHostPC : EventLoop
{
    public abstract JObject WaitForMessage(string type, int timeout);
    public abstract void Connect();
    public abstract void HandleMessage(string message, DateTime time);
    public abstract void SendMessage(string type, Dictionary<string, object> data);
}

public class ElememListener
{
#if !UNITY_WEBGL
    ElememInterface elemem;
    Byte[] buffer;
    const Int32 bufferSize = 2048;

    private volatile ManualResetEventSlim callbackWaitHandle;
    private ConcurrentQueue<string> queue = null;

    string messageBuffer = "";
    public ElememListener(ElememInterface _elemem)
    {
        elemem = _elemem;
        buffer = new Byte[bufferSize];
        callbackWaitHandle = new ManualResetEventSlim(true);
    }

    public bool IsListening()
    {
        return !callbackWaitHandle.IsSet;
    }

    public ManualResetEventSlim GetListenHandle()
    {
        return callbackWaitHandle;
    }

    public void RegisterMessageQueue(ConcurrentQueue<string> messages)
    {
        queue = messages;
    }

    public void RemoveMessageQueue()
    {
        queue = null;
    }

    public void Listen()
    {
        if (IsListening())
        {
            throw new AccessViolationException("Already Listening");
        }

        NetworkStream stream = elemem.GetReadStream();
        callbackWaitHandle.Reset();
        stream.BeginRead(buffer, 0, bufferSize, Callback,
                        new Tuple<NetworkStream, ManualResetEventSlim, ConcurrentQueue<string>>
                            (stream, callbackWaitHandle, queue));
    }

    private void Callback(IAsyncResult ar)
    {
        NetworkStream stream;
        ConcurrentQueue<string> queue;
        ManualResetEventSlim handle;
        int bytesRead;

        Tuple<NetworkStream, ManualResetEventSlim, ConcurrentQueue<string>> state = (Tuple<NetworkStream, ManualResetEventSlim, ConcurrentQueue<string>>)ar.AsyncState;
        stream = state.Item1;
        handle = state.Item2;
        queue = state.Item3;

        bytesRead = stream.EndRead(ar);

        foreach (string msg in ParseBuffer(bytesRead))
        {
            queue?.Enqueue(msg); // queue may be deleted by this point, if wait has ended
        }

        handle.Set();
    }

    private List<string> ParseBuffer(int bytesRead)
    {
        messageBuffer += System.Text.Encoding.ASCII.GetString(buffer, 0, bytesRead);
        List<string> received = new List<string>();

        UnityEngine.Debug.Log("ParseBuffer");
        UnityEngine.Debug.Log(messageBuffer.ToString());

        while (messageBuffer.IndexOf("\n") != -1)
        {
            string message = messageBuffer.Substring(0, messageBuffer.IndexOf("\n") + 1);
            received.Add(message);
            UnityEngine.Debug.Log("added to received list");    
            messageBuffer = messageBuffer.Substring(messageBuffer.IndexOf("\n") + 1);

            ReportMessage(message);
        }

        return received;
    }

    private void ReportMessage(string message)
    {
        elemem.Do(new EventBase<string, DateTime>(elemem.HandleMessage, message, HighResolutionDateTime.UtcNow));
    }
}

// NOTE: the gotcha here is avoiding deadlocks when there's an error
// message in the queue and some blocking wait in the EventLoop thread
public class ElememInterface : IHostPC
{
    public InterfaceManager im;

    int messageTimeout = 1000;
    int heartbeatTimeout = 8000; // TODO: configuration

    private TcpClient elemem;
    private ElememListener listener;
    private int heartbeatCount = 0;
    private bool Connected = false;

    private NetMQ.Sockets.PairSocket server;

    public ElememInterface(InterfaceManager _im)
    {
        im = _im;
        listener = new ElememListener(this);
        Start();
        UnityEngine.Debug.Log("creating elemem interface");
        Do(new EventBase(Connect));
    }

    ~ElememInterface()
    {
        elemem.Close();
    }


    public NetworkStream GetWriteStream()
    {
        // TODO implement locking here
        if (elemem == null)
        {
            throw new InvalidOperationException("Socket not initialized.");
        }

        return elemem.GetStream();
    }

    public NetworkStream GetReadStream()
    {
        // TODO implement locking here
        if (elemem == null)
        {
            throw new InvalidOperationException("Socket not initialized.");
        }

        return elemem.GetStream();
    }

    public override void Connect()
    {
        UnityEngine.Debug.Log("connecting");
        
        
        
        elemem = new TcpClient();

        try
        {
            // IAsyncResult result = elemem.BeginConnect((string)im.GetSetting("ip"), (int)im.GetSetting("port"), null, null);
            UnityEngine.Debug.Log("elemem connecting now");
            IAsyncResult result = elemem.BeginConnect((string)TCP_Config.HostIPAddress, (int)TCP_Config.ConnectionPort, null, null);

            result.AsyncWaitHandle.WaitOne(messageTimeout);
            elemem.EndConnect(result);
        }
        catch (SocketException e)
        {    // TODO: set hostpc state on task side
            UnityEngine.Debug.Log("caught socket exception " + e.ToString());
            im.Do(new EventBase<string>(im.SetHostPCStatus, "ERROR"));
            throw new OperationCanceledException("Failed to Connect");
        }
        
        im.Do(new EventBase<string>(im.SetHostPCStatus, "INITIALIZING"));

        UnityEngine.Debug.Log("sending CONNECTED message");
        SendMessage("CONNECTED", new Dictionary<string, object>()); // Awake

        UnityEngine.Debug.Log("waiting for response to CONNECTED message");
        WaitForMessage("CONNECTED_OK", messageTimeout);

        UnityEngine.Debug.Log("received response");

        UnityEngine.Debug.Log("making config dict");
        
        Dictionary<string, object> configDict = new Dictionary<string, object>();
        configDict.Add("stim_mode", Configuration.stimMode);
        configDict.Add("experiment", Experiment.ExpName);
        configDict.Add("subject", Experiment.Instance.subjectName);
        configDict.Add("session", Experiment.sessionID);
        
        UnityEngine.Debug.Log("sending CONFIGURE message");
     //   SendMessage("CONFIGURE", new Dictionary<string, object>());
        SendMessage("CONFIGURE", configDict);
        UnityEngine.Debug.Log("waiting for response to CONFIGURE message");
        WaitForMessage("CONFIGURE_OK", messageTimeout);
        UnityEngine.Debug.Log("received response");
        Experiment.Instance.tcpServer.SetConnected(true);

        // excepts if there's an issue with latency, else returns
        UnityEngine.Debug.Log("doing latency check");
      //  DoLatencyCheck();

        // start heartbeats
          int interval = Configuration.heartbeatInterval;
          DoRepeating(new EventBase(Heartbeat), -1, 0, interval);

        if (Experiment.Instance != null)
            Experiment.Instance.uiController.connectionSuccessPanel.alpha = 1f;
        else
            UnityEngine.Debug.Log("WARNING:  EXP instance is null");

        UnityEngine.Debug.Log("sending READY message");
        SendMessage("READY", new Dictionary<string, object>());
        Experiment.Instance.tcpServer.SetGameStatus(true);
        im.Do(new EventBase<string>(im.SetHostPCStatus, "READY"));

    }

    private void DoLatencyCheck()
    {
        // except if latency is unacceptable
        Stopwatch sw = new Stopwatch();
        float[] delay = new float[20];

        for (int i = 0; i < 20; i++)
        {
            sw.Restart();
            Heartbeat();
            sw.Stop();

            delay[i] = sw.ElapsedTicks * (1000f / Stopwatch.Frequency);
            if (delay[i] > 20)
            {
                break;
            }

            Thread.Sleep(50 - (int)delay[i]);
        }

        float max = delay.Max();
        float mean = delay.Sum() / delay.Length;
        float acc = (1000L * 1000L * 1000L) / Stopwatch.Frequency;
        UnityEngine.Debug.Log("MEAN " + mean.ToString());

        Dictionary<string, object> dict = new Dictionary<string, object>();
        dict.Add("max_latency", max);
        dict.Add("mean_latency", mean);
        dict.Add("accuracy", acc);

        im.Do(new EventBase<string, Dictionary<string, object>>(im.ReportEvent, "latency check", dict));
    }

    public override JObject WaitForMessage(string type, int timeout)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();

        ManualResetEventSlim wait;
        int waitDuration;
        ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
        JObject json;

        listener.RegisterMessageQueue(queue);
        while (sw.ElapsedMilliseconds < timeout)
        {
            listener.Listen();
            wait = listener.GetListenHandle();
            waitDuration = timeout - (int)sw.ElapsedMilliseconds;
            waitDuration = waitDuration > 0 ? waitDuration : 0;

            wait.Wait(waitDuration);

            string message;
            while (queue.TryDequeue(out message))
            {
                UnityEngine.Debug.Log("dequeued message  " + message);
                json = JObject.Parse(message);
                if (json.GetValue("type").Value<string>() == type)
                {
                    listener.RemoveMessageQueue();
                    return json;
                }
            }
        }

        listener.RemoveMessageQueue();
        throw new TimeoutException("Timed out waiting for response");
    }

    public override void HandleMessage(string message, DateTime time)
    {
        JObject json = JObject.Parse(message);
        json.Add("task pc time", time);

        string type = json.GetValue("type").Value<string>();
        ReportMessage(json.ToString(), false);

        if (type.Contains("ERROR"))
        {
            throw new Exception("Error received from Host PC.");
        }
        if (type == "EXIT")
        {
            return;
        }

        // // start listener if not running
        // if(!listener.IsListening()) {
        //     listener.Listen();
        // }
    }
    public override void SendMessage(string type, Dictionary<string, object> data)
    {
        DataPoint point = new DataPoint(type, HighResolutionDateTime.UtcNow, data);
        string message = point.ToJSON();

        UnityEngine.Debug.Log("Sent Message");
        UnityEngine.Debug.Log(message);

        Byte[] bytes = System.Text.Encoding.UTF8.GetBytes(message);

        NetworkStream stream = GetWriteStream();
        stream.Write(bytes, 0, bytes.Length);
        ReportMessage(message, true);
    }

    private void Heartbeat()
    {
        var data = new Dictionary<string, object>();
        data.Add("count", heartbeatCount);
        heartbeatCount++;
        SendMessage("HEARTBEAT", data);
        WaitForMessage("HEARTBEAT_OK", heartbeatTimeout);
    }

    private void ReportMessage(string message, bool sent)
    {
        Dictionary<string, object> messageDataDict = new Dictionary<string, object>();
        messageDataDict.Add("message", message);
        messageDataDict.Add("sent", sent.ToString());

        im.Do(new EventBase<string, Dictionary<string, object>, DateTime>(im.ReportEvent, "network",
                                messageDataDict, HighResolutionDateTime.UtcNow));
    }
#endif
}