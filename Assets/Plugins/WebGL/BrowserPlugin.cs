#if UNITY_WEBGL && !UNITY_EDITOR

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace UnityEngine
{
    public class BrowserPlugin
    {
        [DllImport("__Internal")]
        public static extern int Getvar();
        [DllImport("__Internal")]
        public static extern void Setvar(int a);
        [DllImport("__Internal")]
        public static extern void Init();
        [DllImport("__Internal")]
        public static extern void RecordStart(string fileName);

        [DllImport("__Internal")]
        private static extern void PrintString(string str);
        [DllImport("__Internal")]
        private static extern void RecordStop();
        [DllImport("__Internal")]
        private static extern void WriteToFile(string str);

        [DllImport("__Internal")]
        private static extern int ReturnMicStatus();

        [DllImport("__Internal")]
        private static extern void PutTextFile();

        [DllImport("__Internal")]
        private static extern void CheckMic();

        [DllImport("__Internal")]
        private static extern void SubjectSet(string subj);

        private static List<Action> _sActions = new List<Action>();

        public static void Update()
        {
            for (int i = 0; i < _sActions.Count; ++i)
            {
                Action action = _sActions[i];
                action.Invoke();
            }
        }

/*
        public static string[] devices
        {
            get
            {
                List<string> list = new List<string>();
                int size = GetNumberOfMicrophones();
                for (int index = 0; index < size; ++index)
                {
                    string deviceName = GetMicrophoneDeviceName(index);
                    list.Add(deviceName);
                }
                return list.ToArray();
            }
        }

        public static float[] volumes
        {
            get
            {
                List<float> list = new List<float>();
                int size = GetNumberOfMicrophones();
                for (int index = 0; index < size; ++index)
                {
                    float volume = GetMicrophoneVolume(index);
                    list.Add(volume);
                }
                return list.ToArray();
            }
        }


        public static bool IsRecording(string deviceName)
        {
            return false;
        }
*/

                public static void PrintToLog(string str)
                {
                UnityEngine.Debug.Log("print to browser log: " + str);
                PrintString(str);
                }
        public static void StartRecording(string filename)
        {
            UnityEngine.Debug.Log("starting recording " + filename);
            RecordStart(filename);

        }

public static void Setup()
{
UnityEngine.Debug.Log("browser plugin setup");
Init();
}

public static int InquireMicStatus()
{
UnityEngine.Debug.Log("inquiring mic status");
int micStatus = 0;
micStatus = ReturnMicStatus();

return micStatus;
}

public static void WriteOutput(string outputLine)
{
//UnityEngine.Debug.Log("writing : " + outputLine);
string line = outputLine + "\n";
WriteToFile(line);

}

public static void CheckMicStatus()
{
UnityEngine.Debug.Log("checking mic status");
CheckMic();
}

public static void SendTextFileToS3()
{
UnityEngine.Debug.Log("sending text file to S3");
PutTextFile();
}

        public static void SetSubject(string str)
        {
            UnityEngine.Debug.Log("setting subject " + str);
            SubjectSet(str);
        }
        public static void StopRecording()
        {
            UnityEngine.Debug.Log("stopping recording");
            RecordStop();
        }
    }
}

#endif