using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TrialManager : MonoBehaviour {
	public SpawnManager spawnManager;


	//ui element
	public Text harvestText;
	public Text carInstructionText;

	public int numOfBoxes=3;
	public GameObject farmPlayer;
	public GameObject carPlayer;
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
		for (int i = 0; i < numOfBoxes; i++) {
			spawnManager.SpawnBox ();
			yield return spawnManager.currentBox.WaitForPlayerCollision ();
		}
		yield return StartCoroutine("ActivateCarPlayer");
		yield return StartCoroutine (WaitForCarToReachFarm ());

		yield return null;
	}
	IEnumerator ActivateCarPlayer()
	{
		farmPlayer.SetActive (false);
		carPlayer.SetActive (true);
		SetCarInstruction ("Travel to Farm B");
		yield return null;

	}

	IEnumerator WaitForCarToReachFarm()
	{
		yield return null;
	}
}
