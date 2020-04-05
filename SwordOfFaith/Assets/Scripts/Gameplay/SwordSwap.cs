using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwordSwap : MonoBehaviour
{
    //object references
    [SerializeField] private Transform sword;
    [SerializeField] private Transform playerOne;
    [SerializeField] private Transform playerTwo;
    public float lerpTime;

    [Header("Cameras")]
    [Space(10)]
    [SerializeField] private Camera playerOneCamera;
    [SerializeField] private Camera playerTwoCamera;

    [Header("UI")]
    [Space(10)]
    [SerializeField] private RectTransform swordUI;

    [Header("Sound")]
    [Space(10)]
    public AudioClip swapSound;
    public AudioClip recieveSound;

    private bool dir = false;
    private bool swapping = false;

    [HideInInspector]
    Camera startCam, endCam;

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !swapping)
        {
            swapping = true;
            //GameVars.instance.audioManager.PlaySFX(swapSound, 0.5f, sword.position);
            StartCoroutine(Swap());
        }
    }

    private IEnumerator Swap()
    {
        float t = 0;
        float p = 0;
        float a = 0;

        sword.gameObject.SetActive(true);
        swordUI.gameObject.SetActive(true);
        Image swordImg = swordUI.GetComponent<Image>();

        //switch direction
        dir = !dir;

        //calculate the viewport start position
        Vector3 playerStartPosition = dir ? playerOne.position : playerTwo.position;
        startCam = (dir ? playerOneCamera : playerTwoCamera);
        Vector3 viewportStartPos = startCam.WorldToViewportPoint(playerStartPosition);
        viewportStartPos.x *= startCam.rect.width;
        viewportStartPos.y *= startCam.rect.height;
        viewportStartPos += (Vector3)startCam.rect.position;

        startCam.gameObject.GetComponent<PlayerCamera>().player.GetComponent<PlayerControl>().ToggleSword(false);
        
        float swordY = sword.position.y;

        while (t < lerpTime)
        { 
            //calcualte the viewport end position
            Vector3 playerEndPosition = dir ? playerTwo.position : playerOne.position;
            endCam = (dir ? playerTwoCamera : playerOneCamera);
            Vector3 viewportEndPos = endCam.WorldToViewportPoint(playerEndPosition);
            viewportEndPos.x *= endCam.rect.width;
            viewportEndPos.y *= endCam.rect.height;
            viewportEndPos += (Vector3)endCam.rect.position;

            //incrase time and calculate percentage of total lerp time 
            t += Time.deltaTime;
            p = t / lerpTime;

            //modify p to start fast and slow down
            p = Mathf.Sin(p * Mathf.PI * 0.5f); 

            //calculate viewport space position of the sword
            Vector3 viewportPos = Vector3.Lerp(viewportStartPos, viewportEndPos, p);
            //viewportPos.y +=  0.125f * (- Mathf.Cos(2 * Mathf.PI * p) / 2 + 0.5f); //rise and fall with wave

            //set the alpha value
            Color swordColor = swordImg.color;
            a = p < 0.25f || p > 0.75f ? 1 : (-Mathf.Cos(4 * Mathf.PI * p) / 2 + 0.5f);
            swordColor.a = a;

            //set the sword UI 
            swordUI.position = new Vector2(Screen.width * viewportPos.x, Screen.height * viewportPos.y);
            swordImg.color = swordColor;


            Vector3 worldPosition;
            //when p < 0.5 get position in Start Camera viewport
            if (p < 0.5f)
            {
                //convert back to worldSpace
                viewportPos -= (Vector3)startCam.rect.position;
                viewportPos.x /= startCam.rect.width;
                viewportPos.y /= startCam.rect.height;

                worldPosition = startCam.ViewportToWorldPoint(viewportPos);

            }
            //when p > 0.5 get position in End Cameara viewport
            else
            {
                //convert back to worldSpace
                viewportPos -= (Vector3)endCam.rect.position;
                viewportPos.x /= endCam.rect.width;
                viewportPos.y /= endCam.rect.height;

                worldPosition = endCam.ViewportToWorldPoint(viewportPos);
            }

            worldPosition.y = swordY + (5 * (-Mathf.Cos(2 * Mathf.PI * p) / 2 + 0.5f)); //rise and fall with wave

            sword.localScale = Vector3.one * ((5 * (-Mathf.Cos(2 * Mathf.PI * p) / 2 + 0.5f)) + 1); //grow and shrink with wave
            sword.position = worldPosition;

            yield return new WaitForEndOfFrame();
        }

        //GameVars.instance.audioManager.PlaySFX(recieveSound, 0.5f, sword.position);
        endCam.gameObject.GetComponent<PlayerCamera>().player.GetComponent<PlayerControl>().ToggleSword(true);
        sword.gameObject.SetActive(false);
        swordUI.gameObject.SetActive(false);
        swapping = false;
    }
}
