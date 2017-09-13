﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
public class TimelockManager : MonoBehaviour {

	//ui element
	public Text harvestText;
	public Text carInstructionText;

	//speed
	public float minSpeed=8f;
	public float maxSpeed=20f;
	public float speed=10f;
	public Rigidbody carBody;
	public Transform startTransform;
	public Transform endTransform;

	private float minX=120f;
	private float maxX=236f;
	private float fixedDistance = 0f;

	private float minTime=5f;
	private float maxTime=12f;
	private float fixedTime=0f;

	enum TrialType {
		Distance,
		Time};

	private TrialType trialType;

	//singleton
	private static TimelockManager _instance;
	public static TimelockManager Instance
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

	// Use this for initialization
	void Start () {
		TurnOffHarvestText ();
		TurnOffCarInstruction ();
		StartCoroutine ("RunTrial");
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
	
	// Update is called once per frame
	void Update () {
		
	}

	public void ResetPlayer()
	{
		carBody.transform.position = startTransform.position;
	}

	IEnumerator RunTrial()
	{
		//distance-fixed
		trialType=TrialType.Distance;
		speed=ChooseRandomSpeed();
		SetCarInstruction ("Watch carefully at what distance the gear is changed");
		fixedDistance = ChooseFixedDistance ();
		while (carBody.transform.position.x<fixedDistance) {
			
			carBody.velocity=Vector3.right * speed;
			yield return 0;
		}
		TurnOffCarInstruction ();
		ChangeHarvestText("TURBO ACTIVATED");
		StartCoroutine(PlayTurboAnim ());
		speed += 10f;
		while (Vector3.Distance(carBody.transform.position,endTransform.position)>7f) {
			carBody.velocity=Vector3.right * speed;
			yield return 0;
		}

		TurnOffHarvestText();
		yield return new WaitForSeconds (1.5f);
		ResetPlayer ();

		//retrieval
		speed=ChooseRandomSpeed();
		SetCarInstruction ("Press (X) where you think the gear was changed");
		while ((Input.GetAxis ("Action Button") ==0f) && (Vector3.Distance(carBody.transform.position,endTransform.position)>7f)) {
			carBody.velocity=Vector3.right * speed;
			yield return 0;
		}
		MeasureScore (carBody.transform.position.x, fixedDistance,trialType);
		TurnOffCarInstruction ();
		ChangeHarvestText("TURBO ACTIVATED");
		StartCoroutine(PlayTurboAnim ());
		speed += 10f;
		while (Vector3.Distance(carBody.transform.position,endTransform.position)>7f) {
			carBody.velocity=Vector3.right * speed;
			yield return 0;
		}
		TurnOffHarvestText();
		yield return new WaitForSeconds (1.5f);
		ResetPlayer ();


		//time-fixed

		trialType=TrialType.Time;
		speed=ChooseRandomSpeed();
		SetCarInstruction ("Watch carefully at what time the gear is changed");
		fixedTime = ChooseFixedTime();
		float timer = 0f;
		while (timer<fixedTime) {
			timer += Time.deltaTime;
			carBody.velocity=Vector3.right * speed;
			yield return 0;
		}
		TurnOffCarInstruction ();
		ChangeHarvestText("TURBO ACTIVATED");
		StartCoroutine(PlayTurboAnim ());
		speed += 10f;
		while (Vector3.Distance(carBody.transform.position,endTransform.position)>7f) {
			carBody.velocity=Vector3.right * speed;
			yield return 0;
		}
		TurnOffHarvestText();
		yield return new WaitForSeconds (1.5f);
		ResetPlayer ();

		//retrieval
		speed=ChooseRandomSpeed();
		SetCarInstruction ("Press (X) when you think the gear was changed");
		while ((Input.GetAxis ("Action Button") ==0f) && (Vector3.Distance(carBody.transform.position,endTransform.position)>7f)) {
			timer += Time.deltaTime;
			carBody.velocity=Vector3.right * speed;
			yield return 0;
		}
		MeasureScore (timer, fixedTime,trialType);
		TurnOffCarInstruction ();
		ChangeHarvestText("TURBO ACTIVATED");
		StartCoroutine(PlayTurboAnim ());
		speed += 10f;
		while (Vector3.Distance(carBody.transform.position,endTransform.position)>7f) {
			carBody.velocity=Vector3.right * speed;
			yield return 0;
		}
		TurnOffHarvestText();
		yield return new WaitForSeconds (1.5f);
		ResetPlayer ();




		yield return null;
	}

	void MeasureScore(float playerVal, float fixedVal, TrialType trialType)
	{
		float score = Mathf.Abs (playerVal - fixedVal);
		UnityEngine.Debug.Log ("the " + trialType.ToString() +" score is: " + score.ToString ());
		
	}

	void MeasureDistanceScore(float scoreDist,float fixedDist)
	{
	}

	IEnumerator PlayTurboAnim()
	{
		float t = 0f;
		while (harvestText.enabled) {
			t += Time.deltaTime;
			harvestText.color = Color.Lerp (Color.red, Color.blue,t);
			if (t >= 1f)
				t = 0f;
			yield return 0;
		}
	}
	float ChooseRandomSpeed()
	{
		return Random.Range (minSpeed, maxSpeed);
	}

	float ChooseFixedDistance()
	{
		return Random.Range (minX, maxX);
	}
	float ChooseFixedTime()
	{
		return Random.Range (minTime, maxTime);
	}
}
