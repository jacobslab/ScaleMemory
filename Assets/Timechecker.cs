using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine.UI;
using UnityEngine;

public class Timechecker : MonoBehaviour
{
    public Text ts_val;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CheckTime()
    {
        UnityEngine.Debug.Log("utc time " + GameClock.SystemTime_Milliseconds.ToString());
        ts_val.text = GameClock.SystemTime_Milliseconds.ToString();
    }
}
