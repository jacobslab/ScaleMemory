using UnityEngine;
using System.Collections;

public class InstructionVideoLogTrack : LogTrack {

	public void LogInstructionVideoStarted(){
		if (Experiment.isLogging) {
			subjectLog.Log (GameClock.SystemTime_Milliseconds, "0" + separator + "INSTRUCT_VIDEO" + separator + "ON");
		}
	}

	public void LogInstructionVideoStopped(){
		if (Experiment.isLogging) {
			subjectLog.Log (GameClock.SystemTime_Milliseconds, "0" + separator + "INSTRUCT_VIDEO" + separator + "OFF");
		}
	}

}
