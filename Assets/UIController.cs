using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UIController : MonoBehaviour {


	public Text lapsCompletedText;
	public Text harvestText;
	public Text carInstructionText;
	public Text scoreText;
	public CanvasGroup tornadoWarning;
	public Text tornadoWarningText;

	// Use this for initialization
	void Start () {
		scoreText.text = "";
		TurnOffLapText ();
		TurnOffHarvestText ();
		TurnOffCarInstruction ();
		TurnOffTornadoWarning ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetTornadoWarningText(int laneNum)
	{
		tornadoWarning.alpha = 1f;
		switch (laneNum) {
		case 0:
			tornadoWarningText.text = "Tornado imminent in LEFT lane in " + Configuration.tornadoArrivalTime.ToString () + " seconds";
			break;
		case 1:
			tornadoWarningText.text = "Tornado imminent in CENTER lane in " + Configuration.tornadoArrivalTime.ToString () + " seconds";
			break;
		case 2:
			tornadoWarningText.text = "Tornado imminent in RIGHT lane in " + Configuration.tornadoArrivalTime.ToString () + " seconds";
			break;
		}
	}

	public void TurnOffTornadoWarning()
	{
		tornadoWarning.alpha = 0f;
		tornadoWarningText.text = "";
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

	public void ChangeLapText(string text)
	{
		lapsCompletedText.enabled = true;
		lapsCompletedText.text = text;
	}

	public void TurnOffLapText()
	{
		lapsCompletedText.text = "";
		lapsCompletedText.enabled = false;

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

}
