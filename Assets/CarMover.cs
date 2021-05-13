using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMover : MonoBehaviour
{
    public GameObject playerCube;
    public Transform startTransform;
    public GameObject spawnPointCollection;

    [SerializeField]
    public float minTrackPointThreshold = 2f;

    private bool canMove = false;

    int spawnPointCount = 0;
    List<GameObject> spawnPoints;
    // Start is called before the first frame update
    void Start()
    {
        canMove = true;
        GetAllSpawnPoints();
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
    }

    GameObject GetTrackPoint(int pointIndex)
    {
        if (pointIndex < spawnPoints.Count)
            return spawnPoints[pointIndex].gameObject;
        else
        {
            UnityEngine.Debug.Log("exceeded child count;ending lap");
            ToggleCarMovement(false);
           // pointIndex = 0;
            return null;
        }
    }

    public void ToggleCarMovement(bool movementFlag)
    {
        canMove = movementFlag;
    }

    IEnumerator MoveCar()
    {
        //move player to start transform
        playerCube.transform.position = startTransform.position;
        playerCube.transform.rotation = startTransform.rotation;

        int currIndex = 0;

        //find next waypoint to move to
        while(canMove)
        {
            GameObject closestPoint = GetTrackPoint(currIndex);
            Vector3 startPos = playerCube.transform.position; //where the player is at start of interpolation 
            Vector3 startEuler = playerCube.transform.localEulerAngles; //where the player is at start of interpolation 
            Quaternion startRot = playerCube.transform.rotation;

            Vector3 endPos = closestPoint.transform.position;
            Vector3 endEuler = closestPoint.transform.localEulerAngles;
            Quaternion endRot = closestPoint.transform.rotation;

            float dist = Vector3.Distance(startPos, endPos);
          //  UnityEngine.Debug.Log("starting dist is " + dist.ToString());
            float lerpFactor = 0f; //reset factor

            //interpolate until you hit threshold distance to look for the next track point
            while(dist > 2f)
            {
                lerpFactor += Time.deltaTime;
                playerCube.transform.position = Vector3.Lerp(startPos, endPos, lerpFactor);
                //playerCube.transform.localEulerAngles = Vector3.Lerp(startEuler, endEuler, lerpFactor);

                //to avoid rotating around when the angles are within floating point precision error
                if(Vector3.Distance(startEuler,endEuler)>1.2f)
                    playerCube.transform.rotation = Quaternion.Slerp (startRot, endRot, lerpFactor);
                dist = Vector3.Distance(playerCube.transform.position, endPos);
                yield return 0;
            }

         //   UnityEngine.Debug.Log("incrementing point index");
            //increment the track point index
            currIndex++;

            yield return 0;
        }
        

        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            ToggleCarMovement(true);
            StartCoroutine("MoveCar");
        }
        
    }
}
