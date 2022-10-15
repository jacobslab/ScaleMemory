using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Runtime.InteropServices;
using System.IO;
using System;
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
	public void LogCorner(int frame)
	{
		Debug.Log("LOGGING CORNER");
		Transform presentationTrans = Experiment.Instance.GetTransformForFrame(frame);

		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "LOGGING_CORNER" + separator + "AssetBundleFrame" + separator + frame.ToString() + separator + presentationTrans.position.x.ToString() + separator + presentationTrans.position.y.ToString() + separator + presentationTrans.position.z.ToString());
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

	public void LogBlockRedo(int blockNum, bool hasBegun)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "BLOCK" + separator + blockNum.ToString() + separator + ((hasBegun) ? "STARTED" : "ENDED") + separator + "REDO");
	}

	public void LogQuestionContinual(bool hasBegun)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "NEW INSTANCE" + separator + "CONTINUAL_Q?" + separator + ((hasBegun) ? "YES" : "NO"));
		if (hasBegun == true) {
			subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "NEW INSTANCE" + separator + "CREATING A NEW DIRECTORY" + separator + ((hasBegun) ? "YES" : "NO"));

		}
	}

	public void LogTrialLoop(int loopNum, bool hasBegun)
    {
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "TRIAL_LOOP" + separator + loopNum.ToString() + separator + ((hasBegun) ? "STARTED" : "ENDED"));
	}

	public void LogTestVersionList(List<int> list, int index)
	{
		var res = string.Join(",", list);
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "TestVersionGlobal" + separator + res.ToString() + separator + index.ToString());
	}

	public void LogTestVersion(int testversion)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "TestVersion" + separator + testversion.ToString());
	}

	public void LogDefaultFixSpeed(float nnew)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "SpeedUpdate" + separator + "SpeedDefaultChangeTo" + separator + nnew.ToString());
		//Debug.Log("Hello");
	}

	public void LogSpeedUp(float prev, float nnew)
	{

		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "SpeedUpdate" + separator + "IncreasingSpeedFrom" + separator + prev.ToString() + separator + "to" + separator + nnew.ToString());
		//Debug.Log("Hello");
	}

	public void LogSpeedDown(float prev, float nnew)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "SpeedUpdate" + separator + "DecreasingSpeedFrom" + separator + prev.ToString() + separator + "to" + separator + nnew.ToString());
		//Debug.Log("Hello");
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
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "LOCATION_CUED_REACTIVATION" + separator + stimObject.name + separator + (Experiment.Instance.StimuliDict[stimObject.name]).ToString() + separator + currTrans.position.x.ToString() + separator + currTrans.position.y.ToString() + separator + currTrans.position.z.ToString() + separator + isLure.ToString() + separator + retIndex.ToString());

	}
	public void LogItemCuedReactivation(GameObject stimObject, bool isLure, int retIndex)
	{
		Transform currTrans = exp.GetTransformForFrame(exp.videoLayerManager.GetMainLayerCurrentFrameNumber());
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "ITEM_CUED_REACTIVATION" + separator + stimObject.name + separator + (Experiment.Instance.StimuliDict[stimObject.name]).ToString() + separator + currTrans.position.x.ToString() + separator + currTrans.position.y.ToString() + separator + currTrans.position.z.ToString() + separator + isLure.ToString() + separator + retIndex.ToString());

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
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "ITEM" + separator + "ENCODING" + separator + objName + separator + (Experiment.Instance.StimuliDict[objName]).ToString() + separator + encodingOrder.ToString() +  separator + frameNum.ToString() + separator + presentationTrans.position.x.ToString() + separator + presentationTrans.position.y.ToString() + separator + presentationTrans.position.z.ToString());
	}

	public void LogItemPresentation(string objName, int objKey, bool isActive)
    {
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "ITEM" + separator + "PRESENTATION" + separator + objName + separator + objKey.ToString() + separator +((isActive)? "STARTED" : "ENDED"));

	}

	public void LogRandIndex(int rndIndex)
	{
		if (exp.beginScreenSelect == -1)
		{
			subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "practice_picking_randindex" + separator + rndIndex.ToString());
		}
		else {
			subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "picking_randindex" + separator + rndIndex.ToString());
		}
	}


	public void LogRetrievalStartPosition(Vector3 pos)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "RETRIEVAL_START_POSITION" + separator + pos.x.ToString() + separator + pos.y.ToString() + separator + pos.z.ToString());

	}
	public void LogVerbalRetrievalAttempt(GameObject objQueried, string fileName)
	{
		Transform currTrans = exp.GetTransformForFrame(exp.videoLayerManager.GetMainLayerCurrentFrameNumber());
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "VERBAL_RETRIEVAL" + separator + objQueried.name + separator + (Experiment.Instance.StimuliDict[objQueried.name]).ToString() + separator + fileName + separator + currTrans.position.x.ToString() + separator + currTrans.position.y.ToString() + separator + currTrans.position.z.ToString());

	}
	public void LogElememConnectionSuccess()
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "ELEMEM_CONNECTION_SUCCESSFUL");
	}
	
	public void LogItemDisplay(string objName,bool isShown)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "ITEM_SCREENING" + separator + objName + separator + (Experiment.Instance.StimuliDict[objName]).ToString() + separator + ((isShown)? "ON" : "OFF"));
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
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "RETRIEVAL_ATTEMPT" + separator + "ITEM" + separator + targetObjName + separator + (Experiment.Instance.StimuliDict[targetObjName]).ToString() + separator + currTrans.position.x.ToString() + separator + currTrans.position.y.ToString() + separator + currTrans.position.z.ToString());
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "RETRIEVAL_ATTEMPT" + separator + "DISTANCE_ERROR" + separator + targetObjName + separator + (Experiment.Instance.StimuliDict[targetObjName]).ToString() + separator + dist.ToString());


	}

	public void LogTemporalOrderTest(GameObject firstItem, GameObject secondItem,bool hasStarted)
    {
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "TEST_PROBE" + separator + "TEMPORAL_ORDER" + separator + firstItem.gameObject.name +separator  + (Experiment.Instance.StimuliDict[firstItem.gameObject.name]).ToString() + separator + secondItem.gameObject.name +  separator + (Experiment.Instance.StimuliDict[secondItem.gameObject.name]).ToString() + separator + ((hasStarted) ? "STARTED" : "ENDED"));

	}

	public void LogTemporalDistanceTest(BlockTestPair testPair, bool hasStarted)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "TEST_PROBE" + separator + "TEMPORAL_DISTANCE" + separator + testPair.firstItem.gameObject.name + separator + (Experiment.Instance.StimuliDict[testPair.firstItem.gameObject.name]).ToString() + separator + testPair.secondItem.gameObject.name + separator + (Experiment.Instance.StimuliDict[testPair.secondItem.gameObject.name]).ToString() + separator + ((hasStarted) ? "STARTED" : "ENDED"));

	}


	public void LogContextRecollectionTest(GameObject testObj, bool hasStarted)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "TEST_PROBE" + separator + "CONTEXT_RECOLLECTION" + separator + testObj.name + separator + (Experiment.Instance.StimuliDict[testObj.name]).ToString() + separator + ((hasStarted) ? "STARTED" : "ENDED"));

	}

	public void LogUserChoiceSelection(int selectionIndex, string selectionType)
    {
		String[] arr = new string[4];
		switch (selectionType) {
			case "Item":
				arr[0] = "Remember";
				arr[1] = "familiar";
				arr[2] = "No";
				arr[3] = "Nothing";
				break;
			case "Location":
				arr[0] = "Yes";
				arr[1] = "No";
				arr[2] = "Nothing";
				arr[3] = "Nothing";
				break;
			case "TemporalOrder":
				arr[0] = "Item_1";
				arr[1] = "Item_2";
				arr[2] = "Nothing";
				arr[3] = "Nothing";
				break;
			case "TemporalDistance":
				arr[0] = "Very Far";
				arr[1] = "Far";
				arr[2] = "Close";
				arr[3] = "Very Close";
				break;
			case "ContextRecollection":
				arr[0] = "Sunny";
				arr[1] = "Rainy";
				arr[2] = "Night";
				arr[3] = "Nothing";
				break;
		}

		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "USER_SELECTION_INDEX" + separator + arr[selectionIndex] + separator + selectionIndex.ToString() + separator + selectionType.ToString());

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
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "ENCODING_ITEM" + separator + objName + separator + (Experiment.Instance.StimuliDict[objName]).ToString() + separator + pos.x.ToString() + separator + pos.y.ToString() + separator + pos.z.ToString());
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

	public void LogPressedKey(KeyCode vCode)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "KeyCodePress" + separator + vCode.ToString());

	}

	public void LogDistractorTask(bool hasStarted)
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "DistractorTask" + separator + ((hasStarted) ? "STARTED" : "ENDED"));
	}

	public void LogDistractorTaskText()
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "DistractorTask" + separator + "LoggedNumber" + separator + exp.uiController.DistractorText.text);
	}

	public void LogMetaData()
	{
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "Experiment" + separator + "Started");
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "MetaData" + separator + "Version" + separator + "CityBlock");
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "MetaData" + separator + "Build" + separator + "V25_11");
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "MetaData" + separator + "Language" + separator + "English");
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "MetaData" + separator + "Platform" + separator + "Unity");
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "MetaData" + separator + "OS Platform" + separator + "MAC");
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "ValidationInfo" + separator + "ExpNumBlocks" + separator + "6");
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "ValidationInfo" + separator + "ExpNumTrials" + separator + "24");
	}

	public void LogPosition(int activeLayer, int frameNum)
	{
		String str = "";
		switch (activeLayer)
		{
			case 0:
				str = "Nothing";
				break;
			case 1:
				str = "SUNNY";
				break;
			case 2:
				str = "RAINY";
				break;
			case 3:
				str = "NIGHT";
				break;
		}
		Transform presentationTrans = Experiment.Instance.GetTransformForFrame(frameNum);
		subjectLog.Log(GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), "LogPosition" + separator + str + separator + frameNum.ToString() + separator + presentationTrans.position.x.ToString() + separator + presentationTrans.position.y.ToString() + separator + presentationTrans.position.z.ToString());

	}
}