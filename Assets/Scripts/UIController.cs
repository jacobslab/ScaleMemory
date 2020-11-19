using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class UIController : MonoBehaviour {


	private float prevLapTime=0f;
	public CanvasGroup retrievalTextPanel;
	public CanvasGroup targetTextPanel;
	public Text zRetrievalText;
	public Text mRetrievalText;
	public Text itemName;

	//item screening
	public CanvasGroup itemScreeningPanel;

	//track screening
	public CanvasGroup trackScreeningPanel;

	//encoding panel
	public CanvasGroup encodingPanel;

	//retrieval panel
	public CanvasGroup retrievalPanel;
	public CanvasGroup verbalRetrievalPanel;
	public Text itemOneName;
	public Text itemTwoName;
	public CanvasGroup spatialRetrievalFeedbackPanel;

	//fixation panel
	public CanvasGroup fixationPanel;
	public CanvasGroup fixationCross;

	//blackrock connection
	public CanvasGroup blackrockConnectionPanel;
	public Text connectionText;

	//ip entry
	public CanvasGroup ipEntryPanel;
	public InputField ipAddrInput;

	//lap time
	public CanvasGroup lapTimePanel;
	public Text currentLapTimeText;
	public Text bestLapTimeText;
	public Text timeSplitText;

	//arrow
	public CanvasGroup leftTurnArrow;
	public CanvasGroup rightTurnArrow;

	//crash notification
	public CanvasGroup crashNotification;

	public CanvasGroup verbalInstruction;

	//black screen
	public CanvasGroup blackScreen;

	//end session
	public CanvasGroup endSessionPanel;

	// info text
	public TextMeshPro infoText;
	

	// Use this for initialization
	void Start () {
		targetTextPanel.alpha = 0f;
		retrievalTextPanel.alpha = 0f;
	}
	
	// Update is called once per frame
	void Update () {
	}

	

}
