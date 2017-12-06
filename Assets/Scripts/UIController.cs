﻿using System.Collections;
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

	//item-presentation related
	public CanvasGroup encodingGroup;
	public CanvasGroup objGroup;
	public Text objNameText;

	//item-retrieval
	public CanvasGroup retrievalQuestionGroup;
	public CanvasGroup retrievalOptionA;
	public CanvasGroup retrievalOptionX;
	public Text retrievalQuestion;
	public Text retrievalObjectNameX;
	public Text retrievalObjectNameA;

	public string temporalQuestionText;
	public string spatialQuestionText;

	//response-related
	public CanvasGroup correctResponseGroup;
	public CanvasGroup wrongResponseGroup;

	private float prevLapTime=0f;
	// Use this for initialization
	void Start () {
		scoreText.text = "";
		UpdateCoinText (0);
		TurnOffLapText ();
		TurnOffHarvestText ();
		EnableEncodingGroup ();
//		TurnOffCarInstruction ();
		TurnOffTornadoWarning ();
		DisableObjectRetrieval ();
		correctResponseGroup.alpha = 0f;
		wrongResponseGroup.alpha = 0f;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void EnableEncodingGroup()
	{
		encodingGroup.alpha = 1f;
	}
	public void DisableEncodingGroup()
	{
		encodingGroup.alpha = 0f;
	}

	public void DisableObjectRetrieval()
	{
		retrievalQuestionGroup.alpha = 0f;
		retrievalOptionA.alpha = 0f;
		retrievalOptionX.alpha = 0f;
	}
	public void EnableObjectPresentation(string objName)
	{
		objGroup.alpha = 1f;
		objNameText.text = objName.ToUpper ();
	}

	public void DisableObjectPresentation()
	{
		objGroup.alpha = 0f;
		objNameText.text = "";
	}

	public IEnumerator ShowCorrectResponse()
	{
		correctResponseGroup.alpha = 1f;
		yield return new WaitForSeconds (2f);
		correctResponseGroup.alpha = 0f;
		DisableObjectRetrieval ();
		EnableEncodingGroup ();
		yield return null;
	}

	public IEnumerator ShowWrongResponse()
	{
		wrongResponseGroup.alpha = 1f;
		yield return new WaitForSeconds (2f);
		wrongResponseGroup.alpha = 0f;
		DisableObjectRetrieval ();
		EnableEncodingGroup ();
		yield return null;
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

	public void SetPunctureWarningText()
	{
		tornadoWarning.alpha = 1f;
		tornadoWarningText.text = "Sharp objects approaching in around " + Configuration.tornadoArrivalTime.ToString () + " seconds";
	}

	public void SetTyreActivationText()
	{
		tornadoWarning.alpha = 1f;
		tornadoWarningText.text = "Activating Steel Tyres!";

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

	public void TurnOffScoreText()
	{
		scoreText.enabled = false;
	}

	public void UpdateCoinText(int coinCount)
	{
		coinText.text=coinCount.ToString();
	}

	public void ChangeLapText(float lapTime)
	{
		lapInfo.alpha = 1f;
		string splitSign = "+";
		float splitTime = 0f;
		if (prevLapTime != 0f) {
			splitTime= lapTime-prevLapTime;
		}
		if (prevLapTime > lapTime)
			splitSign = ""; //no sign as it will be negative on its own
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
