using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {

	public GameObject cardboardBox;
	private float minX=119f;
	private float maxX=160f;
	private float minZ=211f;
	private float maxZ=277f;
	private float fixedY=1.0f;

	public CardboardBox currentBox;
	// Use this for initialization
	private static SpawnManager _instance;
	public List<GameObject> spawnList;
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

	public void SpawnBox()
	{
		GameObject box = Instantiate (cardboardBox, new Vector3 (Random.Range(minX,maxX), fixedY, Random.Range(minZ,maxZ)), Quaternion.identity) as GameObject;
		currentBox = box.GetComponent<CardboardBox>();
	}

}

