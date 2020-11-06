//SCRIPT MODIFIED FROM: http://wiki.unity3d.com/index.php/Mic_Input

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class AudioRecorder : MonoBehaviour
{

    Experiment exp { get { return Experiment.Instance; } }
    public AudioLogTrack audioLogger;
    public enum micActivation
    {
        HoldToSpeak,
        PushToSpeak,
        ConstantSpeak
    }

    public float sensitivity = 100;
    public int samplerate = 44100;
    public float ramFlushSpeed = 5;//The smaller the number the faster it flush's the ram, but there might be performance issues...
    [Range(0, 100)]
    public float sourceVolume = 100;//Between 0 and 100
    public bool GuiSelectDevice = true;
    public micActivation micControl;
    //
    public string selectedDevice { get; private set; }
    public float loudness { get; private set; } //dont touch
                                                //
                                                //private bool micSelected = false;
    private float ramFlushTimer;
    private int amountSamples = 256; //increase to get better average, but will decrease performance. Best to leave it
    private int minFreq, maxFreq;

    public AudioSource beepHigh;
    public AudioSource beepLow;
    AudioSource audio;

    void Start()
    {
        audio = GetComponent<AudioSource>();

        if (CheckForRecordingDevice())
        {
            Debug.Log(Microphone.devices.Length);
            audio.loop = true; // Set the AudioClip to loop
            audio.mute = false; // Mute the sound, we don't want the player to hear it
            selectedDevice = Microphone.devices[0].ToString();
            //micSelected = true;
            GetMicCaps();

        }
    }

    public static bool CheckForRecordingDevice()
    {
        if (Microphone.devices.Length > 0)
        {
            return true;
        }
        return false;
    }

    public void GetMicCaps()
    {
        Microphone.GetDeviceCaps(selectedDevice, out minFreq, out maxFreq);//Gets the frequency of the device
        if ((minFreq + maxFreq) == 0)//These 2 lines of code are mainly for windows computers
            maxFreq = 44100;
    }

    /* //FOR DEBUGGING / TESTING
	int numRecordings = 0;
	void GetInput(){
		if (Input.GetKeyDown (KeyCode.A)) {
			StartCoroutine(Record("/Users/coreynovich/Desktop/Unity/DeliveryBoy/TextFiles", "testRecord" + numRecordings, 4));
			numRecordings++;
		}
	}

	void Update() {
		GetInput ();
	}*/

    public Text recordText;
    public IEnumerator Record(string filePath, string fileName, int duration)
    {
        if (Microphone.devices.Length > 0)
        {
            Debug.Log("about to record");
            //Color origTextColor = recordText.color;
            //recordText.color = Color.red;
            if (Experiment.Instance != null)
                audioLogger.LogRecording(fileName, true);
            StartMicrophone(duration);
            yield return new WaitForSeconds(duration);

            StopMicrophone();
            if (Experiment.Instance != null)
                audioLogger.LogRecording(fileName, false);
            //recordText.color = origTextColor;

            SavWav.Save(filePath, fileName, audio.clip);
        }
        else
        {
            Debug.Log("No mic to record with!");
            yield return new WaitForSeconds(duration);
        }
    }

    public IEnumerator RecordContinuous(string filePath, string fileName)
    {
        if (Microphone.devices.Length > 0)
        {
            //Color origTextColor = recordText.color;
            //recordText.color = Color.red;
            //audioLogger.LogRecording(fileName, true);
            int arbitraryDuration = 100;
            StartMicrophone(arbitraryDuration); //arbitrarily large value
            Debug.Log("beginning record");

            while (Input.GetKey(KeyCode.S))
            {
                yield return 0;
            }
            //WaitForSeconds(duration);

            int position = StopMicrophoneAndGetPosition();
            Debug.Log("ending record at " + position.ToString());
            //audioLogger.LogRecording(fileName, false);


            float[] samples = new float[audio.clip.samples * audio.clip.channels];
            Debug.Log("total clip samples " + (audio.clip.samples).ToString());
            audio.clip.GetData(samples, 0);
            float[] newSamples = new float[position];

            AudioClip newClip = AudioClip.Create("newClip", samplerate * 2, 1, samplerate, false);

            for (int i = 0; i < position; i++)
            {
                newSamples[i] = samples[i];
            }

            newClip.SetData(newSamples, 0);
            Debug.Log("adjusted clip size" + newSamples.Length.ToString() + " in real " + newClip.samples.ToString());
            SavWav.Save(filePath, fileName, newClip);


        }
        else
        {
            Debug.Log("no mic to record with");
        }
        yield return 0;
    }

    public void StartMicrophone(int duration)
    {
        audio.clip = Microphone.Start(selectedDevice, true, duration, 16000);//Starts recording
        while (!(Microphone.GetPosition(selectedDevice) > 0)) { } // Wait until the recording has started
                                                                  //audio.Play(); // Play the audio source!
    }

    public void StopMicrophone()
    {
        audio.Stop();//Stops the audio
        Microphone.End(selectedDevice);//Stops the recording of the device	
    }

    public int StopMicrophoneAndGetPosition()
    {
        int position = Microphone.GetPosition(selectedDevice);
        audio.Stop();
        Microphone.End(selectedDevice);
        return position;
    }

    private void RamFlush()
    {
        if (ramFlushTimer >= ramFlushSpeed && Microphone.IsRecording(selectedDevice))
        {
            StopMicrophone();
            StartMicrophone(10);
            ramFlushTimer = 0;
        }
    }

    float GetAveragedVolume()
    {
        float[] data = new float[amountSamples];
        float a = 0;
        audio.GetOutputData(data, 0);
        foreach (float s in data)
        {
            a += Mathf.Abs(s);
        }
        return a / amountSamples;
    }
}