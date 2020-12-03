using System.Collections;

using System.Collections.Generic;
using UnityEngine;

public class ArmZones : MonoBehaviour
{
    public enum Zone
    {
        Left,
        Right,
        Straight
    };
    public Zone associatedZone;
    public bool isExit = false;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider col)
    {
        if (!isExit)
        {
            if (col.gameObject.tag == "Player")
            {
                Experiment.Instance.trialLogTrack.LogZoneExit("Straight");
                Experiment.Instance.trialLogTrack.LogZoneEntry(associatedZone.ToString());
            }
        }
    }

    void OnTriggerExit(Collider col)
    {
        if (isExit)
        {
            if (col.gameObject.tag == "Player")
            {
                Experiment.Instance.trialLogTrack.LogZoneExit(associatedZone.ToString());
                Experiment.Instance.trialLogTrack.LogZoneEntry("Straight");
            }
        }

    }
}
