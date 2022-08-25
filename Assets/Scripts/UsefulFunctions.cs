using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using System.Collections.Generic;

public class UsefulFunctions
{

	public static Experiment exp { get { return Experiment.Instance; } }
	
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
			int randomIndex = UnityEngine.Random.Range(0, inOrderList.Count);
			randomOrderList.Add(inOrderList[randomIndex]);
			inOrderList.RemoveAt(randomIndex);
		}

		return randomOrderList;
	}

	public static IEnumerator WaitForExitButton()
	{
		while (!Input.GetButtonDown("Exit"))
		{
			yield return 0;
		}
		yield return null;
	}


	public static IEnumerator WaitForActionButton()
	{
		if (exp.beginScreenSelect == 0)
		{
			while (!Input.GetButtonDown("Action"))
			{
				yield return 0;
			}
		}
		else {
			while (!Input.GetButtonDown("Action2"))
			{
				yield return 0;
			}
		}
		yield return null;
	}

	public static IEnumerator WaitForHeartBeatButton()
	{
		while (!Input.GetButtonDown("HB"))
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
		UnityEngine.Debug.Log("ListLength: Return: " + listLength);
		for (int i = 0; i < listLength; i++)
		{
			tempList.Add(i);
		}

		for (int i = 0; i < listLength; i++)
		{
			int randIndex = UnityEngine.Random.Range(0, tempList.Count);
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

	public static void Debug_PrintListToConsole(List<int> listToPrint)
    {
		for(int i=0;i<listToPrint.Count;i++)
        {
			UnityEngine.Debug.Log("DEBUG LIST : INDEX : " + i.ToString() + " VALUE: " + listToPrint[i].ToString());
        }
    }

	public static IEnumerator WaitForJitter(float minJitter, float maxJitter)
	{
		float randomJitter = UnityEngine.Random.Range(minJitter, maxJitter);
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

	public static Tuple<List<int>,List<int>> SplitIntList(List<int> targetList)
    {

		//check if even
		if (targetList.Count % 2 == 0 && targetList.Count > 0)
		{
			UnityEngine.Debug.Log("list is even");
		}
		else
		{
			UnityEngine.Debug.Log("ERROR: We will get uneven lists");
		}
		int halfIndex = targetList.Count / 2;

		List<int> listA = new List<int>();
		List<int> listB = new List<int>();
		//store first half in one list
		for (int i = 0; i < halfIndex; i++)
		{
			listA.Add(targetList[0]);
			targetList.RemoveAt(0);
		}
		//duplicate the remaining part of the list and store it in the second list
		listB = DuplicateList(targetList);
		return Tuple.Create(listA, listB); //return as a tuple
	}

	//split list into half and return both as lists as part of a tuple
	public static Tuple<TrialConditions,TrialConditions> SplitTrialConditions(TrialConditions trialConditions)
    {

		List<int> retType = trialConditions.retrievalTypeList;
		List<int> weatherChange = trialConditions.weatherChangeTrials;
		List<int> randomizedWeather = trialConditions.randomizedWeatherOrder;




		Tuple<List<int>, List<int>> tuple_retType = SplitIntList(retType);
		Tuple<List<int>, List<int>> tuple_weatherChange = SplitIntList(weatherChange);
		Tuple<List<int>, List<int>> tuple_randomizedWeather = SplitIntList(randomizedWeather);

		TrialConditions session_1 = new TrialConditions(tuple_retType.Item1,tuple_weatherChange.Item1,tuple_randomizedWeather.Item1);
		TrialConditions session_2 = new TrialConditions(tuple_retType.Item2,tuple_weatherChange.Item2,tuple_randomizedWeather.Item2) ;



		return Tuple.Create(session_1, session_2);

	}


	///WRITING PERMISSIONS
    public bool IsDirectoryWritable(string dirPath, bool throwIfFails = false)
{
    try
    {
        using (FileStream fs = File.Create(
			Path.Combine(
				dirPath,
				Path.GetRandomFileName()
            ), 
            1,
            FileOptions.DeleteOnClose)
        )
        { }
        return true;
    }

	catch
{
	if (throwIfFails)
		throw;
	else
		return false;
}
}

}
