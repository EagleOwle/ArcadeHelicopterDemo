using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class HelicopterHudInfo : MonoBehaviour
{
    HelicopterController helicopterController;
    new  Rigidbody rigidbody;

    private void Awake()
    {
        helicopterController = GetComponent<HelicopterController>();
        rigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        UnitHud.Singleton.SetHelicopter = this;
    }

    public float GetSpeed()
    {
        return rigidbody.velocity.sqrMagnitude;
    }

    public float GetRoll()
    {
       return GetAngle(transform.localEulerAngles.z);
    }

    public float GetPitch()
    {
        return GetAngle(transform.localEulerAngles.x);
    }

    public float GetYaw()
    {
        return GetAngle(transform.localEulerAngles.y);
    }

    public float GetScrewPitch()
    {
        return helicopterController.currentScrewAngle;
    }

    public float GetTargetHoldAltitude()
    {
        return helicopterController.targetAltitude;
    }

    public float GetAltitude()
    {
        return helicopterController.currentAltitude;
    }

    public float GetTargetEngineValue()
    {
        return helicopterController.targetEnginePower;
    }

    public float GetCurrentEngineValue()
    {
        return helicopterController.currentEnginePower;
    }

    public bool GetHoldAltitude()
    {
        return helicopterController.HoldAltitude;
    }

    public bool GetLooking()
    {
        return helicopterController.looking;
    }

    public bool GetRadioAltitude()
    {
        return helicopterController.RadioAltitude;
    }

    public bool GetAutoStableTilt()
    {
        return helicopterController.autoStableTilt;
    }

    float GetAngle(float value, bool ads = false)
    {
        value = Mathf.RoundToInt(value);

        if (ads == true)
        {
            if (value > 180)
            {
                return (360 - value);
            }
            else
            {
                return value;
            }
        }
        else
        {
            if (value > 180)
            {
                return (360 - value) * -1;
            }
            else
            {
                return value;
            }
        }
    }
}
