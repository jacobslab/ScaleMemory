﻿using System.Collections;
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
	public Text itemOneName;
	public Text itemTwoName;

	//fixation panel
	public CanvasGroup fixationPanel;
	public CanvasGroup fixationCross;

	//turn decision
	public CanvasGroup leftArrowProgress;
	public CanvasGroup rightArrowProgress;
	public CanvasGroup leftArrow;
	public CanvasGroup rightArrow;
	public CanvasGroup choiceOrPanel;
	public CanvasGroup leftCorrectImagePanel;
	public CanvasGroup rightCorrectImagePanel;
	public CanvasGroup leftIncorrectImagePanel;
	public CanvasGroup rightIncorrectImagePanel;
	public CanvasGroup youChoseLeft;
	public CanvasGroup youChoseRight;
	

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


	//crash notification
	public CanvasGroup crashNotification;


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
