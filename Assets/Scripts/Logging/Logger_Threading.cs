using UnityEngine;
using System.Collections;


using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
//using System.Runtime.InteropServices;
using System.Threading;



//CLASS BASED OFF OF: http://answers.unity3d.com/questions/357033/unity3d-and-c-coroutines-vs-threading.html

//SEE THREADEDJOB CLASS


public class LoggerQueue
{

	public Queue<String> logQueue;

	public bool hasMessages = false;
	public LoggerQueue(){
		logQueue = new Queue<String> ();
	}

	public void AddToLogQueue(string newLogInfo){
		lock (logQueue) {
			//if(!string.IsNullOrEmpty(newLogInfo))
				logQueue.Enqueue (newLogInfo);
		}
	}

	public bool CheckForMessages()
    {
		return hasMessages;
    }

	public String GetFromLogQueue(){
		string toWrite = "";
		lock (logQueue) {
			toWrite = logQueue.Dequeue ();
			if (toWrite == null)
			{
				UnityEngine.Debug.Log("IS NULL making blank line");
				toWrite = "";
			}
		}
		return toWrite;
	}

}

public class LoggerWriter : ThreadedJob
{
	public bool isRunning = false;

	//LOGGING
	protected long microseconds = 1;
	protected string workingFile = "";
	private StreamWriter logfile;
	private LoggerQueue loggerQueue;

	public LoggerWriter(string filename, LoggerQueue newLoggerQueue) {
		workingFile = filename;
		logfile = new StreamWriter ( workingFile, true );

		loggerQueue = newLoggerQueue;
	}

	public LoggerWriter() {

	}

	protected override void ThreadFunction()
	{
		isRunning = true;
		// Do your threaded task. DON'T use the Unity API here
		while (isRunning) {
			while(loggerQueue.logQueue.Count > 0){
				log (loggerQueue.GetFromLogQueue());
			}
		}

		close ();

	}
		
	protected override void OnFinished()
	{
		// This is executed by the Unity main thread when the job is finished

	}

	public void End(){
		isRunning = false;
	}

	public virtual void close()
	{
		//logfile.WriteLine ("EOF");
		logfile.Flush ();
		logfile.Close();	
		Debug.Log ("flushing & closing");
	}


	public virtual void log(string msg) {

		logfile.WriteLine (msg);
	}

}

public class Logger_Threading : MonoBehaviour{
	public static string LogTextSeparator = "\t";

	public LoggerQueue myLoggerQueue;
	LoggerWriter myLoggerWriter;

	public static bool canLog = false;
	public bool isRunning=false;
	long frameCount;
	public StreamWriter logfile;

	public string fileName;

	void Start ()
	{
			//myLoggerQueue = new LoggerQueue ();

			//			myLoggerWriter = new LoggerWriter (fileName, myLoggerQueue);
			//		
			//			myLoggerWriter.Start ();

			//	myLoggerWriter.log ("DATE: " + DateTime.Now.ToString ("M/d/yyyy")); //might not be needed
		
	}

	public Logger_Threading(string file){
		fileName = file;
	}

	public IEnumerator BeginLogging()
{
	myLoggerQueue = new LoggerQueue();
	UnityEngine.Debug.Log("beginning logging");
#if !UNITY_WEBGL
		StartCoroutine("LogWriter");
#endif
		yield return null;
    }

	

#if UNITY_WEBGL
	IEnumerator LogWriter()
	{
		isRunning = true;
		while (!canLog)
		{
			yield return 0;
		}
		UnityEngine.Debug.Log("filename is " + fileName);
		logfile = new StreamWriter(fileName, true, Encoding.ASCII, 0x10000);
		UnityEngine.Debug.Log("running logwriter coroutine writing at " + fileName);
		while (isRunning)
		{

			while (myLoggerQueue.logQueue.Count > 0)
			{
			//	string msg = myLoggerQueue.GetFromLogQueue ();

				//UnityEngine.Debug.Log ("writing: " + msg);

#if UNITY_WEBGL && !UNITY_EDITOR
			//	BrowserPlugin.WriteOutput(msg);
#endif
				//	logfile.WriteLine (msg);
				yield return 0;
			}
			yield return 0;
		}
		UnityEngine.Debug.Log("closing this");
		yield return null;
	}
#else
IEnumerator LogWriter()
	{
		isRunning = true;
		UnityEngine.Debug.Log("filename is " + fileName);
		logfile = new StreamWriter ( fileName, true,Encoding.ASCII, 0x10000);
		UnityEngine.Debug.Log ("running logwriter coroutine writing at " + fileName);
		while (isRunning) {

			while (myLoggerQueue.logQueue.Count > 0) {
				string msg = myLoggerQueue.GetFromLogQueue ();

				//UnityEngine.Debug.Log ("writing: " + msg);
				logfile.WriteLine (msg);
				yield return 0;
			}
			yield return 0;
		}
		UnityEngine.Debug.Log ("closing this");
		yield return null;
	}

	public IEnumerator FlushLogFile()
    {
		if (logfile != null)
		{
			UnityEngine.Debug.Log("flushing logfile");
			logfile.Flush();
		}
		yield return null;
    }
#endif
	//logging itself can happen in regular update. the rate at which ILoggable objects add to the log Queue should be in FixedUpdate for framerate independence.
	void Update()
	{
		frameCount++;
		//		if (myLoggerWriter != null)
		//		{
		//			if (myLoggerWriter.Update())
		//			{
		//				// Alternative to the OnFinished callback
		//				myLoggerWriter = null;
		//			}
		//		}
	}

	public long GetFrameCount(){
		return frameCount;
	}

	public void Log(long timeLogged,string newLogInfo){
		if (myLoggerQueue != null) {
			myLoggerQueue.AddToLogQueue (timeLogged + LogTextSeparator + newLogInfo);
		}
	}

	public void Log(long timeLogged, long frame, string newLogInfo){
		if (myLoggerQueue != null) {
			myLoggerQueue.AddToLogQueue (timeLogged + LogTextSeparator + frame + LogTextSeparator + newLogInfo);
		}
	}

	void OnApplicationQuit()
	{
		isRunning = false;
	}

	//must be called by the experiment class OnApplicationQuit()
	public void close(){
		//Application stopped running -- close() was called
		//applicationIsRunning = false;
		//		UnityEngine.Debug.Log("is running will be false");

		if (logfile != null)
		{
			logfile.Flush();
			logfile.Close();
		}
		isRunning=false;
		//		myLoggerWriter.End ();
	}



}