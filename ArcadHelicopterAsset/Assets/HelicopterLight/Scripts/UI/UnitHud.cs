using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnitHud : MonoBehaviour
{
    #region SINGLETON
    public static UnitHud Singleton
    {
        get
        {
            return GameObject.FindObjectOfType<UnitHud>();
        }
    }
    #endregion

    private HelicopterHudInfo helicopter;
    public HelicopterHudInfo SetHelicopter
    {
        set
        {
            helicopter = value;

            if (helicopter == null)
            {
                hudPanel.SetActive(false);
            }
            else
            {
                hudPanel.SetActive(true);
            }
        }
    }

    public GameObject hudPanel;

    public Text speedText;
    public Text rollText;
    public Text pitchText;
    public Text yawText;
    public Text screwPitchText;
    public Text targetHoldAltitudeText;
    public Text altitudeText;
    public Text engyneText;

    public Toggle holdAltitudeToggle;
    public Toggle lookingToggle;
    public Toggle radioAltitudeToggle;
    public Toggle stabilizationToggle;


    private void LateUpdate()
    {
        if (helicopter == null)
            return;

        speedText.text = "Speed: " + helicopter.GetSpeed().ToString("f1");
        rollText.text = "Roll: " + helicopter.GetRoll().ToString("f1");
        pitchText.text = "Pitch: " + helicopter.GetPitch().ToString("f1");
        yawText.text = "Yaw: " + helicopter.GetYaw().ToString("f1");
        screwPitchText.text = "Screw Pitch: " + helicopter.GetScrewPitch().ToString("f1");
        targetHoldAltitudeText.text = "Target Altitude: " + helicopter.GetTargetHoldAltitude().ToString("f1");
        altitudeText.text = "Altitude: " + helicopter.GetAltitude().ToString("f1");
        engyneText.text = "Engyne: " + helicopter.GetCurrentEngineValue().ToString("f1") + " / " + helicopter.GetTargetEngineValue().ToString("f1");

        holdAltitudeToggle.isOn = helicopter.GetHoldAltitude();
        lookingToggle.isOn = helicopter.GetLooking();
        radioAltitudeToggle.isOn = helicopter.GetRadioAltitude();
        stabilizationToggle.isOn = helicopter.GetAutoStableTilt();
    }

}
