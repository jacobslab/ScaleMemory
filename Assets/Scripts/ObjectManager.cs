using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectManager : MonoBehaviour {

	public List<GameObject> spawnables;
	public List<GameObject> spawnSequence;
	// Use this for initialization
	void Awake()
	{
		PopulateSpawnList ();
	}

	void PopulateSpawnList()
	{
		Object[] spawnArr = Resources.LoadAll ("Prefabs/Objects");
		for (int i = 0; i < spawnArr.Length; i++) {
			spawnables.Add ((GameObject)spawnArr [i]);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public IEnumerator SelectSpawnSequence()
	{
		//make sure we clear any previous spawn sequence
		spawnSequence.Clear ();
		int currentSequenceIndex = 0;
		List<GameObject> tempList = spawnables;
		while (currentSequenceIndex < Configuration.sequenceLength) {

			//creating a copy list
			int seqInt = Random.Range (0, tempList.Count - 1);
			GameObject seqObj = tempList [seqInt];
			tempList.RemoveAt (seqInt);
			spawnSequence.Add (seqObj);
			currentSequenceIndex++;
			yield return 0;
		}
	}

	public void SpawnAtLocation(int currentIndex, Vector3 carLocation)
	{
		Instantiate (spawnSequence [currentIndex], carLocation + new Vector3(0.08f,1.93f,1.78f), Quaternion.identity);
	}
}
