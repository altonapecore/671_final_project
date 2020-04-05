using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIManager : MonoBehaviour
{
    [Header("Setup")]
    public Camera playerCamera;
    public GameObject currentActiveObjective;
    [SerializeField] private GameObject player;

    [Header("UI")]
    [Space(10)]
    public Text instructionsLayer;
    public Image objectiveArrow;

    private List<Image> imageElements;
    private List<Text> textElements;

    // Start is called before the first frame update
    private void Start()
    {
        imageElements = new List<Image>();
        textElements = new List<Text>();

        textElements.Add(instructionsLayer);
        //imageElements.Add(objectiveArrow);

        ResetTextLayers();
        DisableAllImageLayers();
    }

    private void Update()
    {
        if(currentActiveObjective != null)
        { 
            objectiveArrow.gameObject.SetActive(true);

            //get objective Viewport position and direction from player
            Vector2 playerPosition = playerCamera.WorldToViewportPoint(player.transform.position);
            Vector2 objectivePosition = playerCamera.WorldToViewportPoint(currentActiveObjective.transform.position);
            Vector2 direction = objectivePosition - playerPosition;

            //rotate to point at objective
            Vector3 rotation = objectiveArrow.transform.eulerAngles;
            rotation.z = -Vector2.Angle(Vector2.down, direction);
            if (direction.x > 0)
            {
                rotation.z *= -1;
            }
            objectiveArrow.transform.eulerAngles = rotation;

            //fade out the alpha when close
            Color c = objectiveArrow.color;
            c.a = Mathf.Min((direction.sqrMagnitude * 3f) - 0.1f, 1); //buffer zone of 0.1 with fade range of 1/3
            objectiveArrow.color = c;


            Vector2 screenPos = Vector2.zero;
            
            if (objectivePosition.x >= 0 && objectivePosition.x <= 1 &&
                objectivePosition.y >= 0 && objectivePosition.y <= 1)
            {
                //objective is on screen 
                screenPos.x = (objectivePosition.x - 0.5f) * (Screen.width / 2);
                screenPos.y = (objectivePosition.y - 0.5f) * Screen.height;
            }
            else
            {
                //objective is off screen
                Vector2 positionOnEdge = Vector2.zero;

                //calculate positon on top or bottom edge
                positionOnEdge.x = (0.5f * Mathf.Sign(direction.y)) * (direction.x / direction.y);
                positionOnEdge.y = (0.5f * Mathf.Sign(direction.y));

                //gone off the edge of screen
                if (positionOnEdge.x < -0.5f || positionOnEdge.x > 0.5f)
                {
                    //calculate positon on left or right edge
                    positionOnEdge.x = (0.5f * Mathf.Sign(direction.x));
                    positionOnEdge.y = (0.5f * Mathf.Sign(direction.x)) * (direction.y / direction.x);
                }

                //set position on the screen
                screenPos.x = positionOnEdge.x * (Screen.width / 2);
                screenPos.y = positionOnEdge.y * Screen.height;
            }

            objectiveArrow.transform.localPosition = screenPos;

        }
        else
        {
            objectiveArrow.gameObject.SetActive(false);
        }
    }

    public void DisableAllImageLayers()
    {
        foreach(Image element in imageElements)
        {
            element.enabled = false;
        }
    }

    public void ResetTextLayers()
    {
        foreach (Text element in textElements)
        {
            if (element)
            {
                element.text = "";
            }
        }
    }

    public void ChangeInstructions(string newInstructions)
    {
        instructionsLayer.text = newInstructions;
    }
}


