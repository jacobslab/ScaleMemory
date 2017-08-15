using UnityEngine;
using System.Collections;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using UnityEngine.UI;
using System.IO;
using UnityEngine.SceneManagement;
using iView;
public class TrialController : MonoBehaviour {
	Experiment exp { get { return Experiment.Instance; } }

	List<Store> orderedStores;
	List<string> orderedItemsDelivered;

	public enum TrialState{
		presentationLearning,
		navigationLearning,
		delivery,
		recall
	}
	public TrialState currentState = TrialState.presentationLearning;


	//hardware connection
	bool isConnectingToHardware = false;

	//paused?!
	public static bool isPaused = false;

    //lst generation
    public string lstContents = "";

	//UI
	public CanvasGroup PauseUI;
	public CanvasGroup ConnectionUI;
	public Text ConnectionText; //changed in TrialController from "connecting..." to "press start..." etc.
	public CanvasGroup RecallUI;
	public CanvasGroup InitialDeliveryInstructionGroup;
	public Text InitialDeliveryInstructionText;
	public Text DeliveryInstructionText;
	public Text LearningSessionProgressText;

	//audio
	public AudioSource recallStartBeep;
	public AudioSource recallEndBeep;

	//delivery timer
	public SimpleTimer learningPhaseTimer;
    public SimpleTimer deliveryTimer;

	//store presentation variables
	public Transform storePresentationTransform;
	public Transform recallStorePresentationTransform;
	public VisibilityToggler presentationBackgroundCube;

	public Text pauseText;

	int numRealTrials = 0; //used for logging trial ID's


	[HideInInspector] public GameObject currentDefaultObject; //current treasure chest we're looking for. assuming a one-by-one reveal.
	

	void Start(){
		presentationBackgroundCube.TurnVisible (false);
		orderedStores = new List<Store>();
		orderedItemsDelivered = new List<string>();
		//disable player camera
		exp.player.playerCam.enabled = false;
		pauseText.enabled = false;
		InitUIText ();
	}

	public Text GamePausedText;
	public Text HowToPauseInstructionText;
	void InitUIText(){
	#if GERMAN
		GamePausedText.text = "Pause";
		HowToPauseInstructionText.text = "(B) Pause";
	#endif
	}

	
	void Update(){
		if(!isConnectingToHardware && currentState != TrialState.recall){
			GetPauseInput ();
		}

//		if (Input.GetKeyDown (KeyCode.Alpha3))
//			SceneManager.LoadScene (2);
	}

	bool isPauseButtonPressed = false;
	void GetPauseInput(){
		//if (Input.GetAxis ("Pause Button") > 0) {
		if(Input.GetKeyDown(KeyCode.B) || Input.GetKey(KeyCode.JoystickButton2)){ //B JOYSTICK BUTTON TODO: move to input manager.
			Debug.Log("PAUSE BUTTON PRESSED");
			if(!isPauseButtonPressed){
				isPauseButtonPressed = true;
				Debug.Log ("PAUSE OR UNPAUSE");
				TogglePause (); //pause
			}
		} 
		else{
			isPauseButtonPressed = false;
		}
	}

	public void TogglePause(){
		isPaused = !isPaused;
		exp.eventLogger.LogPauseEvent (isPaused);

		if (isPaused) {
			//exp.player.controls.Pause(true);
			PauseUI.alpha = 1.0f;
			Time.timeScale = 0.0f;
			TCPServer.Instance.SetState (TCP_Config.DefineStates.PAUSED, true);
		} 
		else {
			Time.timeScale = 1.0f;
			PauseUI.alpha = 0.0f;
			TCPServer.Instance.SetState (TCP_Config.DefineStates.PAUSED, false);
		}
	}

    IEnumerator DoDeliveryDays(int numDelivDays)
    {
        int numDelivDaysComplete = 0;

		bool doAnotherDeliveryDay = true;
#if HOSPITAL
        deliveryTimer.StartTimer();
        bool isDeliveryTimerRunning=deliveryTimer.IsRunning;
   
		Debug.Log("Checking if enough items left");
		yield return StartCoroutine(exp.storeController.CheckStoresAndItemsLeft());

		for (int i = 0; ExperimentSettings.sufficientItemsForDeliveryDay; i++)
#else
        for (int i = 0; i < numDelivDays && ExperimentSettings.sufficientItemsForDeliveryDay; i++)
#endif
        {

			bool validInput = false;
			numDelivDaysComplete = i;
			UnityEngine.Debug.Log ("in delivery day  " + i + " / " + numDelivDays);
			exp.player.controls.ShouldLockControls = true;
			//check for eye reconnection
#if EYETRACKER
            if (exp.gazeController.edgeConfidence)
                yield return StartCoroutine(exp.gazeController.ShowEyeReconnectionScreen());
#endif

			if (i == 0) {

#if GERMAN
						yield return StartCoroutine (exp.instructionsController.ShowInstructionScreen (exp.instructionsController.recapDeliveryInstructions_German, true, false, Config.minDefaultInstructionTime));
#else
				yield return StartCoroutine (exp.instructionsController.ShowInstructionScreen (exp.instructionsController.recapDeliveryInstructions, true, false, Config.minDefaultInstructionTime));
#endif
			} else {

#if HOSPITAL

				exp.instructionsController.instructionPanel.alpha = 1f;
#if GERMAN
				exp.instructionsController.text.color=Color.white;
				exp.instructionsController.text.text= "Drücken sie (X) um mit Auslieferungs-Runde " + (i + 1) + " zu beginnen. \n Drücken sie (A) zum Ende";

				while (!validInput) {
					if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown (KeyCode.JoystickButton0)) {
						doAnotherDeliveryDay = true;
						validInput = true;
					} else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown (KeyCode.JoystickButton1)) {
						doAnotherDeliveryDay = false;
						validInput = true;
					}
					yield return 0;
				}

				exp.instructionsController.instructionPanel.alpha = 0f;
				exp.instructionsController.text.text="";
//				yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("Drücken sie (X) um mit Auslieferungs-Runde " + (i + 1) + " zu beginnen. \n Drücken sie (A) zum Ende", true, true, false, Config.minDefaultInstructionTime));
#else
				exp.instructionsController.text.color=Color.white;
				exp.instructionsController.text.text= "Press (X) to begin delivery day number " + (i + 1) + " \n Press (A) to end delivery session and proceed to final recall";

				while (!validInput) {
					if (Input.GetKeyDown(KeyCode.X) || Input.GetKeyDown (KeyCode.JoystickButton0)) {
						doAnotherDeliveryDay = true;
						validInput = true;
					} else if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown (KeyCode.JoystickButton1)) {
						doAnotherDeliveryDay = false;
						validInput = true;
					}
					yield return 0;
				}

				exp.instructionsController.instructionPanel.alpha = 0f;
				exp.instructionsController.text.text="";
//				yield return StartCoroutine(exp.instructionsController.ShowSingleInstruction("Press (X) to begin delivery day number " + (i + 1) + " \n Press (A) to end delivery session and proceed to final recall", true, true, false, Config.minDefaultInstructionTime));
#endif
#else
#if GERMAN
				yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("Drücken sie (x) um mit Auslieferungs-Runde " + (i + 1) + "/" + ExperimentSettings.numDelivDays + " zu beginnen.", true, true, false, Config.minDefaultInstructionTime));
#else
                yield return StartCoroutine(exp.instructionsController.ShowSingleInstruction("Press (X) to begin delivery day number " + (i + 1) + "/" + numDelivDays + ".", true, true, false, Config.minDefaultInstructionTime));
#endif
#endif
			}
			if (doAnotherDeliveryDay) {
				exp.player.controls.ShouldLockControls = false;

				//DELIVERY DAY
				orderedStores.Clear ();
				orderedItemsDelivered.Clear ();
				yield return StartCoroutine (DoStoreDeliveryPhase (i));


				//record all items delivered in LST for annotation later


				// Make sure directory exists if user is saving to sub dir.
				UnityEngine.Debug.Log ("LST contents are: " + lstContents);
				string trialLstName = exp.SessionDirectory + "audio/" + i.ToString () + ".lst";
				Directory.CreateDirectory (Path.GetDirectoryName (trialLstName));
  
				//write contents
				System.IO.File.WriteAllText (trialLstName, lstContents);
				//reset lstContents for the next delivery day
				lstContents = "";
				//RECALL
				//TODO: implement different kinds of recall phases
				Config.RecallType recallType = Config.RecallType.FreeThenCued;
				if (recallType == Config.RecallType.FreeThenCued) {
					yield return StartCoroutine (DoRecallPhase (Config.RecallType.FreeItemRecall, i));
					RecallUI.alpha = 1f;
					#if GERMAN
				exp.recallInstructionsController.DisplayText("Welchen Gegenstand haben Sie zu diesem Geschäft geliefert?");
					#else
					exp.recallInstructionsController.DisplayText ("Which object did you deliver to this store?");
					#endif
					yield return new WaitForSeconds (2f); //wait for 2 seconds
					RecallUI.alpha = 0f;
					exp.recallInstructionsController.DisplayText ("");
					exp.audioRecorder.recordText.color = Color.red;
					yield return StartCoroutine (DoRecallPhase (Config.RecallType.CuedRecall, i));
				} else {
					yield return StartCoroutine (DoRecallPhase (recallType, i));
				}

			} else
				break;
		}

		if (!ExperimentSettings.sufficientItemsForDeliveryDay || !doAnotherDeliveryDay) {
			//exp.EndExperiment ();

			exp.player.controls.ShouldLockControls = true;
			//show final instructions screen
#if GERMAN
				yield return StartCoroutine (exp.instructionsController.ShowInstructionScreen(exp.instructionsController.finishedDeliveryInstructions_German,true,false,Config.minDefaultInstructionTime));
#else
			yield return StartCoroutine (exp.instructionsController.ShowInstructionScreen (exp.instructionsController.finishedDeliveryInstructions, true, false, Config.minDefaultInstructionTime));
#endif
			//FINAL RECALL
			if (Config.doFinalStoreRecall) {
				yield return StartCoroutine (DoRecallPhase (Config.RecallType.FinalStoreRecall, numDelivDaysComplete + 1)); //it's an extra recall phase! +1
			}

			if (Config.doFinalItemRecall) {
				yield return StartCoroutine (DoRecallPhase (Config.RecallType.FinalItemRecall, numDelivDaysComplete + 1));
			}
		}
    }

    //FILL THIS IN DEPENDING ON EXPERIMENT SPECIFICATIONS
    public IEnumerator RunExperiment(){
		if (!ExperimentSettings.isReplay) {
			exp.player.controls.ShouldLockControls = true;
			if(Config.isSystem2 || Config.isSyncbox || Config.isSYS3){
				yield return StartCoroutine( WaitForEEGHardwareConnection() );
			}
			else{
				ConnectionUI.alpha = 0.0f;
			}


			bool isLearningSession = false;
			if(ExperimentSettings.Instance.mySessionType == ExperimentSettings.SessionType.learningSession){
				isLearningSession = true;
			}


			//CREATE SESSION STARTED FILE!
			exp.CreateSessionStartedFile();




			#if EYETRACKER
            exp.gazeController.EnableCalibrationUI(true);
            //wait till eyetracker finished calibration & validation procedure
            while(SMIGazeController.Instance.isDoingCalibration)
            {
                yield return 0;
            }


            exp.gazeController.EnableCalibrationUI(false);
#endif

            //show instructions for exploring, wait for the action button
            //yield return StartCoroutine (exp.instructionsController.PlayStartInstructions());

            //learning phase/session instructions
            //			if(isLearningSession){
            //				yield return StartCoroutine(exp.instructionsController.PlayLearningInstructions());
            //			}
            //


            //show video instructions
            Debug.Log("ABOUT TO SHOW VIDEO INSTRUCTIONS");
#if HOSPITAL
			if(isLearningSession)
				yield return StartCoroutine(exp.instructionsController.PlayVideoInstructions(true));
			else
				yield return StartCoroutine(exp.instructionsController.PlayVideoInstructions(false));
#else

            yield return StartCoroutine(exp.instructionsController.PlayVideoInstructions(false));
#endif

            //do mic test here
			exp.micTest.gameObject.SetActive (true);
            yield return StartCoroutine(exp.micTest.RunMicTest());
			exp.micTest.gameObject.SetActive (false);

            //enable pause instruction again
            pauseText.enabled=true;
			if(isLearningSession){
				//STORE PRESENTATION PHASE
				if(Config.doPresentationPhase){
                    Debug.Log("doing store presentation now");
					yield return StartCoroutine(DoStorePresentationPhase());
				}

                Debug.Log("finished store presentation now");
                //should play SHORT INSTRUCTION VIDEO HERE
                exp.eventLogger.LogSessionStarted(Experiment.sessionID, true);
				exp.player.playerCam.enabled = true;
                //LEARNING

                Debug.Log("starting learning session now");
                yield return StartCoroutine(DoLearningPhase(Config.numLearningIterationsSession));

#if !HOSPITAL
                //for scalp we will have 2 delivery days in the first session
                yield return StartCoroutine(DoDeliveryDays(Config.numFirstSessionDelivDays));
#endif
            }
			else { //if not in the learning session! ==> in delivery session

				//LEARNING
				if(Config.doLearningPhase){
                    if (Config.doPresentationPhase)
                    {
                        Debug.Log("doing store presentation now");
                        yield return StartCoroutine(DoStorePresentationPhase());
                    }

                    Debug.Log("finished store presentation now");
                    //do one familiarization trial
                    	yield return StartCoroutine(DoLearningPhase(Config.numLearningIterationsPhase));
                }
				exp.eventLogger.LogSessionStarted(Experiment.sessionID, false);
#if HOSPITAL
                yield return StartCoroutine(DoDeliveryDays(1000)); //number of delivery days doesn't matter as it is constrained by Config.numDelivTime
#else
                //do six delivery days for scalp
                yield return StartCoroutine(DoDeliveryDays(Config.numDelivDays));          
#endif
            }
				
			}

			exp.player.controls.ShouldLockControls = true;

#if GERMAN
			yield return StartCoroutine(exp.instructionsController.ShowSingleInstruction("Sie haben die Sitzung beendet! \nDrücken Sie (X) um fortzufahren.", true, true, false, 0.0f));
#else
			yield return StartCoroutine(exp.instructionsController.ShowSingleInstruction("You have completed the session! \nPress (X) to proceed.", true, true, false, 0.0f));

#endif
		
		
	}

	IEnumerator WaitForEEGHardwareConnection(){
		isConnectingToHardware = true;

		ConnectionUI.alpha = 1.0f;
		if(Config.isSystem2 || Config.isSYS3){
			while(!TCPServer.Instance.isConnected){
				Debug.Log("Waiting for system 2 or 3 connection...");
				yield return 0;
			}
			ConnectionText.text = "Press START on host PC...";
			while (!TCPServer.Instance.canStartGame) {
				Debug.Log ("Waiting for system 2 start command...");
				yield return 0;
			}
		}
		else if (Config.isSyncbox){
			while(!SyncboxControl.Instance.isUSBOpen){
				Debug.Log("Waiting for sync box to open...");
				yield return 0;
			}
		}
		ConnectionUI.alpha = 0.0f;
		isConnectingToHardware = false;
	}

	IEnumerator DoLearningPhase(int numIterations){

		currentState = TrialState.navigationLearning;
		yield return StartCoroutine(exp.instructionsController.PlayPracticeInstructions());

		TCPServer.Instance.SetState (TCP_Config.DefineStates.LEARNING_NAVIGATION_PHASE, true);
		Debug.Log ("NUMBER OF ITERATIONS IS: " + numIterations);
		Debug.Log ("session type is:" + ExperimentSettings.Instance.mySessionType);
//#if GERMAN
//		yield return StartCoroutine(exp.instructionsController.ShowSingleInstruction("Drücken Sie (X), um die Übungsphase zu beginnen.", true, true, false, 0.0f));
//#else
//		//yield return StartCoroutine(exp.instructionsController.ShowSingleInstruction("Press (X) to begin the practice session.", true, true, false, 0.0f));
//#endif

		learningPhaseTimer.StartTimer ();
		bool isRunning = learningPhaseTimer.IsRunning;
		exp.eventLogger.LogLearningPhaseStarted (true);

		for (int currNumIterations = 0; currNumIterations < numIterations; currNumIterations++) {

#if HOSPITAL
            //if we're past the third phase (or we're in deliv. session), continue to the next round of buildings.
            if (ExperimentSettings.Instance.mySessionType == ExperimentSettings.SessionType.deliverySession || (currNumIterations < Config.numLearningIterationsSession && learningPhaseTimer.GetSecondsInt()<(Config.maxLearningTimeMinutes*60)) ){
#else
                if (ExperimentSettings.Instance.mySessionType == ExperimentSettings.SessionType.deliverySession || (currNumIterations < Config.numLearningIterationsSession)){
#endif
                //LearningSessionProgressText.text = "Learning Round " + (currNumIterations + 1) + "/" + numIterations;

                Debug.Log ("CURRENT LEARNING ITERATION: " + currNumIterations);
				//log learning iteration
				exp.eventLogger.LogLearningIteration (currNumIterations);

				List<Store> storeLearningOrder = exp.storeController.GetLearningOrderStores ();

				for (int i = 0; i < storeLearningOrder.Count; i++) {
					Debug.Log(learningPhaseTimer.GetSecondsInt());
					yield return StartCoroutine (DoVisitStoreCommand (storeLearningOrder [i], true, -1));
				}

				yield return 0;

			}
		}

		learningPhaseTimer.ResetTimer ();

		LearningSessionProgressText.text = " ";

		exp.eventLogger.LogLearningPhaseStarted (false);
		TCPServer.Instance.SetState (TCP_Config.DefineStates.LEARNING_NAVIGATION_PHASE, false);
	}

	IEnumerator DoStorePresentationPhase(){
		currentState = TrialState.presentationLearning;
		TCPServer.Instance.SetState (TCP_Config.DefineStates.LEARNING_PRESENTATION_PHASE, true);

        //calibration instructions

        /*#if EYETRACKER
yield return StartCoroutine(exp.instructionsController.PlayCalibrationInstructions());
#endif
                */
        exp.eventLogger.LogPresentationPhase (true);

		yield return StartCoroutine(exp.instructionsController.PlayPresentationInstructions());

		presentationBackgroundCube.TurnVisible (true);
		exp.mainCanvas.GetComponent<CanvasGroup> ().alpha = 0.0f;
        List<int> presentationOrder = new List<int>();
        for (int i=0;i<exp.storeController.stores.Length;i++)
        {
            presentationOrder.Add(i);
        }
		//exp.instructionsController.SetInstructionsColorful ();
		//for each store, move it to the rotation location, rotate it for x seconds, return it to its original location
		for (int i = 0; i < exp.storeController.stores.Length; i++) {
            int currentIndex = presentationOrder[Random.Range(0, presentationOrder.Count - 1)];
			Store currStore = exp.storeController.stores[currentIndex];
            presentationOrder.Remove(currentIndex);

			exp.eventLogger.LogStoreLearningPresentation(currStore, true);

			//move store
			currStore.PresentSelf(storePresentationTransform);
			//GameObject storeImage = TurnOnStoreImage(currStore.name);

			//presentation timing
			yield return new WaitForSeconds(Config.storePresentationTime);

			//put store back
			currStore.ResetStore();
			//if(storeImage != null){
			//	storeImage.GetComponent<VisibilityToggler>().TurnVisible(false);
			//}

			//jitter blank screen
			yield return StartCoroutine(UsefulFunctions.WaitForJitter(Config.betweenStoreBlankScreenTimeMin, Config.betweenStoreBlankScreenTimeMax));


			/*//rotate store
			float currTime = 0.0f;

			float totalDegreesToRotate = Config.numStoreRotations * 360.0f;

			float degPerSecond = totalDegreesToRotate / Config.storeRotateTime;

			while( currTime < Config.storeRotateTime ){
				yield return 0;
				float degToRotate = degPerSecond * Time.deltaTime;
				currStore.transform.RotateAround(currStore.transform.position, Vector3.up, degToRotate);
				currTime += Time.deltaTime;
			}

			//put store back
			currStore.ResetStore();
			*/
			exp.eventLogger.LogStoreLearningPresentation(currStore, false);
		}
		exp.mainCanvas.GetComponent<CanvasGroup> ().alpha = 1.0f;
		presentationBackgroundCube.TurnVisible (false);

		exp.eventLogger.LogPresentationPhase (false);
		TCPServer.Instance.SetState (TCP_Config.DefineStates.LEARNING_PRESENTATION_PHASE, false);
	}

	Store lastStore;	
	IEnumerator DoVisitStoreCommand(Store storeToVisit, bool isLearning, int numDeliveryToday){ //if it's not learning, it's a delivery!
		lastStore = storeToVisit;

		exp.eventLogger.LogStoreStarted (storeToVisit, isLearning, true, numDeliveryToday);
		SetServerStoreTarget(numDeliveryToday, true);

		exp.player.controls.ShouldLockControls = true;

		//show initial delivery command instruction
		InitialDeliveryInstructionGroup.alpha = 1.0f;
#if GERMAN
		InitialDeliveryInstructionText.text = "Bitte finden Sie " + storeToVisit.FullGermanName + ".\nDrücken Sie (X) um fortzufahren.";
#else
		InitialDeliveryInstructionText.text = "Please find the " + storeToVisit.GetDisplayName() + ".\nPress (X) to continue.";
#endif
		yield return StartCoroutine (UsefulFunctions.WaitForActionButton ());
		InitialDeliveryInstructionGroup.alpha = 0.0f;

		exp.player.controls.ShouldLockControls = false;

		//show instruction at top of screen, don't wait for button, wait for collision
#if GERMAN
		DeliveryInstructionText.text = "Bitte finden Sie " + storeToVisit.FullGermanName + ".";
#else
		DeliveryInstructionText.text = "Please find the " + storeToVisit.GetDisplayName() + ".";
#endif
		yield return StartCoroutine (exp.player.WaitForStoreTrigger (storeToVisit));
		DeliveryInstructionText.text = " ";

		exp.eventLogger.LogStoreStarted (storeToVisit, isLearning, false, numDeliveryToday);
		SetServerStoreTarget(numDeliveryToday, false);
	}
	
	IEnumerator DoStoreDeliveryPhase(int deliveryDay){

		currentState = TrialState.delivery;

		exp.player.controls.GoToStartPosition ();

		exp.eventLogger.LogDeliveryDay (deliveryDay, true);
		TCPServer.Instance.SetState (TCP_Config.DefineStates.DELIVERY_NAVIGATION, true);

		List<Store> deliveryStores = exp.storeController.GetRandomDeliveryStores();

		//if the first delivery store in this list is the same as the last one in the last list...
			//swap it with another one so that they're not consecutive!
		/*if (deliveryStores [0] == lastStore) {
			int randomSwapIndex = Random.Range(1, deliveryStores.Count);
			Store temp = deliveryStores[0];
			deliveryStores[0] = deliveryStores[randomSwapIndex];
			deliveryStores[randomSwapIndex] = temp;
		}*/

		//reset the trigger object to null so that IF the last store delivered to is the first store of the new day...
			//...we won't automatically deliver to the store without actually going there!
		exp.player.ResetStoreTriggerObject ();


		for (int numDelivery = 0; numDelivery < deliveryStores.Count; numDelivery++) {

			//visit store
			yield return StartCoroutine(DoVisitStoreCommand(deliveryStores[numDelivery], false, numDelivery));

			//if not the last delivery, deliver an item.
			if(numDelivery < deliveryStores.Count - 1){
				exp.player.controls.ShouldLockControls = true;

				if(Config.isAudioDelivery){
					yield return StartCoroutine(DeliverItemAudio(numDelivery));
				}
				else{
					yield return StartCoroutine(DeliverItemVisible(deliveryStores [numDelivery], numDelivery));
				}

				exp.player.controls.ShouldLockControls = false;

			}
		}

		exp.eventLogger.LogDeliveryDay (deliveryDay, false);
		TCPServer.Instance.SetState (TCP_Config.DefineStates.DELIVERY_NAVIGATION, false);
	}

	IEnumerator DeliverItemVisible(Store toStore, int numDelivery){
		//show delivered item
		GameObject itemDelivered = exp.objectController.SpawnDeliverable(Vector3.zero);
		string itemDisplayText = exp.objectController.GetDeliverableText(itemDelivered);

		string itemName = itemDelivered.GetComponent<SpawnableObject> ().GetName ();
		exp.eventLogger.LogItemDelivery(itemName, toStore, numDelivery, false, true);
		SetServerItemDelivered(numDelivery, true);

		yield return StartCoroutine (exp.instructionsController.ShowSingleInstruction ("You delivered " + itemDisplayText + " to the " + toStore.GetDisplayName(), true, false, false, Config.deliveryCompleteInstructionsTime));

		exp.eventLogger.LogItemDelivery(itemName, toStore, numDelivery, false, false);
		SetServerItemDelivered(numDelivery, false);

		Destroy(itemDelivered);

		orderedStores.Add(toStore);
		orderedItemsDelivered.Add(itemName);
	}

	IEnumerator DeliverItemAudio(int numDelivery){
		GameObject playerCollisionObject = exp.player.GetStoreTriggerObject ();

		Debug.Log ("Deliver item audio to: " + playerCollisionObject.name);

		if (playerCollisionObject != null) {
		
			//play store audio! as long as it's not the last store.
			if(playerCollisionObject.tag == "StoreTrigger"){
				if(playerCollisionObject.transform.parent.tag == "Store"){
					Store collisionStore = playerCollisionObject.transform.parent.GetComponent<Store>();

					//TRIAL LOGGER LOGS THIS IN PLAYDELIVERYAUDIO() COROUTINE
					SetServerItemDelivered(numDelivery, true);
					yield return StartCoroutine(collisionStore.PlayDeliveryAudio(numDelivery));
					SetServerItemDelivered(numDelivery, false);

					string item = collisionStore.GetComponent<AudioSource>().clip.name;

					orderedStores.Add(collisionStore);
					orderedItemsDelivered.Add(item);
				}
			}
		
		}

		yield return 0;
	}

	IEnumerator StartRecall(){
		Debug.Log ("START BEEP");
		recallStartBeep.Play ();
		while (recallStartBeep.isPlaying) {
			yield return 0;
		}
	}
	
	IEnumerator EndRecall(float minEndTime){
		Debug.Log ("END BEEP");
		float currTime = 0.0f;
		recallEndBeep.Play ();
		while(recallEndBeep.isPlaying){ //wait for the beep to finish first
			yield return 0;
			currTime += Time.deltaTime;
		}

		if (currTime < minEndTime) { //wait for the rest of the end time if necessary
			yield return new WaitForSeconds(minEndTime - currTime);
		}
	}

	IEnumerator DoRecallPhase(Config.RecallType recallType, int numDeliveryDay){ //numDelivery is only used in CUED recall

		currentState = TrialState.recall;

		RecallUI.alpha = 1.0f;
		Debug.Log ("doing RECALL " + recallType.ToString());
		exp.player.controls.ShouldLockControls = true;

		//record audio to a file in the session directory for the duration of the recall period
		string fileName = numDeliveryDay.ToString();

		int recallTime = 0;

		TCP_Config.DefineStates recallState = TCP_Config.DefineStates.RECALL_CUED;

		exp.recallInstructionsController.SetInstructionsColorful ();

		switch(recallType){
			case Config.RecallType.FreeItemRecall:
				recallState = TCP_Config.DefineStates.RECALL_FREE_ITEM;
				recallTime = Config.freeRecallTime;
#if GERMAN
			exp.recallInstructionsController.DisplayText ("Bitte erinnern Sie die in dieser Runde zugestellten Gegenstände"); //TODO: GET BETTER TRANSLATION.
#else
			exp.recallInstructionsController.DisplayText ("PLEASE RECALL OBJECTS FROM THIS DELIVERY DAY");
#endif
				break;
			case Config.RecallType.CuedRecall:
				exp.eventLogger.LogRecallPhaseStarted (recallType, true);
				TCPServer.Instance.SetState(recallState, true);
				fileName += "_";
				yield return StartCoroutine( DoCuedRecall (fileName));
			break;
			case Config.RecallType.FreeThenCued:
			/*recallState = TCP_Config.DefineStates.RECALL_FREE_AND_CUED;
				recallTime = Config.freeRecallTime;
				exp.recallInstructionsController.DisplayText ("Free recall STORES DELIVERED TO");*/
				Debug.Log("SHOULD JUST BE RUNNING FREE, THEN CUED. NOT BOTH AT ONCE.");
				break;
			case Config.RecallType.FinalItemRecall:
				recallState = TCP_Config.DefineStates.FINALRECALL_ITEM;
				recallTime = Config.finalFreeItemRecallTime;
				RecallUI.alpha = 0.0f;
			#if GERMAN
				exp.recallInstructionsController.DisplayText("Bitte erinnern Sie die zugestellten GEGENSTÄNDE aller Runden");
			#else
				exp.recallInstructionsController.DisplayText("Please recall objects from all delivery days");
			#endif
				fileName = "ffr";
				//yield return StartCoroutine(exp.instructionsController.ShowSingleInstruction(InstructionsController.finalItemRecallInstructions, true, true, false, 0.0f));
				RecallUI.alpha = 1.0f;
				//exp.recallInstructionsController.DisplayText ("Speak aloud all items that you remember.");
				break;
			case Config.RecallType.FinalStoreRecall:
				recallState = TCP_Config.DefineStates.FINALRECALL_STORE;
				recallTime = Config.finalStoreRecallTime;
				RecallUI.alpha = 0.0f;
			#if GERMAN
				exp.recallInstructionsController.DisplayText("Bitte erinnern Sie die GESCHÄFTE aller Runden");
			#else
				exp.recallInstructionsController.DisplayText("Please recall stores from all delivery days");
			#endif
				fileName = "sr";
				//yield return StartCoroutine(exp.instructionsController.ShowSingleInstruction(InstructionsController.finalStoreRecallInstructions, true, true, false, 0.0f));
				RecallUI.alpha = 1.0f;
				//exp.recallInstructionsController.DisplayText ("Speak aloud all stores that you remember.");
				break;
		}

		//only record here if free recall! cued recall recording handled within DoCuedRecall()
		if(recallType != Config.RecallType.CuedRecall){

			exp.eventLogger.LogRecallPhaseStarted (recallType, true);
			TCPServer.Instance.SetState(recallState, true);

			yield return StartCoroutine(StartRecall());

			if (ExperimentSettings.isLogging) {
				yield return StartCoroutine (exp.audioRecorder.Record (exp.SessionDirectory + "audio", fileName, recallTime));
			}
			else{
				yield return new WaitForSeconds(recallTime);
			}

			yield return StartCoroutine(EndRecall(0.0f));
		}

		exp.player.controls.ShouldLockControls = false;
		exp.recallInstructionsController.DisplayText ("");
		RecallUI.alpha = 0.0f;

		exp.eventLogger.LogRecallPhaseStarted (recallType, false);
		TCPServer.Instance.SetState(recallState, false);
	}

	IEnumerator DoCuedRecall(string origFileName){
		//go through all item-store pairs, and cue half with the store and half with the item

		//randomize order of indices
		// want to use number of items delivered, because we don't deliver to the last store
		List<int> randomIndexOrder = UsefulFunctions.GetRandomIndexOrder(orderedItemsDelivered.Count);

		string cueName = "";
		string shouldRecallName = "";

		string recordFileName = origFileName;

		//make list of 0's and 1's for store vs. item cue
		List<int> storeOrItemCue = new List<int> ();
		for (int i = 0; i < orderedItemsDelivered.Count; i++) {
			storeOrItemCue.Add (0);
//			if (i%2 == 0){
//				storeOrItemCue.Add(0);
//			}
//			else{
//				storeOrItemCue.Add(1);
//			}
		}

		for(int i = 0; i < randomIndexOrder.Count; i++){
			int index = randomIndexOrder[i];

			GameObject storeImage = null;

			int randomStoreOrItemIndex = Random.Range(0, storeOrItemCue.Count);
			int isStoreOrItem = storeOrItemCue[randomStoreOrItemIndex];
			storeOrItemCue.RemoveAt(randomStoreOrItemIndex);

			presentationBackgroundCube.TurnVisible (true);

			//if divisible by 2, make it store cued
//			if(isStoreOrItem % 2 == 0){
				Debug.Log ("cued recall happened");
				exp.recallInstructionsController.background.color = new Color(0,0,0,0);

				recordFileName = origFileName + i + "i"; //i - items are being recalled! (stores are cues)

				cueName = orderedStores[index].name;
				shouldRecallName = orderedItemsDelivered[index];

				exp.eventLogger.LogCuedRecallPresentation(cueName, shouldRecallName, true, false, true, i);

				yield return StartCoroutine (StartRecall());

				//show image
				//storeImage = TurnOnStoreImage(cueName);
				orderedStores[index].PresentSelf(recallStorePresentationTransform);

				exp.eventLogger.LogCuedRecallPresentation(cueName,shouldRecallName, true, false, false, i);
				SetServerStoreCueState(index, true);
//			}
//			else{	//item cued
//				recordFileName = origFileName + i + "s"; //s - stores are being recalled! (items are cues)
//
//				cueName = orderedItemsDelivered[index];
//				shouldRecallName = orderedStores[index].name;
//
//				exp.eventLogger.LogCuedRecallPresentation(cueName, shouldRecallName, false, true, true, i);
//
//#if GERMAN
//				exp.recallInstructionsController.DisplayText ("Zu welchem Geschäft haben Sie diesen Gegenstand geliefert?");
//#else
//				exp.recallInstructionsController.DisplayText ("Which store did you deliver the spoken object to?");
//#endif
//
//				exp.audioRecorder.recordText.color = Color.red;
//				yield return StartCoroutine (StartRecall());
//				//play audio
//				orderedStores[index].PlayCurrentAudio();
//				while(orderedStores[index].GetIsAudioPlaying()){ //wait for audio to finish playing before proceeding
//					yield return 0;
//				}
//
//				exp.eventLogger.LogCuedRecallPresentation(cueName, shouldRecallName, false, true, false, i);
//				SetServerItemCueState(index, true);
//			}


			float timeBeforeEndBeep = Config.cuedRecallTime - Config.cuedEndBeepTimeBeforeEnd;
			if (ExperimentSettings.isLogging) {
				StartCoroutine (exp.audioRecorder.Record (exp.SessionDirectory + "audio", recordFileName, Config.cuedRecallTime));
				yield return new WaitForSeconds(timeBeforeEndBeep); //need to play end beep before the end of the rec. period
			}
			else{
				yield return new WaitForSeconds(timeBeforeEndBeep);
			}

			yield return StartCoroutine(EndRecall(Config.cuedEndBeepTimeBeforeEnd)); //play end beep, wait for the rest of the recall recording time

			//if we're in store cued mode, reset the store now
			if(isStoreOrItem %2 == 0){
				exp.recallInstructionsController.SetInstructionsColorful ();
				orderedStores[index].ResetStore();
			}

			if(storeImage != null){
				storeImage.GetComponent<VisibilityToggler>().TurnVisible(false);
				SetServerStoreCueState(index, false);
				//Destroy(storeImage);
			}
			else{
				SetServerItemCueState(index, false);
			}
            //wait for one second before going onto next cue
			exp.recallInstructionsController.DisplayText ("");
			exp.audioRecorder.recordText.color = Color.white;
            yield return new WaitForSeconds(Config.timeBetweenCuedRecalls);
		}

		presentationBackgroundCube.TurnVisible (false);

		yield return 0;
	}

	GameObject TurnOnStoreImage(string storeName){
		GameObject storeImage = exp.objectController.GetStoreImage(storeName);
		if(storeImage != null){
			storeImage.GetComponent<VisibilityToggler>().TurnVisible(true);
		}

		return storeImage;
	}

	void SetServerStoreCueState(int numStore, bool isTrue){
		switch (numStore){
		case 0:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_0, isTrue);
			break;
		case 1:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_1, isTrue);
			break;
		case 2:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_2, isTrue);
			break;
		case 3:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_3, isTrue);
			break;
		case 4:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_4, isTrue);
			break;
		case 5:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_5, isTrue);
			break;
		case 6:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_6, isTrue);
			break;
		case 7:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_7, isTrue);
			break;
		case 8:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_8, isTrue);
			break;
		case 9:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_9, isTrue);
			break;
		case 10:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_10, isTrue);
			break;
		case 11:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_CUE_11, isTrue);
			break;
		}

	}

	void SetServerItemCueState(int numItem, bool isTrue){
		switch (numItem){
		case 0:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_0, isTrue);
			break;
		case 1:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_1, isTrue);
			break;
		case 2:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_2, isTrue);
			break;
		case 3:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_3, isTrue);
			break;
		case 4:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_4, isTrue);
			break;
		case 5:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_5, isTrue);
			break;
		case 6:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_6, isTrue);
			break;
		case 7:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_7, isTrue);
			break;
		case 8:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_8, isTrue);
			break;
		case 9:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_9, isTrue);
			break;
		case 10:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_10, isTrue);
			break;
		case 11:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_CUE_11, isTrue);
			break;
		}
	}

	void SetServerStoreTarget(int numStore, bool isTrue){
		switch (numStore){
		case 0:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_0, isTrue);
			break;
		case 1:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_1, isTrue);
			break;
		case 2:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_2, isTrue);
			break;
		case 3:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_3, isTrue);
			break;
		case 4:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_4, isTrue);
			break;
		case 5:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_5, isTrue);
			break;
		case 6:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_6, isTrue);
			break;
		case 7:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_7, isTrue);
			break;
		case 8:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_8, isTrue);
			break;
		case 9:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_9, isTrue);
			break;
		case 10:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_10, isTrue);
			break;
		case 11:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_11, isTrue);
			break;
		case 12:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.STORE_TARGET_12, isTrue);
			break;
		}
	}

	void SetServerItemDelivered(int numItem, bool isTrue){
		switch (numItem){
		case 0:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_0, isTrue);
			break;
		case 1:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_1, isTrue);
			break;
		case 2:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_2, isTrue);
			break;
		case 3:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_3, isTrue);
			break;
		case 4:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_4, isTrue);
			break;
		case 5:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_5, isTrue);
			break;
		case 6:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_6, isTrue);
			break;
		case 7:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_7, isTrue);
			break;
		case 8:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_8, isTrue);
			break;
		case 9:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_9, isTrue);
			break;
		case 10:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_10, isTrue);
			break;
		case 11:
			TCPServer.Instance.SetState (TCP_Config.DefineStates.ITEM_DELIVERY_11, isTrue);
			break;
		}
	}

	void DestroyGameObjectList(List<GameObject> listOfGameObjects){
		int numObjects = listOfGameObjects.Count;
		for (int i = 0; i < numObjects; i++) {
			Destroy (listOfGameObjects [i]);
		}
		listOfGameObjects.Clear ();
	}
	
}
