using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class VideoLayer : MonoBehaviour
{
    public VideoPlayer videoPlayer;
    public VideoClip vidClip;
    public RawImage rawImage;

    //colors
    private Color visibleColor = new Color(1, 1, 1, 1);
    private Color hiddenColor = new Color(0,0,0,0);



    private void Awake()
    {
        rawImage.color = hiddenColor; //transparent by default
    }

    void Start()
    {

    }

    private void Update()
    {
        if(gameObject.name == "BACKGROUND")
            UnityEngine.Debug.Log("playback time " + videoPlayer.time.ToString());
    }

    public IEnumerator PrepareVideoTexture()
    {
        if (videoPlayer == null || rawImage == null)
            yield break;


        //yield return StartCoroutine(VideoLayerManager.Instance.AddToActiveVideoLayer(this));

        UnityEngine.Debug.Log("preparing  " + gameObject.name);
        videoPlayer.clip = vidClip;
        videoPlayer.renderMode = VideoRenderMode.APIOnly;
        videoPlayer.Prepare();
        while (!videoPlayer.isPrepared)
            yield return new WaitForSeconds(1);

        rawImage.texture = videoPlayer.texture;
        videoPlayer.Play();
        videoPlayer.Pause();
        yield return null;
    }
    //this usually corresponds to the layer being added to the "active stack"
    public IEnumerator BeginPlayback()
    {
        UnityEngine.Debug.Log("beginning playback of " + gameObject.name);

        videoPlayer.Play();
        yield return null;
    }

    public void ToggleLayerVisibility(bool isVisible)
    {
        UnityEngine.Debug.Log("making " + gameObject.name + " visibility: " + isVisible.ToString());
        rawImage.color = (isVisible) ? visibleColor : hiddenColor;
    }

    public double GetPlaybackTime()
    {
        return videoPlayer.time;
    }    

    public IEnumerator TogglePlayback(bool shouldPause)
    {
        if (shouldPause)
            videoPlayer.Pause();
        else
            videoPlayer.Play();
        yield return null;
    }

    public IEnumerator ScrollToFrame(int frameNum)
    {
        videoPlayer.frame = frameNum;
        yield return null;
    }

    public IEnumerator ScrollToPlaybackTime(double playbackTime)
    {
        videoPlayer.time = playbackTime;
        yield return null;
    }

    public IEnumerator AbortPlayback()
    {
        //videoPlayer.Stop();
        //rawImage.color = hiddenColor;
        yield return null;
    }
}
