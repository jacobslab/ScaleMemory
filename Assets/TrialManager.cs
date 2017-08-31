using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
		SetCarInstruction ("Drive to farm");
		for (int i = 0; i < numOfBoxes; i++) {
			StartCoroutine (trainMover.TrainMove());
			yield return StartCoroutine(spawnManager.SpawnBox ());
			Debug.Log ("spawn boxes");
			trainMover.ResetPlayer ();
			SwitchPerspective ();
//			yield return spawnManager.currentBox.WaitForPlayerCollision ();
		}

		yield return null;
	}
	IEnumerator ActivateTopDown()
	{
		
		SetCarInstruction ("Travel to Farm B");
		yield return null;

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
