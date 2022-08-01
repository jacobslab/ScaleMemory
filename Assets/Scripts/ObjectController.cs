﻿using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using UnityEngine;

public class ObjectController : MonoBehaviour
{
	public List<Texture> permanentImageList;
	private List<Texture> sessionImageList;

	private List<Texture> stimuliImageList;

	public List<Texture> encodingList;

    public List<Texture> practiceList;
    public ObjectSpawner objSpawner;
	public GameObject textPrefab;
    public GameObject itemBoxColliderPrefab;
    public GameObject lurePrefab;
    public GameObject treasureChestPrefab;

	public GameObject lureColliderPrefab;

	public GameObject placeholder;


	public Texture currentStimuliImage;

	Experiment exp { get { return Experiment.Instance; } }
	// Start is called before the first frame update
	void Awake()
	{
		permanentImageList = new List<Texture>();

		sessionImageList= new List<Texture>();
		stimuliImageList = new List<Texture>();

	}


	public IEnumerator FillPermanentImageList(Object[] imageObjects)
    {
		UnityEngine.Debug.Log("image objects length " + imageObjects.Length.ToString());
		for (int i = 0; i < imageObjects.Length; i++)
		{
			permanentImageList.Add((Texture)imageObjects[i]);
		}

		CreateSpecialImageList(); //fill the stim image list immediately after

		yield return null;
    }
	public IEnumerator CreateSessionImageList(string[] indicesArr)
	{
		stimuliImageList.Clear(); //clear the list
		for (int i = 0;i<indicesArr.Length; i++)
        {
			string currStr = indicesArr[i];
			int currIndex = -1;
			if(int.TryParse(indicesArr[i], out currIndex))
            {
				if (currIndex <= permanentImageList.Count - 1)
					stimuliImageList.Add(permanentImageList[currIndex]);
				else
					UnityEngine.Debug.Log("exceeded max arr length");
            }
        }

		UnityEngine.Debug.Log("added total " + stimuliImageList.Count.ToString() + " to stim list");
		yield return null;
    }


	void CreateSpecialImageList()
	{
        stimuliImageList.Clear();
        UnityEngine.Debug.Log("filling");
        Object[] prefabs;
#if MRIVERSION
		if(Config_CoinTask.isPractice){
			prefabs = Resources.LoadAll("Prefabs/MRIPracticeObjects");
		}
		else{
			prefabs = Resources.LoadAll("Prefabs/Objects");
		}
#else
		UnityEngine.Debug.Log("Permanent Image List Permanent Image List: " + permanentImageList.Count);
		for (int i=0;i<permanentImageList.Count;i++)
        {
			stimuliImageList.Add(permanentImageList[i]);

		}
		UnityEngine.Debug.Log("Stimuli Image List Stimuli Image List: " + stimuliImageList.Count);
		//#else
		//		prefabs = Resources.LoadAll("Prefabs/Images",typeof(Texture));
#endif

		//#if !UNITY_WEBGL
		//      for (int i = 0; i < prefabs.Length; i++)
		//{
		//	stimuliImageList.Add((Texture)prefabs[i]);
		//}
		//#endif

		UnityEngine.Debug.Log("finished filling");
	}

	public List<Texture> SelectImagesForLures()
    {
		List<Texture> lureImages = new List<Texture>();
		for(int i=0;i<Configuration.luresPerTrial;i++)
        {
			Texture selectedImg = ChooseRandomImage();
			if (selectedImg != null)
				lureImages.Add(ChooseRandomImage());
			else
				UnityEngine.Debug.Log("WARNING: Returned a null texture");
        }
		return lureImages;
    }


	//selects a random image from the BOSS database list and removes it so it isn't repeated again
	Texture ChooseRandomImage()
	{
		if (stimuliImageList.Count == 0)
		{
			Debug.Log("No MORE images pick! Recreating image list.");
			CreateSpecialImageList(); //IN ORDER TO REFILL THE LIST ONCE ALL OBJECTS HAVE BEEN USED
			if (stimuliImageList.Count == 0)
			{
				Debug.Log("No images to pick at all!"); //if there are still no objects in the list, then there weren't any to begin with...
				return null;
			}
		}


		int randomImageIndex = Random.Range(0, stimuliImageList.Count);
		Texture chosenImage = stimuliImageList[randomImageIndex];
        stimuliImageList.RemoveAt(randomImageIndex);

		return chosenImage;
	}

	public List<Texture> ReturnRandomlySelectedImages(int objCount)
	{
		List<Texture> randList = new List<Texture>();

		List<Texture> tempList = new List<Texture>();


		for(int i=0;i< stimuliImageList.Count;i++)
		{
			tempList.Add(stimuliImageList[i]);

		}

		for(int j=0;j<objCount;j++)
		{
			int randIndex = UnityEngine.Random.Range(0, tempList.Count);
			randList.Add(tempList[randIndex]);
			tempList.RemoveAt(randIndex);
		}
		return randList;

	}

    public IEnumerator SelectPracticeItems()
    {
        practiceList = new List<Texture>();


		for (int i = 0; i < Experiment.listLength; i++)
		{
			if (i >= stimuliImageList.Count)
			{
				practiceList.Add(stimuliImageList[stimuliImageList.Count - 1]);
				stimuliImageList.RemoveAt(stimuliImageList.Count - 1);
				UnityEngine.Debug.Log("exceeded! picking from the last in the list now");
			}
			else
			{
				practiceList.Add(stimuliImageList[i]);
				UnityEngine.Debug.Log("adding to encoding list " + stimuliImageList[i].ToString());
				stimuliImageList.RemoveAt(i);
			}
		}
		yield return null;
    }

	public IEnumerator SelectEncodingItems()
	{
		
		encodingList = new List<Texture>();

		UnityEngine.Debug.Log("RunBlock: SelectEncoding: Stimuli Image List: " + stimuliImageList.Count);
		for(int i=0;i < Experiment.listLength; i++)
		{
			if (i >= stimuliImageList.Count)
			{
				encodingList.Add(stimuliImageList[stimuliImageList.Count - 1]);
                stimuliImageList.RemoveAt(stimuliImageList.Count-1);
				UnityEngine.Debug.Log("exceeded! picking from the last in the list now");
			}
			else
			{
				encodingList.Add(stimuliImageList[i]);
				UnityEngine.Debug.Log("adding to encoding list " + stimuliImageList[i].ToString());
				stimuliImageList.RemoveAt(i);
			}
		}


		yield return null;
	}

    public string ReturnStimuliDisplayText()
    {
		string dispText = "";
		//if null, we pick a random image from a relevant list
		if (currentStimuliImage == null)
		{
			if (Experiment.isPractice)
			{
				UnityEngine.Debug.Log("practice list " + practiceList.Count.ToString());
				dispText = practiceList[Random.Range(0, practiceList.Count - 1)].name;
			}
			else
			{
				UnityEngine.Debug.Log("encoding list " + encodingList.Count.ToString());
				dispText = encodingList[Random.Range(0, encodingList.Count - 1)].name;
			}
		}
		else
		{
			dispText = currentStimuliImage.name;
		}
        UnityEngine.Debug.Log("display name of current stimuli: " + dispText);
        return dispText;
    }

    public Texture ReturnStimuliToPresent()
    {

        Texture imgToSpawn;

        UnityEngine.Debug.Log("choosing random image");
        imgToSpawn = ChooseRandomImage();

        currentStimuliImage = imgToSpawn;

        return imgToSpawn;
    }

	//spawn random object at a specified location
	public GameObject SpawnSpecialObject(Vector3 spawnPos)
	{
		Texture imgToSpawn;
        UnityEngine.Debug.Log("choosing random image");
        imgToSpawn = ChooseRandomImage();

		if (imgToSpawn != null)
		{

			GameObject newObject = Instantiate(Experiment.Instance.imagePlanePrefab, spawnPos, Quaternion.identity) as GameObject;

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
		UnityEngine.Debug.Log("adding spawn object");

		exp.spawnedObjects.Add(spawnObj);
		UnityEngine.Debug.Log("spawn objects count " + exp.spawnedObjects.Count.ToString());
		//exp.spawnLocations.Add(spawnPos);
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
