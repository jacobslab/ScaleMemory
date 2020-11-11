using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using UnityEngine;

public class ObjectController : MonoBehaviour
{
    private List<GameObject> spawnableObjectList;

	public List<GameObject> encodingList;
	public ObjectSpawner objSpawner;
	public GameObject textPrefab;
	Experiment exp { get { return Experiment.Instance; } }
	// Start is called before the first frame update
	void Awake()
	{
		spawnableObjectList = new List<GameObject>();
		CreateSpecialObjectList(spawnableObjectList);

	}

	void CreateSpecialObjectList(List<GameObject> gameObjectListToFill)
	{
		gameObjectListToFill.Clear();
		Object[] prefabs;
#if MRIVERSION
		if(Config_CoinTask.isPractice){
			prefabs = Resources.LoadAll("Prefabs/MRIPracticeObjects");
		}
		else{
			prefabs = Resources.LoadAll("Prefabs/Objects");
		}
#else
		prefabs = Resources.LoadAll("Prefabs/Objects");
#endif
		for (int i = 0; i < prefabs.Length; i++)
		{
			gameObjectListToFill.Add((GameObject)prefabs[i]);
		}
	}

	GameObject ChooseRandomObject()
	{
		if (spawnableObjectList.Count == 0)
		{
			Debug.Log("No MORE objects to pick! Recreating object list.");
			CreateSpecialObjectList(spawnableObjectList); //IN ORDER TO REFILL THE LIST ONCE ALL OBJECTS HAVE BEEN USED
			if (spawnableObjectList.Count == 0)
			{
				Debug.Log("No objects to pick at all!"); //if there are still no objects in the list, then there weren't any to begin with...
				return null;
			}
		}


		int randomObjectIndex = Random.Range(0, spawnableObjectList.Count);
		GameObject chosenObject = spawnableObjectList[randomObjectIndex];
		spawnableObjectList.RemoveAt(randomObjectIndex);

		return chosenObject;
	}

	public List<GameObject> ReturnRandomlySelectedObjects(int objCount)
	{
		List<GameObject> randList = new List<GameObject>();

		List<GameObject> tempList = new List<GameObject>();


		for(int i=0;i<spawnableObjectList.Count;i++)
		{
			tempList.Add(spawnableObjectList[i]);

		}

		for(int j=0;j<objCount;j++)
		{
			int randIndex = UnityEngine.Random.Range(0, tempList.Count);
			randList.Add(tempList[randIndex]);
			tempList.RemoveAt(randIndex);
		}
		return randList;

	}

	public IEnumerator SelectEncodingItems()
	{
		encodingList = new List<GameObject>();
		List<int> allInts = new List<int>();
		List<int> randInts = new List<int>();
		for (int i=0;i<spawnableObjectList.Count;i++)
		{
			allInts.Add(i);
		}
		for(int j=0;j<Experiment.listLength;j++)
		{
			int randomIndex = UnityEngine.Random.Range(0, allInts.Count-1);
			randInts.Add(allInts[randomIndex]);
			allInts.RemoveAt(randomIndex);
		}

		for(int i=0;i<randInts.Count;i++)
		{
			if (randInts[i] >= spawnableObjectList.Count)
			{
				encodingList.Add(spawnableObjectList[spawnableObjectList.Count - 1]);
				spawnableObjectList.RemoveAt(spawnableObjectList.Count-1);
			}
			else
			{
				encodingList.Add(spawnableObjectList[randInts[i]]);
				spawnableObjectList.RemoveAt(randInts[i]);
				UnityEngine.Debug.Log("added " + spawnableObjectList[randInts[i]].name + " to encoding list");
			}
		}


		yield return null;
	}

	//spawn random object at a specified location
	public GameObject SpawnSpecialObject(Vector3 spawnPos)
	{
		GameObject objToSpawn;
		
		objToSpawn = ChooseRandomObject();

		if (objToSpawn != null)
		{

			GameObject newObject = Instantiate(objToSpawn, spawnPos, objToSpawn.transform.rotation) as GameObject;

			//float randomRot = GenerateRandomRotationY();
			//newObject.transform.RotateAround(newObject.transform.position, Vector3.up, randomRot);

			//CurrentTrialSpecialObjects.Add(newObject);

			//make object face the player -- MUST MAKE SURE OBJECT FACES Z-AXIS
			//don't want object to tilt downward at the player -- use object's current y position
			UsefulFunctions.FaceObject(newObject, exp.player.gameObject, false);

			return newObject;
		}
		else
		{
			return null;
		}
	}

	public IEnumerator InitiateSpawnSequence()
    {
		Vector3 spawnPos = exp.player.transform.position + exp.player.transform.forward * 5f;
		GameObject spawnObj = SpawnSpecialObject(spawnPos);
		exp.spawnedObjects.Add(spawnObj);
		exp.spawnLocations.Add(spawnPos);
		yield return StartCoroutine(objSpawner.MakeObjAppear(spawnObj));
		yield return null;
    }



	// Update is called once per frame
	void Update()
    {
	//	if (Input.GetKeyDown(KeyCode.Q))
	//	{
	//		StartCoroutine("InitiateSpawnSequence");
	//	}
	}
}
