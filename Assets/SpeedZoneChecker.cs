using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedZoneChecker : MonoBehaviour
{
    private bool isZoneActive = false;
    private bool didPress = false;
    // Start is called before the first frame update

    void Start()
    {
        transform.parent.GetChild(0).GetComponent<FacePosition>().TargetPositionTransform = Experiment.Instance.player.transform;
    }

    // Update is called once per frame
    void Update()
    {
        if (isZoneActive)
        {
            if (Input.GetKeyDown(KeyCode.M) && !didPress)
            {
              //  Experiment.Instance.ActivateSpeedZone(true);
                didPress = true;

            }
        }

    }
    void OnTriggerEnter(Collider col)
    {
        isZoneActive = true;
    }

    void OnTriggerExit(Collider col)
    {
        //if no button has been pressed, penalize
        if (!didPress)
        {
           // Experiment.Instance.ActivateSpeedZone(false);
        }
        isZoneActive = false;
        didPress = false;
    }
}

