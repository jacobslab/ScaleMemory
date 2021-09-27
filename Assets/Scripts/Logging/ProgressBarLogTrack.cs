using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ProgressBarLogTrack: LogTrack {

//	bl_ProgressBar myProgressBar;
	Image currentImage;
	float currentValue=0f;
	float maxValue=1f;
	Color currentColor = Color.black;

	bool firstLog = false; //should make an initial log

	// Use this for initialization
	void Awake () {
//		myProgressBar = GetComponent<bl_ProgressBar> ();
		currentImage = GetComponent<Image> ();
	}

	//log on late update so that everything for that frame gets set first
	void LateUpdate () {
//		if (myProgressBar == null) {
//			Debug.Log("Progress Bar not found!");
//		}
		if (!firstLog) {
			firstLog = true;
			LogProgressBar ();
			LogColor();
		}
//		if(ExperimentSettings.isLogging && ( currentValue != myProgressBar.Value ) ){ //if the text has changed, log it!
//			LogProgressBar ();
//		}
		if(Experiment.isLogging && ( currentColor != currentImage.color ) ){ //if the text has changed, log it!
			LogColor ();
		}
	}

	void LogProgressBar()
	{

//		maxValue = myProgressBar.MaxValue;
//		currentValue = myProgressBar.Value;
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), gameObject.name + separator + "PROGRESS_BAR" + separator + currentValue.ToString() + separator + maxValue.ToString());
	}



	void LogColor(){

		currentColor = currentImage.color;
		subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount(), gameObject.name + separator + "IMAGE_COLOR" + separator + currentColor.r + separator + currentColor.g + separator + currentColor.b + separator + currentColor.a);
	}
}