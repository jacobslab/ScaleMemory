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

	public static bool onCorrectArm = false; //this will be read by treasure chest on each arm to determine its contents if player is on correct arm (reward) or not (empty)

	public List<Transform> leftSpawnableWaypoints;
	public List<Transform> rightSpawnableWaypoints;


	//traffic light controller
	public TrafficLightController trafficLightController;

	public enum TaskStage
	{
		ItemScreening,
		TrackScreening,
		Encoding,
		Retrieval,
		PostTaskScreening
	}

	public TaskStage currentStage = TaskStage.ItemScreening;

	public List<GameObject> spawnedObjects;
	public List<Vector3> spawnLocations;

	private List<GameObject> retrievalObjList;
	private List<Vector3> retrievalPositions;

	public GameObject treasureChestPrefab;
	private GameObject leftChest;
	private GameObject rightChest;

	private Vector3 leftSpawnPos;
	private Vector3 rightSpawnPos;

	public AudioSource feedbackAudio;
	public AudioClip magicWand;
	public AudioClip smokePoof;
	public GameObject coinPrefab;


	public Camera itemScreeningCam;

	public WaypointCircuit waypointCircuit;

	public static int blockLength = 2;

	public static int itemsPerBlock = 2;

	private List<GameObject> itemScreeningSeq;

	public Transform itemScreeningTransform;

	public Transform startTransform;

	//prefabs
	public GameObject slowZonePrefab;
	public GameObject speedZonePrefab;

	private GameObject leftSpawnObj;
	private GameObject rightSpawnObj;

	private bool pickOnce = false;
	private GameObject correctChest;
	//blackrock variables
	public static string ExpName = "T2";
	public static string BuildVersion = "0.9.8";
	public static bool isSystem2 = false;


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

	public GameObject chequeredFlag;

	public int reward = 0;

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

	public static bool isCrashing = false; //activated by one of the turn zones when you fail to press a turn prompted keypress in time

	private float fixedTime = 1f;

	private int maxLaps = 12;

	private float prevLapTime = 0f;
	private float bestLapTime = 1000f;


	public TrialLogTrack trialLogTrack;
	private int objLapper = 0;

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
	}
	// Use this for initialization
	void Start()
	{
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
		int leftRandInt = Random.Range(0, leftSpawnableWaypoints.Count - 1);
		int rightRandInt = Random.Range(0, rightSpawnableWaypoints.Count - 1);

		/*
		if (leftRandInt <= 7 || leftRandInt > 26)
		{
			leftYAngle = 90f;
		}
		else
		{
			leftYAngle = 0f;
		}

		if (rightRandInt <= 7 || rightRandInt > 26)
		{
			rightYAngle = 90f;
		}
		else
		{
			rightYAngle = 0f;
		}
		*/
		//randomize which side gets slow and speed zones

		leftSpawnPos = leftSpawnableWaypoints[leftRandInt].position;
		rightSpawnPos = rightSpawnableWaypoints[rightRandInt].position;

		GameObject chestOne = Instantiate(treasureChestPrefab, leftSpawnableWaypoints[leftRandInt].position, Quaternion.Euler(new Vector3(0f, 90f, 0f))) as GameObject;
			trialLogTrack.LogTreasureChest(leftSpawnPos);
			leftChest = chestOne;

			GameObject chestTwo = Instantiate(treasureChestPrefab, rightSpawnableWaypoints[rightRandInt].position, Quaternion.Euler(new Vector3(0f, 90f, 0f))) as GameObject;
			trialLogTrack.LogTreasureChest(rightSpawnPos);
			rightChest = chestTwo;
		

		yield return null;

	}

	IEnumerator SpawnChestAgain()
    {
		//destroy existing chests first
		if (leftChest != null)
			Destroy(leftChest);
		if (rightChest != null)
			Destroy(rightChest);

		GameObject chestOne = Instantiate(treasureChestPrefab, leftSpawnPos, Quaternion.Euler(new Vector3(0f, 90f, 0f))) as GameObject;
		leftChest = chestOne;

		GameObject chestTwo = Instantiate(treasureChestPrefab, rightSpawnPos, Quaternion.Euler(new Vector3(0f, 90f, 0f))) as GameObject;
		rightChest = chestTwo;
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
		//	yield return StartCoroutine(objController.SelectEncodingItems());
		//yield return StartCoroutine("BeginItemScreening");
		//	StartCoroutine("RandomizeTravelSpeed");
		//	yield return StartCoroutine("BeginTrackScreening");

		reward = 0;
		yield return StartCoroutine("SpawnZones");
		//repeat blocks twice
		yield return StartCoroutine("BeginTaskBlock");

		uiController.endSessionPanel.alpha = 1f;
		yield return StartCoroutine(WaitForActionButton());

		Application.Quit();
		yield return null;
	}


	public void PushCarTowardsTarget(Transform moveTarget)
    {
		Vector3 dir =  moveTarget.position - player.transform.position;
		player.gameObject.GetComponent<Rigidbody>().AddForce(dir * 100f, ForceMode.Acceleration);
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
		for(int i=0;i<itemsPerBlock;i++)
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
		trialLogTrack.LogTaskStage(currentStage, true);
		player.gameObject.SetActive(false);
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
		trialLogTrack.LogTaskStage(currentStage,false);
		yield return null;
	}

	public void ShowTurnDirection(WaypointProgressTracker.TrackDirection turnDirection,GameObject associatedTurnZone)
    {
		switch (turnDirection)
        {
			case WaypointProgressTracker.TrackDirection.Left:
				uiController.leftArrowProgress.alpha = 1f;
				break;
			case WaypointProgressTracker.TrackDirection.Right:
				uiController.rightArrowProgress.alpha = 1f;
				break;
        }
    }

	public void HideTurnDirection(WaypointProgressTracker.TrackDirection turnDirection)
	{
		switch (turnDirection)
		{
			case WaypointProgressTracker.TrackDirection.Left:
				uiController.leftArrowProgress.alpha = 0f;
				break;
			case WaypointProgressTracker.TrackDirection.Right:
				uiController.rightArrowProgress.alpha = 0f;
				break;
		}

	}

	public IEnumerator BeginCrashSequence(Transform crashZone)
	{
		UnityEngine.Debug.Log("beginning crash sequence");
		SetCarBrakes(true);
		Vector3 origTransform = player.transform.position;
		float lerpTimer = 0f;

		while(lerpTimer < 2f)
        {
			//UnityEngine.Debug.Log("crash lerp " + lerpTimer.ToString());
			lerpTimer += Time.deltaTime;
			player.transform.position = Vector3.Lerp(origTransform, crashZone.position, lerpTimer/2f);
			yield return 0;
        }
		uiController.crashNotification.alpha = 1f;
		yield return new WaitForSeconds(2f);
		UnityEngine.Debug.Log("finished crashing");

		uiController.crashNotification.alpha = 0f;

		//conceal the transformation
		uiController.blackScreen.alpha = 1f;
		//make sure we transport the car back to the starting transform
		player.transform.position = startTransform.position + (Vector3.up * 15f);
		player.transform.rotation = startTransform.rotation;

		player.GetComponent<WaypointProgressTracker>().Reset(); //reset the waypoint system to begin from the beginning

		UnityEngine.Debug.Log("moved back to start");
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

	IEnumerator ShowRetrievalInstructions()
	{
		uiController.retrievalPanel.alpha = 1f;
		yield return StartCoroutine(WaitForActionButton());
		uiController.retrievalPanel.alpha = 0f;
		yield return null;
	}


	public IEnumerator WaitForTurnChoice(WaypointProgressTracker.TrackDirection correctDirection, Transform associatedCrashZone)
    {
		SetCarBrakes(true);
		yield return new WaitForSeconds(0.5f);
		uiController.leftArrow.alpha = 1f;
		uiController.rightArrow.alpha = 1f;
		uiController.choiceOrPanel.alpha = 1f;
		bool turnedCorrectly = false;

		WaypointProgressTracker.TrackDirection chosenDirection = WaypointProgressTracker.TrackDirection.None;
		while(!Input.GetKeyDown(KeyCode.LeftArrow) && !Input.GetKeyDown(KeyCode.RightArrow))
        {
			yield return 0;
		}
		if (Input.GetKey(KeyCode.LeftArrow))
		{
			UnityEngine.Debug.Log("chose left");
			player.GetComponent<WaypointProgressTracker>().SetActiveDirection(WaypointProgressTracker.TrackDirection.Left);
			chosenDirection = WaypointProgressTracker.TrackDirection.Left;
		}
		else if (Input.GetKey(KeyCode.RightArrow))
		{
			UnityEngine.Debug.Log("chose right");

			player.GetComponent<WaypointProgressTracker>().SetActiveDirection(WaypointProgressTracker.TrackDirection.Right);
			chosenDirection = WaypointProgressTracker.TrackDirection.Right;
		}
		if (chosenDirection == correctDirection)
        {
			UnityEngine.Debug.Log("turned correctly");
			turnedCorrectly = true;
			if(chosenDirection == WaypointProgressTracker.TrackDirection.Left)
            {
				uiController.youChoseLeft.alpha = 1f;
				//uiController.leftCorrectImagePanel.alpha = 1f;
				//uiController.rightIncorrectImagePanel.alpha = 1f;
            }
			else if(chosenDirection==WaypointProgressTracker.TrackDirection.Right)
			{
				uiController.youChoseRight.alpha = 1f;
				//uiController.rightCorrectImagePanel.alpha = 1f;
				//uiController.leftIncorrectImagePanel.alpha = 1f;
			}
			else
            {
				UnityEngine.Debug.Log("WARNING, KEYPRESS NOT REGISTERED PROPERLY");
            }
        }
		else
		{
			UnityEngine.Debug.Log("WRONG turn");
			turnedCorrectly = false; 
			if (chosenDirection == WaypointProgressTracker.TrackDirection.Left)
			{
				uiController.youChoseLeft.alpha = 1f;
				//uiController.leftIncorrectImagePanel.alpha = 1f;
				//uiController.rightCorrectImagePanel.alpha = 1f;
			}
			else
			{
				uiController.youChoseRight.alpha = 1f;
				//uiController.rightIncorrectImagePanel.alpha = 1f;
				//uiController.leftCorrectImagePanel.alpha = 1f;

			}
		}

		yield return new WaitForSeconds(1f);


		//reset everything
		uiController.leftArrow.alpha = 0f;
		uiController.rightArrow.alpha = 0f;
		uiController.choiceOrPanel.alpha = 0f;
		uiController.youChoseRight.alpha = 0f;
		uiController.youChoseLeft.alpha = 0f;
		uiController.leftIncorrectImagePanel.alpha = 0f;
		uiController.rightCorrectImagePanel.alpha = 0f;
		uiController.rightIncorrectImagePanel.alpha = 0f;
		uiController.leftCorrectImagePanel.alpha = 0f;
		
		SetCarBrakes(false);
		onCorrectArm = turnedCorrectly;
		//no crashing
		/*
		if(!turnedCorrectly)
			yield return StartCoroutine(BeginCrashSequence(associatedCrashZone));
		*/
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

	public void SetChequeredFlagStatus(bool isActive)
    {
		chequeredFlag.SetActive(isActive);
    }

	void HideLapDisplay()
    {
		uiController.lapTimePanel.alpha = 0f;
    }
	IEnumerator BeginTaskBlock()
	{
		currentStage = TaskStage.Encoding;
		trialLogTrack.LogTaskStage(currentStage, true);

		SetCarBrakes(true);
		//show instructions
		for (int i = 0; i < blockLength; i++)
		{
			LapCounter.lapCount = 0;
			trialLogTrack.LogInstructions(true);
			player.transform.position = startTransform.position;
			yield return StartCoroutine(ShowEncodingInstructions());
			trialLogTrack.LogInstructions(false);
		//	yield return StartCoroutine(PickEncodingLocations());
		//	yield return StartCoroutine(SpawnEncodingObjects()); //this will spawn all encoding objects on the track
			trafficLightController.MakeVisible(true);
			yield return StartCoroutine(trafficLightController.StartCountdownToGreen());
			SetCarBrakes(false);
			trafficLightController.MakeVisible(false);

			player.GetComponent<WaypointProgressTracker>().SetActiveDirection(WaypointProgressTracker.TrackDirection.Right);
			bool canChangeDirection = true;

			while (LapCounter.lapCount < 3)
			{
				UnityEngine.Debug.Log("lap count " + LapCounter.lapCount.ToString());
				//reset lap timer and show display
				ResetLapDisplay();

				//both chests are active; whether they're empty or filled with reward depends on player choice at decision point
				leftChest.SetActive(true);
				rightChest.SetActive(true);
				if (canChangeDirection)
				{
					//switch direction
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
				}
				UnityEngine.Debug.Log("began lap number : " + LapCounter.lapCount.ToString());

				//turn off chequered flag temporarily
				//SetChequeredFlagStatus(false);

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
				while (!Input.GetKeyDown(KeyCode.Space) && !forceStopped)
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
				if(!onCorrectArm)
                {
					uiController.alternateReminderPanel.alpha = 1f;
					yield return StartCoroutine(WaitForActionButton());
					uiController.alternateReminderPanel.alpha = 0f;
				}
				//RESET onCorrectArm before restarting the lap
				onCorrectArm = false;
				yield return StartCoroutine(SpawnChestAgain());

				yield return StartCoroutine(ShowFixation());
				player.transform.position = startTransform.position;
				yield return 0;
			}

			//retrieval time

			//hide encoding objects and text
			for (int j=0;j< spawnedObjects.Count;j++)
			{
				spawnedObjects[j].gameObject.SetActive(false);
				
			}

			//	uiController.itemOneName.text = spawnedObjects[0].name.Split('(')[0];
			//	uiController.itemTwoName.text = spawnedObjects[1].name.Split('(')[0];

		//	leftChest.GetComponent<VisibilityToggler>().TurnVisible(false);
		//	rightChest.GetComponent<VisibilityToggler>().TurnVisible(false);

			leftChest.SetActive(false);
			rightChest.SetActive(false);

			trialLogTrack.LogTaskStage(currentStage, false);
			currentStage = Experiment.TaskStage.Retrieval;
			trialLogTrack.LogTaskStage(currentStage, true);
			trialLogTrack.LogInstructions(true);
			player.transform.position = startTransform.position;
			yield return StartCoroutine(ShowRetrievalInstructions());
			trialLogTrack.LogInstructions(false);
			LapCounter.lapCount = 0; //reset lap count for retrieval 

			string targetNames = "";
			int targetMode = 0;
			GameObject targetChest = null;
			while (LapCounter.lapCount < 16)
			{
				pickOnce = false; //reset
				targetMode = LapCounter.lapCount % 2;
				switch (targetMode)
                {
					case 0:
						correctChest = leftChest;
						//	leftChest.SetActive(true);
						//	rightChest.SetActive(false);
						//	UnityEngine.Debug.Log("chose left direction");
						//	player.GetComponent<WaypointProgressTracker>().SetActiveDirection(WaypointProgressTracker.TrackDirection.Left);
						/*
						if(leftSpawnObj==slowZoneObj)
                        {

							uiController.zRetrievalText.text = "OIL PATCH :\n Press Z";
							targetNames = "OIL PATCH (Press Z)";
						}
						else
						{
							uiController.zRetrievalText.text = "SPEED ZONE :\n Press M";
							targetNames = "SPEED ZONE (Press M)";

						}
						*/
						break;
					case 1:
					//	leftChest.SetActive(false);
					//	rightChest.SetActive(true);
						UnityEngine.Debug.Log("chose right direction");
						correctChest = rightChest;
						player.GetComponent<WaypointProgressTracker>().SetActiveDirection(WaypointProgressTracker.TrackDirection.Right);
						/*
						if (rightSpawnObj == slowZoneObj)
						{

							uiController.zRetrievalText.text = "OIL PATCH :\n Press Z";
							targetNames = "OIL PATCH (Press Z)";
						}
						else
						{
							uiController.zRetrievalText.text = "SPEED ZONE :\n Press M";
							targetNames = "SPEED ZONE (Press M)";

						}
						*/
						break;
						/*
					case 2:
						UnityEngine.Debug.Log("both zones are the target");
						slowZoneObj.SetActive(true);
						speedZoneObj.SetActive(true);
						targetNames = "OIL PATCH (Press Z)\n and \n SPEED ZONE (Press M)";
						uiController.zRetrievalText.text = "OIL PATCH : Press Z \n SPEED ZONE: Press M";
						break;
						*/
                }

				//uiController.zRetrievalText.color = Color.white;
				//uiController.retrievalTextPanel.alpha = 1f;
				//uiController.itemName.text = targetNames;
				//yield return StartCoroutine(WaitForActionButton());
				//uiController.retrievalTextPanel.alpha = 0f;


			trafficLightController.MakeVisible(true);
			yield return StartCoroutine(trafficLightController.StartCountdownToGreen());
			SetCarBrakes(false);

			//reset lap timer and show display
			ResetLapDisplay();

			trafficLightController.MakeVisible(false);
			uiController.targetTextPanel.alpha = 1f;
		//	uiController.zRetrievalText.text = spawnedObjects[0].name.Split('(')[0] + " : Z";
		//	uiController.zRetrievalText.color = Color.white;
		//	uiController.mRetrievalText.text =  spawnedObjects[1].name.Split('(')[0] + " : M";
		//	uiController.mRetrievalText.color = Color.white;

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
				while (!Input.GetKeyDown(KeyCode.Space) && !forceStopped)
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
				SetCarBrakes(true);


				UpdateLapDisplay();
				HideLapDisplay();

				trafficLightController.MakeVisible(false);
				yield return new WaitForSeconds(1f);
				player.transform.position = startTransform.position;
				yield return StartCoroutine(ShowFixation());
				player.transform.position = startTransform.position;

			yield return 0;
			}


			//reset everything before the next block begins
			spawnLocations.Clear();
			spawnedObjects.Clear();
		//	objController.encodingList.Clear();
			LapCounter.lapCount = 0;
			player.transform.position = startTransform.position;
			uiController.targetTextPanel.alpha = 0f;

			trialLogTrack.LogTaskStage(currentStage, false);
		}
		yield return null;
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
	/*
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

		for(int i=0;i<itemsPerBlock;i++)
		{
			int randInt = UnityEngine.Random.Range(3, intPicker.Count-3); // we won't be picking too close to beginning/end
			chosenEncodingLocations.Add(waypointLocations[randInt]);
			waypointLocations.RemoveAt(randInt);
			if(randInt > 0)
			{
				waypointLocations.RemoveAt(randInt - 1);
			}
			if(randInt < waypointLocations.Count)
			{
				waypointLocations.RemoveAt(randInt + 1);
			}
			spawnLocations.Add(chosenEncodingLocations[i]);
		}
		yield return null;
	}
	*/

	IEnumerator SpawnEncodingObjects()
	{
		UnityEngine.Debug.Log("number of spawn locations " + spawnLocations.Count.ToString());
		for(int i=0;i<spawnLocations.Count;i++)
		{
			GameObject encodingObj = Instantiate(objController.encodingList[i], new Vector3(spawnLocations[i].x,spawnLocations[i].y+1.5f,spawnLocations[i].z), Quaternion.identity) as GameObject;
			spawnedObjects.Add(encodingObj);
			trialLogTrack.LogEncodingItemSpawn(encodingObj.name.Split('(')[0], encodingObj.transform.position);
			encodingObj.GetComponent<FacePosition>().ShouldFacePlayer = true;
			GameObject textObj = Instantiate(objController.textPrefab, new Vector3(spawnLocations[i].x, spawnLocations[i].y + 3f, spawnLocations[i].z), Quaternion.identity) as GameObject;
			textObj.transform.parent = encodingObj.transform;
			textObj.transform.GetChild(0).GetComponent<TextMeshPro>().text = encodingObj.name.Split('(')[0];
			encodingObj.GetComponent<FacePosition>().TargetPositionTransform = player.transform;
			textObj.GetComponent<FacePosition>().TargetPositionTransform = player.transform;
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

	public IEnumerator WaitForTreasurePause()
    {
		yield return null;
    }

	IEnumerator GenerateRetrievalList()
	{
		List<int> tempList = new List<int>();
		List<int> retrievalSeqList = new List<int>();
		for (int i = 0; i < itemsPerBlock; i++)
		{
			tempList.Add(i);
		}
		for(int j=0;j< itemsPerBlock; j++)
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

	void LockChestAnswer()
    {
		//uiController.chestRetrievalText.color = Color.gray;
    }

	void LockZAnswer()
	{
		uiController.zRetrievalText.color = Color.gray;
	}
	void LockMAnswer()
	{
		uiController.mRetrievalText.color = Color.gray;
	}

	IEnumerator EvaluateResponse(Vector3 playerLoc)
    {
		SetCarBrakes(true);
		bool turnedCorrectly = Experiment.onCorrectArm;
		bool guessedLocCorrectly = false;

		float dist = Vector3.Distance(correctChest.transform.position, playerLoc);
		if(dist<20f)
        {
			guessedLocCorrectly = true;
        }

		bool correctFeedback = false;
		
		//turned correctly AND guessed correctly
		if(guessedLocCorrectly && onCorrectArm)
        {
			feedbackAudio.PlayOneShot(magicWand);
			uiController.IncrementAndUpdateReward();
			GameObject coinObj = Instantiate(coinPrefab, player.transform.position + player.transform.forward * 5f, Quaternion.Euler(new Vector3(90f, 90f, 0f))) as GameObject;
			yield return StartCoroutine(ShowFeedbackPanel(uiController.correctLocationPanel));
			Destroy(coinObj);
        }
		//turned correctly but guessed wrong
		else if(!guessedLocCorrectly && onCorrectArm)
		{
			feedbackAudio.PlayOneShot(smokePoof);
			yield return StartCoroutine(ShowFeedbackPanel(uiController.wrongLocationCorrectTurnPanel));

		}
		//turned wrong but guessed right
		else if(guessedLocCorrectly && !onCorrectArm)
		{
			feedbackAudio.PlayOneShot(smokePoof);
			yield return StartCoroutine(ShowFeedbackPanel(uiController.wrongTurnPanel));

		}
		//did everything wrong
		else
        {
			feedbackAudio.PlayOneShot(smokePoof);
			yield return StartCoroutine(ShowFeedbackPanel(uiController.wrongTurnWrongLocationPanel));
		}

		SetCarBrakes(false);
		yield return null;

    }

	IEnumerator ShowFeedbackPanel(CanvasGroup feedbackPanel)
    {
		feedbackPanel.alpha = 1f;
		yield return StartCoroutine(WaitForActionButton());
		feedbackPanel.alpha = 0f;
		yield return null;
    }

	// Update is called once per frame
	void Update () {
		if (currentStage == Experiment.TaskStage.Retrieval && !pickOnce)
		{
			if(Input.GetKey(KeyCode.X))
            {
				LockChestAnswer();
				pickOnce = true;
				Vector3 playerPos = player.transform.position;
				StartCoroutine("EvaluateResponse", playerPos);
                trialLogTrack.LogChestRetrievalAttempt(correctChest, player.gameObject);
			}
			/*
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
			*/
		}

#if UNITY_EDITOR
		if(Input.GetKeyDown(KeyCode.L))
        {
			SetCarBrakes(false);
        }
		if (Input.GetKeyDown(KeyCode.H))
		{
			SetCarBrakes(true);
		}
#endif



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
