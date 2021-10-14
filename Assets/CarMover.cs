﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMover : MonoBehaviour
{
    public GameObject playerCube;

    public Transform presentationTransform;
    public Transform startTransform;
    public GameObject spawnPointCollection;

    public Rigidbody playerRigidbody;

    public GameObject spatialRetrievalIndicator;

    [SerializeField]
    public float minTrackPointThreshold = 2f;

    private bool directionChanged = false;
    public static bool carActive = false;

    private float speedFactor = 1.5f;

    private bool canMove = false;

    public enum DriveMode
    {
        Auto,
        Manual
    }

    public DriveMode currentDriveMode = DriveMode.Auto;



    public enum MovementDirection
    {
        Forward,
        Reverse,
        Neutral
    }

    public MovementDirection currentMovementDirection = MovementDirection.Forward;

    private int currIndex = 0;

    int spawnPointCount = 0;
    List<GameObject> spawnPoints;
    // Start is called before the first frame update
    void Start()
    {
        ToggleSpatialRetrievalIndicator(false);
        canMove = true;
        GetAllSpawnPoints();
    }

    public void ToggleSpatialRetrievalIndicator(bool isVisible)
    {
        spatialRetrievalIndicator.SetActive(isVisible);
    }

    void GetAllSpawnPoints()
    {
        spawnPoints = new List<GameObject>();
        spawnPointCount = spawnPointCollection.transform.childCount;
        for (int i = 0; i < spawnPointCount; i++)
        {
            spawnPoints.Add(spawnPointCollection.transform.GetChild(i).gameObject);

        }

        //   UnityEngine.Debug.Log("added total " + spawnPoints.Count.ToString() + " points ");
    }

    public IEnumerator SetMovementDirection(MovementDirection newMovementDirection)
    {
        //only make change if the movement direction has changed
        if (newMovementDirection != currentMovementDirection)
        {
            //exit out of the movement loop
            //canMove = false;
            //ToggleCarMovement(false);
            directionChanged = true;

           // StopCoroutine("DriveCar");

            UnityEngine.Debug.Log("set new direction");
            currentMovementDirection = newMovementDirection;


            //  yield return new WaitForSeconds(0.5f);
           // UnityEngine.Debug.Log("set new movement direction  " + currentMovementDirection.ToString());
        }
        yield return null;
    }

    public bool CheckIfMoving()
    {
        return canMove;
    }

    GameObject FindClosestTrackPoint()
    {
        Vector3 currPos = playerCube.transform.position;
        float minDist = 1000f;
        GameObject closestPoint = null;
        for(int i=0;i<spawnPoints.Count;i++)
        {
            float currDist = Vector3.Distance(currPos, spawnPoints[i].transform.position);
            if(currDist<=minDist)
            {
                closestPoint = spawnPoints[i];
                minDist = currDist;
            }
        }
        if (closestPoint != null)
            return closestPoint;
        else
        {
         //   UnityEngine.Debug.Log("could not find track object");
            return closestPoint;
        }
    }

    public void Reset()
    {
        //reset movement
        UnityEngine.Debug.Log("reset movement");
    }

    public void ResetTargetWaypoint(Transform newStartTransform)
    {

        //we first find which of the randomized start positions is closest to our drive/spawn points
        float shortestDist = 100f;
        int shortestIndex = -1;
        for(int i=0;i< spawnPointCount;i++)
        {
            float currDist = Vector3.Distance(newStartTransform.position, spawnPoints[i].transform.position);
            if(currDist< shortestDist)
            {
                shortestDist = currDist;
                shortestIndex = i;
            }
        }

        //we then assign it to our next waypoint target indicator
        currIndex = shortestIndex;
    }

    GameObject GetTrackPoint(int pointIndex)
    {
        if (pointIndex < spawnPoints.Count && pointIndex>=0)
            return spawnPoints[pointIndex].gameObject;
        else
        {
            //only auto ends drive mode
            if (currentDriveMode == DriveMode.Auto)
            {
                if (Experiment.Instance.currentStage == Experiment.TaskStage.VerbalRetrieval)
                {

                    pointIndex = 0;
                    ResetWaypointTarget();
                    UnityEngine.Debug.Log("completed lap in AUTO but it's retrieval; so continue moving");
                    return spawnPoints[0].gameObject;
                }
                else
                {
                    UnityEngine.Debug.Log("exceeded child count in AUTO mode;ending lap");
                    LapCounter.canStop = true; // flag lap as complete
                    ToggleCarMovement(false);
                    ResetWaypointTarget(); //resets it to the beginning of the lap
                    LapCounter.CompleteLap(); //INCREMENT lap count
                    return null;
                }
            }
            else if(currentDriveMode == DriveMode.Manual)
            {
                UnityEngine.Debug.Log("exceeded child count in MANUAL mode;adjusting waypoints properly");
                //return start of the lap if moving in forward direction
                if (currentMovementDirection == MovementDirection.Forward)
                {
                    UnityEngine.Debug.Log("going forward");
                    currIndex = 0;
                    return spawnPoints[currIndex].gameObject;
                }
                //else return the last waypoint if moving in the reverse direction
                else if (currentMovementDirection == MovementDirection.Reverse)
                {
                    UnityEngine.Debug.Log("going reverse");
                    currIndex = spawnPoints.Count - 1;
                    return spawnPoints[currIndex].gameObject;
                }
                else
                {
                    currIndex = 0;
                    UnityEngine.Debug.Log("WARNING; NOT SET PROPERLY");
                    return spawnPoints[currIndex].gameObject;
                }
            }
            else
            {
                currIndex = 0;
                UnityEngine.Debug.Log("WARNING; NOT SET PROPERLY");
                return spawnPoints[currIndex].gameObject;
            }
        }
    }

    public void ResetWaypointTarget()
    {
        currIndex = 0;
    }

    float RandomizeSpeed()
    {
        //slower speed during encoding to maintain the "five-second gap" for stim
        if (Experiment.Instance.currentStage == Experiment.TaskStage.Encoding)
            return UnityEngine.Random.Range(2f, 2.5f);
        else
            return UnityEngine.Random.Range(1f, 1.25f);
    }

    public void ToggleCarMovement(bool movementFlag)
    {
        UnityEngine.Debug.Log("car movement " + movementFlag.ToString());
        canMove = movementFlag;
    }

    //this will define overall whether the car can move or not
    public void TurnCarEngine(bool isOn)
    {
        carActive = isOn;
    }

    IEnumerator DriveCar(MovementDirection targetDirection)
    {
        GameObject closestPoint = GetTrackPoint(currIndex);

        //UnityEngine.Debug.Log("curr index " + currIndex.ToString());
        //exit out of the loop if you cannot find the next point on the track; should indicate the end of the lap
        /*

        if(closestPoint!=null)
        {
            UnityEngine.Debug.Log("could not find the next track point");
            canMove = false;
        }
        */

        Vector3 startPos = playerCube.transform.position; //where the player is at start of interpolation 
        Vector3 startEuler = playerCube.transform.localEulerAngles; //where the player is at start of interpolation 
        Quaternion startRot = playerCube.transform.rotation;

        speedFactor = RandomizeSpeed();

        Vector3 endPos, endEuler;
        Quaternion endRot;
        //if closest point is found, then make that our target
        if (closestPoint != null)
        {
            endPos = closestPoint.transform.position;
            endEuler = closestPoint.transform.localEulerAngles;
            endRot = closestPoint.transform.rotation;
        }
        //else make the target to be the same as start -- meaning no movement should happen; only gets triggered upon the completion of the lap
        else
        {
            endPos = startPos;
            endEuler = startEuler;
            endRot = startRot;
        }
        float dist = Vector3.Distance(startPos, endPos);
        //UnityEngine.Debug.Log("starting dist is " + dist.ToString());
        float lerpFactor = 0f; //reset factor

        //interpolate until you hit threshold distance to look for the next track point
        while (dist > 2f && canMove && carActive && !directionChanged)
        {
            // UnityEngine.Debug.Log("CURRENT DIST: " + dist.ToString());
            lerpFactor += Time.deltaTime;
            playerCube.transform.position = Vector3.Lerp(startPos, endPos, lerpFactor/speedFactor);
            //playerCube.transform.localEulerAngles = Vector3.Lerp(startEuler, endEuler, lerpFactor);

            //to avoid rotating around when the angles are within floating point precision error
            if (Vector3.Distance(startEuler, endEuler) > 1.2f)
                playerCube.transform.rotation = Quaternion.Slerp(startRot, endRot, lerpFactor/speedFactor);
            dist = Vector3.Distance(playerCube.transform.position, endPos);
            yield return 0;
        }
        UnityEngine.Debug.Log("exited loop");

        //increment the track point index
        if (currentMovementDirection == MovementDirection.Forward)
            currIndex++;
        else if (currentMovementDirection == MovementDirection.Reverse)
            currIndex--;
        UnityEngine.Debug.Log("incrementing point index to " + currIndex.ToString());

        UnityEngine.Debug.Log("exiting DriveCar");

        //if we are exiting the loop due to change in direction, then toggle the variable back so we can move in the new direction again
        if (directionChanged)
        {
            ToggleCarMovement(true);
            directionChanged = false; //reset direction change
        }
        yield return null;
    }

    public IEnumerator MoveCar()
    {
        //move player to start transform
        playerCube.transform.position = startTransform.position;
        playerCube.transform.rotation = startTransform.rotation;


        ResetWaypointTarget(); //reset before beginning the movement
        while (carActive)
        {
            //find next waypoint to move to
            while (canMove)
            {

                if (currentDriveMode == DriveMode.Auto)
                {
                    //we always move forward when in auto
                    yield return StartCoroutine(DriveCar(MovementDirection.Forward));

                }
                else if(currentDriveMode == DriveMode.Manual)
                {
                   
                    //StopCoroutine("DriveCar");
                    UnityEngine.Debug.Log("moving inside Manual MoveCar");
                    yield return StartCoroutine(DriveCar(currentMovementDirection));

                }
                yield return 0;
            }

         //   UnityEngine.Debug.Log("exited movement loop");
            yield return 0;
        }

        UnityEngine.Debug.Log("exiting car active loop");
        yield return null;
    }

    public void SetDriveMode(DriveMode newDriveMode)
    {
        UnityEngine.Debug.Log("setting drive mode to " + newDriveMode.ToString());
        currentDriveMode = newDriveMode;
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(Input.GetKeyDown(KeyCode.Space) && !canMove)
        {
            ToggleCarMovement(true);
            StartCoroutine("MoveCar");
        }
        */
        
    }
}
