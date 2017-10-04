using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityStandardAssets.Vehicles.Car;

public class SimpleDistanceMeasure : MonoBehaviour {

	public Text distanceText;
	float distance = 0;
	public CarController carController;

	bool isRunning = false;
	public bool IsRunning { get { return isRunning; } } //public getter. don't want people setting isRunning outside of here.

	//public delegate void ResetDelegate();
	//public ResetDelegate myResetDelegate;

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {
		if (isRunning) {
			UpdateTimer();
		}
	}

	void UpdateTimer(){
		distance += carController.CurrentSpeed/2.23693629f * Time.deltaTime;

		if(distanceText != null){
			distanceText.text = distance.ToString ("F2");
		}
	}

	public void StartTimer(){
		isRunning = true;
	}

	public void StopTimer(){
		isRunning = false;
	}

	public void ResetTimer(){
		//myResetDelegate ();
		distance = 0;
	}

	public int GetDistanceInt(){
		return Mathf.FloorToInt(distance);
	}

	public float GetDistanceFloat(){
		return distance;
	}
}
