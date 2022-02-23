using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class Configuration : MonoBehaviour {


	public static float timedEventInterval=10f;
	public static float lapsToBeCompleted=3;
	public static int tornadoArrivalTime=10;
	public static float tornadoPenaltyTime=5f;
	public static float tornadoWarningDisplayTime=3f;

#if UNITY_EDITOR
    public static float familiarizationMaxTime = 1f;
#else
    public static float familiarizationMaxTime = 60f;
#endif

#if UNITY_WEBGL
    public static string audioFileExtension = ".ogg";
#else
    public static string audioFileExtension = ".wav";
#endif


    public static float distanceThreshold=5f; //minimum distance before an object is said to be "on the point"
	public static float timeThreshold=2f; //minimum time before or after an action can be said to be "on cue"

	public static float timeBetweenLaps=4f;

	public static float minTornadoWaitTime=7f;
	public static float maxTornadoWaitTime=15f;
	public static int spawnCount=3;
	public static float itemPresentationTime=1.6f;

    public static int heartbeatInterval = 1000;

    public static int minBufferLures = 50;

    public static int minGapBetweenStimuli = 3; //measured in waypoints

    //presentation jitter time
    public static float minJitterTime = 0.25f;
    public static float maxJitterTime = 0.8f;


    //frame speed
    public static float minFrameSpeed = 0.9f;
    public static float maxFrameSpeed = 1.2f;

    public static float minRetrievalFrameSpeed = 1f;
    public static float maxRetrievalFrameSpeed = 1.4f;

    //spawn possibility buffer to start and end of loop
    public static int startBuffer = 90;
    public static int endBuffer = 90;

    public static int minFramesBetweenStimuli = 150;

    public static int luresPerTrial = 2;

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

    public static int elememTimeoutMS = 1000;
    public static int elememHeartbeatTimeoutMS = 20;

    public enum StimMode
    {
        NONSTIM,
        STIM
    };

    public static StimMode stimMode = StimMode.NONSTIM;


    public static int ReturnWeatherTypes()
    {
        return Enum.GetNames(typeof(Weather.WeatherType)).Length;
    }

    // Use this for initialization
    void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
