using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using TMPro;
using UnityEngine;
using UnityStandardAssets.Utility;
using UnityStandardAssets.Vehicles.Car;

public class Experiment : MonoBehaviour {
	
	//EXPERIMENT IS A SINGLETON
	private static Experiment _instance;
	public ObjectManager objManager;
	public UIController uiController;
	public ObjectController objController;
	public GameObject player;

	private GameObject slowZoneObj;
	private GameObject speedZoneObj;

	public List<Transform> spawnableWaypoints;
	//public List<Transform> rightSpawnableWaypoints;

	private float carSpeed = 0f; //this is used exclusively to control car speed directly during spatial retrieval phase


	//traffic light controller
	public TrafficLightController trafficLightController;

	public enum TaskStage
	{
		ItemScreening,
		TrackScreening,
		Encoding,
		SpatialRetrieval,
		VerbalRetrieval,
        Feedback,
		PostTaskScreening
	}

	public TaskStage currentStage = TaskStage.ItemScreening;

	public List<GameObject> spawnedObjects;
	public List<Vector3> spawnLocations;

	private List<GameObject> retrievalObjList;
	private List<Vector3> retrievalPositions;

	public List<Transform> startableTransforms;


	public Camera itemScreeningCam;

	public WaypointCircuit waypointCircuit;

	public static int recallTime = 6;

	public static int blockLength = 24;

	public static int listLength = 4;

	private List<GameObject> itemScreeningSeq;

	public Transform itemScreeningTransform;

	public Transform startTransform;

	private bool wasMovingForward = false;
    private bool wasMovingReverse = false;
	private float movementTimer = 0f;

	//prefabs
	public GameObject slowZonePrefab;
	public GameObject speedZonePrefab;

	private GameObject leftSpawnObj;
	private GameObject rightSpawnObj;


	//blackrock variables
	public static string ExpName = "T2";
	public static string BuildVersion = "0.9.8";
	public static bool isSystem2 = false;

	public bool verbalRetrieval = false;

	//track screening
	public GameObject overheadCam;
	public GameObject trackFamiliarizationQuad;
	public GameObject feedbackQuad;
	public GameObject playerIndicatorSphere;

	//spatial retrieval
	public GameObject correctIndicator;
	public GameObject wrongIndicator;
	private List<bool> spatialFeedbackStatus;
	private List<Vector3> spatialFeedbackPosition;

	//logging
	public static bool isLogging = true;
	private string subjectLogfile; //gets set based on the current subject in Awake()
	public Logger_Threading subjectLog;
	private string eegLogfile; //gets set based on the current subject in Awake()
	public Logger_Threading eegLog;
	public string sessionDirectory;
	public static string sessionStartedFileName = "sessionStarted.txt";
	public static int sessionID;

	public string subjectName = "";

	public SubjectReaderWriter subjectReaderWriter;

	private bool ipAddressEntered = false;

	public IPAddress targetAddress;

	public static string defaultLoggingPath; //SET IN RESETDEFAULTLOGGINGPATH();
												  //string DB3Folder = "/" + Config.BuildVersion.ToString() + "/";
												  //public Text defaultLoggingPathDisplay;
												  //public InputField loggingPathInputField;


	public static Subject currentSubject
	{
		get { return _currentSubject; }
		set
		{
			_currentSubject = value;
			//fileName = "TextFiles/" + _currentSubject.name + "Log.txt";
		}
	}

	private static Subject _currentSubject;
	public SubjectSelectionController subjectSelectionController;


	public SimpleTimer lapTimer;

	public GameObject chequeredFlag;

	private float fixedTime = 1f;

	private int maxLaps = 1;

	public Transform targetWaypoint;

	private float prevLapTime = 0f;
	private float bestLapTime = 1000f;


	public TrialLogTrack trialLogTrack;
	private int objLapper = 0;

	public AudioRecorder audioRecorder;
	private int retCount = 0;
	private int trialCount = 0;


	public TCPServer tcpServer;
	public static Experiment Instance{
		get{
			return _instance;
		}
	}

	void Awake(){
		if (_instance != null) {
			UnityEngine.Debug.Log ("Instance already exists!");
			return;
		}
		_instance = this;
		tcpServer.gameObject.SetActive(false);
		defaultLoggingPath = Application.dataPath;
		carSpeed = 0f;
	}
	// Use this for initialization
	void Start()
	{
		player.GetComponent<CarController>().ChangeMaxSpeed(40f);
		spatialFeedbackStatus = new List<bool>();
		spatialFeedbackPosition = new List<Vector3>();
		SetCarBrakes(true);
		StartCoroutine("BeginExperiment");
		spawnedObjects = new List<GameObject>();
		spawnLocations = new List<Vector3>();
		retrievalObjList = new List<GameObject>();
		retrievalPositions = new List<Vector3>();

	}

	public void ActivateSpeedZone(bool didPress)
    {
		if(didPress)
        {
			StartCoroutine("NitroSpeedCar");
        }
		else
        {
			//no change
			StartCoroutine("NoSpeedCar");
        }
    }

	public void ActivateSlowZone(bool didPress)
    {
		if(didPress)
        {
			StartCoroutine("EvadeSlowZone");
        }
		else
        {
			StartCoroutine("SlowCar");
        }
		
    }

	//upon successfully pressing button when inside SPEED zone
	IEnumerator NitroSpeedCar()
    {
		UnityEngine.Debug.Log("ACTIVATE NITRO SPEED");
		player.GetComponent<CarController>().ChangeMaxSpeed(70f);
		yield return StartCoroutine(FlashInfo("You activated \n speed boost", Color.green));
		yield return new WaitForSeconds(2f);
		player.GetComponent<CarController>().ChangeMaxSpeed(40f);
		yield return null;
    }


	//upon failing to press button when inside speed zone
	IEnumerator NoSpeedCar()
	{
		UnityEngine.Debug.Log("NO SPEED");
		player.GetComponent<CarController>().ChangeMaxSpeed(20f);
		yield return StartCoroutine(FlashInfo("You failed to \n activate speed boost", Color.red));
		yield return new WaitForSeconds(1f);
		player.GetComponent<CarController>().ChangeMaxSpeed(40f);
		yield return null;
    }


	//upon successfully pressing button when inside SLOW zone
	IEnumerator EvadeSlowZone()
	{
		UnityEngine.Debug.Log("EVADE SLOW ZONE");
		player.GetComponent<CarController>().ChangeMaxSpeed(50f);
		yield return StartCoroutine(FlashInfo("You avoided \n oil patch", Color.green));
		yield return new WaitForSeconds(2f);
		player.GetComponent<CarController>().ChangeMaxSpeed(40f);
		yield return null;
    }


	//upon failing to press button when inside slow zone
	IEnumerator SlowCar()
	{
		UnityEngine.Debug.Log("CAR HAS BEEN SLOWED");
		player.GetComponent<CarController>().ChangeMaxSpeed(1f);
		yield return StartCoroutine(FlashInfo("You were slowed \n by the oil patch", Color.red));
		yield return new WaitForSeconds(1f);
		player.GetComponent<CarController>().ChangeMaxSpeed(40f);
		yield return null;
    }

	IEnumerator FlashInfo(string infoStr,Color textColor)
    {
		uiController.infoText.text = infoStr;
		uiController.infoText.color = textColor;
		uiController.infoText.enabled = true;
		yield return new WaitForSeconds(2f);
		uiController.infoText.enabled = false;
		yield return null;
    }

	public void MarkIPAddrEntered()
	{
		ipAddressEntered = true;
	}

	//TODO: move to logger_threading perhaps? *shrug*
	IEnumerator InitLogging()
	{
		string subjectDirectory = defaultLoggingPath + "/" + subjectName + "/";
		sessionDirectory = subjectDirectory + "session_0" + "/";

		sessionID = 0;
		string sessionIDString = "_0";

		if (!Directory.Exists(subjectDirectory))
		{
			Directory.CreateDirectory(subjectDirectory);
		}
		while (File.Exists(sessionDirectory + sessionStartedFileName))
		{//Directory.Exists(sessionDirectory)) {
			sessionID++;

			sessionIDString = "_" + sessionID.ToString();

			sessionDirectory = subjectDirectory + "session" + sessionIDString + "/";
		}

		//delete old files.
		if (Directory.Exists(sessionDirectory))
		{
			DirectoryInfo info = new DirectoryInfo(sessionDirectory);
			FileInfo[] fileInfo = info.GetFiles();
			for (int i = 0; i < fileInfo.Length; i++)
			{
				File.Delete(fileInfo[i].ToString());
			}
		}
		else
		{ //if directory didn't exist, make it!
			Directory.CreateDirectory(sessionDirectory);
		}

		subjectLog.fileName = sessionDirectory + subjectName + "Log" + ".txt";
		eegLog.fileName = sessionDirectory + subjectName + "EEGLog" + ".txt";


		yield return null;
	}

	//In order to increment the session, this file must be present. Otherwise, the session has not actually started.
	//This accounts for when we don't successfully connect to hardware -- wouldn't want new session folders.
	//Gets created in TrialController after any hardware has conneinitcted and the instruction video has finished playing.
	public void CreateSessionStartedFile()
	{
		StreamWriter newSR = new StreamWriter(sessionDirectory + sessionStartedFileName);
	}
	/*
	IEnumerator SpawnZones()
    {
		int prevRandom = 0;
		int waypointCount = spawnableWaypoints.Count;
		List<int> tempIntList = new List<int>();
		for (int i=0;i<waypointCount;i++)
        {
			tempIntList.Add(i);
        }

		List<Transform> chosenWaypoints = new List<Transform>();
		for(int j=0;j<2;j++)
        {
			int random = UnityEngine.Random.Range(2, tempIntList.Count-3);
			
			//here we make sure the two spawn locations are not too close to each other
			if(j ==1)
            {
				int diff = Mathf.Abs(random - prevRandom); 
				while(diff < 5)
                {
					random = UnityEngine.Random.Range(2, tempIntList.Count - 3);
					diff = Mathf.Abs(random - prevRandom);
					yield return 0;
                }
            }
			else
            {
				prevRandom = random; //set index for later comparison
            }
			UnityEngine.Debug.Log("random int " + random.ToString());

			chosenWaypoints.Add(spawnableWaypoints[random]);
			tempIntList.RemoveAt(random);
			tempIntList.RemoveAt(random-1);
			tempIntList.RemoveAt(random - 2);

			if (random + 1 < tempIntList.Count)
			{
				tempIntList.RemoveAt(random + 1);
			}

			if (random + 2 < tempIntList.Count)
			{
				tempIntList.RemoveAt(random + 2);
			}

			float yAngle = 0f;

			if(random<=7 || random>26)
            {
				yAngle = 0f;
            }
			else
            {
				yAngle = 90f;
            }

			if(j==1)
            {
				slowZoneObj = Instantiate(slowZonePrefab, chosenWaypoints[chosenWaypoints.Count - 1].position,Quaternion.Euler(new Vector3(0f,yAngle,0f))) as GameObject;
				trialLogTrack.LogSlowZoneLocation(slowZoneObj.transform.position);
            }
			else
            {
				speedZoneObj = Instantiate(speedZonePrefab, chosenWaypoints[chosenWaypoints.Count - 1].position, Quaternion.Euler(new Vector3(0f, yAngle, 0f))) as GameObject;
				trialLogTrack.LogSpeedZoneLocation(speedZoneObj.transform.position);
			}

		}
		yield return null;
    }
	*/


	IEnumerator SpawnZones()
    {
		/*
		int leftRandInt = Random.Range(0, leftSpawnableWaypoints.Count - 1);
		int rightRandInt = Random.Range(0, rightSpawnableWaypoints.Count - 1);


		float yAngle = -180f;

		//randomize which side gets slow and speed zones
		if (Random.value > 0.5f)
		{

			slowZoneObj = Instantiate(slowZonePrefab, leftSpawnableWaypoints[leftSpawnableWaypoints.Count - 1].position, Quaternion.Euler(new Vector3(0f, yAngle, 0f))) as GameObject;
			trialLogTrack.LogSlowZoneLocation(slowZoneObj.transform.position);

			speedZoneObj = Instantiate(speedZonePrefab, rightSpawnableWaypoints[rightSpawnableWaypoints.Count - 1].position, Quaternion.Euler(new Vector3(0f, yAngle, 0f))) as GameObject;
			trialLogTrack.LogSpeedZoneLocation(speedZoneObj.transform.position);
		}
		else
		{
			slowZoneObj = Instantiate(slowZonePrefab, rightSpawnableWaypoints[rightSpawnableWaypoints.Count - 1].position, Quaternion.Euler(new Vector3(0f, yAngle, 0f))) as GameObject;
			trialLogTrack.LogSlowZoneLocation(slowZoneObj.transform.position);

			speedZoneObj = Instantiate(speedZonePrefab, leftSpawnableWaypoints[leftSpawnableWaypoints.Count - 1].position, Quaternion.Euler(new Vector3(0f, yAngle, 0f))) as GameObject;
			trialLogTrack.LogSpeedZoneLocation(speedZoneObj.transform.position);
		}
		*/

		yield return null;

	}


	IEnumerator BeginExperiment()
	{
		subjectName = "subj_"+GameClock.SystemTime_MillisecondsString;
#if !UNITY_EDITOR
		yield return StartCoroutine(InitLogging());
#endif
		UnityEngine.Debug.Log("set subject name: " + subjectName);
		trialLogTrack.LogBegin();
		//only run if system2 is expected
		if (isSystem2)
		{
			/*
			uiController.ipEntryPanel.alpha = 1f;

			while(!ipAddressEntered)
			{
				yield return 0;
			}
			int portNum = int.Parse(uiController.ipAddrInput.text);
			UnityEngine.Debug.Log("target port  " + portNum.ToString());
			TCP_Config.ConnectionPort = portNum;
			ipAddressEntered = false;
			uiController.ipEntryPanel.alpha = 0f;
			*/
			

			tcpServer.gameObject.SetActive(true);
			uiController.blackrockConnectionPanel.alpha = 1f;
			trialLogTrack.LogBlackrockConnectionAttempt();
			uiController.connectionText.text = "Attempting to connect with server...";
			//wait till the SYS2 Server connects
			while (!tcpServer.isConnected)
			{
				yield return 0;
			}
			uiController.connectionText.text = "Waiting for server to start...";
			while (!tcpServer.canStartGame)
			{
				yield return 0;
			}

			uiController.blackrockConnectionPanel.alpha = 0f;
		}
		else
		{
			uiController.blackrockConnectionPanel.alpha = 0f;
		}
		trialLogTrack.LogBlackrockConnectionSuccess();
		trialLogTrack.LogIntroInstruction(true);
		uiController.taskIntroPanel.alpha = 1f;

		yield return StartCoroutine(WaitForActionButton());
		uiController.taskIntroPanel.alpha = 0f;
		trialLogTrack.LogIntroInstruction(false);

		//yield return StartCoroutine("BeginItemScreening");
		//	StartCoroutine("RandomizeTravelSpeed");
		yield return StartCoroutine("BeginTrackScreening");

	//	yield return StartCoroutine("SpawnZones");
		//repeat blocks twice
		yield return StartCoroutine("BeginTaskBlock");

		uiController.endSessionPanel.alpha = 1f;
		yield return StartCoroutine(WaitForActionButton());

		Application.Quit();
		yield return null;
	}

	IEnumerator RandomizeTravelSpeed()
	{
		while (currentStage != Experiment.TaskStage.PostTaskScreening)
		{
			while (!player.GetComponent<Rigidbody>().isKinematic)
			{
				float timer = 0f;
				float maxTime = UnityEngine.Random.Range(3f, 10f);
				float speed = UnityEngine.Random.Range(30f, 60f);
				UnityEngine.Debug.Log("new max time " + maxTime.ToString());
				while (timer < maxTime)
				{
					timer += Time.deltaTime;
					yield return 0;
				}
					player.GetComponent<CarController>().ChangeMaxSpeed(speed);
					//player.GetComponent<CarController>().SetCurrentSpeed(speed);
					UnityEngine.Debug.Log("changed speed " + speed.ToString());
					timer = 0f;
				yield return 0;
			}
			
			yield return 0;
		}
		yield return null;
	}

	IEnumerator BeginItemScreening()
	{
		currentStage = TaskStage.ItemScreening;
		player.gameObject.SetActive(false);
		uiController.itemScreeningPanel.alpha = 1f;
		yield return new WaitForSeconds(2f);
		uiController.itemScreeningPanel.alpha = 0f;
		itemScreeningCam.enabled = true;

		for(int i=0;i<12;i++)
		{
			yield return StartCoroutine(GenerateRandomItemScreeningSequence());
			for (int j = 0; j < itemScreeningSeq.Count; j++)
			{
				GameObject objToSpawn = Instantiate(itemScreeningSeq[j], itemScreeningTransform.position, Quaternion.identity) as GameObject;
				trialLogTrack.LogItemDisplay(objToSpawn.gameObject.name,true);
				objToSpawn.GetComponent<FacePosition>().FaceItemScreeningCam();
				yield return new WaitForSeconds(1f);
				Destroy(objToSpawn);
				trialLogTrack.LogItemDisplay(objToSpawn.gameObject.name,false);
			}
		}

		trialLogTrack.LogTaskStage(currentStage, false);
		yield return null;
	}

	IEnumerator GenerateRandomItemScreeningSequence()
	{
		List<GameObject> tempList = new List<GameObject>();
		for(int i=0;i<listLength;i++)
		{
			tempList.Add(objController.encodingList[i]);
		}

		List<GameObject> randObjects = objController.ReturnRandomlySelectedObjects(3);
		
		for(int j=0;j<randObjects.Count;j++)
		{
			tempList.Add(randObjects[j]);
		}

		itemScreeningSeq = new List<GameObject>();

		int max = tempList.Count;
		for(int i=0;i<max;i++)
		{
			int randIndex = UnityEngine.Random.Range(0, tempList.Count);
			itemScreeningSeq.Add(tempList[randIndex]);
			tempList.RemoveAt(randIndex);
		}

		yield return null;
	}

	IEnumerator BeginTrackScreening()
	{

		currentStage = TaskStage.TrackScreening;
		overheadCam.SetActive(true);
		trackFamiliarizationQuad.SetActive(true);
		playerIndicatorSphere.SetActive(true);
		trialLogTrack.LogTaskStage(currentStage, true);
		SetCarBrakes(true);
		uiController.itemScreeningPanel.alpha = 0f;
		uiController.trackScreeningPanel.alpha = 1f;
		yield return StartCoroutine(WaitForActionButton());
		uiController.trackScreeningPanel.alpha = 0f;
		player.gameObject.SetActive(true);
		trafficLightController.MakeVisible(true);
		yield return StartCoroutine(trafficLightController.StartCountdownToGreen());
		SetCarBrakes(false);
		trafficLightController.MakeVisible(false);
		while (LapCounter.lapCount < 2)
		{
			yield return 0;
		}
		LapCounter.lapCount = 0;
		SetCarBrakes(true); 
		overheadCam.SetActive(false);
		trackFamiliarizationQuad.SetActive(false);
		playerIndicatorSphere.SetActive(false);
		trialLogTrack.LogTaskStage(currentStage,false);
		yield return null;
	}

	public void ShowTurnDirection(WaypointProgressTracker.TrackDirection turnDirection)
    {
		switch (turnDirection)
        {
			case WaypointProgressTracker.TrackDirection.Left:
				uiController.leftTurnArrow.alpha = 1f;
				break;
			case WaypointProgressTracker.TrackDirection.Right:
				uiController.rightTurnArrow.alpha = 1f;
				break;
        }
    }

	public void HideTurnDirection(WaypointProgressTracker.TrackDirection turnDirection)
	{
		switch (turnDirection)
		{
			case WaypointProgressTracker.TrackDirection.Left:
				uiController.leftTurnArrow.alpha = 0f;
				break;
			case WaypointProgressTracker.TrackDirection.Right:
				uiController.rightTurnArrow.alpha = 0f;
				break;
		}

	}

	public IEnumerator BeginCrashSequence(Transform crashZone)
	{
		SetCarBrakes(true);
		Vector3 origTransform = player.transform.position;
		float lerpTimer = 0f;

		while(lerpTimer < 2f)
        {
			lerpTimer += Time.deltaTime;
			player.transform.position = Vector3.Lerp(origTransform, crashZone.position, lerpTimer/2f);
			yield return 0;
        }
		uiController.crashNotification.alpha = 1f;
		yield return new WaitForSeconds(2f);

		uiController.crashNotification.alpha = 0f;

		//conceal the transformation
		uiController.blackScreen.alpha = 1f;
		//make sure we transport the car back to the starting transform
		player.transform.position = startTransform.position;
		player.transform.rotation = startTransform.rotation;

		player.GetComponent<WaypointProgressTracker>().Reset(); //reset the waypoint system to begin from the beginning

		uiController.blackScreen.alpha = 0f;
		SetCarBrakes(false);
		//	player.GetComponent<CarController>().SetCurrentSpeed(0f);
		yield return null;
    }

	IEnumerator ShowEncodingInstructions()
	{
		uiController.encodingPanel.alpha = 1f;
		yield return StartCoroutine(WaitForActionButton());
		uiController.encodingPanel.alpha = 0f;
		yield return null;
	}
	IEnumerator ShowVerbalRetrievalInstructions()
	{
		uiController.verbalRetrievalPanel.alpha = 1f;
		yield return StartCoroutine(WaitForActionButton());
		uiController.verbalRetrievalPanel.alpha = 0f;
		yield return null;
	}


	IEnumerator ShowRetrievalInstructions()
	{
		uiController.retrievalPanel.alpha = 1f;
		yield return StartCoroutine(WaitForActionButton());
		uiController.retrievalPanel.alpha = 0f;
		yield return null;
	}

	string FormatTime(float timeInSeconds)
    {
		int displayMinutes = (int)Mathf.Floor(timeInSeconds / 60.0f);
		int displaySeconds = (int)Mathf.Floor(timeInSeconds - (displayMinutes * 60));

		string result = "";
		if (displaySeconds < 10)
		{
			result = displayMinutes + ":0" + displaySeconds;
		}
		else
		{
			result = displayMinutes + ":" + displaySeconds;
		}

		return result;
	}

	void ResetLapDisplay()
    {
		uiController.lapTimePanel.alpha = 1f;
		lapTimer.ResetTimer();
		lapTimer.StartTimer();
	}

	void UpdateLapDisplay()
    {
		lapTimer.StopTimer();
		prevLapTime= lapTimer.GetSecondsFloat();
		if(prevLapTime < bestLapTime)
        {
			bestLapTime = prevLapTime;
			uiController.bestLapTimeText.text = FormatTime(bestLapTime);
        }
    }

	void HideLapDisplay()
    {
		uiController.lapTimePanel.alpha = 0f;
    }

	IEnumerator BeginTaskBlock()
	{

		verbalRetrieval = false;

		//yield return StartCoroutine("BeginTrackScreening");

		SetCarBrakes(true);
		//show instructions
		for (int i = 0; i < blockLength; i++)
		{
            UnityEngine.Debug.Log("in encoding now");
			currentStage = TaskStage.Encoding;
			trialLogTrack.LogTaskStage(currentStage, true);
			trialCount = i + 1;
			LapCounter.lapCount = 0;
			yield return StartCoroutine(objController.SelectEncodingItems());
			trialLogTrack.LogInstructions(true);
			player.transform.position = startTransform.position;
			player.transform.rotation = startTransform.rotation;
			int targetIndex = player.GetComponent<CarAIControl>().FindSubsequentWaypointIndexFromStart(player.transform); //this sets the target waypoint when you start from a random location; will ALWAYS be facing the forward direction
			player.GetComponent<CarAIControl>().SetTarget(player.GetComponent<WaypointProgressTracker>().currentCircuit.Waypoints[targetIndex], targetIndex);

			chequeredFlag.transform.position = startTransform.position;
			chequeredFlag.transform.rotation = startTransform.rotation;

			player.GetComponent<CarAIControl>().ResetTargetToStart(); //reset waypoint target transform to forward facing the startTransform
			yield return StartCoroutine(ShowEncodingInstructions());
			trialLogTrack.LogInstructions(false);
			yield return StartCoroutine(PickEncodingLocations());
			yield return StartCoroutine(SpawnEncodingObjects()); //this will spawn all encoding objects on the track


			while (LapCounter.lapCount < 1)
			{
			/*	
				List<Transform> validTransforms = GetValidStartTransforms();

				int randStartWaypoint = UnityEngine.Random.Range(0, validTransforms.Count - 1);

				Transform randTransform = validTransforms[randStartWaypoint];

				player.transform.position = randTransform.position;
				player.transform.rotation = randTransform.rotation;
				chequeredFlag.transform.position = randTransform.position;
				chequeredFlag.transform.rotation = randTransform.rotation;

				trialLogTrack.LogEncodingStartPosition(player.transform.position);
				

				player.GetComponent<WaypointProgressTracker>().Reset();

				*/
				trafficLightController.MakeVisible(true);
				yield return StartCoroutine(trafficLightController.StartCountdownToGreen());
				SetCarBrakes(false);
				trafficLightController.MakeVisible(false);

				//reset lap timer and show display
				ResetLapDisplay();
				//player.GetComponent<WaypointProgressTracker>().SetActiveDirection(WaypointProgressTracker.TrackDirection.Left);


				/*//switch direction
				if (player.GetComponent<WaypointProgressTracker>().currentDirection == WaypointProgressTracker.TrackDirection.Left)
				{
					UnityEngine.Debug.Log("chose right direction");
					player.GetComponent<WaypointProgressTracker>().SetActiveDirection(WaypointProgressTracker.TrackDirection.Right);
				}
				else
				{
					UnityEngine.Debug.Log("chose left direction");
					player.GetComponent<WaypointProgressTracker>().SetActiveDirection(WaypointProgressTracker.TrackDirection.Left);
				}
				*/
				UnityEngine.Debug.Log("began lap number : " + LapCounter.lapCount.ToString());
				SetCarBrakes(false);
				LapCounter.canStop = false;
				while (!LapCounter.canStop)
				{
					yield return 0;
				}
				LapCounter.canStop = false;
				//can press spacebar to stop
				float forceStopTimer = 0f;
				trafficLightController.MakeVisible(true);
				yield return StartCoroutine(trafficLightController.ShowRed());
				
				bool forceStopped = false;
				while (!forceStopped)
				{
					forceStopTimer += Time.deltaTime;
					if (forceStopTimer > 1.15f)
					{
						forceStopTimer = 0f;
						forceStopped = true;
					}
					yield return 0;
				}
				
				forceStopped = false;


				//update lap time by stopping timer first and then hide it before fixation
				UpdateLapDisplay();
				HideLapDisplay();

				SetCarBrakes(true);
				trafficLightController.MakeVisible(false);

				yield return new WaitForSeconds(1f);
				player.transform.position = startTransform.position;
				yield return StartCoroutine(ShowFixation());
				player.transform.position = startTransform.position;
				yield return 0;
			}

			//retrieval time

			HideLapDisplay();
			//hide encoding objects and text
			for (int j=0;j< spawnedObjects.Count;j++)
			{
				//spawnedObjects[j].gameObject.SetActive(false);
				spawnedObjects[j].gameObject.GetComponent<VisibilityToggler>().TurnVisible(false);
				
			}

			//	uiController.itemOneName.text = spawnedObjects[0].name.Split('(')[0];
			//	uiController.itemTwoName.text = spawnedObjects[1].name.Split('(')[0];

			//slowZoneObj.GetComponent<VisibilityToggler>().TurnVisible(false);
		//	speedZoneObj.GetComponent<VisibilityToggler>().TurnVisible(false);

			trialLogTrack.LogTaskStage(currentStage, false);
		//	currentStage = Experiment.TaskStage.Retrieval;

			//pick a randomized starting retrieval position
			List<Transform> validStartTransforms = GetValidStartTransforms(); //get valid waypoints that don't have an object already spawned there
			int randWaypoint = UnityEngine.Random.Range(0, validStartTransforms.Count - 1);

			Transform randStartTransform = validStartTransforms[randWaypoint];
			targetIndex = player.GetComponent<CarAIControl>().FindSubsequentWaypointIndexFromStart(randStartTransform); //this sets the target waypoint when you start from a random location; will ALWAYS be facing the forward direction
			player.GetComponent<CarAIControl>().SetTarget(player.GetComponent<WaypointProgressTracker>().currentCircuit.Waypoints[targetIndex],targetIndex);
			player.transform.position = randStartTransform.position;
			player.transform.rotation = randStartTransform.rotation;
			chequeredFlag.transform.position = randStartTransform.position;
			chequeredFlag.transform.rotation = randStartTransform.rotation;

			trialLogTrack.LogRetrievalStartPosition(player.transform.position);

			player.GetComponent<WaypointProgressTracker>().Reset();

			LapCounter.lapCount = 0; //reset lap count for retrieval 

			string targetNames = "";


			if (trialCount % 2 == 0)
			{
				verbalRetrieval = true;

				currentStage = TaskStage.VerbalRetrieval;
			}
			else
            {
				currentStage = TaskStage.SpatialRetrieval;
            }

			//log the retrieval stage
			trialLogTrack.LogTaskStage(currentStage, true);
			bool finishedRetrieval = false;
			retCount = 0;


			while (LapCounter.lapCount < 10 && !finishedRetrieval)
			{
				if (verbalRetrieval)
				{
					trialLogTrack.LogInstructions(true);
					yield return StartCoroutine(ShowVerbalRetrievalInstructions());
					trialLogTrack.LogInstructions(false);
					trafficLightController.MakeVisible(true);
					yield return StartCoroutine(trafficLightController.StartCountdownToGreen());
					SetCarBrakes(false);

					//reset lap timer and show display
					ResetLapDisplay();
					HideLapDisplay();

					trafficLightController.MakeVisible(false);

					while(retCount < listLength)
                    {
						yield return 0;
                    }
					verbalRetrieval = false;
					SetCarBrakes(true);
					UpdateLapDisplay();
					HideLapDisplay();

					trafficLightController.MakeVisible(false);
					yield return new WaitForSeconds(1f);

					finishedRetrieval = true;
				}
				else
				{
					carSpeed = 0f;
					spatialFeedbackStatus.Clear();
					spatialFeedbackStatus = new List<bool>();
					UnityEngine.Debug.Log("beginning spatial retrieval");
					trialLogTrack.LogInstructions(true);
					yield return StartCoroutine(ShowRetrievalInstructions());
					trialLogTrack.LogInstructions(false);
					chequeredFlag.SetActive(false);

					//spatial retrieval
					List<int> intPool = new List<int>();
					List<int> randIndex = new List<int>();
					for (int j = 0; j < listLength; j++)
					{
						intPool.Add(j);
					}
					for (int j = 0; j < listLength; j++)
					{
						int randInt = UnityEngine.Random.Range(0, intPool.Count - 1);
						randIndex.Add(intPool[randInt]);
						intPool.RemoveAt(randInt);
					}

					trafficLightController.MakeVisible(true);
					yield return StartCoroutine(trafficLightController.StartCountdownToGreen());
					SetCarBrakes(false);

					//reset lap timer and show display
					ResetLapDisplay();
					HideLapDisplay();

					trafficLightController.MakeVisible(false);
					for (int j = 0; j < listLength; j++)
					{
						UnityEngine.Debug.Log("retrieval num " + j.ToString());
						targetNames = spawnedObjects[randIndex[j]].gameObject.name.Split('(')[0];
						uiController.zRetrievalText.color = Color.white;
						uiController.zRetrievalText.text = targetNames;

						uiController.targetTextPanel.alpha = 1f;

						while (!Input.GetKeyDown(KeyCode.X))
						{
							yield return 0;
						}
						float dist = Vector3.Distance(spawnedObjects[randIndex[j]].transform.position, player.transform.position);
						UnityEngine.Debug.Log("spatial feedback dist for  " + spawnedObjects[randIndex[j]].gameObject.name + " is  " + dist.ToString());
						if(dist<15f)
                        {
							spatialFeedbackStatus.Add(true);
                        }
						else
                        {
							spatialFeedbackStatus.Add(false);
                        }
						spatialFeedbackPosition.Add(player.transform.position);
						trialLogTrack.LogRetrievalAttempt(spawnedObjects[randIndex[j]].gameObject, player);

						yield return new WaitForSeconds(0.2f);

					}
                    currentStage = TaskStage.Feedback;
					UnityEngine.Debug.Log("finished all retrievals");


                    finishedRetrieval = true;
                    SetCarBrakes(true);

					uiController.targetTextPanel.alpha = 0f;
					uiController.spatialRetrievalFeedbackPanel.alpha = 1f;
					yield return StartCoroutine("PerformSpatialFeedback");
                    UnityEngine.Debug.Log("finished spatial feedback");
					uiController.spatialRetrievalFeedbackPanel.alpha = 0f;

					UpdateLapDisplay();
					HideLapDisplay();

					trafficLightController.MakeVisible(false);
					yield return new WaitForSeconds(1f);
					yield return StartCoroutine("MoveForward");
					//MoveForward(); //we'll reset the movement to forward for the next navigation/encoding phase
					/*
					player.transform.position = startTransform.position;
					yield return StartCoroutine(ShowFixation());
					player.transform.position = startTransform.position;
					*/

					yield return 0;
				}
			}
			finishedRetrieval = false;


			for(int k=0;k<spawnedObjects.Count;k++)
            {
				Destroy(spawnedObjects[k]);
            }

			//reset everything before the next block begins
			spawnLocations.Clear();
			spawnedObjects.Clear();


			player.GetComponent<CarController>().ChangeMaxSpeed(40f);
			chequeredFlag.SetActive(true);

			//	objController.encodingList.Clear();
			LapCounter.lapCount = 0;
			player.transform.position = startTransform.position;

			uiController.targetTextPanel.alpha = 0f;
			SetCarBrakes(true);
			trialLogTrack.LogTaskStage(currentStage, false);
		}
		yield return null;
	}

	IEnumerator PerformSpatialFeedback()
    {
		List<GameObject> indicatorsList = new List<GameObject>();
		overheadCam.SetActive(true);
		feedbackQuad.SetActive(true);


		for (int k = 0; k < spawnedObjects.Count; k++)
		{
			spawnedObjects[k].GetComponent<VisibilityToggler>().TurnVisible(true);
			//spawnedObjects[k].GetComponent<FacePosition>().ShouldFacePlayer = false;
			//spawnedObjects[k].GetComponent<FacePosition>().TargetPositionTransform = overheadCam.transform;
			//spawnedObjects[k].GetComponent<FacePosition>().ShouldFaceOverheadCam = true;

			int childCount = spawnedObjects[k].transform.childCount;
			spawnedObjects[k].transform.GetChild(childCount - 1).gameObject.GetComponent<FacePosition>().ShouldFacePlayer = false;
			spawnedObjects[k].transform.GetChild(childCount - 1).gameObject.GetComponent<FacePosition>().TargetPositionTransform = overheadCam.transform;
            spawnedObjects[k].transform.GetChild(childCount - 1).gameObject.GetComponent<FacePosition>().ShouldFaceOverheadCam = true;

			spawnedObjects[k].transform.GetChild(childCount - 1).localScale *= 10f;
			GameObject prefabToSpawn = null;
			if(spatialFeedbackStatus[k])
            {
				prefabToSpawn = correctIndicator;
            }
			else
            {
				prefabToSpawn = wrongIndicator;
            }
			GameObject indicatorObj = Instantiate(prefabToSpawn, spatialFeedbackPosition[k], Quaternion.identity) as GameObject;
			indicatorsList.Add(indicatorObj);

			yield return new WaitForSeconds(1f);

		}

		while(!Input.GetKeyDown(KeyCode.Space))
        {
			yield return 0;
		}

		for (int i=0;i<indicatorsList.Count;i++)
        {
			Destroy(indicatorsList[i]);
        }
		indicatorsList.Clear();

		overheadCam.SetActive(false);
		feedbackQuad.SetActive(false);
		yield return null;

    }


	public IEnumerator StartVerbalRetrieval(GameObject objectQueried)
	{
		SetCarBrakes(true);
		yield return new WaitForSeconds(1f);
		uiController.verbalInstruction.alpha = 1f;
		string fileName = trialCount.ToString() + "_" + retCount.ToString();
		audioRecorder.beepHigh.Play();


		//start recording
		yield return StartCoroutine(audioRecorder.Record(sessionDirectory + "audio", fileName, recallTime));
		trialLogTrack.LogVerbalRetrievalAttempt(objectQueried, fileName);
		//play off beep
		audioRecorder.beepLow.Play();

		retCount++;
		uiController.verbalInstruction.alpha = 0f;
		SetCarBrakes(false);
		yield return null;
    }

	List<Transform> GetValidStartTransforms()
    {
		List<Transform> result = new List<Transform>();
	//	WaypointCircuit currentCircuit = player.GetComponent<WaypointProgressTracker>().leftCircuit;

		List<int> excludedIndex = new List<int>();
		for(int i=0;i<listLength;i++)
        {
            for (int j = 0; j < startableTransforms.Count; j++)
            {
                float dist = Vector3.Distance(startableTransforms[j].position, spawnedObjects[i].transform.position);
                if (dist < 1f)
				{
					excludedIndex.Add(j);
					j = startableTransforms.Count;
                }


            }
        }

		for(int i=0;i<startableTransforms.Count;i++)
        {
			result.Add(startableTransforms[i]);
        }

		for(int i=0;i<excludedIndex.Count;i++)
        {
			result.RemoveAt(excludedIndex[i]);
        }

		return result;
    }
	public void ShowPathDirection()
    {
		switch(player.GetComponent<WaypointProgressTracker>().currentDirection)
        {
			case WaypointProgressTracker.TrackDirection.Left:
				break;
			case WaypointProgressTracker.TrackDirection.Right:
				break;

        }

    }



	IEnumerator ShowFixation()
	{
		trialLogTrack.LogFixation(true);
		uiController.fixationPanel.alpha = 1f;
		uiController.fixationCross.alpha = 1f;
		float totalFixationTime = fixedTime + UnityEngine.Random.Range(0.1f, 0.3f);
		uiController.fixationCross.alpha = 0f;
		yield return new WaitForSeconds(totalFixationTime);
		uiController.fixationPanel.alpha = 0f;

		trialLogTrack.LogFixation(false);
		yield return null;
	}
	
	IEnumerator PickEncodingLocations()
	{
		List<int> intPicker = new List<int>();
		List<Vector3> waypointLocations = new List<Vector3>();
		List<Vector3> chosenEncodingLocations = new List<Vector3>();
		for(int i=0;i< spawnableWaypoints.Count;i++)
		{
			intPicker.Add(i);
			waypointLocations.Add(spawnableWaypoints[i].position);
		}

		List<int> tempStorage = new List<int>();

		//UnityEngine.Debug.Log("list length is " + listLength.ToString());
		for(int i=0;i< listLength; i++)
		{
			//UnityEngine.Debug.Log("int picker count " + intPicker.Count.ToString());
			int randIndex= UnityEngine.Random.Range(0, intPicker.Count-1); // we won't be picking too close to beginning/end
			int randInt = intPicker[randIndex];
			//UnityEngine.Debug.Log("rand int " + randInt.ToString());
			//UnityEngine.Debug.Log("waypoint location count " + waypointLocations.Count.ToString());
			//UnityEngine.Debug.Log("randindex " + randIndex.ToString());
			//UnityEngine.Debug.Log("randint " + randInt.ToString());
			chosenEncodingLocations.Add(waypointLocations[randInt]);
			//waypointLocations.RemoveAt(randIndex);
			string temp = "";
			for(int j=0;j<intPicker.Count;j++)
            {
				temp += intPicker[j].ToString() + ",";
            }
			uiController.debugText.text = temp;
			//UnityEngine.Debug.Log("pre count " + intPicker.Count.ToString());

			//UnityEngine.Debug.Log("picked " + randInt.ToString());

			//UnityEngine.Debug.Log("removing  " + intPicker[randIndex].ToString());
			intPicker.RemoveAt(randIndex);
		//	yield return StartCoroutine(WaitForActionButton());
			if(randIndex - 2>0 && randIndex + 2 < intPicker.Count - 1)
            {
				//UnityEngine.Debug.Log("removing " + intPicker[randIndex].ToString() + " and " + intPicker[randIndex+1].ToString());
				intPicker.RemoveAt(randIndex);
				intPicker.RemoveAt(randIndex);
				//UnityEngine.Debug.Log("removing " + intPicker[randIndex -1].ToString() + " and " + intPicker[randIndex-2].ToString());
				intPicker.RemoveAt(randIndex-1);
				intPicker.RemoveAt(randIndex - 1);
			}

			//UnityEngine.Debug.Log("post count " + intPicker.Count.ToString());
			/*
			if(randInt > 0)
			{
				waypointLocations.RemoveAt(randInt - 1);
			}
			if(randInt < waypointLocations.Count)
			{
				waypointLocations.RemoveAt(randInt + 1);
			}
			*/
			tempStorage.Add(randInt);
			spawnLocations.Add(chosenEncodingLocations[i]);
		}
		for(int i=0;i<tempStorage.Count;i++)
        {

        }



		yield return null;
	}
	
	public IEnumerator StopCarTemporarily()
    {
		SetCarBrakes(true);
		yield return new WaitForSeconds(1.5f);
		SetCarBrakes(false);
		yield return null;
    }
	IEnumerator SpawnEncodingObjects()
	{
		UnityEngine.Debug.Log("number of spawn locations " + spawnLocations.Count.ToString());
		for(int i=0;i<spawnLocations.Count;i++)
		{
			GameObject encodingObj = Instantiate(objController.encodingList[i], new Vector3(spawnLocations[i].x,spawnLocations[i].y+1.5f,spawnLocations[i].z), Quaternion.identity) as GameObject;
			spawnedObjects.Add(encodingObj);
			trialLogTrack.LogEncodingItemSpawn(encodingObj.name.Split('(')[0], encodingObj.transform.position);
			encodingObj.GetComponent<FacePosition>().ShouldFacePlayer = true;
			GameObject textObj = Instantiate(objController.textPrefab, new Vector3(spawnLocations[i].x, spawnLocations[i].y + 4f, spawnLocations[i].z), Quaternion.identity) as GameObject;
			textObj.transform.parent = encodingObj.transform;
			textObj.transform.GetChild(0).GetComponent<TextMeshPro>().text = encodingObj.name.Split('(')[0];
			encodingObj.GetComponent<FacePosition>().TargetPositionTransform = player.transform;
			textObj.GetComponent<FacePosition>().TargetPositionTransform = player.transform;
			encodingObj.GetComponent<VisibilityToggler>().TurnVisible(false);
		}
		yield return null;
	}

	public IEnumerator WaitForActionButton()
	{
		while(!Input.GetKeyDown(KeyCode.Space))
		{
			yield return 0;
		}
		yield return null;
	}

	public void SetCarBrakes(bool shouldStop)
    {
		trialLogTrack.LogCarBrakes(shouldStop);
		player.GetComponent<Rigidbody>().isKinematic = shouldStop;
    }

	IEnumerator GenerateRetrievalList()
	{
		List<int> tempList = new List<int>();
		List<int> retrievalSeqList = new List<int>();
		for (int i = 0; i < listLength; i++)
		{
			tempList.Add(i);
		}
		for(int j=0;j< listLength; j++)
        {
			int randIndex = UnityEngine.Random.Range(0, tempList.Count);
			retrievalSeqList.Add(tempList[randIndex]);
			tempList.RemoveAt(randIndex);
        }

		retrievalObjList = new List<GameObject>();
		retrievalPositions = new List<Vector3>();

		//finally add them to the retrieval lists
		for (int i = 0; i < retrievalSeqList.Count; i++)
		{
			UnityEngine.Debug.Log("adding " + spawnedObjects[retrievalSeqList[i]]);
			UnityEngine.Debug.Log("adding " + spawnLocations[retrievalSeqList[i]]);
			retrievalObjList.Add(spawnedObjects[retrievalSeqList[i]]);
			retrievalPositions.Add(spawnLocations[retrievalSeqList[i]]);
		}

		yield return null;
	}

	public IEnumerator SpawnObjects()
    {
		while (LapCounter.lapCount < maxLaps)
		{
			//retrieval lap
			if (LapCounter.isRetrieval)
			{
				SetCarBrakes(true);
				List<Vector3> chosenLocations = new List<Vector3>();
				yield return StartCoroutine(GenerateRetrievalList());
				for (int i = 0; i < blockLength; i++)
				{
					UnityEngine.Debug.Log("retrieval loop " + i.ToString());
					LapCounter.finishedLap = false;
					SetCarBrakes(true);
					UnityEngine.Debug.Log("retrieval obj list outside " + retrievalObjList.Count.ToString());
					UnityEngine.Debug.Log(" what is " + retrievalObjList[i].ToString());
					string objName= retrievalObjList[i].gameObject.name.Split('(')[0];
					uiController.zRetrievalText.text = objName;
					uiController.itemName.text = objName;
					uiController.retrievalTextPanel.alpha = 1f;
					while(!Input.GetKeyDown(KeyCode.Space))
                    {
						yield return 0;
					}  
					uiController.retrievalTextPanel.alpha = 0f;
					uiController.targetTextPanel.alpha = 1f;
					uiController.itemName.text = objName;
					SetCarBrakes(false);
					yield return new WaitForSeconds(1f);
					while(!Input.GetKeyDown(KeyCode.Space) && !LapCounter.finishedLap)
                    {
						yield return 0;
                    }
					chosenLocations.Add(player.transform.position);
					float difference = Vector3.Distance(retrievalPositions[i], player.transform.position);
					UnityEngine.Debug.Log("difference: " + difference.ToString());

					while(!LapCounter.finishedLap)
                    {
						yield return 0;
                    }
					UnityEngine.Debug.Log("finished waiting for lap in retrieval");
					LapCounter.finishedLap = false;

					
				}
				LapCounter.isRetrieval = false;
				UnityEngine.Debug.Log("retrieval mode off");
				uiController.targetTextPanel.alpha = 0f;
				uiController.retrievalTextPanel.alpha = 0f;
				SetCarBrakes(false);

				//reset retrieval lists
				spawnedObjects.Clear();
				spawnLocations.Clear();
				yield return 0;
			}
			else
			{ 
			float randTimeSpawn = UnityEngine.Random.Range(5f, 45f);
			UnityEngine.Debug.Log("random spawn timer " + randTimeSpawn.ToString());
			float timer = 0f;
			while (timer < randTimeSpawn)
			{
				timer += Time.deltaTime;
			//	UnityEngine.Debug.Log(timer.ToString() + "/" + randTimeSpawn.ToString());
				yield return 0;
			}
			UnityEngine.Debug.Log("initiating spawn sequence");
			yield return StartCoroutine(objController.InitiateSpawnSequence());

			UnityEngine.Debug.Log("objlapper " + objLapper.ToString() + " lapcount " + LapCounter.lapCount.ToString());
			while (objLapper == LapCounter.lapCount)
			{
				yield return 0;
			}
			UnityEngine.Debug.Log("objlapper incrementing");
			objLapper++;
			}
			yield return 0;
		}

		yield return null;
    }

	void LockZAnswer()
	{
		uiController.zRetrievalText.color = Color.gray;
	}
	void LockMAnswer()
	{
		uiController.mRetrievalText.color = Color.gray;
	}

	IEnumerator MoveForward()
    {
		if (player.GetComponent<CarAIControl>().isReverse)
		{
			SetCarBrakes(true);

			player.GetComponent<WaypointProgressTracker>().SetActiveDirection(WaypointProgressTracker.TrackDirection.Left);

			player.GetComponent<CarAIControl>().ForwardMovement();
			trialLogTrack.LogReverseMovement(false);
			trialLogTrack.LogForwardMovement(true);
			yield return new WaitForSeconds(0.25f);
			SetCarBrakes(false);
		}
		yield return null;
	}

	IEnumerator MoveReverse()
    {
		if (!player.GetComponent<CarAIControl>().isReverse)
		{
			SetCarBrakes(true);


			player.GetComponent<WaypointProgressTracker>().SetActiveDirection(WaypointProgressTracker.TrackDirection.Reverse);
			player.GetComponent<CarAIControl>().ReverseMovement();
			trialLogTrack.LogForwardMovement(false);
			trialLogTrack.LogReverseMovement(true);
			yield return new WaitForSeconds(0.25f);
			SetCarBrakes(false);
		}
		yield return null;
	}


	// Update is called once per frame
	void Update () {
		//UnityEngine.Debug.Log("player current  speed " + player.GetComponent<CarController>().CurrentSpeed.ToString());
		//UnityEngine.Debug.Log("car speed " + carSpeed.ToString());
			if (currentStage == TaskStage.SpatialRetrieval)
			{
			if (Input.GetKey(KeyCode.UpArrow))
		{
			SetCarBrakes(false);
			if (wasMovingReverse)
				{
					carSpeed = 0f;
					movementTimer = 0f;
					wasMovingReverse = false;
				}
			/*
				if(movementTimer>1f && player.GetComponent<CarController>().CurrentSpeed < 1f)
			{
				UnityEngine.Debug.Log("force rotated");
				//player.transform.Rotate(2f, 2f, 2f, Space.Self);
				player.transform.position = new Vector3(player.transform.position.x,player.transform.position.y + 3f, player.transform.position.z);
				player.GetComponent<CarController>().SetCurrentSpeed(2f);
				movementTimer = 0f;

				}
				movementTimer += Time.deltaTime;
			*/
				wasMovingForward = true;
				//UnityEngine.Debug.Log("moving forward");
				StopCoroutine("MoveReverse");
				StartCoroutine("MoveForward");
				carSpeed += 0.5f;
			if (carSpeed > 40f)
			{
				//UnityEngine.Debug.Log("exceeded max speed");
				carSpeed = player.GetComponent<CarController>().MaxSpeed;
			}
			//UnityEngine.Debug.Log("increasing speed " + carSpeed.ToString());


			}
			else if (Input.GetKey(KeyCode.DownArrow))
		{
			SetCarBrakes(false);
			if (wasMovingForward)
                {
					carSpeed = 0f;
					movementTimer = 0f;
					wasMovingForward = false;
                }
				wasMovingReverse = true;
			/*
				if (movementTimer > 1f && player.GetComponent<CarController>().CurrentSpeed < 1f)
			{
				UnityEngine.Debug.Log("force rotated");
				//player.transform.Rotate(2f, 2f, 2f, Space.Self);
				player.transform.position = new Vector3(player.transform.position.x, player.transform.position.y + 3f, player.transform.position.z);
				player.GetComponent<CarController>().SetCurrentSpeed(2f);
					movementTimer = 0f;
					

				}
				movementTimer += Time.deltaTime;
			*/
				//UnityEngine.Debug.Log("moving reverse");
				StopCoroutine("MoveForward");
				StartCoroutine("MoveReverse"); 
				carSpeed += 0.5f;
				if (carSpeed > 40f)
				{
					//UnityEngine.Debug.Log("exceeded max speed");
					carSpeed = player.GetComponent<CarController>().MaxSpeed;
				}
				//UnityEngine.Debug.Log("increasing speed " + carSpeed.ToString());

			}
			else
            {
			SetCarBrakes(true);
				carSpeed-=0.5f;
				if (carSpeed <= 0f)
				carSpeed = 0f;
			}
		//	carSpeed = Mathf.Clamp(carSpeed,0f, player.GetComponent<CarController>().MaxSpeed);
		//UnityEngine.Debug.Log("clamped speed " + carSpeed.ToString());
		/*
		if (carSpeed >= 0.5f)
			player.GetComponent<CarController>().ChangeMaxSpeed(carSpeed);
		else
		{
			carSpeed = 0.2f;
			player.GetComponent<CarController>().ChangeMaxSpeed(carSpeed);
		}
		*/
		//UnityEngine.Debug.Log("car speed " + carSpeed.ToString());
		}
		/*
		if (currentStage == Experiment.TaskStage.Retrieval)
		{
			if (Input.GetKeyDown(KeyCode.Z))
			{
				LockZAnswer();
				trialLogTrack.LogRetrievalAttempt(slowZoneObj,player.gameObject);
			}
			if(Input.GetKeyDown(KeyCode.M))
			{
				LockMAnswer();

				trialLogTrack.LogRetrievalAttempt(speedZoneObj,player.gameObject);
			}
			
			if(Input.GetKeyDown(KeyCode.UpArrow))
            {
				player.GetComponent<WaypointProgressTracker>().SetActiveDirection(WaypointProgressTracker.TrackDirection.Left);
            }
			else if(Input.GetKeyDown(KeyCode.DownArrow))
            {

				player.GetComponent<WaypointProgressTracker>().SetActiveDirection(WaypointProgressTracker.TrackDirection.Reverse);
			}
			
		}
		
		if (Input.GetKeyDown(KeyCode.UpArrow))
		{
			player.GetComponent<WaypointProgressTracker>().SetActiveDirection(WaypointProgressTracker.TrackDirection.Left);
		}
		else if (Input.GetKeyDown(KeyCode.DownArrow))
		{

			player.GetComponent<WaypointProgressTracker>().SetActiveDirection(WaypointProgressTracker.TrackDirection.Reverse);
		}
		
		*/



	}

	public void OnExit()
	{ //call in scene controller when switching to another scene!
		if (isLogging)
		{
			subjectLog.close();
			eegLog.close();
		}
	}

	void OnApplicationQuit()
	{
		if (isLogging)
		{
			subjectLog.close();
			eegLog.close();
			//File.Copy("/Users/" + System.Environment.UserName + "/Library/Logs/Unity/Player.log", sessionDirectory + "Player.log");
		}
	}
}
