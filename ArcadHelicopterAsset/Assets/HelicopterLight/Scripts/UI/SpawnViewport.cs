using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnViewport : MonoBehaviour
{
    public GameObject buttonPrefab;
    public Transform content;
    
    private GameObject[] buttons = new GameObject[0];

    private void ClearButtons()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            Destroy(buttons[i]);

        }
    }

    private void CreateButtons()
    {
        if (Spawner.Singleton == null) return;

        buttons = new GameObject[Spawner.Singleton.prefabs.Length];
        for (int i = 0; i < buttons.Length; i++)
        {
            GameObject tmp = Instantiate(buttonPrefab, content);
            buttons[i] = tmp;
            tmp.GetComponentInChildren<Text>().text = Spawner.Singleton.prefabs[i].name;
            int index = i;
            tmp.GetComponent<Button>().onClick.AddListener(delegate { Spawner.Singleton.OnSpawn(index); });
            tmp.SetActive(true);
        }
    }

    private void OnEnable()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        ClearButtons();
        CreateButtons();
    }
}
