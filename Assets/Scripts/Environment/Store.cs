using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof (AudioSource))]
public class Store : MonoBehaviour {

	Experiment exp { get { return Experiment.Instance; } }

	public Transform StoreCenterTransform { get { return GetStoreCenterTransform(); } }
	Transform storeCenterTransform;

	public bool hasRotationVisuals = false;
	public Vector3 presentationRotation;
	public EnableChildrenLogTrack rotationVisualsParent;
	public EnableChildrenLogTrack regularVisualsParent;
	
	public Transform Signs;
	public string FullGermanName; //with article
	string shortGermanName;
	
	Vector3 origPosition;
	Quaternion origRotation;
	
	public List<AudioClip> audioLeftToUse;
	AudioSource myAudioPlayer;
	AudioClip[] storeAudioClips;
	// Use this for initialization
	void Awake () {
		origPosition = transform.position;
		origRotation = transform.rotation;

		#if GERMAN
		shortGermanName = FullGermanName.Replace("das ", "");
		shortGermanName = shortGermanName.Replace("den ", ""); //next use the short german name, or we'll end up with the full german name most likely...
		shortGermanName = shortGermanName.Replace("die ", "");

		ChangeToGerman();
		#endif

		if (myAudioPlayer == null) {
			myAudioPlayer = GetComponent<AudioSource> ();
		}
	}

	void ChangeToGerman(){
		foreach (Transform sign in Signs){
			TextMesh[] signTexts = sign.GetComponentsInChildren<TextMesh> ();
			for( int i = 0 ; i < signTexts.Length; i++){
				TextMesh currTextMesh = signTexts[i];

				//resize text if bigger than original text & 
				int minResizeLength = 7;
				if( shortGermanName.Length > currTextMesh.text.Length && shortGermanName.Length > minResizeLength){
					int lengthDiff = shortGermanName.Length - currTextMesh.text.Length;
					if(currTextMesh.text.Length < minResizeLength){ //if currText length is too small, don't want to resize as much (EX: "GYM")
						lengthDiff = shortGermanName.Length - minResizeLength;
					}
					currTextMesh.fontSize = currTextMesh.fontSize - (20*lengthDiff);
				}

				currTextMesh.text = shortGermanName.ToUpper();
			}
		}
	}

	Transform GetStoreCenterTransform(){
		if (storeCenterTransform != null) {
			return storeCenterTransform;
		} else {
			foreach (Transform t in transform){
				if(t.tag == "StoreCenter"){
					storeCenterTransform = t;
					return storeCenterTransform;
				}
			}
		}
		return null;
	}

	//called in StoreController for each store, also called to refill audio
	public void InitAudio(){

		if (Config.isStoreCorrelatedDelivery) {
			audioLeftToUse = new List<AudioClip> ();

		#if GERMAN
			string folder = "StoreAudioGerman/" + GetDisplayName(); //just happens to be organized with the full name...
		#else
			string folder = "StoreAudioEnglish/" + GetDisplayName(); //just happens to be organized with the display name...
		#endif
			UnityEngine.Debug.Log ("searching for " + folder);
			storeAudioClips = Resources.LoadAll<AudioClip> (folder);
			UnityEngine.Debug.Log ("init audio " + storeAudioClips.Length.ToString () + " clips in the folder");
			for (int i = 0; i < storeAudioClips.Length; i++) {
				audioLeftToUse.Add (storeAudioClips [i]);
			}
		}
	}
	//used by StoreController for adding unused audio.
	public void CleanOutAudioLeft(List<string> audioNames){
//		int numAudioLeft = audioLeftToUse.Count;
		int audioLeftIndex = 0;
		for (int i = 0; i < storeAudioClips.Length; i++) {
			if (!audioNames.Contains (storeAudioClips [i].name)) {
				UnityEngine.Debug.Log ("removing a clip");
				audioLeftToUse.Remove (storeAudioClips [i]);
			}
//			if(!audioNames.Contains(audioLeftToUse[audioLeftIndex].name)){
//				audioLeftToUse.RemoveAt(audioLeftIndex);
//			}
			else{
				audioLeftIndex++; //only increment the index if we didn't delete the item in that spot
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public bool GetIsAudioPlaying(){
		if (myAudioPlayer.isPlaying) {
			return true;
		} else {
			return false;
		}
	}

	public void PlayCurrentAudio(){
		if (myAudioPlayer.clip != null) {
			myAudioPlayer.Play();
		}
	}
	
	public IEnumerator PlayDeliveryAudio(int deliverySerialPosition){
		Debug.Log ("Should play delivery audio! " + gameObject.name);

		AudioClip audioDeliveryClip;
		if (!Config.isStoreCorrelatedDelivery) {
			audioDeliveryClip = ChooseDeliveryAudio (exp.storeController.allStoreAudioLeftToUse);
		} else {
			audioDeliveryClip = ChooseDeliveryAudio (audioLeftToUse);
		}

		if(audioDeliveryClip != null){
			myAudioPlayer.clip = audioDeliveryClip;
			myAudioPlayer.Play ();
            exp.trialController.lstContents += myAudioPlayer.clip.name + "\n";
			exp.eventLogger.LogItemDelivery(myAudioPlayer.clip.name, this, deliverySerialPosition, true, true); //TODO: move into trial controller if possible.

			while(myAudioPlayer.isPlaying){
				yield return 0;
			}

			exp.eventLogger.LogItemDelivery(myAudioPlayer.clip.name, this, deliverySerialPosition, true, false); //TODO: move into trial controller if possible.
		}
		else{
			Debug.Log("No audio for this store!" + gameObject.name);
			yield return 0;
		}
	}

	AudioClip ChooseDeliveryAudio(List<AudioClip> audioList){

		if (audioList.Count == 0) {
			if(audioList == audioLeftToUse){
				InitAudio ();
				audioList = audioLeftToUse;
			}
			else if (audioList == exp.storeController.allStoreAudioLeftToUse){
				exp.storeController.InitAudio();
				audioList = exp.storeController.allStoreAudioLeftToUse;
			}
		}

		if (audioList.Count != 0) {
			int randomAudioIndex = Random.Range (0, audioList.Count);
			AudioClip audio = audioList [ randomAudioIndex ];
			audioList.RemoveAt(randomAudioIndex);
			return audio;
		}

		return null;
	}

	public void PresentSelf(Transform positionTransform){
		transform.rotation = Quaternion.Euler(presentationRotation);

		if (hasRotationVisuals) {
			UsefulFunctions.EnableChildren(regularVisualsParent.transform, false);
			regularVisualsParent.LogChildrenEnabled(false);

			UsefulFunctions.EnableChildren(rotationVisualsParent.transform, true);
			rotationVisualsParent.LogChildrenEnabled(true);
		}

		transform.position = positionTransform.position;
	}
	
	public void ResetStore(){
		transform.position = origPosition;
		transform.rotation = origRotation;

		if (hasRotationVisuals) {
			UsefulFunctions.EnableChildren (regularVisualsParent.transform, true);
			regularVisualsParent.LogChildrenEnabled (true);
		
			UsefulFunctions.EnableChildren (rotationVisualsParent.transform, false);
			rotationVisualsParent.LogChildrenEnabled (false);
		}
	}

	public string GetDisplayName(){
		string displayName = gameObject.name;
		displayName = displayName.Replace ("_", " ");

		return displayName;
	}

}

