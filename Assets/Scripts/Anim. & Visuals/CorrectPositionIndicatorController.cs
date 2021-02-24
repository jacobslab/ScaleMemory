using UnityEngine;
using System.Collections;

public class CorrectPositionIndicatorController : MonoBehaviour
{
   // public TextMesh PointsScoredText;

    public Color RightScoreColor;
    public Color WrongScorecolor;

   // public Renderer PointsScoredBackground;
	public GameObject sphere;

    public Color lineRendererStartColor;
    LineRenderer toTargetLine;
    float lineHeight = 0.3f;
    Vector3 startPos { get { return GetStartPos(); } }
   // LineRendererLogTrack lineLogTrack;

    void Awake()
    {
        toTargetLine = GetComponent<LineRenderer>();
        //lineLogTrack = GetComponent<LineRendererLogTrack>();

        //start off with them both at the start pos.
        Debug.Log("setting start pos as : " + startPos.ToString());
        toTargetLine.SetPosition(0, startPos);
        toTargetLine.SetPosition(1, startPos);

        //log start pos as the beginning and end of the line.
      //  lineLogTrack.LogPoint(startPos, 0);
      //  lineLogTrack.LogPoint(startPos, 1);
    }

    void Start()
    {
        Debug.Log("start pos: " + startPos.ToString());

    }

    // Update is called once per frame
    void Update()
    {

    }
	public void ChangeToRightColor()
	{
	//	PointsScoredBackground.material.color = RightScoreColor;
		//sphere.GetComponent<Renderer> ().material.color = RightScoreColor;
	}

	public void ChangeToWrongColor()
	{
		//PointsScoredBackground.material.color = WrongScorecolor;
		//sphere.GetComponent<Renderer> ().material.color = WrongScorecolor;
	}
    public void SetLineTarget(Vector3 targetPos, Color targetColor)
    {

        if (toTargetLine == null)
        {
            toTargetLine = GetComponent<LineRenderer>();
        }

        Vector3 endPos = new Vector3(targetPos.x, transform.position.y + lineHeight, targetPos.z); //use the current object's height
        toTargetLine.SetPosition(0, startPos);
        toTargetLine.SetPosition(1, endPos);

        //log whenever the points change/are set.
       // lineLogTrack.LogPoint(startPos, 0);
      //  lineLogTrack.LogPoint(endPos, 1);


        toTargetLine.SetColors(lineRendererStartColor, targetColor);
        //log line renderer colors
        //lineLogTrack.LogColors(lineRendererStartColor, targetColor);

    }

    Vector3 GetStartPos()
    {
        return new Vector3(transform.position.x, transform.position.y + lineHeight, transform.position.z);
    }
    /*
    public void SetPointsText(int points)
    {
        if (points <= 0)
        {
            PointsScoredText.color = WrongScorecolor;
        }
        else
        {
            PointsScoredText.color = RightScoreColor;
        }

        PointsScoredText.text = "";
        if (points < 0)
        {
            PointsScoredText.text = points.ToString(); // '-' sign added automatically, as it's a negative number
        }
        else if (points >= 0)
        {
            PointsScoredText.text = "+" + points;
        }
        PointsScoredText.text = PointsScoredText.text;
    }
    */
}