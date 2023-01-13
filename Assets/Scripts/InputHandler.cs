using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    public Experiment exp { get { return Experiment.Instance; } }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {


        /*
         * INPUT HANDLER
        */

        if ((exp.beginScreenSelect != 0) && !((exp.beginScreenSelect == -1) && (exp.beginPracticeSelect == 0)))
        {
            if (exp.currentStage == Experiment.TaskStage.SpatialRetrieval)
            {
                if (Input.GetKey(KeyCode.UpArrow))
                {
                    StartCoroutine(exp.player.GetComponent<CarMover>().SetMovementDirection(CarMover.MovementDirection.Forward));

                }
                else if (Input.GetKey(KeyCode.DownArrow))
                {
                    StartCoroutine(exp.player.GetComponent<CarMover>().SetMovementDirection(CarMover.MovementDirection.Reverse));

                }

            }
        }
        if (exp.uiController.showInstructions)
        {
            if ((exp.beginScreenSelect == 0) ||
                ((exp.beginScreenSelect == -1) && (exp.beginPracticeSelect == 0)))
            {
                if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    exp.uiController.PerformUIPageChange(UIController.OptionSelection.Left);
                }
                if (Input.GetKeyDown(KeyCode.Alpha7))
                {
                    exp.uiController.PerformUIPageChange(UIController.OptionSelection.Right);
                }
            }
            else {
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    exp.uiController.PerformUIPageChange(UIController.OptionSelection.Left);
                }
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    exp.uiController.PerformUIPageChange(UIController.OptionSelection.Right);

                }
            }
        }

        if (exp.uiController.Loop2Instructions)
        {
            if ((exp.beginScreenSelect == 0) ||
                ((exp.beginScreenSelect == -1) && (exp.beginPracticeSelect == 0)))
            {
                if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    exp.uiController.PerformUILoopSetChange(UIController.OptionSelection.Left);
                }
                if (Input.GetKeyDown(KeyCode.Alpha7))
                {
                    exp.uiController.PerformUILoopSetChange(UIController.OptionSelection.Right);

                }
            }
            else
            {
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    exp.uiController.PerformUILoopSetChange(UIController.OptionSelection.Left);
                }
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    exp.uiController.PerformUILoopSetChange(UIController.OptionSelection.Right);

                }
            }
        }

        if (exp.CanSelectUI())
        {
            if ((exp.beginScreenSelect == 0) ||
                ((exp.beginScreenSelect == -1) && (exp.beginPracticeSelect == 0)))
            {
                if (Input.GetKeyDown(KeyCode.Alpha6))
                {
                    exp.uiController.PerformSelection(UIController.OptionSelection.Left);
                }
                if (Input.GetKeyDown(KeyCode.Alpha7))
                {
                    exp.uiController.PerformSelection(UIController.OptionSelection.Right);
                }
            }
            else {
                if (Input.GetKeyDown(KeyCode.LeftArrow))
                {
                    exp.uiController.PerformSelection(UIController.OptionSelection.Left);
                }
                if (Input.GetKeyDown(KeyCode.RightArrow))
                {
                    exp.uiController.PerformSelection(UIController.OptionSelection.Right);
                }
            }

        }
        //handle pause
        if ((Input.GetButtonDown("Pause")) && (exp.skipPause == false))
            StartCoroutine(TogglePause());

    }

    public IEnumerator TogglePause()
    {
        //flip it
        Experiment.isPaused = !Experiment.isPaused;

        Time.timeScale = ((Experiment.isPaused) ? 0f : 1f);
        exp.uiController.ShowPauseScreen(Experiment.isPaused);

        yield return null;
    }

}
