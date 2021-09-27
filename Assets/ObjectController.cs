using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using UnityEngine;

public class ObjectController : MonoBehaviour
{
    private List<Texture> stimuliImageList;

	public List<Texture> encodingList;

    public List<Texture> practiceList;
    public ObjectSpawner objSpawner;
	public GameObject textPrefab;
    public GameObject itemBoxColliderPrefab;
    public GameObject lurePrefab;
    public GameObject treasureChestPrefab;

    public Texture currentStimuliImage;

	Experiment exp { get { return Experiment.Instance; } }
	// Start is called before the first frame update
	void Awake()
	{
        stimuliImageList = new List<Texture>();
		CreateSpecialImageList(stimuliImageList);

	}

	void CreateSpecialImageList(List<Texture> imageListToFill)
	{
        imageListToFill.Clear();
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
		prefabs = Resources.LoadAll("Prefabs/Images",typeof(Texture));
#endif
		for (int i = 0; i < prefabs.Length; i++)
		{
            imageListToFill.Add((Texture)prefabs[i]);
		}

        UnityEngine.Debug.Log("finished filling");
	}

	Texture ChooseRandomImage()
	{
		if (stimuliImageList.Count == 0)
		{
			Debug.Log("No MORE images pick! Recreating image list.");
			CreateSpecialImageList(stimuliImageList); //IN ORDER TO REFILL THE LIST ONCE ALL OBJECTS HAVE BEEN USED
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
        List<int> allInts = new List<int>();
        List<int> randInts = new List<int>();
        for (int i = 0; i < Experiment.listLength*2; i++)
        {
            allInts.Add(i);
        }
        for (int j = 0; j < Experiment.listLength; j++)
        {
            int randomIndex = UnityEngine.Random.Range(0, allInts.Count - 1);
            randInts.Add(allInts[randomIndex]);
            allInts.RemoveAt(randomIndex);
        }

        UnityEngine.Debug.Log("spawnable list count " + stimuliImageList.Count.ToString());

        for (int i = 0; i < randInts.Count; i++)
        {
            if (randInts[i] >= stimuliImageList.Count)
            {
                practiceList.Add(stimuliImageList[stimuliImageList.Count - 1]);
                stimuliImageList.RemoveAt(stimuliImageList.Count - 1);
            }
            else
            {
                practiceList.Add(stimuliImageList[randInts[i]]);
                stimuliImageList.RemoveAt(randInts[i]);
                //	UnityEngine.Debug.Log("added " + spawnableObjectList[randInts[i]].name + " to encoding list");
            }
        }
        yield return null;
    }

	public IEnumerator SelectEncodingItems()
	{
		encodingList = new List<Texture>();
		List<int> allInts = new List<int>();
		List<int> randInts = new List<int>();
		for (int i=0;i< stimuliImageList.Count;i++)
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
			if (randInts[i] >= stimuliImageList.Count)
			{
				encodingList.Add(stimuliImageList[stimuliImageList.Count - 1]);
                stimuliImageList.RemoveAt(stimuliImageList.Count-1);
			}
			else
			{
				encodingList.Add(stimuliImageList[randInts[i]]);
                stimuliImageList.RemoveAt(randInts[i]);
			//	UnityEngine.Debug.Log("added " + spawnableObjectList[randInts[i]].name + " to encoding list");
			}
		}


		yield return null;
	}

    public string ReturnStimuliDisplayText()
    {
        string dispText = currentStimuliImage.name;
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
