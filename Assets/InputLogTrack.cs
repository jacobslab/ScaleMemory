using UnityEngine;
using System.Collections;
using UnityStandardAssets.Utility;
using UnityEditor;
using System.Runtime.InteropServices;

public class InputLogTrack : LogTrack
{
    private KeyCode lastHitKey;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void OnGUI()
    {
        if (Input.anyKeyDown)
        {
            Event e = Event.current;
            if (e != null)
            {
                if (e.isKey)
                {
                    lastHitKey = e.keyCode;
                    LogKey(lastHitKey);
                }
            }
        }
    }

    void LogKey(KeyCode key)
    {
        UnityEngine.Debug.Log("key hit " + key.ToString());
        subjectLog.Log(GameClock.SystemTime_Milliseconds, "KEY_PRESS" + separator + key.ToString());
    }
}
