using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ObjectiveInfo : MonoBehaviour
{
    [Header("References")]
    public QTHandler quickTimeHandler;

    [Header("Settings")]
    [Space(10)]
    public ObjectiveManager.ObjectiveSide objectiveSide; //Only Left Or Right
    public ObjectiveType objectiveType;
    public CompletionType completionType;
    public bool isFinalDoor = false;

    [HideInInspector]
    public bool isTriggered;

    public enum ObjectiveType { QuickTime, Positional }
    public enum CompletionType { Trigger, EventBool }

    private void Start()
    {
        if (quickTimeHandler)
        {
            quickTimeHandler.objectiveInfo = this;

            if (objectiveSide == ObjectiveManager.ObjectiveSide.Left)
            {
                quickTimeHandler.currentPlayerUI = CameraManager.instance.playerOneCamera.GetComponent<PlayerCamera>().playerUI;
                quickTimeHandler.playerSide = CameraManager.instance.playerOneCamera.GetComponent<PlayerCamera>().player;
            }
            else if (objectiveSide == ObjectiveManager.ObjectiveSide.Right)
            {
                quickTimeHandler.currentPlayerUI = CameraManager.instance.playerTwoCamera.GetComponent<PlayerCamera>().playerUI;
                quickTimeHandler.playerSide = CameraManager.instance.playerTwoCamera.GetComponent<PlayerCamera>().player;
            }
        }
    }

    private void OnEnable()
    {
        isTriggered = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!isTriggered && other.gameObject.layer == 10) //Player layer
        {
            if (completionType == CompletionType.Trigger)
            {
                isTriggered = true;
                CompleteObjective();
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (!isTriggered && other.gameObject.layer == 10) //Player layer
        {
            if (quickTimeHandler && completionType == CompletionType.EventBool)
            {
                if(objectiveSide == ObjectiveManager.ObjectiveSide.Left)
                {
                    quickTimeHandler.currentPlayerUI.ChangeInstructions("Hit \"E\"  To Start");
                    if(Input.GetKeyUp(KeyCode.E) && quickTimeHandler.quickTimeStatus == QTHandler.QTStatus.Nonactive)
                    {
                        isTriggered = true;
                        quickTimeHandler.currentPlayerUI = CameraManager.instance.playerOneCamera.GetComponent<PlayerCamera>().playerUI;
                        quickTimeHandler.BeginQTEvent();
                    }
                }
                else if (objectiveSide == ObjectiveManager.ObjectiveSide.Right)
                {
                    quickTimeHandler.currentPlayerUI.ChangeInstructions("Hit \"NUMPAD_PERIOD\"  To Start");
                    if (Input.GetKeyUp(KeyCode.KeypadPeriod) && quickTimeHandler.quickTimeStatus == QTHandler.QTStatus.Nonactive)
                    {
                        isTriggered = true;
                        
                        quickTimeHandler.BeginQTEvent();
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer == 10) //Player layer
        {
            if (isTriggered)
            {
                isTriggered = false;
            }

            if (quickTimeHandler)
            {
                quickTimeHandler.currentPlayerUI.ResetTextLayers();
            }
        }
    }

    /// <summary>
    /// Completes this objective with a reaction based on settings
    /// </summary>
    public void CompleteObjective()
    {
        if(objectiveType == ObjectiveType.QuickTime)
        {
            ObjectiveManager.instance.FinishObjective(objectiveSide, true, true, true);

            if(quickTimeHandler)
            {
                quickTimeHandler.PlayerSideStatusToggle(PlayerCamera.PlayerSideStatus.Active);
            }
        }
        else if (objectiveType == ObjectiveType.Positional)
        {
            if(isFinalDoor)
            {
                if (ObjectiveManager.instance.currentObjectiveSide == ObjectiveManager.ObjectiveSide.Both)
                {
                    if (objectiveSide == ObjectiveManager.ObjectiveSide.Left)
                    {
                        ObjectiveManager.instance.isFinalLeftComplete = true;
                    }
                    else if (objectiveSide == ObjectiveManager.ObjectiveSide.Right)
                    {
                        ObjectiveManager.instance.isFinalRightComplete = true;
                    }

                    ObjectiveManager.instance.FinishObjective(objectiveSide, false, false, true);
                }
            }
            else
            {
                ObjectiveManager.instance.FinishObjective(objectiveSide, false, false, false);
            }
        }

        isTriggered = false;
    }
    private void OnDrawGizmos()
    {
        //Up Vector
        //Gizmos.color = Color.yellow;
        //Gizmos.DrawLine(transform.position, transform.position + (transform.up * 25));
    }
}
