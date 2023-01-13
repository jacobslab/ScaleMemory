using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoinSpawner : MonoBehaviour {

	public Transform coinParent;
	public GameObject coinObj;
	public List<Transform> waypointParents; 
	// Use this for initialization
	void Start () {
		
	}

	public void SpawnCoins()
	{
		for (int i = 0; i < 40; i++) {
			
			//0=left , 1=center and 2=right
			int randLane = Random.Range (0, 3);
			Vector3 chosenPos=waypointParents[randLane].GetChild(i).transform.position;
			GameObject coins= Instantiate(coinObj,new Vector3(chosenPos.x,6.2f,chosenPos.z),Quaternion.Euler(new Vector3(90f,0f,0f))) as GameObject;
			coins.transform.parent = coinParent;
		}
	}

	public void CleanUpCoins()
	{
			foreach(Transform child in coinParent)
			{
			Destroy (child.gameObject);
			}
	}


	// Update is called once per frame
	void Update () {
		
	}
}
