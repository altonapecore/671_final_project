using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class GameOverMenuControl : MonoBehaviour
{
    private bool switchingScenes = false;

    // Update is called once per frame
    void Update()
    {
        if (!switchingScenes && Input.GetKeyDown(KeyCode.Space))
        {
            ReloadMainLevel();
        }
    }

    void ReloadMainLevel()
    {
        switchingScenes = true;
        SceneManager.LoadScene("Main Menu");
    }
}
