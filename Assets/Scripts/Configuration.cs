using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Configuration : MonoBehaviour {


	public static float timedEventInterval=10f;
	public static float lapsToBeCompleted=3;
	public static int tornadoArrivalTime=10;
	public static float tornadoPenaltyTime=5f;
	public static float tornadoWarningDisplayTime=3f;

	public static float distanceThreshold=5f; //minimum distance before an object is said to be "on the point"
	public static float timeThreshold=2f; //minimum time before or after an action can be said to be "on cue"

	public static float timeBetweenLaps=4f;

	public static float minTornadoWaitTime=7f;
	public static float maxTornadoWaitTime=15f;
	public static int spawnCount=3;
	public static float itemPresentationTime=1.5f;

    public static float prePlayFadeInterval = 0.5f;

    public static float normalSpeed = 60f; // used for encoding and all other passive movement stages
    public static float fastSpeed = 100f; // during fast move to retrieval start position


    public static int totalTrials = 18;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
