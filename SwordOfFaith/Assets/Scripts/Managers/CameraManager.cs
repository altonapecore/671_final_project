using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    #region Variables & Inspector Settings
    public Camera playerOneCamera, playerTwoCamera;
    public ObjectiveManager objectiveManager;
    public CameraViewMode cameraViewMode = CameraViewMode.Split;
    [ConditionalHide("isSplit", true, true)]
    public FullCameraView fullCameraView = FullCameraView.PlayerOneView;
    public GameObject camerasCenterPoint;
    public GameObject uiCanvas;
    public GameObject WinCamera;

    #region Stored Values
    [HideInInspector]
    public static CameraManager instance; //Singleton
    [HideInInspector]
    public bool isSplit = true;
    public enum CameraViewMode { Split, Full }
    public enum FullCameraView { PlayerOneView, PlayerTwoView }
    #endregion
    #endregion

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

    /// <summary>
    /// Make sure user has filled in correct fields
    /// </summary>
    private void Start()
    {
        if(playerOneCamera == null || playerTwoCamera == null)
        {
            Debug.LogError("[ERROR] Camera Manager is missing a camera");
        }

        if(GameVars.instance)
        {
            //GameVars.instance.audioManager.PlayMusic("Level1Theme");
        }
    }

    /// <summary>
    /// Updates center point of camera
    /// </summary>
    private void Update()
    {
        if(camerasCenterPoint)
        {
            camerasCenterPoint.transform.position = GetCamerasMidPoint();
        }
    }

    /// <summary>
    /// Sets the positions of both cameras to a given vector3 with given offsets
    /// </summary>
    /// <param name="newPosition"></param>
    public void SetBothCamerasPosition(Vector3 newPosition, bool doLerp = false, float lerpTime = 1.0f, Vector3 playerOneCameraOffset = new Vector3(), Vector3 playerTwoCameraOffset = new Vector3())
    {
        if (!doLerp)
        {
            playerOneCamera.transform.position = newPosition + playerOneCameraOffset;
            playerTwoCamera.transform.position = newPosition + playerTwoCameraOffset;
        }
        else
        {
            StartCoroutine(LerpCamera(FullCameraView.PlayerOneView, newPosition + playerOneCameraOffset, lerpTime));
            StartCoroutine(LerpCamera(FullCameraView.PlayerTwoView, newPosition + playerTwoCameraOffset, lerpTime));
        }
    }

    /// <summary>
    /// Sets the positions of both cameras to be the midpoint between the two cameras
    /// </summary>
    /// <param name="doLerp"></param>
    /// <param name="lerpTime"></param>
    public void MoveCamerasToCenter(bool doLerp = false, float lerpTime = 1.0f)
    {
        Vector3 camerasMidPoint = GetCamerasMidPoint();

        if(!doLerp)
        {
            playerOneCamera.transform.position = camerasMidPoint;
            playerTwoCamera.transform.position = camerasMidPoint;
        }
        else
        {
            StartCoroutine(LerpCamera(FullCameraView.PlayerOneView, camerasMidPoint, lerpTime));
            StartCoroutine(LerpCamera(FullCameraView.PlayerTwoView, camerasMidPoint, lerpTime));
        }
    }

    /// <summary>
    /// Snap a given camera to the position of another
    /// </summary>
    /// <param name="cameraToSyncTo"></param>
    public void SyncCameraPositions(FullCameraView cameraToSyncTo, bool doLerp = false, float lerpTime = 1.0f)
    {
        if(cameraToSyncTo == FullCameraView.PlayerOneView)
        {
            if (!doLerp)
            {
                playerTwoCamera.transform.position = playerOneCamera.transform.position;
                playerTwoCamera.transform.rotation = playerOneCamera.transform.rotation;
            }
            else
            {
                StartCoroutine(LerpCamera(FullCameraView.PlayerTwoView, playerOneCamera.transform.position, lerpTime));
            }
        }
        else
        {
            if (!doLerp)
            {
                playerOneCamera.transform.position = playerTwoCamera.transform.position;
                playerOneCamera.transform.rotation = playerTwoCamera.transform.rotation;
            }
            else
            {
                StartCoroutine(LerpCamera(FullCameraView.PlayerOneView, playerTwoCamera.transform.position, lerpTime));
            }
        }
    }

    /// <summary>
    /// Public method to allow for easy camera mode toggling
    /// </summary>
    /// <param name="fullCameraViewToSwitch"></param>
    public void ToggleCameraMode(FullCameraView fullCameraViewToSwitch = FullCameraView.PlayerOneView)
    {
        if(cameraViewMode == CameraViewMode.Full)
        {
            SwitchToSplit();
        }
        else
        {
            SwitchToFull(fullCameraViewToSwitch);
        }
    }

    /// <summary>
    /// Switches camera mode to full view of given camera
    /// </summary>
    /// <param name="fullCameraViewToSwitch"></param>
    public void SwitchToFull(FullCameraView fullCameraViewToSwitch)
    {
        fullCameraView = fullCameraViewToSwitch;
        cameraViewMode = CameraViewMode.Full;
        uiCanvas.SetActive(false);
        UpdateCameras();
    }

    /// <summary>
    /// Switches camera mode to split dividing each camera view in half
    /// </summary>
    public void SwitchToSplit()
    {
        cameraViewMode = CameraViewMode.Split;
        uiCanvas.SetActive(true);
        UpdateCameras();
    }

    /// <summary>
    /// Updates the current data of each camera depending on the view mode
    /// </summary>
    private void UpdateCameras()
    {
        if(cameraViewMode == CameraViewMode.Full)
        {
            if(fullCameraView == FullCameraView.PlayerOneView)
            {
                playerOneCamera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
                playerOneCamera.gameObject.SetActive(true);

                playerTwoCamera.gameObject.SetActive(false);
            }
            else
            {
                playerTwoCamera.rect = new Rect(0.0f, 0.0f, 1.0f, 1.0f);
                playerTwoCamera.gameObject.SetActive(true);

                playerOneCamera.gameObject.SetActive(false);
            }
        }
        else
        {
            playerOneCamera.rect = new Rect(0.0f, 0.0f, 0.5f, 1.0f);
            playerTwoCamera.rect = new Rect(0.5f, 0.0f, 0.5f, 1.0f);
            playerOneCamera.gameObject.SetActive(true);
            playerTwoCamera.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Given a camera and a destination, the camera will be lerped to the destination over a given time
    /// </summary>
    /// <param name="cameraViewToLerp"></param>
    /// <param name="destination"></param>
    /// <param name="lerpTime"></param>
    /// <returns></returns>
    private IEnumerator LerpCamera(FullCameraView cameraViewToLerp, Vector3 destination, float lerpTime)
    {
        GameObject cameraToLerp;
        float t = 0;

        if(cameraViewToLerp == FullCameraView.PlayerOneView)
        {
            cameraToLerp = playerOneCamera.gameObject;
        }
        else
        {
            cameraToLerp = playerTwoCamera.gameObject;
        }

        while ((destination-cameraToLerp.transform.position).magnitude > 0.01)
        {
            t += Time.deltaTime / lerpTime;
            cameraToLerp.transform.position = Vector3.Lerp(cameraToLerp.transform.position, destination, t);
            yield return null;
        }

        cameraToLerp.transform.position = destination;
    }

    /// <summary>
    /// Returns the midpoint of the cameras
    /// </summary>
    /// <returns></returns>
    private Vector3 GetCamerasMidPoint()
    {
        return (playerOneCamera.transform.position + playerTwoCamera.transform.position) / 2;
    }

    /// <summary>
    /// Conditional Hide Information
    /// </summary>
    private void OnValidate()
    {
        if(cameraViewMode == CameraViewMode.Full && isSplit)
        {
            isSplit = false;
        }
        else if (cameraViewMode == CameraViewMode.Split && !isSplit)
        {
            isSplit = true;
        }
    }

    /// <summary>
    /// Debug Draw Lines
    /// </summary>
    private void OnDrawGizmos()
    {
        //Up Vector
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(playerOneCamera.transform.position, playerOneCamera.transform.position + (playerOneCamera.transform.up * 2));
        Gizmos.DrawLine(playerTwoCamera.transform.position, playerTwoCamera.transform.position + (playerTwoCamera.transform.up * 2));

        //Up Vector
        Gizmos.color = Color.red;
        Gizmos.DrawLine(playerOneCamera.transform.position, playerOneCamera.transform.position + (playerOneCamera.transform.forward * 25));
        Gizmos.DrawLine(playerTwoCamera.transform.position, playerTwoCamera.transform.position + (playerTwoCamera.transform.forward * 25));
    }
}
