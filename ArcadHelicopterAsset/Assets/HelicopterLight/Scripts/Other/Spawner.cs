using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    #region SINGLETON
    public static Spawner Singleton
    {
        get
        {
            return GameObject.FindObjectOfType<Spawner>();
        }
    }
    #endregion

    [Header("Добавьте сюда ссылки на префабы")]
    public GameObject[] prefabs;

    public void OnSpawn(int value)
    {
        GameObject tmp = Instantiate(prefabs[value], transform.position, Quaternion.identity);
        CameraFollowTarget.Singleton.target = tmp;
        Menu.Singleton.ToGame();
    }
}
