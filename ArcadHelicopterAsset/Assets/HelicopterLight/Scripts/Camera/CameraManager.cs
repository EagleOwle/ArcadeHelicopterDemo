using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CameraType
{
    Follow,
    Freefly,
    None
}

public class CameraManager : MonoBehaviour
{
    public GameObject[] cameras;

    public CameraType SetCamera
    {
        set
        {
            
        }
    }
}
