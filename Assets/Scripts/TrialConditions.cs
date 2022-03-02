using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrialConditions : MonoBehaviour
{
    public List<int> retrievalTypeList = new List<int>();
    public List<int> weatherChangeTrials = new List<int>();
    public List<int> randomizedWeatherOrder = new List<int>();

    public TrialConditions()
    {

    }
    public TrialConditions(List<int> _retType, List<int> _weatherChange, List<int> _randWeather)
    {
        retrievalTypeList = UsefulFunctions.DuplicateList(_retType);
        weatherChangeTrials = UsefulFunctions.DuplicateList(_weatherChange);
        randomizedWeatherOrder = UsefulFunctions.DuplicateList(_randWeather);
    }

    public string ToJSONString()
    {
        return JsonUtility.ToJson(this);
    }
}
