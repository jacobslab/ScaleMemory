using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.PostProcessing;
using UnityStandardAssets.Vehicles.Car;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.Utility;
public class RacingTimelocked : MonoBehaviour {

	//ui element

	//speed
	public float minSpeedFactor=8f;
	public float maxSpeedFactor=20f;
	public float speedFactor=10f;

	public float maxSpeed=75f;
	public Rigidbody carBody;
	public Transform startTransform;

	private float minX=50f;
	private float maxX=330f;
	private float fixedDistance = 0f;

	private float minTime=5f;
	private float maxTime=15f;
	private float fixedTime=0f;

	private List<float> fixedDistanceList;
	private List<float> fixedTimeList;

	private int[] chequeredFlagIndices;
	public List<Transform> chequeredFlagTransforms;

	/// <summary>
	///  +40 = 0 
	/// +20 = 1
	/// 0 = 2
	/// -20 = 3
	/// -40 = 4
	/// </summary>
	private int currentChequeredFlagIndex=0; 

	private int currentCircuit=1; //setting center lane to be default
	public List<WaypointCircuit> waypoints; // 0 is left, 1 is center and 2 is right
	float responseFactor=1f;


	//config 
	private int lapsToBeFinished=3;

	//timer/measure
	public SimpleTimer simpleTimer;
	public SimpleDistanceMeasure distanceMeasure;

	public ChequeredFlag chequeredFlag;
	private int coinsCollected=0;

	public Camera standardCam;
	public Camera freeLookCam;

	public UIController uiController;
	private PostProcessingProfile pp_profile;
	private CarController carController;
	private WaypointProgressTracker waypointTracker;
	private CarAIControl carAI;
	enum TrialType {
		Distance,
		Time};

	private TrialType trialType;

	//singleton
	private static RacingTimelocked _instance;
	public static RacingTimelocked Instance
	{
		get
		{
			return _instance;
		}
	}

	void Awake()
	{
		if (_instance != null)
		{
			Debug.Log("Instance already exists!");
			return;
		}
		_instance = this;

	}
	void OnEnable()
	{
		var behaviour = carBody.transform.GetChild(0).gameObject.GetComponent<PostProcessingBehaviour>();

		if (behaviour.profile == null)
		{
			enabled = false;
			return;
		}

		chequeredFlagIndices = new int[3];

		standardCam.enabled = true;
		freeLookCam.transform.parent.gameObject.GetComponent<FirstPersonController> ().enabled = false;
		freeLookCam.enabled = false;

		pp_profile = Instantiate(behaviour.profile);
		behaviour.profile = pp_profile;
		pp_profile.motionBlur.enabled = false;

		carController = carBody.gameObject.GetComponent<CarController> ();
		carController.ChangeMaxSpeed(maxSpeed);
		carAI = carBody.gameObject.GetComponent<CarAIControl> ();
		carAI.ChangeSpeedFactor (speedFactor);

		waypointTracker = carBody.gameObject.GetComponent<WaypointProgressTracker> ();
		waypointTracker.circuit = waypoints[currentCircuit]; //setting the center lane as default
		//instantiate the lists
		fixedDistanceList=new List<float>();
		fixedTimeList = new List<float> ();

	}

	// Use this for initialization
	void Start () {
		ChequeredFlag.lapsCompleted = 0;

		StartCoroutine ("RunTrial");
	}

	public void IncreaseCoinCount()
	{
		coinsCollected++;

	}


	// Update is called once per frame
	void Update () {
//		Debug.Log ("current speed: " + carController.CurrentSpeed.ToString());
//		scoreText.text="distance covered: " + distanceMeasure.GetDistanceFloat().ToString("F2");
		uiController.scoreText.text="Coins Collected: " + coinsCollected.ToString();

		if (Input.GetKeyDown (KeyCode.LeftArrow)) {
			MoveLeft ();
		}
		if (Input.GetKeyDown (KeyCode.RightArrow)) {
			MoveRight ();
		}

	}

	public bool CheckTornadoLane(int tornadoLane)
	{
		if (currentCircuit == tornadoLane)
			return true;
		else
			return false;
	}

	void MoveLeft()
	{
		if(currentCircuit>0)
			waypointTracker.circuit = waypoints [--currentCircuit];
		Debug.Log ("current circuit is: " + currentCircuit.ToString ());
		
	}


	void MoveRight()
	{
		if(currentCircuit<2)
			waypointTracker.circuit = waypoints [++currentCircuit];

		Debug.Log ("current circuit is: " + currentCircuit.ToString ());
	}

	public void ResetPlayer()
	{
		carBody.transform.position = startTransform.position;
	}

	public IEnumerator TemporarilyHaltCar(float haltTime)
	{
		carController.ChangeMaxSpeed(0f);
		yield return new WaitForSeconds (haltTime);
		carController.ChangeMaxSpeed (maxSpeed);
		yield return null;
	}

	void SwitchToFreeLook(bool status)
	{
		if (status) {
			freeLookCam.transform.localRotation = Quaternion.identity;
			freeLookCam.transform.parent.gameObject.GetComponent<FirstPersonController> ().enabled = true;
			freeLookCam.enabled = true;
			standardCam.enabled = false;
		} else {
			freeLookCam.transform.localRotation = Quaternion.identity;
			freeLookCam.transform.parent.gameObject.GetComponent<FirstPersonController> ().enabled = false;
			freeLookCam.enabled = false;
			standardCam.enabled = true;
		}
	}

	IEnumerator FreeLookAround()
	{
		float prevCurrentSpeed = carController.CurrentSpeed;
//		carController.enabled = false;
		carController.ChangeMaxSpeed(0f);
		SwitchToFreeLook (true);
		yield return new WaitForSeconds (2f);
		SwitchToFreeLook (false);
//		carController.enabled = true;
		carController.ChangeMaxSpeed(maxSpeed);
//		carController.SetCurrentSpeed(prevCurrentSpeed);
		yield return null;
	}


	//used during encoding to move chequered flag to randomly chosen location
	IEnumerator PickChequeredFlagPosition()
	{
		int chosenPosition = currentChequeredFlagIndex;
		while (currentChequeredFlagIndex == chosenPosition) {

			chosenPosition = Random.Range (0, chequeredFlagTransforms.Count - 1);
			Debug.Log ("chosen position COULD be: " + chosenPosition.ToString ());
			yield return 0;
		}
		Debug.Log ("chosen position is: " + chosenPosition.ToString ()); 
		chequeredFlagIndices [ChequeredFlag.lapsCompleted] = chosenPosition; //store the chequered flag index in the array to be retrieved later
		yield return new WaitForSeconds (8f);
		chequeredFlag.transform.position = chequeredFlagTransforms [chosenPosition].position;
		currentChequeredFlagIndex = chosenPosition;

		yield return null;
		
	}

	//used during retrieval to move chequered flag to previously chosen location during encoding
	IEnumerator SetChequeredFlagPosition()
	{
		int chosenPosition = chequeredFlagIndices [ChequeredFlag.lapsCompleted];
		yield return new WaitForSeconds (8f);
		chequeredFlag.transform.position = chequeredFlagTransforms [chosenPosition].position;

		yield return null;
	}

	IEnumerator ShowLapCompletion()
	{
		
		uiController.ChangeLapText (simpleTimer.GetSecondsFloat());
		simpleTimer.ResetTimer ();
		yield return new WaitForSeconds (4f);
		uiController.TurnOffLapText ();
	}

	//main logic of the trial
	IEnumerator RunTrial()
	{
		
		while(true){
			ChequeredFlag.lapsCompleted = 0;

			while (ChequeredFlag.lapsCompleted < lapsToBeFinished) {

				simpleTimer.StartTimer ();
				//distance-fixed
				Debug.Log("on lap: " + ChequeredFlag.lapsCompleted.	ToString());
				trialType = TrialType.Distance;
				speedFactor = ChooseRandomSpeed ();
				carAI.ChangeSpeedFactor (speedFactor);
				uiController.SetCarInstruction ("Watch carefully at what distance the turbo is activated");

				StartCoroutine(PickChequeredFlagPosition()); //pick chequered flag position first
				fixedDistance = ChooseFixedDistance ();

				//add this to the list
				fixedDistanceList.Add (fixedDistance);
				Debug.Log("fixed distance is: " + fixedDistance.ToString());

				//activate turbo text
				distanceMeasure.ResetTimer ();
				distanceMeasure.StartTimer ();
				while (distanceMeasure.GetDistanceFloat() < fixedDistance) {
					yield return 0;
				}
				uiController.ChangeHarvestText ("ACTIVATING TURBO...");

				//TEMPORARILY DISABLING FREELOOKAROUND
//				yield return StartCoroutine (FreeLookAround ());    //allow free-look around

				uiController.ChangeHarvestText ("TURBO ACTIVATED");
				pp_profile.motionBlur.enabled = true;
				StartCoroutine (PlayTurboAnim ());
				speedFactor += 0.2f;

				//update speedfactor 
				carAI.ChangeSpeedFactor (speedFactor);

				//wait for 6 seconds before turning off the turbo text
				float turboTimer=0f;
				while (turboTimer<6f) {
					turboTimer += Time.deltaTime;
					yield return 0;
				}
				pp_profile.motionBlur.enabled = false;
				uiController.TurnOffHarvestText ();

				//wait till car finishes the lap
				yield return StartCoroutine(chequeredFlag.WaitForCarToLap()); 
				StartCoroutine (ShowLapCompletion ());
				yield return 0;
			}

			//reset the laps completed
			ChequeredFlag.lapsCompleted = 0;
			//retrieval
			while (ChequeredFlag.lapsCompleted < lapsToBeFinished) {
				//distance-fixed
				int currentLap=ChequeredFlag.lapsCompleted;
				Debug.Log("on lap: " + ChequeredFlag.lapsCompleted.	ToString());
				trialType = TrialType.Distance;
				speedFactor = ChooseRandomSpeed ();
				carAI.ChangeSpeedFactor (speedFactor);
				uiController.SetCarInstruction ("Press (X) where you think the turbo was activated");
				fixedDistance = fixedDistanceList[currentLap];
				StartCoroutine (SetChequeredFlagPosition ());
				//add this to the list

				Debug.Log("fixed distance is: " + fixedDistance.ToString());
				//activate turbo text
				distanceMeasure.ResetTimer ();
				distanceMeasure.StartTimer ();
	
				while ((Input.GetAxis ("Action Button") == 0f) && currentLap==ChequeredFlag.lapsCompleted) {
					yield return 0;
				}
				if (Input.GetAxis ("Action Button") > 0f) {
					uiController.ChangeHarvestText ("TURBO ACTIVATED");
					pp_profile.motionBlur.enabled = true;
					yield return StartCoroutine(MeasureScore (distanceMeasure.GetDistanceFloat(), fixedDistance, trialType));

					StartCoroutine (PlayTurboAnim ());
					speedFactor += 0.2f * responseFactor;

					//update speedfactor 
					carAI.ChangeSpeedFactor (speedFactor);

					//wait for 6 seconds before turning off the turbo text
					float turboTimer = 0f;
					while (turboTimer < 6f) {
						turboTimer += Time.deltaTime;
						yield return 0;
					}
					pp_profile.motionBlur.enabled = false;
					uiController.TurnOffHarvestText ();
				
				//wait till car finishes the lap
				}
				if(currentLap==ChequeredFlag.lapsCompleted)
					yield return StartCoroutine(chequeredFlag.WaitForCarToLap()); 
				yield return 0;
			}

		

			yield return 0;
		}

		yield return null;
	}

	IEnumerator MeasureScore(float playerVal, float fixedVal, TrialType trialType)
	{
		if (trialType == TrialType.Distance) {
			float score = Mathf.Abs (playerVal - fixedVal);
			responseFactor = 1f - (score / 2500f);
			uiController.scoreText.enabled = true;
			uiController.scoreText.text = "Your turbo was boosted by " + (responseFactor * 100f).ToString ("F1") + "%";
		} else {
			float score = Mathf.Abs (playerVal - fixedVal);
			responseFactor = 1f - (score / (2500f / carController.CurrentSpeed));
			uiController.scoreText.enabled = true;
			uiController.scoreText.text = "Your turbo was boosted by " + (responseFactor * 100f).ToString ("F1") + "%";
		}
		yield return null;
	}


	IEnumerator PlayTurboAnim()
	{
		float t = 0f;
		while (uiController.harvestText.enabled) {
			t += Time.deltaTime;
			uiController.harvestText.color = Color.Lerp (Color.red, Color.blue,t);
			if (t >= 1f)
				t = 0f;
			yield return 0;
		}
	}
	float ChooseRandomSpeed()
	{
		return Random.Range (minSpeedFactor, maxSpeedFactor);
	}

	float ChooseFixedDistance()
	{
		return Random.Range (minX, maxX);
	}
	float ChooseFixedTime()
	{
		return Random.Range (minTime, maxTime);
	}
}
