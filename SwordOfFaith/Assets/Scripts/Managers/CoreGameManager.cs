using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState { MainMenu, InLevel, GameOver };

public class CoreGameManager : MonoBehaviour
{
    // Singleton
    public static CoreGameManager Instance;
    // Public Variables
    public PlayerControl PlayerOne;
    public PlayerControl PlayerTwo;
    public GameState gameState;


    private void Awake()
    {
        if (!CoreGameManager.Instance)
        {
            //Debug.Log("Setting Singleton");
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        gameState = GameState.InLevel;
    }


    public void LoadGameOverScreen()
    {
        if (gameState != GameState.GameOver)
        {
            StartCoroutine(HandleGameOverTransition());
        }
    }

    public IEnumerator HandleGameOverTransition()
    {
        gameState = GameState.GameOver;
        yield return new WaitForSeconds(3.0f);
        SceneManager.LoadScene("GameOverScreen");
    }
}
