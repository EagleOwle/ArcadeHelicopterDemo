using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [Header("Work if mouse hide")]
    [Tooltip("The helicopter will process the logic only if the mouse cursor is hidden. \n Вертолет будет обрабатывать логику, только если курсор мыши скрыт.")]
    [SerializeField] private bool onlyMouseHide = false;

    [Header("Sensitivity mouse scroll")]
    [Range(0, 500)]
    [SerializeField] private float scrollSence = 250f;
    [Header("Sensitivity input axis")]
    [Range(0, 500)]
    [SerializeField] private float axisSence = 250f;

    HelicopterController helicopterController;
    float scrollInput;
    Vector3 tiltInput;

    //Обробатывает пользовательский ввод с клавиатуры и мыши
    //Handles user input from the keyboard and mouse
    private void GetInput()
    {
        tiltInput = Vector3.zero;
        scrollInput = 0;

        //если курсор мыши видимый то выходим из метода
        //exit if the mouse cursor is visible
        if (onlyMouseHide && Cursor.visible)
            return;

        #region Hold Altitude
        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            helicopterController.HoldAltitude = !helicopterController.HoldAltitude;
        }
        #endregion

        #region Start\Stop engine
        //Запускает либо останавливает двигатели вертолета
        //Изменяем состояние HelicopterState на противоположное
        // Starts or stops the helicopter engine
        // Change the state of HelicopterState to the opposite...
        if (Input.GetKeyUp(KeyCode.I))
        {
            helicopterController.StartEngine();
        }
        #endregion

        #region Engine Power
        //Изменяем текущую мощность двигателя.
        //Change the current engine power.
        if (Input.GetKey(KeyCode.X))
        {
            helicopterController.EnginePower += 1;
        }

        if (Input.GetKey(KeyCode.Z))
        {
            helicopterController.EnginePower -= 1;
        }
        #endregion

        #region looking\!looking
        //Смотрим, куда смотрит камера
        //We look where the camera is looking
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            helicopterController.looking = !helicopterController.looking;
        }
        #endregion

        #region Stabilization
        //Automatic alignment of the helicopter tilt angles
        //Автоматическое выравнивание углов наклона вертолета
        if (Input.GetKeyUp(KeyCode.H))
        {
            helicopterController.autoStableTilt = !helicopterController.autoStableTilt;
        }
        #endregion

        #region Radio Altitude
        //Используем расчет высоты от препятствий под вертолетом.
        //Use the calculation of the height of the obstacles under the helicopter.
        if (Input.GetKeyUp(KeyCode.R))
        {
            helicopterController.RadioAltitude = !helicopterController.RadioAltitude;
        }
        #endregion

        #region Input Keyboard
        {
            //Handle keybutton to control the helicopter
            //But the processing of input axes gives a better control response.
            //Use InputManager to process input axes.

            // Клавиши для управления вертолетом
            // Но обработка осей пользовательского ввода дает лучшее поведение.
            // Используем InputManager для обработки осей пользовательского ввода.

            #region Input Vertical
            if (Input.GetKey(KeyCode.W))
            {
                tiltInput.x = Mathf.MoveTowards(tiltInput.x, 1, axisSence * Time.deltaTime);
            }
            else
            {
                if (Input.GetKey(KeyCode.S))
                {
                    tiltInput.x = Mathf.MoveTowards(tiltInput.x, -1, axisSence * Time.deltaTime);
                }
                else
                {
                    tiltInput.x = Mathf.MoveTowards(tiltInput.x, 0, axisSence * Time.deltaTime);
                }
            }
            
            //tiltInput.x = Input.GetAxis("Pitch");
            #endregion

            #region Input Turn
            if (Input.GetKey(KeyCode.E))
            {
                tiltInput.y = Mathf.MoveTowards(tiltInput.y, 1, axisSence * Time.deltaTime);
            }
            else
            {
                if (Input.GetKey(KeyCode.Q))
                {
                    tiltInput.y = Mathf.MoveTowards(tiltInput.y, -1, axisSence * Time.deltaTime);
                }
                else
                {
                    tiltInput.y = Mathf.MoveTowards(tiltInput.y, 0, axisSence * Time.deltaTime);
                }
            }
           
            //tiltInput.y = -Input.GetAxis("Yaw");
            #endregion

            #region Input Horizontal
            if (Input.GetKey(KeyCode.A))
            {
                tiltInput.z = Mathf.MoveTowards(tiltInput.z, 1, axisSence * Time.deltaTime);
            }
            else
            {
                if (Input.GetKey(KeyCode.D))
                {
                    tiltInput.z = Mathf.MoveTowards(tiltInput.z, -1, axisSence * Time.deltaTime);
                }
                else
                {
                    tiltInput.z = Mathf.MoveTowards(tiltInput.z, 0, axisSence * Time.deltaTime);
                }
            }
            
            //tiltInput.z = -Input.GetAxis("Roll");
            #endregion
        }
        #endregion

        #region Change Lift Force and target hold altitude
        scrollInput = Input.GetAxis("Mouse ScrollWheel") * scrollSence * Time.deltaTime;

        #endregion

    }

    private void SetInput()
    {
        helicopterController.SetTiltInput(tiltInput);
        helicopterController.SetScrewAngle = scrollInput;
    }

    private void Awake()
    {
        helicopterController = GetComponent<HelicopterController>();
    }

    private void Update()
    {
        GetInput();
        SetInput();
    }

    private float MoveToValue(float baseValue, float targetValue)
    {
        baseValue = Mathf.MoveTowards(baseValue, targetValue, Time.deltaTime);
        return baseValue;
    }

}
