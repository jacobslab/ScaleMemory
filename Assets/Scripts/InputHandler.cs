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
        if (exp.uiController.showInstructions)
        {

            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                exp.uiController.PerformUIPageChange(UIController.OptionSelection.Left);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                exp.uiController.PerformUIPageChange(UIController.OptionSelection.Right);

            }
        }

        if (exp.CanSelectUI())
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
               exp.uiController.PerformSelection(UIController.OptionSelection.Left);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                exp.uiController.PerformSelection(UIController.OptionSelection.Right);

            }

        }
        //handle pause
        if (Input.GetButtonDown("Pause"))
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
