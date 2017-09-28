using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.PostProcessing;
using UnityStandardAssets.Vehicles.Car;
using UnityStandardAssets.Characters.FirstPerson;

public class RacingTimelocked : MonoBehaviour {

	//ui element
	public Text harvestText;
	public Text carInstructionText;
	public Text scoreText;

	//speed
	public float minSpeedFactor=8f;
	public float maxSpeedFactor=20f;
	public float speedFactor=10f;

	public float maxSpeed=75f;
	public Rigidbody carBody;
	public Transform startTransform;

	private float minX=100f;
	private float maxX=1000f;
	private float fixedDistance = 0f;

	private float minTime=5f;
	private float maxTime=15f;
	private float fixedTime=0f;

	private List<float> fixedDistanceList;
	private List<float> fixedTimeList;

	public List<Transform> chequeredFlagTransforms;

	/// <summary>
	///  +40 = 0 
	/// +20 = 1
	/// 0 = 2
	/// -20 = 3
	/// -40 = 4
	/// </summary>
	private int currentChequeredFlagIndex=0; 


	float distTraveled=0f;
	float responseFactor=1f;


	//config 
	private int lapsToBeFinished=3;

	//timer/measure
	public SimpleTimer simpleTimer;
	public SimpleDistanceMeasure distanceMeasure;

	public ChequeredFlag chequeredFlag;


	public Camera standardCam;
	public Camera freeLookCam;

	private PostProcessingProfile pp_profile;
	private CarController carController;
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

		//instantiate the lists
		fixedDistanceList=new List<float>();
		fixedTimeList = new List<float> ();

	}

	// Use this for initialization
	void Start () {
		ChequeredFlag.lapsCompleted = 0;
		scoreText.text = "";
		TurnOffHarvestText ();
		TurnOffCarInstruction ();
		StartCoroutine ("RunTrial");
	}

	public void ChangeHarvestText(string text)
	{
		harvestText.enabled = true;
		harvestText.text = text;
	}
	public void TurnOffHarvestText()
	{
		harvestText.text = "";
		harvestText.enabled = false;
	}

	public void SetCarInstruction(string text)
	{
		carInstructionText.enabled = true;
		carInstructionText.text = text;
	}

	public void TurnOffCarInstruction()
	{
		carInstructionText.text = "";
		carInstructionText.enabled = false;
	}

	// Update is called once per frame
	void Update () {
//		Debug.Log ("current speed: " + carController.CurrentSpeed.ToString());
//		scoreText.text="distance covered: " + distanceMeasure.GetDistanceFloat().ToString("F2");

	}

	public void ResetPlayer()
	{
		carBody.transform.position = startTransform.position;
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
		carController.ChangeMaxSpeed(0f);
		SwitchToFreeLook (true);
		yield return new WaitForSeconds (2f);
		SwitchToFreeLook (false);
		carController.ChangeMaxSpeed(maxSpeed);
		yield return null;
	}

	IEnumerator PickChequeredFlagPosition()
	{
		int chosenPosition = currentChequeredFlagIndex;
		while (currentChequeredFlagIndex == chosenPosition) {

			chosenPosition = Random.Range (0, chequeredFlagTransforms.Count - 1);
			Debug.Log ("chosen position COULD be: " + chosenPosition.ToString ());
			yield return 0;
		}
		Debug.Log ("chosen position is: " + chosenPosition.ToString ()); 
		yield return new WaitForSeconds (5f);
		chequeredFlag.transform.position = chequeredFlagTransforms [chosenPosition].position;
		currentChequeredFlagIndex = chosenPosition;

		yield return null;
		
	}


	IEnumerator RunTrial()
	{
		
		while(true)
		{
			ChequeredFlag.lapsCompleted = 0;
			while (ChequeredFlag.lapsCompleted < lapsToBeFinished) {
				//distance-fixed
				Debug.Log("on lap: " + ChequeredFlag.lapsCompleted.	ToString());
				trialType = TrialType.Distance;
				speedFactor = ChooseRandomSpeed ();
				carAI.ChangeSpeedFactor (speedFactor);
				SetCarInstruction ("Watch carefully at what distance the gear is changed");

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
				ChangeHarvestText ("ACTIVATING TURBO...");
				yield return StartCoroutine (FreeLookAround ());    //allow free-look around
				ChangeHarvestText ("TURBO ACTIVATED");
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
				TurnOffHarvestText ();

				//wait till car finishes the lap
				yield return StartCoroutine(chequeredFlag.WaitForCarToLap()); 
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
				SetCarInstruction ("Press (X) where you think the gear was changed");
				fixedDistance = fixedDistanceList[currentLap];

				//add this to the list

				Debug.Log("fixed distance is: " + fixedDistance.ToString());
				//activate turbo text
				distanceMeasure.ResetTimer ();
				distanceMeasure.StartTimer ();
	
				while ((Input.GetAxis ("Action Button") == 0f) && currentLap==ChequeredFlag.lapsCompleted) {
					yield return 0;
				}
				if (Input.GetAxis ("Action Button") > 0f) {
					ChangeHarvestText ("TURBO ACTIVATED");
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
					TurnOffHarvestText ();
				
				//wait till car finishes the lap
				}
				if(currentLap==ChequeredFlag.lapsCompleted)
					yield return StartCoroutine(chequeredFlag.WaitForCarToLap()); 
				yield return 0;
			}

			//time-fixed

			ChequeredFlag.lapsCompleted = 0;

			while (ChequeredFlag.lapsCompleted < lapsToBeFinished) {
				//distance-fixed
				Debug.Log("on lap: " + ChequeredFlag.lapsCompleted.	ToString());
				trialType = TrialType.Time;
				speedFactor = ChooseRandomSpeed ();
				carAI.ChangeSpeedFactor (speedFactor);
				SetCarInstruction ("Watch carefully at what time the gear is changed");
				fixedTime = ChooseFixedTime ();

				//add this to the list
				fixedTimeList.Add (fixedTime);
				Debug.Log("fixed time is: " + fixedTime.ToString());

				//activate turbo text
				simpleTimer.ResetTimer ();
				simpleTimer.StartTimer ();
				while (simpleTimer.GetSecondsFloat() < fixedTime) {
					yield return 0;
				}

				ChangeHarvestText ("ACTIVATING TURBO...");
				yield return StartCoroutine (FreeLookAround ());    //allow free-look around
				ChangeHarvestText ("TURBO ACTIVATED");
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
				TurnOffHarvestText ();

				//wait till car finishes the lap
				yield return StartCoroutine(chequeredFlag.WaitForCarToLap()); 
				yield return 0;
			}
			//reset the laps completed
			ChequeredFlag.lapsCompleted = 0;
			//retrieval
			while (ChequeredFlag.lapsCompleted < lapsToBeFinished) {
				//distance-fixed
				int currentLap=ChequeredFlag.lapsCompleted;
				Debug.Log("on lap: " + ChequeredFlag.lapsCompleted.	ToString());
				trialType = TrialType.Time;
				speedFactor = ChooseRandomSpeed ();
				carAI.ChangeSpeedFactor (speedFactor);
				SetCarInstruction ("Press (X) when you think the gear was changed");
				fixedTime = fixedTimeList[currentLap];

				//add this to the list

				Debug.Log("fixed time is: " + fixedTime.ToString());
				//activate turbo text
				simpleTimer.ResetTimer ();
				simpleTimer.StartTimer ();

				while ((Input.GetAxis ("Action Button") == 0f) && currentLap==ChequeredFlag.lapsCompleted) {
					yield return 0;
				}
				if (Input.GetAxis ("Action Button") > 0f) {
					ChangeHarvestText ("TURBO ACTIVATED");
					pp_profile.motionBlur.enabled = true;
					yield return StartCoroutine(MeasureScore (simpleTimer.GetSecondsFloat(), fixedTime, trialType));

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
					TurnOffHarvestText ();

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
			scoreText.enabled = true;
			scoreText.text = "Your turbo was boosted by " + (responseFactor * 100f).ToString ("F1") + "%";
		} else {
			float score = Mathf.Abs (playerVal - fixedVal);
			responseFactor = 1f - (score / (2500f / carController.CurrentSpeed));
			scoreText.enabled = true;
			scoreText.text = "Your turbo was boosted by " + (responseFactor * 100f).ToString ("F1") + "%";
		}
		yield return null;
	}


	IEnumerator PlayTurboAnim()
	{
		float t = 0f;
		while (harvestText.enabled) {
			t += Time.deltaTime;
			harvestText.color = Color.Lerp (Color.red, Color.blue,t);
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
