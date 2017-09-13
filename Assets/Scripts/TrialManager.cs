using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;

public class TrialManager : MonoBehaviour {
	public SpawnManager spawnManager;
	public TrainMover trainMover;

	//ui element
	public Text harvestText;
	public Text carInstructionText;

	public int numOfBoxes=3;
	public GameObject topDownPlayer;
	public GameObject perspPlayer;
	// Use this for initialization

	private static TrialManager _instance;

	public static TrialManager Instance
	{
		get
		{
			return _instance;
		}
	}

	void Awake()
	{
		if (_instance != null)
		{
			Debug.Log("Instance already exists!");
			return;
		}
		_instance = this;

	}
	void Start () {
		TurnOffHarvestText ();
		TurnOffCarInstruction ();
		StartCoroutine ("RunTrial");
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ChangeHarvestText(string text)
	{
		harvestText.enabled = true;
		harvestText.text = text;
	}
	public void TurnOffHarvestText()
	{
		harvestText.text = "";
		harvestText.enabled = false;
	}

	public void SetCarInstruction(string text)
	{
		carInstructionText.enabled = true;
		carInstructionText.text = text;
	}

	public void TurnOffCarInstruction()
	{
		carInstructionText.text = "";
		carInstructionText.enabled = false;
	}
	IEnumerator RunTrial()
	{
		SetCarInstruction ("Remember where you pick up farm produce");
		for (int i = 0; i < numOfBoxes; i++) {
			StartCoroutine (trainMover.TrainMove());
			yield return StartCoroutine(spawnManager.SpawnBox ());
			Debug.Log ("spawn boxes");
			trainMover.ResetPlayer ();
			SwitchPerspective ();
//			yield return spawnManager.currentBox.WaitForPlayerCollision ();
		}
		for (int j = 0; j < numOfBoxes; j++) {
			int randInt = Random.Range (0,spawnManager.objSpawned.Count-1);
			GameObject retrievalObject = spawnManager.objSpawned [randInt];
			spawnManager.objSpawned.RemoveAt (randInt);
			string objName = GetName (retrievalObject.name);
			SetCarInstruction ("Press (X) where you think you found " + objName);
			StartCoroutine (trainMover.TrainMove());
			yield return StartCoroutine (UsefulFunctions.WaitForActionButton ());
			MeasureScore (retrievalObject.transform.position,trainMover.transform.position);
			yield return StartCoroutine (trainMover.WaitTillPlayerStopped ());
			trainMover.ResetPlayer ();
			SwitchPerspective ();
		}

		yield return null;
	}
	IEnumerator ActivateTopDown()
	{
		
		SetCarInstruction ("Travel to Farm B");
		yield return null;

	}

	void MeasureScore(Vector3 objPos, Vector3 trainPos)
	{
		float distance = Vector3.Distance (objPos, trainPos);
		Debug.Log ("the distance is: " + distance.ToString ());
		ChangeHarvestText("Distance is: " + distance.ToString());
		
	}

	string GetName(string name)
	{
		name = Regex.Replace( name, "(Clone)", "" );
		name = Regex.Replace( name, "[()]", "" );
		return name;
	}


	void SwitchPerspective()
	{
		if (perspPlayer.activeSelf) {
			perspPlayer.SetActive (false);
			topDownPlayer.SetActive (true);
		} else {
			perspPlayer.SetActive (true);
			topDownPlayer.SetActive (false);
		}
	}



}
