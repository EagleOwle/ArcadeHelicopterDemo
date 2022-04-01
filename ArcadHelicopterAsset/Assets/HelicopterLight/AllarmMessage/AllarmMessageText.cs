using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AllarmMessageText : MonoBehaviour
{
    #region SINGLETONE
    private static AllarmMessageText _singleton;
    public static AllarmMessageText Singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = GameObject.FindObjectOfType<AllarmMessageText>();
            }
            return _singleton;
        }
    }
    #endregion

    private string message;
    public Color messageColor;
    public Text messageText;
    public float crossfadeSpeed = 5;
    public float waitTime = 5;
    private float currentWait;

    public bool onHide = false;

    public string SetMessage
    {
        set
        {
            message = value;
            messageText.text = message;
            messageText.color = messageColor;
        }
    }

    public void OnHide(float time = 0)
    {
        if (time == 0)
        {
            currentWait = waitTime;
        }
        else
        {
            currentWait = 0;
        }
    }

    private void Update()
    {
        if (onHide == false)
        {
            return;
        }

        if (currentWait > 0)
        {
            currentWait -= Time.deltaTime;
            return;
        }

        if (messageText.color.a > 0)
        {
            messageText.color = new Color(messageColor.r, messageColor.g, messageColor.b, messageText.color.a - crossfadeSpeed * Time.deltaTime);
        }
    }

}
