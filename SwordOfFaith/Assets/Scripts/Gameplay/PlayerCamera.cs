using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
public class PlayerCamera : MonoBehaviour
{
    [Header("Setup")]
    public CameraManager cameraManager;
    public PlayerUIManager playerUI;
    public GameObject player;
    public PostProcessVolume postProcessVolume;
    public float greyscaleFadeSpeed = 5f;
    public PlayerSideStatus sideStatus = PlayerSideStatus.Active;
    public bool debug = false;

    private PlayerControl playerController;
    private float CAMERA_VELOCITY = 20;
    private Rigidbody rb;
    private ColorGrading colorGrading;
    private bool greyscale;
    private bool greyscaleIn = false;
    private bool greyscaleOut = false;
    public enum PlayerSideStatus { Active, Paused }

    // Start is called before the first frame update
    void Start()
    {
        if(cameraManager == null)
        {
            cameraManager = CameraManager.instance;
        }

        if(rb == null)
        {
            rb = GetComponent<Rigidbody>();
        }

        if(playerController == null)
        {
            playerController = player.GetComponent<PlayerControl>();
        }

        if (postProcessVolume && postProcessVolume.profile.HasSettings<ColorGrading>())
        {
            colorGrading = postProcessVolume.profile.GetSetting<ColorGrading>();
        }
        transform.position = new Vector3(player.transform.position.x, transform.position.y, player.transform.position.z);
    }

    // Update is called once per frame
    void Update()
    {
        if(!isPositionMatching())
        {
            rb.velocity = GetVectorToMoveTowards() * CAMERA_VELOCITY;
        }
        else
        {
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.deltaTime);
        }

        transform.rotation = Quaternion.Euler(89.9f, 0, 0);

        if (debug && Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (sideStatus == PlayerSideStatus.Active)
            {
                sideStatus = PlayerSideStatus.Paused;
            }
            else
            {
                sideStatus = PlayerSideStatus.Active;
            }

        }
        if (sideStatus == PlayerSideStatus.Paused)
        {
            playerController.LockMovement();
            if (player == AIManager.Instance.playerOne)
            {
                AIManager.Instance.FreezeSideEnemies(AIManager.Instance.leftSideEnemies, true);
            }
            else if(player == AIManager.Instance.playerTwo)
            {
                AIManager.Instance.FreezeSideEnemies(AIManager.Instance.rightSideEnemies, true);
            }
            if (!greyscaleIn)
            {
                GreyscaleScreen(true);
            }

        }
        else if(sideStatus == PlayerSideStatus.Active)
        {
            if (!greyscaleOut)
            {
                GreyscaleScreen(false);
            }
            playerController.UnlockMovement();

            if (player == AIManager.Instance.playerOne)
            {
                AIManager.Instance.FreezeSideEnemies(AIManager.Instance.leftSideEnemies, false);
            }
            else if (player == AIManager.Instance.playerTwo)
            {
                AIManager.Instance.FreezeSideEnemies(AIManager.Instance.rightSideEnemies, false);
            }
        }

        if (greyscale && colorGrading)
        {
            if (greyscaleIn)
            {
                if (colorGrading.saturation.value > -99)
                {
                    colorGrading.saturation.value -= Time.fixedDeltaTime * (greyscaleFadeSpeed * 20);
                }
                else if (colorGrading.saturation <= -99)
                {
                    colorGrading.saturation.Override(-100);
                }
            }

            if (greyscaleOut)
            {
                if (colorGrading.saturation.value < -1)
                {
                    colorGrading.saturation.value += Time.fixedDeltaTime * (greyscaleFadeSpeed * 20);
                }
                else if (colorGrading.saturation >= -1)
                {
                    colorGrading.saturation.Override(0);
                }
            }
        }
    }

    private Vector3 GetVectorToMoveTowards()
    {
        Vector3 newVector = player.transform.position - transform.position;
        return new Vector3(newVector.x, 0, newVector.z);
    }

    private bool isPositionMatching()
    {
        if (Mathf.Abs(transform.position.x - player.transform.position.x) < 0.00001 && (Mathf.Abs(transform.position.z - player.transform.position.z) < 0.00001))
        {
            return true;
        }

        return false;
    }

    public void GreyscaleScreen(bool Greyscale)
    {
        greyscale = true;

        switch (Greyscale)
        {
            case true:
                greyscaleIn = true;
                greyscaleOut = false;
                break;
            case false:
                greyscaleIn = false;
                greyscaleOut = true;
                break;
        }
    }

    public void SetSideStatus(PlayerSideStatus newStatus)
    {
        if (newStatus == PlayerSideStatus.Paused)
        {
            GreyscaleScreen(false);
        }
        else
        {
            GreyscaleScreen(true);
        }
        sideStatus = newStatus;
    }
}
