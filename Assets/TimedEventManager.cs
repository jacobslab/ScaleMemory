using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;
public class TimedEventManager : MonoBehaviour {


	public float eventInterval=10f;
	public RacingTimelocked racingTimelocked;
	private List<WaypointCircuit> waypoints;
	// Use this for initialization
	void Start () {
		waypoints = racingTimelocked.waypoints;
		StartCoroutine (RunTimedEvents ());
		
	}


	IEnumerator RunTimedEvents()
	{
		while (true) {
			//issue a warning
			yield return StartCoroutine(IssueTornadoWarning());
			//wait for specified amount of time
			yield return new WaitForSeconds(Configuration.timedEventInterval);
			//run the required effect
			yield return StartCoroutine(ActivateTornado());
			//wait for lap to be completed

			//repeat
			yield return 0;
		}
		yield return null;
	}


	IEnumerator IssueTornadoWarning()
	{
		//select the lane
		int randLane = Random.Range (0, 3);
		//give text warning


		yield return null;
	}

	IEnumerator ActivateTornado()
	{
		//halt the car

		//activate the tornado

		//attach it to the car's transform

		//wait for penalty time

		//translate the tornado forward until out of sight

		//reset

		yield return null;
	}
	// Update is called once per frame
	void Update () {
		
	}
}
