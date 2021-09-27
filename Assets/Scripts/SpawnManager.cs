using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {

	private float minX=120f;
	private float maxX=236f;
	private float fixedZ=373f;
	private float fixedY=3.0f;
	public float waitTime=1.5f;
	// Use this for initialization
	private static SpawnManager _instance;
	public List<GameObject> spawnList;
	public Transform carPlayer;
	public List<GameObject> objSpawned;
	private int listLength=3;
	public static SpawnManager Instance
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
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public IEnumerator SpawnBox()
	{
			int randInt = Random.Range (0, spawnList.Count - 1);
			UnityEngine.Debug.Log ("instantiated an item");
			GameObject box = Instantiate (spawnList [randInt], new Vector3 (Random.Range (minX, maxX), fixedY, fixedZ), Quaternion.identity) as GameObject;
			box.SetActive (false);
			while (Vector3.Distance (box.transform.position, carPlayer.transform.position) > 20f) {
				yield return 0;
			}
			box.SetActive (true);
			yield return new WaitForSeconds (waitTime);
			box.SetActive (false);
			objSpawned.Add (box);
			yield return StartCoroutine (carPlayer.GetComponent<TrainMover>().WaitTillPlayerStopped ());
	}

}

