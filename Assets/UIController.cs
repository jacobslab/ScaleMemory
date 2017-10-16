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
	public CanvasGroup lapInfo;
	public Text lapTimeText;


	//coin-related
	public Text coinText;
	public Image coinIcon;
	private Vector3 originalScale = new Vector3 (0.304f, 0.304f, 0.304f);

	private float prevLapTime=0f;
	// Use this for initialization
	void Start () {
		scoreText.text = "";
		UpdateCoinText (0);
		TurnOffLapText ();
		TurnOffHarvestText ();
		TurnOffCarInstruction ();
		TurnOffTornadoWarning ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public IEnumerator PulseCoinImage()
	{
		float timer = 0f;
		while (timer < 0.5f) {
			timer += Time.deltaTime;
			coinIcon.transform.localScale = Vector3.Lerp (coinIcon.transform.localScale, originalScale * 2, timer);
			yield return 0;
		}
		while (timer > 0.5f) {
			timer -= Time.deltaTime;
			coinIcon.transform.localScale = Vector3.Lerp (coinIcon.transform.localScale, originalScale, timer);
			yield return 0;
		}
		yield return null;
	}

	public void SetTornadoWarningText(int laneNum)
	{
		tornadoWarning.alpha = 1f;
		switch (laneNum) {
		case 0:
			tornadoWarningText.text = "Imminent in LEFT lane in " + Configuration.tornadoArrivalTime.ToString () + " seconds";
			break;
		case 1:
			tornadoWarningText.text = "Imminent in CENTER lane in " + Configuration.tornadoArrivalTime.ToString () + " seconds";
			break;
		case 2:
			tornadoWarningText.text = "Imminent in RIGHT lane in " + Configuration.tornadoArrivalTime.ToString () + " seconds";
			break;
		}
	}

	public void SetTornadoArrivalText()
	{
		tornadoWarning.alpha = 1f;
		tornadoWarningText.text = "Turning off the car temporarily!";

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

	public void UpdateCoinText(int coinCount)
	{
		coinText.text=coinCount.ToString();
	}

	public void ChangeLapText(float lapTime)
	{
		string splitSign = "+";
		float splitTime = 0f;
		if (prevLapTime != 0f) {
			splitTime= lapTime-prevLapTime;
		}
		if (prevLapTime > lapTime)
			splitSign = "-";
		else
			splitSign = "+";
		prevLapTime = lapTime;
		lapTimeText.text = "Lap Time: \n" + lapTime.ToString ("F2") + "( " + splitSign + splitTime.ToString ("F2") + ")";
		lapInfo.alpha = 1f;
		lapsCompletedText.text = "Laps Completed: \n" + ChequeredFlag.lapsCompleted.ToString () + " / " + Configuration.lapsToBeCompleted.ToString ();
	}

	public void TurnOffLapText()
	{
		lapInfo.alpha = 0f;
		lapTimeText.text = "";
		lapsCompletedText.text = "";

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
