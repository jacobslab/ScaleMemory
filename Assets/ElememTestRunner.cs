using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class ElememTestRunner : MonoBehaviour
{
#if !UNITY_WEBGL

    public InterfaceManager interfaceManager;
    public CanvasGroup connectingPanel;
    public Text connectionText;
    public CanvasGroup successPanel;
    public TCPServer tcpServer;
    // Start is called before the first frame update
    void Start()
    {
        connectingPanel.alpha = 1f;
        successPanel.alpha = 0f;
        StartCoroutine(RunTest());
    }

    public IEnumerator RunTest()
    {
        connectingPanel.alpha = 1f;

        interfaceManager.Do(new EventBase(interfaceManager.LaunchExperiment));
        while (!tcpServer.isConnected)
        {
            yield return 0;
        }
        connectionText.text = "Waiting for server to send START...";
        while (!tcpServer.canStartGame)
        {
            yield return 0;
        }
        connectingPanel.alpha = 0f;
        successPanel.alpha = 1f;
        while (!Input.GetKeyDown(KeyCode.Escape))
        {

            yield return 0;
        }
        Application.Quit();

        yield return null;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

#endif
}
