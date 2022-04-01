using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserMessage : MonoBehaviour
{
    #region SINGLETONE
    private static UserMessage _singleton;
    public static UserMessage Singleton
    {
        get
        {
            if (_singleton == null)
            {
                _singleton = GameObject.FindObjectOfType<UserMessage>();
            }
            return _singleton;
        }
    }
    #endregion

    public Text messageText;
    float timeClearMessage;

    public void SetMessage(string value, float timeClearMessage = 0)
    {
        if (messageText.text == value)
            return;

        this.timeClearMessage = timeClearMessage;
        messageText.text = value;
    }

    private void Update()
    {
        if(timeClearMessage>0)
        {
            timeClearMessage -= Time.deltaTime;
        }
        else
        {
            messageText.text = "";
            timeClearMessage = 0;
        }
    }
}
