using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Runtime.InteropServices;
//using UnityEditor.Experimental.GraphView;

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
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "0" + separator + "B" + separator + "Logging Begins");
	}
	void LogEnd()
	{
		Debug.Log ("LOGGING ENDS");
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "0" + separator + "E" + separator + "Logging Ends");
	}

	public void LogIntroInstruction(bool isActive)
    {
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "INTRO_INSTRUCTIONS" + separator + ((isActive) ? "STARTED" : "ENDED"));

	}

	public void LogPauseEvent(bool isPaused)
	{
		Debug.Log ("game paused");
		if(isPaused)
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "0" + separator + "TASK_PAUSED");
		else
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "0" + separator + "TASK_RESUMED");
	}

	public void LogMicTest()
	{
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "0" + separator + "MIC_TEST");
	}


	public void LogBlock(int blockNum, bool hasBegun)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "BLOCK" + separator + blockNum.ToString() + separator + ((hasBegun) ? "STARTED" : "ENDED"));
	}

	public void LogTrialLoop(int loopNum, bool hasBegun)
    {
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "TRIAL_LOOP" + separator + loopNum.ToString() + separator + ((hasBegun) ? "STARTED" : "ENDED"));
	}

	public void LogProlificWorkerInfo(string prolific_pid, string study_id, string session_id)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "PROLIFIC_PID" + separator + prolific_pid + separator + "STUDY_ID" + separator + study_id + separator + "SESSION_ID" + separator + session_id);
	}


	public void LogProlificFailEvent()
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "PROLIFIC_COLLECTION_FAILED");

	}

	public void LogUIEvent(string eventName, bool isStarted)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), eventName + separator + ((isStarted) ? "STARTED" : "ENDED"));
	}

	void LogSessionStart(){
		Debug.Log ("LOGGED SESSION START");
		string buildVersion = Experiment.BuildVersion.ToString ();
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "0" + separator + "SESS_START" + separator + "1" + separator + buildVersion + " v"+ Experiment.BuildVersion);
	}

    public void LogTreasureLabel(string labelText)
    {
        subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "TREASURE_ITEM" + separator + labelText);
    }

	public void LogLocationCuedReactivation(GameObject stimObject, bool isLure, int retIndex)
	{
		Transform currTrans = exp.GetTransformForFrame(exp.videoLayerManager.GetMainLayerCurrentFrameNumber());
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "LOCATION_CUED_REACTIVATION" + separator + stimObject.name + separator + currTrans.position.x.ToString() + separator + currTrans.position.y.ToString() + separator + currTrans.position.z.ToString() + separator + isLure.ToString() + separator + retIndex.ToString());

	}
	public void LogItemCuedReactivation(GameObject stimObject, bool isLure, int retIndex)
	{
		Transform currTrans = exp.GetTransformForFrame(exp.videoLayerManager.GetMainLayerCurrentFrameNumber());
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "ITEM_CUED_REACTIVATION" + separator + stimObject.name + separator + currTrans.position.x.ToString() + separator + currTrans.position.y.ToString() + separator + currTrans.position.z.ToString() + separator + isLure.ToString() + separator + retIndex.ToString());

	}



	public void LogElememConnectionAttempt()
	{ 
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "ELEMEM_CONNECTION_ATTEMPT");
	}

	public void LogEncodingStartPosition(Vector3 pos)
    {
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "ENCODING_START_POSITION" + separator + pos.x.ToString() + separator + pos.y.ToString() + separator + pos.z.ToString());

	}

	public void LogItemEncodingEvent(string objName, int frameNum, int encodingOrder)
    {

		Transform presentationTrans = Experiment.Instance.GetTransformForFrame(frameNum);
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "ENCODING_ITEM" + separator + objName + separator + encodingOrder.ToString() +  separator + presentationTrans.position.x.ToString() + separator + presentationTrans.position.y.ToString() + separator + presentationTrans.position.z.ToString());
	}

	public void LogItemPresentation(string objName,bool isActive)
    {
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "ITEM_PRESENTATION" + separator + objName + separator + ((isActive)? "BEGAN" : "ENDED"));

	}


	public void LogRetrievalStartPosition(Vector3 pos)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "RETRIEVAL_START_POSITION" + separator + pos.x.ToString() + separator + pos.y.ToString() + separator + pos.z.ToString());

	}
	public void LogVerbalRetrievalAttempt(GameObject objQueried, string fileName)
	{
		Transform currTrans = exp.GetTransformForFrame(exp.videoLayerManager.GetMainLayerCurrentFrameNumber());
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "VERBAL_RETRIEVAL" + separator + objQueried.name + separator + fileName + separator + currTrans.position.x.ToString() + separator + currTrans.position.y.ToString() + separator + currTrans.position.z.ToString());

	}
	public void LogElememConnectionSuccess()
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "ELEMEM_CONNECTION_SUCCESSFUL");
	}
	
	public void LogItemDisplay(string objName,bool isShown)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "ITEM_SCREENING" + separator + objName + separator + ((isShown)? "ON" : "OFF"));
	}
	public void LogStartNSPTime(long initial_nsp_time, long initial_neural_time)
    {
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "NSP_START" + separator + initial_nsp_time + separator + initial_neural_time);
    }


	public void LogNSPSyncTime(long nsp_time, long neural_time)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "NSP_SYNC" + separator + nsp_time + separator + neural_time);
	}


	public void LogTrial(int trialNum, bool hasStarted)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "TRIAL" + separator + trialNum.ToString() + separator + ((hasStarted) ? "STARTED" : "ENDED"));
	}
	public void LogFixation(bool hasStarted)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "FIXATION" + separator + ((hasStarted) ? "STARTED" : "ENDED"));
	}
	public void LogInstructions(bool hasStarted)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "SHOWING_INSTRUCTIONS" + separator + ((hasStarted) ? "STARTED" : "ENDED"));
	}
	public void LogIntermission(bool hasStarted)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "INTERMISSION_PERIOD" + separator + ((hasStarted) ? "STARTED" : "ENDED"));

	}

	public void LogRetrievalAttempt(GameObject targetObj)
	{
		Transform currTrans = exp.GetTransformForFrame(exp.videoLayerManager.GetMainLayerCurrentFrameNumber());
		string targetObjName = targetObj.gameObject.name.Split('(')[0];
		float dist = Vector3.Distance(targetObj.transform.position, currTrans.position);
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "RETRIEVAL_ATTEMPT" + separator + targetObjName + separator + currTrans.position.x.ToString() + separator + currTrans.position.y.ToString() + separator + currTrans.position.z.ToString());
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "RETRIEVAL_ATTEMPT_DISTANCE_ERROR" + separator + targetObjName + separator + dist.ToString());


	}

	public void LogTemporalOrderTest(GameObject firstItem, GameObject secondItem,bool hasStarted)
    {
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "TEMPORAL_ORDER_TEST" + separator + firstItem.gameObject.name +separator  + secondItem.gameObject.name +  separator + ((hasStarted) ? "STARTED" : "ENDED"));

	}

	public void LogTemporalDistanceTest(BlockTestPair testPair, bool hasStarted)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "TEMPORAL_DISTANCE_TEST" + separator + testPair.firstItem.gameObject.name + separator + testPair.secondItem.gameObject.name + separator + ((hasStarted) ? "STARTED" : "ENDED"));

	}


	public void LogContextRecollectionTest(GameObject testObj, bool hasStarted)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "CONTEXT_RECOLLECTION_TEST" + separator + testObj.name + separator + ((hasStarted) ? "STARTED" : "ENDED"));

	}

	public void LogUserChoiceSelection(int selectionIndex, string selectionType)
    {
		//switch(selectionType)
		//      {
		//	case "Item":

		//		break;
		//	case "Location":
		//		break;
		//	case "TemporalOrder":
		//		break;
		//	case "TemporalDistance":
		//		break;
		//	case "ContextRecollection":
		//		break;
		//}

		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "USER_SELECTION_INDEX" + separator + selectionIndex.ToString() + separator + selectionType.ToString());

	}


	public void LogWeather(Weather.WeatherType currWeather)
    {
		UnityEngine.Debug.Log("CHECK WEATHER LOG " + currWeather.ToString());
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "WEATHER_TYPE" + separator + currWeather.ToString());


	}

	public void LogTrafficLightVisibility(bool isVisible)
    {
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "TRAFFIC_LIGHT_VISIBLE" + separator + ((isVisible) ? "TRUE" : "FALSE"));

	}
	public void LogCarMovement(bool isActive)
    {
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "CAR_MOVEMENT" + separator + ((isActive)? "ACTIVE" : "INACTIVE"));

	}

	public void LogEncodingItemSpawn(string objName, Vector3 pos)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "ENCODING_ITEM" + separator + objName + separator + pos.x.ToString() + separator + pos.y.ToString() + separator + pos.z.ToString());
	}

	public void LogSlowZoneLocation (Vector3 location)
    {
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "SLOW_ZONE_LOCATION" + separator + location.x.ToString() + separator + location.y.ToString() + separator + location.z.ToString());
	}
	public void LogSpeedZoneLocation(Vector3 location)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "SPEED_ZONE_LOCATION" + separator + location.x.ToString() + separator + location.y.ToString() + separator + location.z.ToString());
	}


	public void LogTaskStage(Experiment.TaskStage taskStage, bool hasStarted)
	{
		switch(taskStage)
		{
			case Experiment.TaskStage.ItemScreening:
				subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "ITEM_SCREENING_PHASE" + separator + ((hasStarted) ? "STARTED" : "ENDED"));
				break;
			case Experiment.TaskStage.TrackScreening:
				subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "TRACK_SCREENING_PHASE" + separator + ((hasStarted) ? "STARTED" : "ENDED"));
				break;
			case Experiment.TaskStage.Encoding:
				subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "ENCODING" + separator + ((hasStarted) ? "STARTED" : "ENDED"));
				break;
			case Experiment.TaskStage.SpatialRetrieval:
				subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "RETRIEVAL_SPATIAL" + separator + ((hasStarted) ? "STARTED" : "ENDED"));
				break;
			case Experiment.TaskStage.VerbalRetrieval:
				subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "RETRIEVAL_VERBAL" + separator + ((hasStarted) ? "STARTED" : "ENDED"));
				break;
		}
	}

	//public void LogFramePosition(int frameNum)
 //   {
	//	Transform currTransform = Experiment.Instance.GetTransformForFrame(frameNum);
	//	subjectLog.Log(GameClock.SystemTime_Milliseconds, "PLAYER_POSITION" + separator + ((hasStarted) ? "STARTED" : "ENDED"));

	//}

	public void LogForwardMovement(bool hasStarted)
    {
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "FORWARD_MOVEMENT" + separator + ((hasStarted) ? "STARTED" : "ENDED"));

	}
	
	public void LogReverseMovement(bool hasStarted)
    {
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "REVERSE_MOVEMENT" + separator + ((hasStarted) ? "STARTED" : "ENDED"));
	}

	public void LogItemSpawn(bool hasStarted)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "TRACK_SCREENING" + separator + ((hasStarted) ? "STARTED" : "ENDED"));
	}

	//LOGGED ON THE START OF THE TRIAL.
	public void LogTrial(int trialNumber){
		/*
		if(Experiment.practice)
			subjectLog.Log (GameClock.SystemTime_Milliseconds, "0" + separator + "PRACTICE_TRIAL");
		else
		*/
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "0" + separator + "TRIAL" + separator + trialNumber + separator + "NONSTIM");
	}



}