using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Runtime.InteropServices;

public class TrialLogTrack : LogTrack {


	bool firstLog = false;

	//log on late update so that everything for that frame gets set first
	void LateUpdate () {
		//just log the environment info on the first frame
		if (Experiment.isLogging && !firstLog) {
			//presumably testing logging
//			LogBegin ();
//			LogEnd ();
//			LogBegin ();
			//log session
			LogSessionStart();
			//LogMicTest ();

			firstLog = true;
		}
	}

	//gets called from trial controller instead of in update!
	public void Log(int trialNumber){
		if (Experiment.isLogging) {
			LogTrial (trialNumber);
		}
	}

	public void LogBegin()
	{
		Debug.Log ("LOGGING BEGINS");
		subjectLog.Log (GameClock.SystemTime_Milliseconds, "0" + separator + "B" + separator + "Logging Begins");
	}
	void LogEnd()
	{
		Debug.Log ("LOGGING ENDS");
		subjectLog.Log (GameClock.SystemTime_Milliseconds, "0" + separator + "E" + separator + "Logging Ends");
	}

	public void LogPauseEvent(bool isPaused)
	{
		Debug.Log ("game paused");
		if(isPaused)
			subjectLog.Log (GameClock.SystemTime_Milliseconds, "0" + separator + "TASK_PAUSED");
		else
			subjectLog.Log (GameClock.SystemTime_Milliseconds, "0" + separator + "TASK_RESUMED");
	}

	public void LogMicTest()
	{
		subjectLog.Log (GameClock.SystemTime_Milliseconds, "0" + separator + "MIC_TEST");
	}

	void LogSessionStart(){
		Debug.Log ("LOGGED SESSION START");
		string buildVersion = Experiment.BuildVersion.ToString ();
		subjectLog.Log (GameClock.SystemTime_Milliseconds, "0" + separator + "SESS_START" + separator + "1" + separator + buildVersion + " v"+ Experiment.BuildVersion);
	}

	public void LogBlackrockConnectionAttempt()
	{ 
		subjectLog.Log(GameClock.SystemTime_Milliseconds, "BLACKROCK_CONNECTION_ATTEMPT");
	}
	public void LogBlackrockConnectionSuccess()
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, "BLACKROCK_CONNECTION_SUCCESSFUL");
	}
	
	public void LogItemDisplay(string objName,bool isShown)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, "ITEM_SCREENING" + separator + objName + separator + ((isShown)? "ON" : "OFF"));
	}
	public void LogStartNSPTime(long initial_nsp_time, long initial_neural_time)
    {
		subjectLog.Log(GameClock.SystemTime_Milliseconds, "NSP_START" + separator + initial_nsp_time + separator + initial_neural_time);
    }


	public void LogNSPSyncTime(long nsp_time, long neural_time)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, "NSP_SYNC" + separator + nsp_time + separator + neural_time);
	}


	public void LogTrial(int trialNum, bool hasStarted)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, "TRIAL" + separator + trialNum.ToString() + separator + ((hasStarted) ? "STARTED" : "ENDED"));
	}
	public void LogFixation(bool hasStarted)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, "FIXATION" + separator + ((hasStarted) ? "STARTED" : "ENDED"));
	}
	public void LogInstructions(bool hasStarted)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, "SHOWING_INSTRUCTIONS" + separator + ((hasStarted) ? "STARTED" : "ENDED"));
	}

	public void LogRetrievalAttempt(GameObject targetObj, GameObject car)
	{
		string targetObjName = targetObj.gameObject.name.Split('(')[0];
		
		subjectLog.Log(GameClock.SystemTime_Milliseconds, "RETRIEVAL_ATTEMPT" + separator + targetObjName + separator + car.transform.position.x.ToString() + separator + car.transform.position.y.ToString() + separator + car.transform.position.z.ToString());
	}

	public void LogTrafficLightVisibility(bool isVisible)
    {
		subjectLog.Log(GameClock.SystemTime_Milliseconds, "TRAFFIC_LIGHT_VISIBLE" + separator + ((isVisible) ? "TRUE" : "FALSE"));

	}

	public void LogTrafficLightColor(TrafficLightController.TrafficLights trafficLights)
	{
		switch (trafficLights)
		{
			case TrafficLightController.TrafficLights.Red:
				subjectLog.Log(GameClock.SystemTime_Milliseconds, "TRAFFIC_LIGHT" + separator + "RED");
				break;
			case TrafficLightController.TrafficLights.Yellow:
				subjectLog.Log(GameClock.SystemTime_Milliseconds, "TRAFFIC_LIGHT" + separator + "YELLOW");
				break;
			case TrafficLightController.TrafficLights.Green:
				subjectLog.Log(GameClock.SystemTime_Milliseconds, "TRAFFIC_LIGHT" + separator + "GREEN");
				break;
		}

	}

	public void LogCarBrakes(bool isActive)
    {
		subjectLog.Log(GameClock.SystemTime_Milliseconds, "CAR_BRAKES" + separator + ((isActive)? "ACTIVE" : "INACTIVE"));

	}

	public void LogEncodingItemSpawn(string objName, Vector3 pos)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, "ENCODING_ITEM" + separator + objName + separator + pos.x.ToString() + separator + pos.y.ToString() + separator + pos.z.ToString());
	}

	public void LogSlowZoneLocation (Vector3 location)
    {
		subjectLog.Log(GameClock.SystemTime_Milliseconds, "SLOW_ZONE_LOCATION" + separator + location.x.ToString() + separator + location.y.ToString() + separator + location.z.ToString());
	}
	public void LogSpeedZoneLocation(Vector3 location)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, "SPEED_ZONE_LOCATION" + separator + location.x.ToString() + separator + location.y.ToString() + separator + location.z.ToString());
	}


	public void LogTaskStage(Experiment.TaskStage taskStage, bool hasStarted)
	{
		switch(taskStage)
		{
			case Experiment.TaskStage.ItemScreening:
				subjectLog.Log(GameClock.SystemTime_Milliseconds, "ITEM_SCREENING_PHASE" + separator + ((hasStarted) ? "STARTED" : "ENDED"));
				break;
			case Experiment.TaskStage.TrackScreening:
				subjectLog.Log(GameClock.SystemTime_Milliseconds, "TRACK_SCREENING_PHASE" + separator + ((hasStarted) ? "STARTED" : "ENDED"));
				break;
			case Experiment.TaskStage.Encoding:
				subjectLog.Log(GameClock.SystemTime_Milliseconds, "ENCODING" + separator + ((hasStarted) ? "STARTED" : "ENDED"));
				break;
			case Experiment.TaskStage.Retrieval:
				subjectLog.Log(GameClock.SystemTime_Milliseconds, "RETRIEVAL" + separator + ((hasStarted) ? "STARTED" : "ENDED"));
				break;
		}
	}

	public void LogItemSpawn(bool hasStarted)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, "TRACK_SCREENING" + separator + ((hasStarted) ? "STARTED" : "ENDED"));
	}

	//LOGGED ON THE START OF THE TRIAL.
	public void LogTrial(int trialNumber){
		/*
		if(Experiment.practice)
			subjectLog.Log (GameClock.SystemTime_Milliseconds, "0" + separator + "PRACTICE_TRIAL");
		else
		*/
			subjectLog.Log (GameClock.SystemTime_Milliseconds, "0" + separator + "TRIAL" + separator + trialNumber + separator + "NONSTIM");
	}



}