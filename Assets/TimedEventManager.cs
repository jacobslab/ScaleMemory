using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;
public class TimedEventManager : MonoBehaviour {


	public float eventInterval=10f;
	public RacingTimelocked racingTimelocked;
	private List<WaypointCircuit> waypoints;
	private int tornadoLane = 0;

	public GameObject tornadoObj;
	// Use this for initialization
	void Start () {
		tornadoObj.SetActive (false);
		waypoints = racingTimelocked.waypoints;
		StartCoroutine (RunTimedEvents ());
		
	}


	IEnumerator RunTimedEvents()
	{
		while (true) {
			float randWaitTime = Random.Range (Configuration.minTornadoWaitTime, Configuration.maxTornadoWaitTime);
			yield return new WaitForSeconds (randWaitTime);
			//issue a warning
			yield return StartCoroutine(IssueTornadoWarning());
			//wait for specified amount of time
			yield return new WaitForSeconds(Configuration.timedEventInterval);
			//run the required effect
			yield return StartCoroutine(ActivateTornado());
//			//wait for lap to be completed
//			yield return StartCoroutine(racingTimelocked.chequeredFlag.WaitForCarToLap()); 
			yield return 0;
		}
		yield return null;
	}


	IEnumerator IssueTornadoWarning()
	{
		//select the lane
		tornadoLane = Random.Range (0, 3);
		//give text warning related to targeted lane
		racingTimelocked.uiController.SetTornadoWarningText(tornadoLane);
		yield return new WaitForSeconds (Configuration.tornadoWarningDisplayTime);
		racingTimelocked.uiController.TurnOffTornadoWarning ();
		yield return null;
	}


	IEnumerator ActivateTornado()
	{
		//check if the car is in the tornado lane
		bool tornadoCheck=racingTimelocked.CheckTornadoLane(tornadoLane);

		//activate the tornado
		tornadoObj.SetActive(true);
		//if car is in tornado lane, then freeze it immediately
		if (tornadoCheck) {

			//attach it to the car's transform
			tornadoObj.transform.position = racingTimelocked.carBody.transform.position + racingTimelocked.carBody.transform.forward * 3f;
			//halt the car and wait for the penalty time
			yield return StartCoroutine (racingTimelocked.TemporarilyHaltCar (Configuration.tornadoPenaltyTime));
		} 
		// car is not in the tornado lane, so adjust its position accordingly
		else {
			if(tornadoLane==0)
				tornadoObj.transform.position = racingTimelocked.carBody.transform.position + racingTimelocked.carBody.transform.right * -3f;
			else if(tornadoLane == 1)
				tornadoObj.transform.position = racingTimelocked.carBody.transform.position + racingTimelocked.carBody.transform.right * 3f;
			else
				tornadoObj.transform.position = racingTimelocked.carBody.transform.position + racingTimelocked.carBody.transform.right * 6f;

		}

		//translate the tornado forward until out of sight
		TranslateTornadoForward();
		//reset
		tornadoObj.SetActive(false);
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
