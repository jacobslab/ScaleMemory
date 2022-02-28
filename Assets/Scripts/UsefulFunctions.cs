using UnityEngine;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

public class UsefulFunctions
{

	//given the size of an array or a list, will return a list of indices in a random order
	public static List<int> GetRandomIndexOrder(int count)
	{
		List<int> inOrderList = new List<int>();
		for (int i = 0; i < count; i++)
		{
			inOrderList.Add(i);
		}

		List<int> randomOrderList = new List<int>();
		for (int i = 0; i < count; i++)
		{
			int randomIndex = Random.Range(0, inOrderList.Count);
			randomOrderList.Add(inOrderList[randomIndex]);
			inOrderList.RemoveAt(randomIndex);
		}

		return randomOrderList;
	}


	public static IEnumerator WaitForActionButton()
	{
		while (!Input.GetKeyDown(KeyCode.Space))
		{
			yield return 0;
		}
		yield return null;
	}

	//set layer of gameobject and all its children using the layer ID (int)
	public static void SetLayerRecursively(GameObject obj, int newLayer)
	{
		obj.layer = newLayer;

		foreach (Transform child in obj.transform)
		{
			SetLayerRecursively(child.gameObject, newLayer);
		}
	}

	//set layer of gameobject and all its children using the layer name
	public static void SetLayerRecursively(GameObject obj, string newLayer)
	{
		obj.layer = LayerMask.NameToLayer(newLayer);

		foreach (Transform child in obj.transform)
		{
			SetLayerRecursively(child.gameObject, newLayer);
		}
	}

	public static void EnableChildren(Transform parentTransform, bool shouldEnable)
	{
		//enable all children
		for (int i = 0; i < parentTransform.childCount; ++i)
		{
			parentTransform.GetChild(i).gameObject.SetActive(shouldEnable);
		}
	}

	public static int FindIndexOfInt(List<int> intList, int numberToFind)
	{
		int resultIndex = 0;
		for (int i = 0; i < intList.Count; i++)
		{
			if (numberToFind == intList[i])
			{
				resultIndex = i;
				return resultIndex;
			}
		}
		return resultIndex;
	}

	public static List<int> ReturnListOfOrderedInts(int listLength)
	{
		List<int> resultList = new List<int>();

		for (int i = 0; i < listLength; i++)
		{
			resultList.Add(i);
		}

		return resultList;
	}

	public static List<int> ReturnShuffledIntegerList(int listLength)
	{
		List<int> tempList = new List<int>();
		List<int> resultList = new List<int>();
		for (int i = 0; i < listLength; i++)
		{
			tempList.Add(i);
		}

		for (int i = 0; i < listLength; i++)
		{
			int randIndex = Random.Range(0, tempList.Count);
			resultList.Add(tempList[randIndex]);
			tempList.RemoveAt(randIndex);

		}

		return resultList;
	}

	public static List<int> DuplicateList(List<int> originalList)
	{
		List<int> resultList = new List<int>();
		for (int i = 0; i < originalList.Count; i++)
		{
			resultList.Add(originalList[i]);
		}
		return resultList;
	}




	public static void FaceObject(GameObject obj, GameObject target, bool shouldUseYPos)
	{
		Vector3 lookAtPos = target.transform.position;
		if (!shouldUseYPos)
		{
			lookAtPos = new Vector3(target.transform.position.x, obj.transform.position.y, target.transform.position.z);
		}
		obj.transform.LookAt(lookAtPos);
	}


	public static bool CheckVectorsCloseEnough(Vector3 position1, Vector3 position2, float epsilon)
	{
		float xDiff = Mathf.Abs(position1.x - position2.x);
		float yDiff = Mathf.Abs(position1.y - position2.y);
		float zDiff = Mathf.Abs(position1.z - position2.z);

		if (xDiff < epsilon && yDiff < epsilon && zDiff < epsilon)
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public static float GetDistance(Vector3 startPos, Vector3 endPos)
	{
		return (startPos - endPos).magnitude;
	}

	public static float GetDistance(Vector2 startPos, Vector2 endPos)
	{
		return (startPos - endPos).magnitude;
	}

	public static string ParseOutHiddenCharacters(string stringToParse)
	{
		if (stringToParse != null)
		{
			stringToParse = stringToParse.Replace("\n", "");
			stringToParse = stringToParse.Replace("\r", "");
		}
		return stringToParse;
	}

	public static IEnumerator WaitForJitter(float minJitter, float maxJitter)
	{
		float randomJitter = Random.Range(minJitter, maxJitter);
		//		Experiment.Instance.trialController.GetComponent<TrialLogTrack>().LogWaitForJitterStarted(randomJitter);

		float currentTime = 0.0f;
		while (currentTime < randomJitter)
		{
			currentTime += Time.deltaTime;
			yield return 0;
		}

		//		Experiment.Instance.trialController.GetComponent<TrialLogTrack>().LogWaitForJitterEnded(currentTime);
	}

	//WRITE UTILTY FUNCTIONS

	public static void WriteIntoTextFile(string fileName, List<int> intList)
	{
		string res = "";
		UnityEngine.Debug.Log("about to start writing stim list " + intList.Count.ToString());
		for(int i=0;i<intList.Count;i++)
        {
			res += intList[i].ToString() + "\n";
        }
		File.WriteAllText(fileName, res);
		UnityEngine.Debug.Log("writing list " + res.ToString());
	}
}
