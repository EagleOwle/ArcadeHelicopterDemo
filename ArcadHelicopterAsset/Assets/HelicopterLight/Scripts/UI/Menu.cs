using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    #region SINGLETON
    public static Menu Singleton
    {
        get
        {
            return GameObject.FindObjectOfType<Menu>();
        }
    }
    #endregion

    public GameObject menuPanel;
    public GameObject spawnPanel;
    public GameObject gamePanel;

    private void Start()
    {
        ToMainMenu();
    }

    private void Update()
    {
        if (Input.GetKeyUp(KeyCode.Escape))
        {
            ToMainMenu();
        }
    }

    public void ToGame()
    {
        if (CameraFollowTarget.Singleton.target == null)
        {
            ToSpawnMenu();
        }
        else
        {
            menuPanel.SetActive(false);
            spawnPanel.SetActive(false);
            gamePanel.SetActive(true);

            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void ToMainMenu()
    {
        spawnPanel.SetActive(false);
        gamePanel.SetActive(false);
        menuPanel.SetActive(true);

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ToSpawnMenu()
    {
        gamePanel.SetActive(false);
        menuPanel.SetActive(false);
        spawnPanel.SetActive(true);
        
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    
}
