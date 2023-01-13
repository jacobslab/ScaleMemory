﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
public class ObjectManager : MonoBehaviour {

	Experiment exp { get { return Experiment.Instance; } }
	public List<GameObject> spawnables;
	public List<GameObject> spawnSequence;
	public List<Transform> spawnTransformSequence;
	private List<GameObject> spawnedObjects;
	public Transform landmarkTransform;
	public List<GameObject> retrievalSpawnTransform;


	private GameObject currentSpawnedObj;
	// Use this for initialization
	void Awake()
	{
		PopulateSpawnList ();
		spawnedObjects = new List<GameObject> ();
		StartCoroutine("SelectSpawnSequence");
	}

	public void PopulateSpawnList()
	{
		Object[] spawnArr = Resources.LoadAll ("Prefabs/Objects");

		//clear any leftovers
		spawnables.Clear ();
		for (int i = 0; i < spawnArr.Length; i++) {
			spawnables.Add ((GameObject)spawnArr [i]);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public IEnumerator SelectSpawnSequence()
	{
		spawnTransformSequence= new List<Transform>();
		//make sure we clear any previous spawn sequence
		spawnSequence.Clear ();
		spawnedObjects.Clear ();
		currentSpawnedObj = null;

		//check if spawnables needs to be respawned
		if (spawnables.Count < Configuration.spawnCount) {
			Debug.Log ("populating spawn list");
			PopulateSpawnList ();
		}

		int currentSequenceIndex = 0;
		List<GameObject> tempList = spawnables;
		while (currentSequenceIndex < Configuration.spawnCount) {

			//creating a copy list
			Debug.Log("templist count: " + tempList.Count.ToString());
			int seqInt = Random.Range (0, tempList.Count - 1);
			GameObject seqObj = tempList [seqInt];
			tempList.RemoveAt (seqInt);
			spawnSequence.Add (seqObj);
			spawnTransformSequence.Add (seqObj.transform);
			currentSequenceIndex++;
			yield return 0;
		}
	}

		
	public void EndObjectPresentation()
	{
		Debug.Log (currentSpawnedObj.name);
		spawnedObjects.Add (currentSpawnedObj);
		currentSpawnedObj.SetActive (false);
	}

	public List<GameObject> GetRandomObjects(List<GameObject> spawnedObj, int count)
	{
		List<GameObject> tempList = new List<GameObject> ();
		for (int i = 0; i < spawnedObj.Count; i++) {
			tempList.Add (spawnedObj [i]);
			tempList [i].transform.position = spawnTransformSequence [i].position;
		}

		Debug.Log ("tempList Count: " + tempList.Count.ToString());
		List<GameObject> randList = new List<GameObject> ();
		for (int i = 0; i < count; i++) {
			int randInt = Random.Range (0, tempList.Count-1);
			Debug.Log ("rand int is: " + randInt.ToString ());
			randList.Add (tempList [randInt]);
			Debug.Log ("adding:  " + tempList [randInt].name);
			tempList.RemoveAt (randInt);
		}
		Debug.Log ("temp list count: " + tempList.Count.ToString ());
		return randList;
	}
	/*
	public IEnumerator BeginObjectRetrieval()
	{
		for (int i = 0; i < 2; i++) {
			string name = spawnedObjects[i].name;
			name = Regex.Replace (name, "(Clone)", "");
			name = Regex.Replace (name, "[()]", "");
			spawnedObjects [i].name = name;

		}

		List<GameObject> randomList = new List<GameObject> ();
		randomList = GetRandomObjects (spawnedObjects, 2);
		Debug.Log ("random list count : " + randomList.Count.ToString ());
		//temporal retrieval
		int correctAnswer = -1;
		if (Random.value < 0.5f) {
			exp.uiController.DisableEncodingGroup ();
			exp.uiController.retrievalQuestion.text = exp.uiController.temporalQuestionText;
			exp.uiController.retrievalQuestionGroup.alpha = 1f;
			yield return new WaitForSeconds (2f);
			exp.uiController.retrievalObjectNameA.text = randomList [1].name;
			randomList [1].transform.position = retrievalSpawnTransform [1].transform.position;
			randomList [1].SetActive (true);
			exp.uiController.retrievalOptionA.alpha = 1f;
			yield return new WaitForSeconds (1f);
			exp.uiController.retrievalObjectNameX.text = randomList [0].name;
			randomList [0].transform.position = retrievalSpawnTransform [0].transform.position;
			randomList [0].SetActive (true);
			exp.uiController.retrievalOptionX.alpha = 1f;

			correctAnswer = exp.raceManager.ReturnFirstObject (spawnedObjects,randomList [0], randomList [1]);
		}
		else {
			correctAnswer = 1;
			exp.uiController.DisableEncodingGroup ();
			exp.uiController.retrievalQuestion.text = exp.uiController.temporalQuestionText;
			exp.uiController.retrievalQuestionGroup.alpha = 1f;
			yield return new WaitForSeconds (2f);
			exp.uiController.retrievalObjectNameA.text = randomList [0].name;
			randomList [0].transform.position = retrievalSpawnTransform [1].transform.position;
			randomList [0].SetActive (true);
			exp.uiController.retrievalOptionA.alpha = 1f;
			yield return new WaitForSeconds (1f);
			exp.uiController.retrievalObjectNameX.text = randomList [1].name;
			randomList [1].transform.position = retrievalSpawnTransform [0].transform.position;
			randomList [1].SetActive (true);
			exp.uiController.retrievalOptionX.alpha = 1f;

			correctAnswer =  exp.raceManager.ReturnFirstObject (spawnedObjects,randomList [1], randomList [0]);
			
		}
		UnityEngine.Debug.Log ("waiting for key response");
		while (!Input.GetKeyDown (KeyCode.X) && !Input.GetKeyDown (KeyCode.A)) {
			yield return 0;
		}
		UnityEngine.Debug.Log ("key pressed");
		int chosenAnswer = -1;
		if(Input.GetKey(KeyCode.X))
		{
			UnityEngine.Debug.Log ("X was pressed");
			chosenAnswer = 0;
		}
		if(Input.GetKey(KeyCode.A))
		{
			UnityEngine.Debug.Log ("A was pressed");
			chosenAnswer = 1;
		}

		UnityEngine.Debug.Log ("showing appropriate response");
		if (chosenAnswer == correctAnswer)
			yield return StartCoroutine(exp.uiController.ShowCorrectResponse ());
		else
			yield return StartCoroutine(exp.uiController.ShowWrongResponse ());


		//have one second of blackout
		randomList[0].SetActive(false);
		randomList [1].SetActive (false);
		exp.uiController.encodingGroup.alpha = 0f;
		exp.uiController.retrievalQuestionGroup.alpha = 0f;
		exp.uiController.retrievalOptionX.alpha = 0f;
		exp.uiController.retrievalOptionA.alpha = 0f;
		yield return new WaitForSeconds (1f);

		randomList.Clear ();


		//create a separate randomize list for spatial retrieval
		randomList = GetRandomObjects (spawnedObjects, 2);

		Transform[] randTransform = new Transform[2];
		randTransform [0] = randomList [0].transform;
		randTransform [1] = randomList [1].transform;

		//make duplicates
		List<GameObject> duplicateObjList=new List<GameObject>();
		for (int i = 0; i < 2; i++) {
			GameObject duplicate = Instantiate (randomList [i], randTransform [i].position, Quaternion.identity) as GameObject;
			duplicateObjList.Add (duplicate);
		}

		//then do spatial retrieval
		correctAnswer = -1;
		if (Random.value < 0.5f) {
			exp.uiController.DisableEncodingGroup ();
			exp.uiController.retrievalQuestion.text = exp.uiController.spatialQuestionText;
			exp.uiController.retrievalQuestionGroup.alpha = 1f;
			yield return new WaitForSeconds (2f);

			exp.uiController.retrievalObjectNameA.text = randomList [1].name;
			randomList [1].transform.position = retrievalSpawnTransform [1].transform.position;
			randomList [1].SetActive (true);
			exp.uiController.retrievalOptionA.alpha = 1f;
			yield return new WaitForSeconds (1f);
			exp.uiController.retrievalObjectNameX.text = randomList [0].name;
			randomList [0].transform.position = retrievalSpawnTransform [0].transform.position;
			randomList [0].SetActive (true);
			exp.uiController.retrievalOptionX.alpha = 1f;
			Debug.Log ("transforms A : " + duplicateObjList [0].transform.position + " and  B: " + duplicateObjList [1].transform.position);
			correctAnswer = exp.raceManager.ReturnClosestObject(landmarkTransform,duplicateObjList[0].transform,duplicateObjList[1].transform);
			correctAnswer = correctAnswer == 1 ? 0 : 1;
			Debug.Log ("correct answer FLIPPED is: " + correctAnswer.ToString ());
		}
		else {
			exp.uiController.DisableEncodingGroup ();
			exp.uiController.retrievalQuestion.text = exp.uiController.spatialQuestionText;
			exp.uiController.retrievalQuestionGroup.alpha = 1f;
			yield return new WaitForSeconds (2f);
			exp.uiController.retrievalObjectNameA.text = randomList [0].name;
			randomList [0].transform.position = retrievalSpawnTransform [1].transform.position;
			randomList [0].SetActive (true);
			exp.uiController.retrievalOptionA.alpha = 1f;
			yield return new WaitForSeconds (1f);
			exp.uiController.retrievalObjectNameX.text = randomList [1].name;
			randomList [1].transform.position = retrievalSpawnTransform [0].transform.position;
			randomList [1].SetActive (true);
			exp.uiController.retrievalOptionX.alpha = 1f;
			Debug.Log ("transforms A : " + duplicateObjList [0].transform.position + " and  B: " + duplicateObjList [1].transform.position);

			correctAnswer = exp.raceManager.ReturnClosestObject(landmarkTransform,duplicateObjList[0].transform,duplicateObjList[1].transform);
			Debug.Log ("correct answer is: " + correctAnswer.ToString ());
		}
		UnityEngine.Debug.Log ("waiting for key response");
		while (!Input.GetKeyDown (KeyCode.X) && !Input.GetKeyDown (KeyCode.A)) {
			yield return 0;
		}
		UnityEngine.Debug.Log ("key pressed");
		chosenAnswer = -1;
		//left side object was chosen
		if(Input.GetKey(KeyCode.X))
		{
			Debug.Log ("left side object was chosen");
			UnityEngine.Debug.Log ("X was pressed");
			chosenAnswer = 0;
		}
		if(Input.GetKey(KeyCode.A))
		{

			Debug.Log ("right side object was chosen");
			UnityEngine.Debug.Log ("A was pressed");
			chosenAnswer = 1;
		}

		UnityEngine.Debug.Log ("showing appropriate response");
		if (chosenAnswer == correctAnswer)
			yield return StartCoroutine(exp.uiController.ShowCorrectResponse ());
		else
			yield return StartCoroutine(exp.uiController.ShowWrongResponse ());
		
		randomList.Clear ();
		for (int i = 0; i < spawnedObjects.Count; i++) {
			Destroy (spawnedObjects [i]);
		}

		exp.uiController.encodingGroup.alpha = 1f;
		yield return null;
	}
	*/
}
