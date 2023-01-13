using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoScript : MonoBehaviour
{
    string url;
    VideoPlayer current_clip;
    private VideoPlayer thisVideo;
    public Experiment exp { get { return Experiment.Instance; } }

    // Use this for initialization
    void Start()
    {

        switch (gameObject.name)
        {
            case "Base":
                url = "https://spaceheist.s3.us-east-2.amazonaws.com/WebGLTest/base.mov";
                break;
        }
        thisVideo = GetComponent<VideoPlayer>();


    }

    void PlayVideo(string new_url)
    {
        thisVideo.url = new_url;
        thisVideo.Play();
        current_clip = GetComponent<VideoPlayer>();
        UnityEngine.Debug.Log("playing " + new_url);
    }

    private void Update()
    {
        if ((exp.beginScreenSelect == 0) ||
            ((exp.beginScreenSelect == -1) && (exp.beginPracticeSelect == 0)))
        {
            if (Input.GetKeyDown(KeyCode.Alpha6))
            {
                url = "https://spaceheist.s3.us-east-2.amazonaws.com/WebGLTest/base.mov";
                PlayVideo(url);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha7))
            {
                url = "https://spaceheist.s3.us-east-2.amazonaws.com/WebGLTest/sunny_resolve.mp4";
                PlayVideo(url);
            }
        }
        else {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                url = "https://spaceheist.s3.us-east-2.amazonaws.com/WebGLTest/base.mov";
                PlayVideo(url);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                url = "https://spaceheist.s3.us-east-2.amazonaws.com/WebGLTest/sunny_resolve.mp4";
                PlayVideo(url);
            }
        }
    }
}
