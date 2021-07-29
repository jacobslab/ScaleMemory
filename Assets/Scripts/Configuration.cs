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
	public static float itemPresentationTime=1.6f;

    public static int heartbeatInterval = 1000;

    public static int minGapBetweenStimuli = 4; //measured in waypoints

    //presentation jitter time
    public static float minJitterTime = 0.25f;
    public static float maxJitterTime = 0.8f;


    //reactivation times
    public static float itemReactivationTime = 2f;
    public static float locationReactivationTime = 2f;



    //ELEMEM settings

#if UNITY_EDITOR
    public static string ipAddress = "127.0.0.1";
    public static int portNumber = 5555;
#else
    public static string ipAddress = "192.168.137.1";
    public static int portNumber = 8889;
#endif

    public enum StimMode
    {
        NONSTIM,
        STIM
    };

    public static StimMode stimMode = StimMode.NONSTIM;

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
