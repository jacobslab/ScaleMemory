using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityStandardAssets.Utility;

public class TurnZone : MonoBehaviour
{
    private bool canTurn = false;

    public bool isStraightLine = false;

    public Transform associatedCrashZone;
    public Transform zoneEndpoint;
    public bl_ProgressBar leftProgressQuad;
    public bl_ProgressBar rightProgressQuad;

    public bool finalCorner = false;

    private bool performedTurn = false;
    public WaypointProgressTracker.TrackDirection turnDirection;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if(canTurn && !Experiment.isCrashing)
        {
            //is it on straight part of a track; indicating the turn is a fork
       //     if (isStraightLine)
       //     {
                if (turnDirection == WaypointProgressTracker.TrackDirection.Left)
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        UnityEngine.Debug.Log("performed a left turn " + transform.parent.gameObject.name);
                        performedTurn = true;
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        UnityEngine.Debug.Log("performed a right turn " + transform.parent.gameObject.name);   
                        performedTurn = true;
                    }
                }
        //    }
       //     else
       //     {

       //     }
        }

        
    }

    public IEnumerator ShowKeypressProgress()
    {
        bl_ProgressBar chosenProgress = null;
        if (turnDirection == WaypointProgressTracker.TrackDirection.Left)
        {
            chosenProgress = leftProgressQuad;
        }
        else
        {
            chosenProgress = rightProgressQuad;
        }
        chosenProgress.gameObject.GetComponent<Image>().color = Color.red;
        //UnityEngine.Debug.Log("beginning keypress progress");
        while (canTurn && !performedTurn)
        {
            float dist = Vector3.Distance(Experiment.Instance.player.transform.position, zoneEndpoint.transform.position);
           // UnityEngine.Debug.Log("dist " + dist.ToString());
            float percent = Mathf.Clamp(100f- ((dist / 23f) * 100f),0f,100f);
            //UnityEngine.Debug.Log("percent " + percent.ToString());
            chosenProgress.Value = percent;
            yield return 0;
        }
        if(performedTurn)
        {
            chosenProgress.gameObject.GetComponent<Image>().color = Color.green;
        }
            yield return null;
    }

    void ConfigureTurnDirection()
    {
        if (isStraightLine)
        {
          turnDirection = Experiment.Instance.player.GetComponent<WaypointProgressTracker>().currentDirection;

        }
        else
        {
            //do nothing as the direction has already been set via editor
        }
       // UnityEngine.Debug.Log("about to start keypress progress");
        StartCoroutine("ShowKeypressProgress");
        Experiment.Instance.ShowTurnDirection(turnDirection,this.gameObject);
    }

    IEnumerator InitiateCrash()
    {

        UnityEngine.Debug.Log("CRASH!!! Failed to take the turn");
        Experiment.isCrashing = true;
        yield return StartCoroutine(Experiment.Instance.BeginCrashSequence(associatedCrashZone));
        Experiment.isCrashing = false;

        yield return null;
    }


    void OnTriggerEnter(Collider col)
    {
        UnityEngine.Debug.Log("entered zone " + transform.parent.gameObject.name.ToString());
        canTurn = true;
        ConfigureTurnDirection();

    }

    void OnTriggerExit(Collider col)
    {
        Experiment.Instance.HideTurnDirection(turnDirection);
        UnityEngine.Debug.Log("exited zone; performed turn? " + performedTurn.ToString());
       

        if (performedTurn)
        {
            UnityEngine.Debug.Log("successfully took the turn " + transform.parent.gameObject.name);

            //if this is the final corner AND we turned successfully, only then we turn on the chequered flag
            if (finalCorner)
            {
                Experiment.Instance.SetChequeredFlagStatus(true);
            }
        }
        else

        {
            StartCoroutine("InitiateCrash");
        }
        canTurn = false;
        performedTurn = false;       
       
    }
}
