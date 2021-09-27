using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;

public class CardboardBox : MonoBehaviour {
	TrialManager exp { get { return TrialManager.Instance; } }
	public Transform spawnTransform;
	private bool playerCollided=false;
	private Transform objTransform;
	private GameObject spawnedObj;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public IEnumerator WaitForPlayerCollision()
	{
		while(!playerCollided)
		{
			yield return 0;
		}
		yield return new WaitForSeconds (2f);
		TrialManager.Instance.TurnOffHarvestText ();
//		Destroy (spawnedObj);
		gameObject.SetActive(false);
		yield return null;
	}


	void OnCollisionEnter(Collision col)
	{
//		Debug.Log (col.gameObject.name);
		if (col.gameObject.tag == "Player" && !playerCollided) {
			playerCollided = true;
//			Debug.Log ("collided");
			int randNum = Random.Range (0, SpawnManager.Instance.spawnList.Count - 1);
			spawnedObj = Instantiate (SpawnManager.Instance.spawnList [randNum], spawnTransform.position, Quaternion.identity) as GameObject;
			spawnedObj.transform.parent = transform;
			objTransform = spawnedObj.transform;
			Debug.Log ("removing " + SpawnManager.Instance.spawnList [randNum].name);
			SpawnManager.Instance.spawnList.RemoveAt (randNum);
			col.transform.LookAt (objTransform);
			string objName = Regex.Replace (spawnedObj.name, "(Clone)", "");
			objName = Regex.Replace (objName, "[()]", "");
			string harvestText = "You harvested " + objName;
			exp.ChangeHarvestText (harvestText);

		}
		
	}
}
