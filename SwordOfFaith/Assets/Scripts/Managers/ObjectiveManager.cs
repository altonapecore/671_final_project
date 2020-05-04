using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class ObjectiveManager : MonoBehaviour
{
    [Header("Settings")]
    public CameraManager cameraManager;
    public ObjectiveSide currentObjectiveSide = ObjectiveSide.Left;
    public int totalScoreBeforeFinalRush = 6;

    [Header("Sound")]
    [Space(10)]
    public AudioClip generatorComplete;
    public AudioClip generatorFailed;
    public AudioClip generatorInteract;

    [Header("References")]
    [Space(10)]
    public GameObject leftFinalDoor;
    public GameObject rightFinalDoor;
    public List<GameObject> leftObjectives;
    public List<GameObject> rightObjectives;

    [HideInInspector]
    public GameObject leftSideObjective;
    [HideInInspector]
    public GameObject rightSideObjective;
    [HideInInspector]
    public int currentScore;
    [HideInInspector]
    public static ObjectiveManager instance; //Singleton
    [HideInInspector]
    public bool isFinalLeftComplete, isFinalRightComplete;
    public enum ObjectiveSide { Left, Right, Both, None}

    private PlayerCamera playerOneCamera, playerTwoCamera;

    [FMODUnity.EventRef]
    public string gameWon;

    /// <summary>
    /// Define Singleton
    /// </summary>
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        if (ListsInit()) //Verify everything is set up properly
        {
            if (!playerOneCamera)
            {
                playerOneCamera = cameraManager.playerOneCamera.gameObject.GetComponent<PlayerCamera>();
            }

            if (!playerTwoCamera)
            {
                playerTwoCamera = cameraManager.playerTwoCamera.gameObject.GetComponent<PlayerCamera>();
            }

            //Random Side Start
            StartObjectives(ObjectiveSide.Both);
        }
    }

    /// <summary>
    /// Updates currentObjectiveSide and enables that side's objective
    /// </summary>
    /// <param name="side"></param>
    public void ActivateObjective(ObjectiveSide side)
    {
        if(side == ObjectiveSide.Left)
        {
            currentObjectiveSide = ObjectiveSide.Left;
            PickRandomObjectiveObject(ObjectiveSide.Left);
            ToggleObjective(leftSideObjective, true);

            ToggleObjective(rightSideObjective, false);

            playerOneCamera.playerUI.ChangeInstructions("Objective\nActive");
        }
        else if (side == ObjectiveSide.Right)
        {
            currentObjectiveSide = ObjectiveSide.Right;
            PickRandomObjectiveObject(ObjectiveSide.Right);
            ToggleObjective(rightSideObjective, true);

            ToggleObjective(leftSideObjective, false);

            playerTwoCamera.playerUI.ChangeInstructions("Objective\nActive");
        }
        else if (side == ObjectiveSide.Both)
        {
            currentObjectiveSide = ObjectiveSide.Both;
            //PickRandomObjectiveObject(ObjectiveSide.Left);
            ToggleObjective(leftSideObjective, true);

            //PickRandomObjectiveObject(ObjectiveSide.Right);
            ToggleObjective(rightSideObjective, true);
        }
        else if (side == ObjectiveSide.None)
        {
            currentObjectiveSide = ObjectiveSide.None;
            FinishObjective(ObjectiveSide.Both, false, false, false);

            ToggleObjective(leftSideObjective, false);
            ToggleObjective(rightSideObjective, false);
        }
    }

    /// <summary>
    /// Removes an objective from a given side and can increment the score, cause fanfare, and automatically switch Objective sides
    /// </summary>
    /// <param name="side"></param>
    /// <param name="incrementScore"></param>
    /// <param name="automaticallySwitchSides"></param>
    public void FinishObjective(ObjectiveSide side, bool incrementScore, bool playFanfare, bool automaticallySwitchSides)
    {
        if (side == ObjectiveSide.Left)
        {
            if (leftSideObjective)
            {
                playerOneCamera.playerUI.currentActiveObjective = null;
            }

            if (playFanfare)
            {
                //GameVars.instance.audioManager.PlaySFX(generatorComplete, 0.5f, playerOneCamera.player.transform.position);
            }
        }
        else if (side == ObjectiveSide.Right)
        {
            if (rightSideObjective)
            {
                playerTwoCamera.playerUI.currentActiveObjective = null;
            }

            if (playFanfare)
            {
                //GameVars.instance.audioManager.PlaySFX(generatorComplete, 0.5f, playerTwoCamera.player.transform.position);
            }
        }
        else if (side == ObjectiveSide.Both)
        {
            if (leftSideObjective)
            {
                playerOneCamera.playerUI.currentActiveObjective = null;
            }

            if (rightSideObjective)
            {
                playerTwoCamera.playerUI.currentActiveObjective = null;
            }

            if (playFanfare)
            {
                //GameVars.instance.audioManager.PlaySFX(generatorComplete, 0.3f, playerOneCamera.player.transform.position);
                //GameVars.instance.audioManager.PlaySFX(generatorComplete, 0.3f, playerTwoCamera.player.transform.position);
            }
        }

        if(incrementScore)
        {
            currentScore++;                
        }

        if (automaticallySwitchSides)
        {
            if (currentScore < totalScoreBeforeFinalRush)
            {
                if (currentObjectiveSide == ObjectiveSide.Left)
                {
                    ActivateObjective(ObjectiveSide.Right);
                }
                else if (currentObjectiveSide == ObjectiveSide.Right)
                {
                    ActivateObjective(ObjectiveSide.Left);
                }
            }
            else
            {
                if (!isFinalLeftComplete && !isFinalRightComplete)
                {
                    ActivateObjective(ObjectiveSide.None);
                    SetObjectiveGameObject(leftFinalDoor, ObjectiveSide.Left);
                    SetObjectiveGameObject(rightFinalDoor, ObjectiveSide.Right);
                    ActivateObjective(ObjectiveSide.Both);

                    playerOneCamera.playerUI.ChangeInstructions("Get To The Exit!");
                    playerTwoCamera.playerUI.ChangeInstructions("Get To The Exit!");
                }
                else
                {
                    if(isFinalLeftComplete)
                    {
                        //ToggleObjective(leftFinalDoor, false);
                        playerOneCamera.playerUI.ResetTextLayers();
                        playerOneCamera.GetComponent<PlayerCamera>().sideStatus = PlayerCamera.PlayerSideStatus.Paused;
                        AIManager.Instance.KillSideEnemies(AIManager.Instance.leftSideEnemies);
                    }

                    if (isFinalRightComplete)
                    {
                        //ToggleObjective(rightFinalDoor, false);
                        playerTwoCamera.playerUI.ResetTextLayers();
                        playerTwoCamera.GetComponent<PlayerCamera>().sideStatus = PlayerCamera.PlayerSideStatus.Paused;
                        AIManager.Instance.KillSideEnemies(AIManager.Instance.rightSideEnemies);
                    }

                    if(isFinalLeftComplete && isFinalRightComplete)
                    {
                        StartCoroutine(PlayWinScene());
                    }
                }
            }
        }
    }

    private IEnumerator PlayWinScene()
    {
        cameraManager.SwitchToFull(CameraManager.FullCameraView.PlayerOneView);
        leftFinalDoor.transform.GetChild(0).GetComponent<Animator>().SetBool("Open", true);
        rightFinalDoor.transform.GetChild(0).GetComponent<Animator>().SetBool("Open", true);
        yield return new WaitForSeconds(3);
        playerOneCamera.gameObject.SetActive(false);
        playerTwoCamera.gameObject.SetActive(false);
        CameraManager.instance.WinCamera.SetActive(true);
        CameraManager.instance.WinCamera.GetComponent<Animator>().SetBool("Move", true);
        FMODUnity.RuntimeManager.PlayOneShot(gameWon);
        yield return new WaitForSeconds(8);
        SceneManager.LoadScene("Main Menu");
    }

    /// <summary>
    /// Starts the Objective gameplay at a given side or random
    /// </summary>
    /// <param name="side"></param>
    private void StartObjectives(ObjectiveSide side) //Both = Random Side
    {
        if (side == ObjectiveSide.Left)
        {
            ActivateObjective(ObjectiveSide.Left);
        }
        else if (side == ObjectiveSide.Right)
        {
            ActivateObjective(ObjectiveSide.Right);
        }
        else if (side == ObjectiveSide.Both)
        {
            int randomSide = Random.Range(0, 2);

            if(randomSide == 0)
            {
                ActivateObjective(ObjectiveSide.Left);
            }
            else
            {
                ActivateObjective(ObjectiveSide.Right);
            }
        }
    }

    /// <summary>
    /// Disables all objectives
    /// </summary>
    /// <returns></returns>
    private bool ListsInit()
    {
        if(leftObjectives != null && rightObjectives != null)
        {
            if(leftObjectives.Count > 0)
            {
                foreach(GameObject objective in leftObjectives)
                {
                    objective.SetActive(false);
                }
            }
            else
            {
                Debug.LogError("ERROR - Left Objective List has no members");
                return false;
            }

            if (rightObjectives.Count > 0)
            {
                foreach (GameObject objective in rightObjectives)
                {
                    objective.SetActive(false);               
                }
            }
            else
            {
                Debug.LogError("ERROR - Right Objective List has no members");
                return false;
            }
        }
        else
        {
            Debug.LogError("ERROR - Objective Lists are null");
            return false;
        }

        if (!(leftFinalDoor && rightFinalDoor))
        {
            Debug.LogError("ERROR - Final Doors Not Assigned");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Given a side, the side's objective gameObject will be chosen at random from the list
    /// </summary>
    /// <param name="side"></param>
    private void PickRandomObjectiveObject(ObjectiveSide side)
    {
        if (side == ObjectiveSide.Left)
        {
            SetObjectiveGameObject(leftObjectives[Random.Range(0, leftObjectives.Count)], ObjectiveSide.Left);
        }
        else if (side == ObjectiveSide.Right)
        {
            SetObjectiveGameObject(rightObjectives[Random.Range(0, rightObjectives.Count)], ObjectiveSide.Right);
        }
    }

    /// <summary>
    /// Assigns an objective to a player depending on currentActiveObjective
    /// </summary>
    private void UpdateCurrentActiveObjectives()
    {
        if (currentObjectiveSide == ObjectiveSide.Left)
        {
            if (leftSideObjective)
            {
                playerOneCamera.playerUI.currentActiveObjective = leftSideObjective;
            }
            else
            {
                Debug.LogError("ERROR - (LEFT) leftSideObjective is null");
            }
        }
        else if (currentObjectiveSide == ObjectiveSide.Right)
        {
            if (rightSideObjective)
            {
                playerTwoCamera.playerUI.currentActiveObjective = rightSideObjective;
            }
            else
            {
                Debug.LogError("ERROR - (RIGHT) rightSideObjective is null");
            }
        }
        else if (currentObjectiveSide == ObjectiveSide.Both)
        {
            if (leftSideObjective)
            {
                playerOneCamera.playerUI.currentActiveObjective = leftSideObjective;
            }
            else
            {
                Debug.LogError("ERROR - (BOTH) leftSideObjective is null");
            }

            if (rightSideObjective)
            {
                playerTwoCamera.playerUI.currentActiveObjective = rightSideObjective;
            }
            else
            {
                Debug.LogError("ERROR - (BOTH) rightSideObjective is null");
            }
        }
        else if (currentObjectiveSide == ObjectiveSide.None)
        {
            //Debug.Log("No Current Active Objectives");
        }
    }

    /// <summary>
    /// Sets what gameObjective the current objective is for a given side
    /// </summary>
    /// <param name="gameObjectToSetAsObjective"></param>
    /// <param name="side"></param>
    public void SetObjectiveGameObject(GameObject gameObjectToSetAsObjective, ObjectiveSide side)
    {
        if(side == ObjectiveSide.Left)
        {
            leftSideObjective = gameObjectToSetAsObjective;
        }
        else if (side == ObjectiveSide.Right)
        {
            rightSideObjective = gameObjectToSetAsObjective;
        }
        else if (side == ObjectiveSide.Both)
        {
            leftSideObjective = gameObjectToSetAsObjective;
            rightSideObjective = gameObjectToSetAsObjective;
        }
    }

    /// <summary>
    /// Enables or Disables a given side's objective
    /// </summary>
    /// <param name="sideObjective"></param>
    /// <param name="toggle"></param>
    private void ToggleObjective(GameObject sideObjective, bool toggle)
    {
        if (sideObjective == leftSideObjective)
        {
            if (leftSideObjective)
            {
                leftSideObjective.SetActive(toggle);
            }
            else
            {
                //Debug.Log("(Toggle Left) leftSideObjective is null");
            }
        }
        else if (sideObjective == rightSideObjective)
        {
            if (rightSideObjective)
            {
                rightSideObjective.SetActive(toggle);
            }
            else
            {
                //Debug.Log("(Toggle Right) rightSideObjective is null");
            }
        }
        else
        {
            Debug.LogError("ERROR - Nonvalid side gameObject");
        }

        UpdateCurrentActiveObjectives();
    }
}
