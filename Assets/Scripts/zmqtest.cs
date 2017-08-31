using UnityEngine;
using System.Collections;
using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

public class zmqtest : MonoBehaviour {

	[DllImport ("ZMQPlugin")]
	private static extern int ZMQConnect(string hostAddress);


	[DllImport ("ZMQPlugin")]
	private static extern IntPtr ZMQReceive();


	[DllImport ("ZMQPlugin")]
	private static extern void ZMQSend(string message,int length);


	[DllImport ("ZMQPlugin")]
	private static extern void ZMQClose();

	[DllImport ("ZMQPlugin")]
	private static extern int PrintANumber();

	[DllImport ("ZMQPlugin")]
	private static extern int AddTwoIntegers(int i1,int i2);

	[DllImport ("ZMQPlugin")]
	private static extern float AddTwoFloats(float f1,float f2); 
	// Use this for initialization


	ThreadedServer myServer;

	void Start () {
		myServer = new ThreadedServer();
		UnityEngine.Debug.Log ("starting");
		myServer.Start();
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown (KeyCode.A))
			myServer.SendMessage ();
		
	}

	void OnApplicationQuit()
	{
		myServer.End ();
	}

	public class ThreadedServer : ThreadedJob{
		public bool isRunning = false;

		public bool isServerConnected = false;
		public bool isSynced = false;
		public bool canStartGame = false;
		Stopwatch clockAlignmentStopwatch;
		//int numClockAlignmentTries = 0;
		//const int timeBetweenClockAlignmentTriesMS = 500;//500; //half a second
		//const int maxNumClockAlignmentTries = 120; //for a total of 60 seconds of attempted alignment

		int index=0;
		bool canSend=false;

		public string messagesToSend = "";
		string incompleteMessage = "";


	
		public ThreadedServer(){

		}

		protected override void ThreadFunction()
		{
			isRunning = true;
			int connectionStatus=ZMQConnect("tcp://localhost:5555");
			PrintSomething ("connection status " + connectionStatus); 
			// Do your threaded task. DON'T use the Unity API here
			while (isRunning) {
				string message = Marshal.PtrToStringAnsi(ZMQReceive());
//				char msg=zmqreceive();
//				PrintSomething(message);
				if (message != "none") {
					PrintSomething ("received: " + message.ToString ());
					PrintSomething ("length: " + message.ToCharArray ().Length.ToString ());
				}
				if (canSend) {
					string send_message = "hi it's : " + index.ToString ();
					PrintSomething("sending: " + send_message);
					ZMQSend (send_message,128);
					canSend = false;
				}
			}
			ShutDownMessage ();
		}

		void PrintSomething(string msg){
			UnityEngine.Debug.Log (msg);
		}

		public void SendMessage()
		{
			canSend = true;
		}

		public void End()
		{
			isRunning = false;
		}

		void ShutDownMessage()
		{
			ZMQClose ();
			UnityEngine.Debug.Log ("shutting down");
		}
}
}
