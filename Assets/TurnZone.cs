using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Utility;

public class TurnZone : MonoBehaviour
{
    public static bool canTurn = false;

    public bool isStraightLine = false;

    public Transform associatedCrashZone; 

    private bool performedTurn = false;
    public WaypointProgressTracker.TrackDirection turnDirection;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        if(canTurn)
        {
            //is it on straight part of a track; indicating the turn is a fork
       //     if (isStraightLine)
       //     {
                if (turnDirection == WaypointProgressTracker.TrackDirection.Left)
                {
                    if (Input.GetKeyDown(KeyCode.LeftArrow))
                    {
                        UnityEngine.Debug.Log("performed a left turn");
                        performedTurn = true;
                    }
                }
                else
                {
                    if (Input.GetKeyDown(KeyCode.RightArrow))
                    {
                        UnityEngine.Debug.Log("performed a right turn");
                        performedTurn = true;
                    }
                }
        //    }
       //     else
       //     {

       //     }
        }
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
            Experiment.Instance.ShowTurnDirection(turnDirection);
    }

    IEnumerator InitiateCrash()
    {

        UnityEngine.Debug.Log("CRASH!!! Failed to take the turn");
        yield return StartCoroutine(Experiment.Instance.BeginCrashSequence(associatedCrashZone));
        yield return null;
    }


    void OnTriggerEnter(Collider col)
    {
        UnityEngine.Debug.Log("entered zone " + transform.parent.gameObject.name.ToString());
        ConfigureTurnDirection();
        canTurn = true;
    }

    void OnTriggerExit(Collider col)
    {
        Experiment.Instance.HideTurnDirection(turnDirection);
        UnityEngine.Debug.Log("exited zone; performed turn? " + performedTurn.ToString());
        canTurn = false;

        if (performedTurn)
        {
            UnityEngine.Debug.Log("successfully took the turn");
        }
        else

        {
            StartCoroutine("InitiateCrash");
        }
        performedTurn = false;
    }
}
