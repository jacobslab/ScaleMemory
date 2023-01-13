using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMover : MonoBehaviour
{

    public Transform presentationTransform;
    public Transform startTransform;


    public GameObject spatialRetrievalIndicator;

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
    // Start is called before the first frame update
    void Start()
    {
        currentMovementDirection = MovementDirection.Forward;
        ToggleSpatialRetrievalIndicator(false);
        canMove = true;
    }

    public void ToggleSpatialRetrievalIndicator(bool isVisible)
    {
        spatialRetrievalIndicator.SetActive(isVisible);
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
            if (currentMovementDirection == MovementDirection.Forward)
                Experiment.Instance.videoLayerManager.ChangePlaybackDirection(VideoLayerManager.Direction.Forward);
            else
                Experiment.Instance.videoLayerManager.ChangePlaybackDirection(VideoLayerManager.Direction.Backward);




            //  yield return new WaitForSeconds(0.5f);
            // UnityEngine.Debug.Log("set new movement direction  " + currentMovementDirection.ToString());
        }

        UnityEngine.Debug.Log("finished changing direction");
        yield return null;
    }

    public bool CheckIfMoving()
    {
        return canMove;
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

    public void SetDriveMode(DriveMode newDriveMode)
    {
        UnityEngine.Debug.Log("setting drive mode to " + newDriveMode.ToString());
        currentDriveMode = newDriveMode;
        Experiment.Instance.videoLayerManager.SetNewPlaybackMode(currentDriveMode);
    }

}
