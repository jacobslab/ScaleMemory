﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class LapCounter : MonoBehaviour
{
    public static int lapCount = 0;
    public static bool isRetrieval = false;
    public static bool finishedLap = false;

    public static bool canStop = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other)
    {
        UnityEngine.Debug.Log("collided with " + other.gameObject.name);
        if (other.gameObject.tag == "Player")
        {
            UnityEngine.Debug.Log("ON TRIGGER ENTER");

            if (Experiment.Instance.currentStage == Experiment.TaskStage.TrackScreening)
            {
                lapCount++;
            }
            else
            {
                canStop = true;
                if (!isRetrieval)
                {
                    if ((lapCount + 1) % Configuration.totalTrials == 0)
                    {
                        UnityEngine.Debug.Log("retrieval mode active");
                        isRetrieval = true;
                    }
                    lapCount++;
                    UnityEngine.Debug.Log("completed a lap");
                    return;
                }
                else
                {
                    UnityEngine.Debug.Log("finished lap in retrieval mode");
                    lapCount++;
                    UnityEngine.Debug.Log("lap count " + lapCount.ToString());
                    finishedLap = true;
                    return;
                }
            }
        }
    }
}
