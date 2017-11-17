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


	// Use this for initialization
	void Start () {
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
		//give text warning related to targeted lane
		racingTimelocked.uiController.SetPunctureWarningText();
		yield return new WaitForSeconds (Configuration.tornadoWarningDisplayTime);
		racingTimelocked.uiController.TurnOffTornadoWarning ();
		yield return null;
	}


	IEnumerator ActivateTimedEvent()
	{
		cueButtonCheck = false;
		float waitTimer = 0f;
		//wait for specified amount of time 
		while (waitTimer < Configuration.timedEventInterval+1f && (Input.GetAxis ("Time Button") == 0f)) {
			waitTimer += Time.deltaTime;
			yield return 0;
		}

		//check if the cued button was pressed in approx time
		if (Input.GetAxis ("Time Button") > 0f && Mathf.Abs(waitTimer-Configuration.timedEventInterval)<Configuration.timeThreshold) {
			cueButtonCheck = true;
		}
		Debug.Log ("difference was : " + Mathf.Abs (waitTimer - Configuration.timedEventInterval).ToString ());


		//if button was pressed, perform the steel tyres effect and don't apply any penalty
		if (cueButtonCheck) {
			Debug.Log ("button was pressed at correct time");
			//attach it to the car's transform
			//display tornado arrival message
			racingTimelocked.uiController.SetTyreActivationText();
			//halt the car and wait for the penalty time
			yield return new WaitForSeconds(3f);
//			yield return StartCoroutine (racingTimelocked.TemporarilyHaltCar (Configuration.tornadoPenaltyTime));
			racingTimelocked.uiController.TurnOffTornadoWarning ();
		} 

		// button was not pressed, play puncture effect and impose penalty
		else {
			//the player waited too long, so play the penalty immediately
			if (waitTimer - Configuration.timedEventInterval > 0f) {
				//penalty
				Debug.Log("button not pressed; player waited too long, applying penalty immediately");
				StartCoroutine(racingTimelocked.PuncturedCar());
				StartCoroutine(racingTimelocked.DisplayText("TYRES PUNCTURED BY TIME"));
			}
			//else, wait for the remaining time and then play the penalty
			else
			{
				Debug.Log(" button not pressed; waiting for correct time to apply penalty");

				yield return new WaitForSeconds (waitTimer - Configuration.timedEventInterval);
				//play the penalty
				StartCoroutine(racingTimelocked.PuncturedCar());
				StartCoroutine(racingTimelocked.DisplayText("TYRES PUNCTURED BY TIME"));
			}
		}


		yield return null;
	}
	// Update is called once per frame
	void Update () {
		
	}
}
