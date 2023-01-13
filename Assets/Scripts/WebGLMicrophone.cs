using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WebGLMicrophone : MonoBehaviour
{
	Experiment exp { get { return Experiment.Instance; } }

	
	public enum micActivation
	{
		HoldToSpeak,
		PushToSpeak,
		ConstantSpeak
	}

	public float sensitivity = 100;
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
	AudioSource audio;


	void Awake()
	{
		audio = GetComponent<AudioSource>();
#if UNITY_WEBGL && !UNITY_EDITOR
			BrowserPlugin.Init();
#endif
	}

#if UNITY_WEBGL && !UNITY_EDITOR
        void Update()
        {
           // Microphone.Update();
        }
#endif

#if UNITY_WEBGL && !UNITY_EDITOR
	public static void SetSubject(string subjName)
	{
		BrowserPlugin.SetSubject(subjName);
	}
#endif
	public IEnumerator Record(string fileName, int duration)
	{
		audio.PlayOneShot(AssetBundleLoader.Instance.beepHigh);
		yield return new WaitForSeconds(AssetBundleLoader.Instance.beepHigh.length);
#if UNITY_WEBGL && !UNITY_EDITOR
	BrowserPlugin.StartRecording(fileName);
	yield return new WaitForSeconds(duration);
	BrowserPlugin.StopRecording();
#endif

		audio.PlayOneShot(AssetBundleLoader.Instance.beepLow);
		yield return new WaitForSeconds(AssetBundleLoader.Instance.beepLow.length);
	}

}
