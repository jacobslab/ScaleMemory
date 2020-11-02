using UnityEngine;
using System.Collections;

public class ObjectColorLogTrack: LogTrack {
	string nameToLog { get { return GetNameToLog (); } }
	
	bool firstLog = false; //should log spawned on the first log
	
	Renderer myRenderer;
	Color lastColor;
	
	
	void Awake(){
		myRenderer = GetComponent<Renderer> ();
	}

	//log on late update so that everything for that frame gets set first
	void LateUpdate(){ 
		if (Experiment.isLogging) {
			Log ();
		}
	}
	
	void Log ()
	{
		//the following is set up to log properties only when they change, or on an initial log.

		if (lastColor != myRenderer.material.color || !firstLog) {
			LogColor ();
		}
		
		firstLog = true;
	}
	
	void LogColor(){
		lastColor = myRenderer.material.color;
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), nameToLog + separator + "OBJECT_COLOR" + separator + lastColor.r + separator + lastColor.g + separator + lastColor.b + separator + lastColor.a);
	}

	
	string GetNameToLog(){
		string name = gameObject.name;
		return name;
	}
	
}