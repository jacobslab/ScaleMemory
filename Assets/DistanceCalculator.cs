using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceCalculator : MonoBehaviour {
	public GameObject waypointParent;
	// Use this for initialization
	void Start () {
		CalculateDistance ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void CalculateDistance()
	{
		float totalDistance = 0f;
		Vector3 p1, p2;
		for (int i = 0; i < 35; i++) {
			if (i == waypointParent.transform.childCount - 1) {
				p1 = waypointParent.transform.GetChild (i).position;
				p2 = waypointParent.transform.GetChild (0).position;
			} else {
				p1 = waypointParent.transform.GetChild (i).position;
				p2 = waypointParent.transform.GetChild (i + 1).position;
			}
			totalDistance += Vector3.Distance (p1, p2);
		}
		Debug.Log ("total distance is: " + totalDistance.ToString ());
	}
}
