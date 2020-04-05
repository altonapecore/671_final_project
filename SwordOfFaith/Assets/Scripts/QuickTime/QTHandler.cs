using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QTHandler : MonoBehaviour
{
    #region Inspector Settings & Variables
    [Header("Settings")]
    public List<QTEvent> quickTimeEvents;
    public QTStatus quickTimeStatus = QTStatus.Nonactive;
    public bool randomNumberOfEvents = true;
    [ConditionalHide("randomNumberOfEvents", true, true)]
    public int qtEventsTobeDone = 1;

    [Header("Data")]
    [Space(10)]
    public char currentNeededKey; //The current key the user must press to complete the QuickTimeEvent
    public bool pressRhythmKeyNow; //Enables when it's time to hit the specified key
    public int currentIndexReached; //The index in the array the player has reached

    #region Stored Values
    [HideInInspector]
    public bool hasPlayerHitNeededKey; 
    [HideInInspector]
    public ObjectiveInfo objectiveInfo;
    [HideInInspector]
    public PlayerUIManager currentPlayerUI;
    [HideInInspector]
    public GameObject playerSide;

    public enum QTStatus { Nonactive, Active, Completed, Failed }

    List<QTEvent> openList, closedList;

    //KeyCombo && Rhythm
    int totalInputs; //Total Number of inputs in this QTEvent
    int[] inputListPosition; //Array of list positions to pull from for the event

    //Mash
    int totalInputsNeeded; //Total Number of inputs needed to complete this QTEvent
    char mashKey; //Key that needs to be hit to mash
    bool isHoldingKey; //checks if the player is currently holding a specified key
    string previousInputString = ""; //Previously entered inputs
    #endregion
    #endregion

    /// <summary>
    /// Validates there is a quickTimeEvent on the this handler
    /// </summary>
    void Start()
    {
        if(quickTimeEvents == null || quickTimeEvents.Count == 0)
        {
            Debug.LogError("ERROR - QTManager on " + gameObject.name + " has no QTEvent");
        }
        else
        {
            SetUpQuickTimeEvent(true);
        }
    }

    /// <summary>
    /// Sets up the needed values for a QuickTimeEvent
    /// </summary>
    public void SetUpQuickTimeEvent(bool hardReset)
    {
        quickTimeStatus = QTStatus.Nonactive;

        if (hardReset)
        {
            openList = new List<QTEvent>();
            closedList = new List<QTEvent>();

            if (randomNumberOfEvents)
            {
                qtEventsTobeDone = Random.Range(1, quickTimeEvents.Count);
            }

            //Setup openlist
            for (int i = 0; i <= qtEventsTobeDone; i++)
            {
                openList.Add(quickTimeEvents[Random.Range(0, quickTimeEvents.Count)]);
            }
        }

        if (openList[0].quickTimeType == QTEvent.QTType.KeyCombo || openList[0].quickTimeType == QTEvent.QTType.Rhythm)
        {
            totalInputs = Random.Range(openList[0].MIN_TOTAL_INPUTS, openList[0].MAX_TOTAL_INPUTS + 1);
            inputListPosition = new int[totalInputs];

            for (int i = 0; i < inputListPosition.Length; i++)
            {
                inputListPosition[i] = Random.Range(0, openList[0].quickTimePossibleKeys.Count);
            }
        }
        else if (openList[0].quickTimeType == QTEvent.QTType.Mash)
        {
            totalInputsNeeded = Random.Range(openList[0].MIN_POSSIBLE_NEEDED_KEYHITS, openList[0].MAX_POSSIBLE_NEEDED_KEYHITS + 1);
            mashKey = openList[0].quickTimePossibleKeys[Random.Range(0, openList[0].quickTimePossibleKeys.Count)];
        }
    }

    /// <summary>
    /// Toggles the player's ability to move
    /// </summary>
    /// <param name="newSideStatus"></param>
    public void PlayerSideStatusToggle(PlayerCamera.PlayerSideStatus newSideStatus)
    {
        if (objectiveInfo.objectiveSide == ObjectiveManager.ObjectiveSide.Left)
        {
            CameraManager.instance.playerOneCamera.GetComponent<PlayerCamera>().sideStatus = newSideStatus;
        }
        else if (objectiveInfo.objectiveSide == ObjectiveManager.ObjectiveSide.Right)
        {
            CameraManager.instance.playerTwoCamera.GetComponent<PlayerCamera>().sideStatus = newSideStatus;
        }
    }

    /// <summary>
    /// Starts this QuickTimeEvent
    /// </summary>
    public void BeginQTEvent()
    {
        //GameVars.instance.audioManager.PlaySFX(ObjectiveManager.instance.generatorInteract, 0.5f, playerSide.transform.position);
        ResetQTEvent(true);
        PlayerSideStatusToggle(PlayerCamera.PlayerSideStatus.Paused);
        quickTimeStatus = QTStatus.Active;
        if (openList[0].quickTimeType == QTEvent.QTType.KeyCombo)
        {
            //Debug.Log("Starting KeyCombo...");
            StartCoroutine(QuickTimeEventKeyCombo());
            StartCoroutine(QTEventTimer(openList[0].DELAY_BEFORE_QT_FAIL, true));
        }
        else if(openList[0].quickTimeType == QTEvent.QTType.Mash)
        {
            //Debug.Log("Starting Mash...");
            StartCoroutine(QuickTimeEventMash());
            StartCoroutine(QTEventTimer(openList[0].DELAY_BEFORE_QT_FAIL, true));
        }
        else if (openList[0].quickTimeType == QTEvent.QTType.Rhythm)
        {
            //Debug.Log("Starting Rhythm...");
            StartCoroutine(QuickTimeEventRhythm());
        }
    }

    /// <summary>
    /// Used to fail a QuickTimeEvent
    /// </summary>
    public IEnumerator FailedQTEvent()
    {
        if (quickTimeStatus != QTStatus.Completed)
        {
            quickTimeStatus = QTStatus.Failed;
            currentPlayerUI.ChangeInstructions("Objective Failed\nTry Again");

            //GameVars.instance.audioManager.PlaySFX(ObjectiveManager.instance.generatorFailed, 0.5f, playerSide.transform.position);

            yield return new WaitForSeconds(2f);
            currentPlayerUI.ResetTextLayers();
            PlayerSideStatusToggle(PlayerCamera.PlayerSideStatus.Active);
            ResetQTEvent(true);

            objectiveInfo.isTriggered = false;
            quickTimeStatus = QTStatus.Nonactive;
        }
    }

    /// <summary>
    /// Resets all values of the QuickTimeEvent
    /// </summary>
    public void ResetQTEvent(bool stopCoroutines)
    {
        if (stopCoroutines)
        {
            StopAllCoroutines();
        }

        currentIndexReached = 0;
        currentNeededKey = '\0';
        mashKey = '\0';
        hasPlayerHitNeededKey = false;
        pressRhythmKeyNow = false;
        isHoldingKey = false;
        previousInputString = "";

        SetUpQuickTimeEvent(false);
    }

    /// <summary>
    /// Handles timers for QuickTimeEvents
    /// </summary>
    /// <param name="time"></param>
    /// <param name="isTotalTimer"></param>
    /// <returns></returns>
    public IEnumerator QTEventTimer(float time, bool isTotalTimer)
    {
        yield return new WaitForSeconds(time);

        if (isTotalTimer)
        {
            if (quickTimeStatus != QTStatus.Completed)
            {
                StartCoroutine(FailedQTEvent());
            }
        }
        else
        {
            StartCoroutine(FailedQTEvent());
        }
    }

    /// <summary>
    /// Handles the different types of input expected for a QuickTimeEvent
    /// </summary>
    /// <returns></returns>
    public IEnumerator QTInputDetector()
    {
        if (openList[0].quickTimeType == QTEvent.QTType.KeyCombo)
        {
            while (!hasPlayerHitNeededKey)
            {
                if (quickTimeStatus == QTStatus.Active)
                {
                    if (Input.inputString.Contains(currentNeededKey.ToString()))
                    {
                        hasPlayerHitNeededKey = true;
                    }
                }
                yield return null;
            }
        }
        else if(openList[0].quickTimeType == QTEvent.QTType.Mash)
        {
            while (!hasPlayerHitNeededKey)
            {
                if (quickTimeStatus == QTStatus.Active)
                {
                    if (Input.inputString.Contains(currentNeededKey.ToString()) && !previousInputString.Contains(currentNeededKey.ToString()))
                    {
                        if (isHoldingKey == false && hasPlayerHitNeededKey == false)
                        {
                            isHoldingKey = true;
                            hasPlayerHitNeededKey = true;
                        }
                    }
                    else if (!Input.inputString.Contains(currentNeededKey.ToString()) && !previousInputString.Contains(currentNeededKey.ToString()))
                    {
                        if (isHoldingKey == true)
                        {
                            isHoldingKey = false;
                        }
                    }
                    previousInputString = Input.inputString;
                }
                yield return null;
            }
        }
        else if (openList[0].quickTimeType == QTEvent.QTType.Rhythm)
        {
            while (!hasPlayerHitNeededKey)
            {
                if (quickTimeStatus == QTStatus.Active)
                {
                    if (Input.inputString.Contains(currentNeededKey.ToString()))
                    {
                        //Debug.Log("pressRhythmKeyNow: " + pressRhythmKeyNow + " | KeyNeeded: "+ currentNeededKey.ToString());

                        if (pressRhythmKeyNow)
                        {
                            hasPlayerHitNeededKey = true;
                        }
                        else
                        {
                            Debug.Log("pressRhythmKeyNow NOT active");
                            StartCoroutine(FailedQTEvent());
                        }
                    }
                }
                yield return null;
            }
        }
    }

    /// <summary>
    /// Logic for the Rhythm event
    /// </summary>
    /// <returns></returns>
    public IEnumerator QuickTimeEventRhythm()
    {
        //Debug.Log("RHYTHM");
        for (int i = 0; i < inputListPosition.Length; i++)
        {
            if (quickTimeStatus == QTStatus.Active)
            {
                yield return new WaitForSeconds(0.5f);

                hasPlayerHitNeededKey = false;
                pressRhythmKeyNow = false;

                currentNeededKey = openList[0].quickTimePossibleKeys[inputListPosition[i]];
                currentPlayerUI.ChangeInstructions("Wait To Hit " + currentNeededKey + "...");
                Coroutine rhythmKeyInput = StartCoroutine(QTInputDetector());

                yield return new WaitForSeconds(openList[0].INPUT_KEY_DELAY);

                if (quickTimeStatus == QTStatus.Active)
                {
                    pressRhythmKeyNow = true;
                    Coroutine inputTimer = StartCoroutine(QTEventTimer(openList[0].MAX_DELAY_BEFORE_FAIL, false));
                    currentPlayerUI.ChangeInstructions("Wait To Hit " + currentNeededKey + "..." + "\nNOW");

                    yield return new WaitUntil(() => hasPlayerHitNeededKey == true);
                    //GameVars.instance.audioManager.PlaySFX(ObjectiveManager.instance.generatorInteract, 0.2f, playerSide.transform.position);
                    currentPlayerUI.ChangeInstructions("Nailed It!");
                    StopCoroutine(inputTimer);
                    StopCoroutine(rhythmKeyInput);

                    if (quickTimeStatus == QTStatus.Active)
                    {
                        currentIndexReached++;
                    }
                }
            }
        }

        if (currentIndexReached >= inputListPosition.Length && quickTimeStatus != QTStatus.Failed)
        {
            StartCoroutine(CompleteQTEvent());
        }
    }

    /// <summary>
    /// Logic for the Mash event
    /// </summary>
    /// <returns></returns>
    public IEnumerator QuickTimeEventMash()
    {
        //Debug.Log("MASH");
        currentNeededKey = mashKey;
        currentPlayerUI.ChangeInstructions("MASH " + currentNeededKey.ToString());

        for (int i = 0; i < totalInputsNeeded; i++)
        {
            hasPlayerHitNeededKey = false;

            Coroutine input = StartCoroutine(QTInputDetector());
            yield return new WaitUntil(() => hasPlayerHitNeededKey == true);
            //GameVars.instance.audioManager.PlaySFX(ObjectiveManager.instance.generatorInteract, 0.2f, playerSide.transform.position);
            StopCoroutine(input);

            currentIndexReached++;
            currentPlayerUI.ChangeInstructions("MASH " + currentNeededKey.ToString() + "\n" + currentIndexReached + "/" + totalInputsNeeded);
            yield return new WaitForSeconds(0.1f);
        }

        //yield return new WaitUntil(() => currentIndexReached == totalInputsNeeded);

        if (currentIndexReached >= totalInputsNeeded && quickTimeStatus != QTStatus.Failed)
        {
            StartCoroutine(CompleteQTEvent());
        }
    }

    /// <summary>
    /// Logic for the KeyCombo event
    /// </summary>
    /// <returns></returns>
    public IEnumerator QuickTimeEventKeyCombo()
    {
        //Debug.Log("KEYCOMBO");
        for (int i = 0; i < inputListPosition.Length; i++)
        {
            hasPlayerHitNeededKey = false;

            currentNeededKey = openList[0].quickTimePossibleKeys[inputListPosition[i]];
            currentPlayerUI.ChangeInstructions("Hit "+ currentNeededKey);

            Coroutine input = StartCoroutine(QTInputDetector());

            yield return new WaitUntil(() => hasPlayerHitNeededKey == true);
            //GameVars.instance.audioManager.PlaySFX(ObjectiveManager.instance.generatorInteract, 0.2f, playerSide.transform.position);
            StopCoroutine(input);

            currentIndexReached++;
        }

        if (currentIndexReached == inputListPosition.Length && quickTimeStatus != QTStatus.Failed)
        {
            StartCoroutine(CompleteQTEvent());
        }
    }

    /// <summary>
    /// Completes this QTEvent either partically or totally
    /// </summary>
    /// <returns></returns>
    private IEnumerator CompleteQTEvent()
    {
        currentPlayerUI.ChangeInstructions("COMPLETE");
        quickTimeStatus = QTStatus.Completed;

        yield return new WaitForSeconds(2f);
        currentPlayerUI.ResetTextLayers();

        closedList.Add(openList[0]);
        openList.RemoveAt(0);


        if (openList.Count == 0)
        {
            SetUpQuickTimeEvent(true);
            objectiveInfo.CompleteObjective();
        }
        else
        {          
            BeginQTEvent();
        }

    }
}

