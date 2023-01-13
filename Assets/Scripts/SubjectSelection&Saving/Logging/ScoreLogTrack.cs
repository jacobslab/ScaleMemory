using UnityEngine;
using System.Collections;

public class ScoreLogTrack : LogTrack {

	// Use this for initialization
	void Start () {
	
	}

	public void LogTimeBonusAdded(int scoreAdded){
		if (Experiment.isLogging) {
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), gameObject.name + separator + "SCORE_ADDED_TIME" + separator + scoreAdded);
		}
	}

	public void LogMemoryScoreAdded(int scoreAdded){
		if (Experiment.isLogging) {
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), gameObject.name + separator + "SCORE_ADDED_MEMORY" + separator + scoreAdded);
		}
	}

	public void LogObjectScoreAdded(int scoreAdded){
		if (Experiment.isLogging) {
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), gameObject.name + separator + "SCORE_ADDED_OBJECT" + separator + scoreAdded);
		}
	}

	public void LogScoreReset(){
		if (Experiment.isLogging) {
			subjectLog.Log (GameClock.SystemTime_Milliseconds, subjectLog.GetFrameCount (), gameObject.name + separator + "SCORE_RESET");
		}
	}
}
