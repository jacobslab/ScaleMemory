using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPointTester : MonoBehaviour
{
    public GameObject spawnSamplePrefab;

    public GameObject spawnPointCollection;

    int spawnPointCount = 0;
    List<GameObject> spawnPoints;

    List<GameObject> spawnedSamples;
    // Start is called before the first frame update
    void Start()
    {
        GetAllSpawnPoints();
    }

    void GetAllSpawnPoints()
    {
        spawnPoints = new List<GameObject>();
        spawnPointCount = spawnPointCollection.transform.childCount;
        for (int i=0;i<spawnPointCount;i++)
        {
            spawnPoints.Add(spawnPointCollection.transform.GetChild(i).gameObject);

        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.U))
        {
            StartCoroutine("SpawnPointsRandomly");
        }
        
    }

    IEnumerator ResetSpawnedSamples()
    {
        if (spawnedSamples != null)
        {
            for (int i = 0; i < spawnedSamples.Count; i++)
            {
                Destroy(spawnedSamples[i]);
            }
            spawnedSamples.Clear();
        }
        spawnedSamples = new List<GameObject>();
        yield return null;
    }
    public IEnumerator SpawnPointsRandomly()
    {
        yield return StartCoroutine(ResetSpawnedSamples());

        UnityEngine.Debug.Log("starting coroutine");
        List<Vector3> chosenPoints = new List<Vector3>();
        //we will spawn five items in total
        for(int i=0;i<5;i++)
        {
            Vector3 randomPoint = Vector3.zero;
               float dist = 0f;
            UnityEngine.Debug.Log("current dist " + dist.ToString());
            //continue until dist is greater than 7.5
            while (dist < 12f)
            {
                //pick a random point and its next point in the forward direction
                int randWaypoint = Random.Range(0, spawnPointCount - 2);
                int nextWaypoint = randWaypoint + 1;
                Vector3 firstWaypoint = spawnPoints[randWaypoint].transform.position;
                Vector3 secondWaypoint = spawnPoints[nextWaypoint].transform.position;

                //find a random point between the two waypoints
                randomPoint = Vector3.Lerp(firstWaypoint, secondWaypoint, Random.value);

                UnityEngine.Debug.Log("picked a random point " + randomPoint.ToString());

                float minDist = 1000f;
                int minIndex = -1;
                //check to see if it is within the threshold
                for (int j = 0; j < chosenPoints.Count; j++)
                {
                    float currDist = Vector3.Distance(randomPoint, chosenPoints[j]);
                    if (currDist < minDist)
                    {
                        minDist = currDist;
                        minIndex = j;
                    }
                }
                //we automatically add the first point
                if (i == 0)
                {
                    dist = 100f;
                    UnityEngine.Debug.Log("MIN DIST " + minDist.ToString());
                }
                else
                {
                    if (minIndex != -1)
                    {
                        dist = minDist;
                        UnityEngine.Debug.Log("MIN DIST " + minDist.ToString());
                    }
                }
                yield return 0;
            }
            chosenPoints.Add(randomPoint);
            UnityEngine.Debug.Log("spawning at " + randomPoint.ToString());
            GameObject spawnSample = Instantiate(spawnSamplePrefab, randomPoint, Quaternion.identity) as GameObject;
            spawnedSamples.Add(spawnSample);
        }
        yield return null;
    }
}
