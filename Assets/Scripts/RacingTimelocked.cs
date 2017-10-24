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
	private Vector3 fixedDistance = Vector3.zero;

	private float minTime=5f;
	private float maxTime=15f;
	private float fixedTime=0f;

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


	public CameraController camController;
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

	}

	// Use this for initialization
	void Start () {
		ChequeredFlag.lapsCompleted = 0;

		StartCoroutine ("RunTrial");
	}

	public void IncreaseCoinCount()
	{
		coinsCollected++;
		uiController.UpdateCoinText (coinsCollected);

	}


	// Update is called once per frame
	void Update () {
//		Debug.Log ("current speed: " + carController.CurrentSpeed.ToString());
//		scoreText.text="distance covered: " + distanceMeasure.GetDistanceFloat().ToString("F2");



		if (Input.GetKeyDown (KeyCode.R)) {
			ResetVehicle ();
		}

	}

	void ResetVehicle()
	{
		carBody.transform.position = waypointTracker.target.position;
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
//		yield return new WaitForSeconds (8f);
//		chequeredFlag.transform.position = chequeredFlagTransforms [chosenPosition].position;
//		currentChequeredFlagIndex = chosenPosition;

		yield return null;
		
	}

	//used during retrieval to move chequered flag to previously chosen location during encoding
	IEnumerator SetChequeredFlagPosition()
	{
		int chosenPosition = chequeredFlagIndices [ChequeredFlag.lapsCompleted];
		chequeredFlag.transform.position = chequeredFlagTransforms [chosenPosition].position;
		chequeredFlag.transform.eulerAngles = chequeredFlagTransforms [chosenPosition].eulerAngles;
		carBody.transform.position = carController.carStartPosList [chosenPosition].position;
		carBody.transform.eulerAngles = carController.carStartPosList [chosenPosition].eulerAngles;
		waypointTracker.SetProgressNum (carController.carNearestWaypointList [chosenPosition]);
		yield return null;
	}

	IEnumerator ShowLapCompletion()
	{
		
		uiController.ChangeLapText (simpleTimer.GetSecondsFloat());
		simpleTimer.ResetTimer ();
		yield return new WaitForSeconds (Configuration.timeBetweenLaps);
		uiController.TurnOffLapText ();
	}

	IEnumerator SpecialActivationAnim()
	{
		uiController.ChangeHarvestText ("STEEL TYRES ACTIVATED");
		pp_profile.motionBlur.enabled = true;

		speedFactor += 0.2f * responseFactor;

		//update speedfactor 
		carAI.ChangeSpeedFactor (speedFactor);

		//wait for 6 seconds before turning off the turbo text
		yield return new WaitForSeconds(6f);
		pp_profile.motionBlur.enabled = false;
		uiController.TurnOffHarvestText ();
		yield return null;
	}

	//main logic of the trial
	IEnumerator RunTrial()
	{
		
		while(true){
			ChequeredFlag.lapsCompleted = 0;


			//just one lap of encoding needed to show the fixed spatial location
			while (ChequeredFlag.lapsCompleted < 1) {
				simpleTimer.StartTimer ();
				//distance-fixed
				Debug.Log("on lap: " + ChequeredFlag.lapsCompleted.	ToString());
				trialType = TrialType.Distance;
				speedFactor = ChooseRandomSpeed ();
				carAI.ChangeSpeedFactor (speedFactor);
				uiController.SetCarInstruction ("Watch carefully at what distance the steel tyres are activated");

				StartCoroutine(PickChequeredFlagPosition()); //pick chequered flag position first
				fixedDistance = ChooseFixedDistance ();

			
//				fixedDistanceList.Add (fixedDistance); //we only have one value, so this list is no longer needed
				Debug.Log("fixed distance is: " + fixedDistance.ToString());

				//activate turbo text
				distanceMeasure.ResetTimer ();
				distanceMeasure.StartTimer ();

				//wait till the car is close enough to the "puncture zone"
				while (Vector3.Distance(fixedDistance,carBody.transform.position)>Configuration.distanceThreshold) {
//					Debug.Log (Vector3.Distance (fixedDistance, carBody.transform.position).ToString());
					yield return 0;
				}

				//TEMPORARILY DISABLING FREELOOKAROUND
//				yield return StartCoroutine (FreeLookAround ());    //allow free-look around

				StartCoroutine (PlayTurboAnim ());
				StartCoroutine (SpecialActivationAnim ());
				//wait till car finishes the lap
				yield return StartCoroutine(chequeredFlag.WaitForCarToLap()); 
				carController.ChangeMaxSpeed (0f);
				StartCoroutine (ShowLapCompletion ());
				yield return 0;
			}

			//reset the laps completed
			ChequeredFlag.lapsCompleted = 0;



			//retrieval will take place for the actual number of laps to be tested for
			while (ChequeredFlag.lapsCompleted < lapsToBeFinished) {
				
				//blackout and wait for required time between laps

				carController.ChangeMaxSpeed (0f); //turn off the car speed
				camController.EnableBlackout();
				yield return new WaitForSeconds (Configuration.timeBetweenLaps);

				//transport to new chequered flag location
				StartCoroutine (SetChequeredFlagPosition ());

				//disable blackout
				camController.DisableBlackout ();
				carController.ChangeMaxSpeed (maxSpeed);

				//start the lap timer
				simpleTimer.StartTimer ();

				//distance-fixed
				int currentLap=ChequeredFlag.lapsCompleted;
				Debug.Log("on lap: " + ChequeredFlag.lapsCompleted.	ToString());
				trialType = TrialType.Distance;
				speedFactor = ChooseRandomSpeed ();
				carAI.ChangeSpeedFactor (speedFactor);
				uiController.SetCarInstruction ("Press (X) where you think the steel tyres are activated");
//				fixedDistance = fixedDistanceList[currentLap];
				StartCoroutine (SetChequeredFlagPosition ());


				//fixed distance is now constant within trial
				Debug.Log("fixed distance is: " + fixedDistance.ToString());
				//activate turbo text
				distanceMeasure.ResetTimer ();
				distanceMeasure.StartTimer ();
	
				while ((Input.GetAxis ("Action Button") == 0f) && Vector3.Distance(fixedDistance,carBody.transform.position)>Configuration.distanceThreshold) {
					yield return 0;
				}

				//if button was pressed
				if (Input.GetAxis ("Action Button") > 0f) {

					//check if it is within the puncture zone
					if (Vector3.Distance (fixedDistance, carBody.transform.position) < Configuration.distanceThreshold) {

						Debug.Log ("button pressed in puncture zone");
						//check score and show effects
						yield return StartCoroutine (MeasureScore (carBody.transform.position, fixedDistance, trialType));
						StartCoroutine (PlayTurboAnim ());
						StartCoroutine(SpecialActivationAnim());


						Debug.Log ("waiting for lap to be completed");
						//and then wait for lap to be completed
						yield return StartCoroutine (chequeredFlag.WaitForCarToLap ()); 
						StartCoroutine (ShowLapCompletion ());
					} 
					//else if it was not in the puncture zone when the button was pressed
					else {
						Debug.Log ("not in puncture zone when button was pressed");
						//show negative feedback
						StartCoroutine("DisplayText","STEEL TYRES ACTIVATED");

						Debug.Log ("waiting for car to be in puncture zone");
						//wait for the car to enter the puncture zone
						while (Vector3.Distance (fixedDistance, carBody.transform.position) > Configuration.distanceThreshold) {
							yield return 0;
						}
						//show negative effect
						StartCoroutine(PuncturedCar());
						StartCoroutine("DisplayText", "TYRES PUNCTURED");

						Debug.Log ("waiting for lap to be completed");
						//wait for the lap to be completed
						yield return StartCoroutine (chequeredFlag.WaitForCarToLap ()); 
						StartCoroutine (ShowLapCompletion ());
					}

				}
				//if no button was pressed, then we are in the puncture zone
				else if (Vector3.Distance(fixedDistance,carBody.transform.position)<Configuration.distanceThreshold) {
					Debug.Log ("no button was pressed, we are in the puncture zone");
					//show puncture sequence
					//show negative effect
					StartCoroutine(PuncturedCar());
					StartCoroutine("DisplayText", "TYRES PUNCTURED");


					Debug.Log ("waiting for lap to be completed");
					//wait for lap to be completed
					yield return StartCoroutine (chequeredFlag.WaitForCarToLap ()); 
					StartCoroutine (ShowLapCompletion ());
				} else {
					StartCoroutine (ShowLapCompletion ());
				}

				yield return 0;
			}

		

			yield return 0;
		}

		yield return null;
	}

	public IEnumerator PuncturedCar()
	{
		carController.ChangeMaxSpeed (0f);
		yield return new WaitForSeconds (3f);
		carController.ChangeMaxSpeed (maxSpeed);
		yield return null;
	}

	public IEnumerator DisplayText(string text)
	{
		uiController.ChangeHarvestText (text);
		yield return new WaitForSeconds (3f);
		uiController.TurnOffHarvestText ();
		yield return null;
	}

	IEnumerator MeasureScore(Vector3 playerVal, Vector3 fixedVal, TrialType trialType)
	{
		float score = Vector3.Distance(playerVal,fixedVal);
			responseFactor = 1f - (score /100f);
			uiController.scoreText.enabled = true;
			uiController.scoreText.text = "Your turbo was boosted by " + (responseFactor * 100f).ToString ("F1") + "%";
		
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
		uiController.TurnOffLapText ();
	}
	float ChooseRandomSpeed()
	{
		return Random.Range (minSpeedFactor, maxSpeedFactor);
	}

	Vector3 ChooseFixedDistance()
	{
		Transform[] waypointList = waypointTracker.circuit.waypointList.items;
		return waypointList [Random.Range (0, waypointList.Length - 1)].position;
	}
	float ChooseFixedTime()
	{
		return Random.Range (minTime, maxTime);
	}
}
