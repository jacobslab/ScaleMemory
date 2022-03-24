using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ElememTestRunner : MonoBehaviour
{
#if !UNITY_WEBGL

    public ElememInterface interfaceManager;
    public CanvasGroup connectingPanel;
    public Text connectionText;

    public TCPServer tcpServer;

    private static ElememTestRunner _instance;
    public static ElememTestRunner Instance
    {
        get
        {
            return _instance;
        }
    }


    void Awake()
    {
        if (_instance != null)
        {
            UnityEngine.Debug.Log("Instance already exists!");
            return;
        }
        _instance = this;

    }


    void Start()
    {
        connectingPanel.alpha = 1f;
        StartCoroutine(RunTest());
    }

    public IEnumerator RunTest()
    {

        yield return StartCoroutine(interfaceManager.BeginNewSession());
        DisplayStatusText("Completed Elemem connection, invoking HEARTBEAT at a fixed interval. \n Press ESC to exit application");


        yield return StartCoroutine(UsefulFunctions.WaitForExitButton());

        Application.Quit();

        yield return null;

    }

    public void DisplayStatusText(string statusStr)
    {
        connectionText.text = statusStr;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

#endif
}
