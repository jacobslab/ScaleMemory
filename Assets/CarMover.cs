using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarMover : MonoBehaviour
{
    public GameObject playerCube;
    public Transform startTransform;
    public GameObject spawnPointCollection;

    int spawnPointCount = 0;
    List<GameObject> spawnPoints;
    // Start is called before the first frame update
    void Start()
    {
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
    }

    IEnumerator MoveCar()
    {
        //move player to start transform
        playerCube.transform.position = startTransform.position;
        playerCube.transform.rotation = startTransform.rotation;

        yield return null;
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            StartCoroutine("MoveCar");
        }
        
    }
}
