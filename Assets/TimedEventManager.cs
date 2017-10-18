using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;
using UnityEngine.VR;
public class TimedEventManager : MonoBehaviour {


	public float eventInterval=10f;
	public RacingTimelocked racingTimelocked;
	private List<WaypointCircuit> waypoints;
	private int tornadoLane = 0;
	private bool cueButtonCheck=false;


	public GameObject tornadoObj;
	// Use this for initialization
	void Start () {
		tornadoObj.SetActive (false);
		StartCoroutine (RunTimedEvents ());
		
	}


	IEnumerator RunTimedEvents()
	{
		
		while (true) {
			float randWaitTime = Random.Range (Configuration.minTornadoWaitTime, Configuration.maxTornadoWaitTime);
			yield return new WaitForSeconds (randWaitTime);
			//issue a warning
			yield return StartCoroutine(IssueTimedEventWarning());
			//run the required effect
			yield return StartCoroutine(ActivateTimedEvent());
//			//wait for lap to be completed
//			yield return StartCoroutine(racingTimelocked.chequeredFlag.WaitForCarToLap()); 
			yield return 0;
		}
		yield return null;
	}


	IEnumerator IssueTimedEventWarning()
	{
		//select the lane
		tornadoLane = Random.Range (0, 3);
		//give text warning related to targeted lane
		racingTimelocked.uiController.SetTornadoWarningText(tornadoLane);
		yield return new WaitForSeconds (Configuration.tornadoWarningDisplayTime);
		racingTimelocked.uiController.TurnOffTornadoWarning ();
		yield return null;
	}


	IEnumerator ActivateTimedEvent()
	{
		cueButtonCheck = false;
		float waitTimer = 0f;
		//wait for specified amount of time 
		while (waitTimer < Configuration.timedEventInterval+1f && (Input.GetAxis ("Action Button") == 0f)) {
			waitTimer += Time.deltaTime;
			yield return 0;
		}

		//check if the cued button was pressed in approx time
		if (Input.GetAxis ("Action Button") > 0f && Mathf.Abs(waitTimer-Configuration.timedEventInterval)<1f) {
			cueButtonCheck = true;
		}


		//if button was pressed, perform the steel tyres effect and don't apply any penalty
		if (cueButtonCheck) {
			Debug.Log ("button was pressed");
			//attach it to the car's transform
			tornadoObj.transform.position = racingTimelocked.carBody.transform.position + racingTimelocked.carBody.transform.forward * 3f;
			//display tornado arrival message
			racingTimelocked.uiController.SetTornadoArrivalText();
			//halt the car and wait for the penalty time
			yield return StartCoroutine (racingTimelocked.TemporarilyHaltCar (Configuration.tornadoPenaltyTime));
			racingTimelocked.uiController.TurnOffTornadoWarning ();
		} 

		// button was not pressed, play puncture effect and impose penalty
		else {
			//the player waited too long, so play the penalty immediately
			if (waitTimer - Configuration.timedEventInterval < 0f) {
				//penalty
			}
			//else, wait for the remaining time
			else
			{
				yield return new WaitForSeconds (waitTimer - Configuration.timedEventInterval);
			}
		}


		yield return null;
	}

	void TranslateTornadoForward()
	{
		float timer = 0f;
		if (timer < 1f) {
			timer += Time.deltaTime;
			tornadoObj.transform.position = Vector3.Lerp (tornadoObj.transform.position, tornadoObj.transform.position + tornadoObj.transform.forward * 100f, timer);
		}
	}
	// Update is called once per frame
	void Update () {
		
	}
}
