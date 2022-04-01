using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HelicopterState
{
    On,
    Off,
    Start,
    Stop
}

public class HelicopterController : MonoBehaviour
{
    [Header("Start On Sky")]
    [Tooltip("Spawn helicopter with start engyne. \n Спаун вертолет с запущеным двигателем.")]
    [SerializeField] private bool StartOnSky = false;

    [Header("Hold Altitude")]
    [Tooltip("The helicopter is aiming for a given altitude. \n Вертолет стремится к заданной высоте.")]
    [SerializeField] private bool holdAltitude = false;
    public bool HoldAltitude
    {
        get { return holdAltitude; }
        set
        {
            holdAltitude = value;

            if (holdAltitude == true)
            {
                targetAltitude = currentAltitude;
            }
        }
        
    }

    [HideInInspector]
    public float currentAltitude;
    [HideInInspector]
    public float targetAltitude;

    [Header("Use Radion Altitude")]
    [Tooltip("Altitude is calculated from the obstacles under the helicopter. \n Высота рассчитывается от препятствий под вертолетом.")]
    [SerializeField] private bool radioAltitude = false;
    public bool RadioAltitude
    {
        get
        {
            return radioAltitude;
        }
        set
        {
            radioAltitude = value;

            //Сразу назначаем текущую высоту как высоту для удержания
            //Что бы избежать резкого набора или потери высоты.
            //Immediately assign the current height as the height to hold
            //To avoid sudden climb or loss of height.
            targetAltitude = currentAltitude;
        }

    }

    [Header("Rotation stabilization")]
    [Tooltip("The helicopter stabilizes along the axes if the control keys are not pressed. \n Вертолет стабилизируется по осям, если не нажаты клавиши управления.")]
    public bool autoStableTilt = true;

    [Header("Looking to camera look")]
    [Tooltip("The helicopter will turn in the direction of the camera view. \n Вертолет повернется в направлении обзора камеры.")]
    public bool looking = false;

    [Header("Landing Collider")]
    [Tooltip("Helicopter collider, collision with which is perceived as landing. \n Коллайдер вертолета, столкновение с которым воспринимается как посадка.")]
    public Collider landingCollider;

    [Header("Collision Shocktime")]
    [Tooltip("Time that the helicopter will not respond to control after a collision. \n Время, которое вертолет не будет отвечать на управление после столкновения.")]
    [Range(0, 10)]
    [SerializeField] private float collisionShocktime = 1f;

    [Header("Displacement intensity")]
    [Tooltip("The speed of the helicopter in the horizontal plane. \n Скорость вертолета в горизонтальной плоскости.")]
    [Range(0, 500)]
    [SerializeField] private float displacementIntensity = 150f;

    [Header("Stabilization speed")]
    [Tooltip("Helicopter auto-leveling speed. \n Скорость автоматического выравнивания вертолета.")]
    [Range(0.1f, 100)]
    [SerializeField] private float stableSpeed = 40;

    [Header("Mass Adjustment")]
    [Tooltip("Adjustment of the current mass of the helicopter for a more accurate calculation of the lifting force required for the hang. \n Регулировка текущей массы вертолета для более точного расчета подъемной силы, необходимой для зависания.")]
    [SerializeField] private float massAdjustment = 9.85f;

    [Header("Maximal tilt angle")]
    [Tooltip("The maximum possible tilt angle of the helicopter. \n Максимально возможный угол наклона вертолета.")]
    [Range(1, 180)]
    [SerializeField] private float maxTiltAngle = 35;

    [Header("Maximal altitude")]
    [Tooltip("The maximum possible altitude of the helicopter. \n Максимально возможная высота вертолета.")]
    [Range(1, 500)]
    [SerializeField] private float maxAltitude = 200;

    [Header("Repulsive force")]
    [Tooltip("The force of repulsion of a helicopter from an obstacle. \n Сила отталкивания вертолета от препятствия.")]
    [Range(0, 500)]
    [SerializeField] private float forceRepulsion = 150;

    [Header("Maximum angle for screw")]
    [Range(0, 45)]
    [SerializeField] private float maxScrewAngle = 15f;
    [HideInInspector] public float currentScrewAngle;
    public float SetScrewAngle
    {
        set
        {
            currentScrewAngle += value;
            currentScrewAngle = Mathf.Clamp(currentScrewAngle, -maxScrewAngle, maxScrewAngle);

            if(value!= 0)
            {
                waitHoldScrewAngle = 1;
            }
        }
    }
    //Данное поле определяет, через какое время подьемная сила вертолета начнет стремится к 0
    //This field determines how long the helicopter’s lift force will begin to approach 0.
    private float waitHoldScrewAngle;

    [Header("Collision mask on down")]
    [Tooltip("Mask for calculating the altitude under the helicopter. \n Маска для расчета высоты под вертолетом.")]
    [SerializeField] private LayerMask layerMaskOnDown = 2;

    [Header("Start engine power")]
    [Tooltip("Base engine power. \n Базовая мощность двигателя.")]
    [Range(0, 100)]
    public float startEnginePower = 80;//the unwinding period of the helicopter's screws for the transition from the Start state to the On state
    [HideInInspector] public float currentEnginePower;
    [HideInInspector] public float targetEnginePower;
    public float EnginePower
    {
        get { return currentEnginePower; }
        set
        {
            targetEnginePower = value;
            targetEnginePower = Mathf.Clamp(targetEnginePower, 0, 100);
        }
    }

    [Header("Speed ​​set engine power")]
    [Tooltip("The speed at which the engine changes its power. \n Скорость, с которой двигатель меняет мощность.")]
    [Range(1, 100)]
    [SerializeField] private float speedChangePower = 25;

    [Header("Pitch sound corrector")]
    [Tooltip("Change this value if the sound does not match the speed of rotation of the screws. \n Измените это значение, если звук не соответствует скорости вращения винтов.")]
    [Range(0, 500)]
    [SerializeField] private float pitchCorrector = 25;

    [Header("Screw settings")]
    public RotorController[] rotors;

    [Header("Tilt speed")]
    public TiltSpeed tiltSpeed;

    //Состояние вертолета.
    //Не изменяйте значение этой переменной напрямую. Используйте HelicopterState, для этого.
    //State of Helicopter.  
    //Do not change the value of this variable directly. Use HelicopterState to do this.
    private HelicopterState _helicopterState;
    private HelicopterState HelicopterState
    {
        get { return _helicopterState; }
        set
        {
            _helicopterState = value;

            switch (value)
            {
                case HelicopterState.Off:
                    audioSource.Stop();
                    holdAltitude = false;
                    targetAltitude = 0;
                    break;

                case HelicopterState.On:
                    break;

                case HelicopterState.Start:
                    targetEnginePower = startEnginePower;
                    audioSource.loop = true;
                    audioSource.Play();
                    break;

                case HelicopterState.Stop:
                    targetEnginePower = 0;
                    break;
            }
        }
    }

    public void SetTiltInput(Vector3 value)
    {
        tiltInput = value;
    }

    private float shocktime;
    private float upForceCurrent;
    private Vector3 tiltInput;
    private AudioSource audioSource;
    private new Rigidbody rigidbody;

    private bool onFly;//Does not allow helicopter tilts if false

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    { 
        if (StartOnSky)
        {
            currentEnginePower = startEnginePower;
            targetEnginePower = startEnginePower;
            HelicopterState = HelicopterState.On;
            
        }
        else
        {
            HelicopterState = HelicopterState.Off;
        }
        
        onFly = true; 
    }

    private void FixedUpdate()
    {
        //Обработка наклонов вертолета. В качестве аргумента принимает пользовательский ввод с клавиатуры либо мыши. 
        // Processing helicopter slopes. It takes user input from the keyboard or mouse as an argument.
        PitchYawRollProcess(tiltInput);

        //Премещение вертолета в пространстве
        //Move the helicopter in space
        MoveProcess();

        //Редактируем высоту вертолета
        //Editing the height of the helicopter
        CheckAltitude();
    }

    private void Update()
    {
        //Здесь обрабатываются данные в зависимости от состояния HelicopterState
        //Data is processed here depending on the state of HelicopterState
        WorkEngine();

        //Когда пользователь изменяет значение подьемной силы, данная переменная становится равной 1 и каждый цикл стремится к 0.
        //Если танная переменная равна 0 то подьемная сила вертолета будет стремится к 0.
        //When the user changes the lift value, this variable becomes equal to 1 and each cycle tends to 0
        //If the variable is equal to 0, then the helicopter's lifting force will tend to 0.
        waitHoldScrewAngle = Mathf.MoveTowards(waitHoldScrewAngle, 0, Time.deltaTime);

        if(waitHoldScrewAngle <= 0)
        {
           currentScrewAngle = 0;
        }

        //Вращение роторов и воспроизведение звука
        RotorRotation();

        if(HelicopterState == HelicopterState.Off)
        {
            if (UserMessage.Singleton != null) { UserMessage.Singleton.SetMessage("Press I for start engine"); }
        }

        if(shocktime > 0)
        {
            shocktime -= Time.deltaTime;
        }
    }

    //Обработка наклонов вертолета. В качестве аргумента принимает пользовательский ввод с клавиатуры либо мыши. 
    // Processing helicopter slopes. It takes user input from the keyboard or mouse as an argument.
    private void PitchYawRollProcess(Vector3 value)
    {
        //Если вертолет не в воздухе, то не выполняем наклонов
        //If the helicopter is not in the air, then we do not tilt
        if (!onFly)
            return;
        
        #region Looking for camera look point
        //Если вертолет следит за направлением камеры, вместо пользовательского ввода используем расчет направления вертолета в сторону взгляда камеры
        //If the helicopter is following the direction of the camera, instead of user input, use the calculation of the direction of the helicopter in the direction of the camera look
        if (looking)
        {
            Vector2 dotVector2 = Vector2.zero;

            if (value.x == 0)
            {
                dotVector2.x = Vector3.Dot(CameraFollowTarget.Singleton.transform.forward, transform.up) * 10;
                value.x = -dotVector2.x;
                dotVector2.x = Mathf.Clamp(dotVector2.x, -1, 1);
            }

            if (value.y == 0)
            {
                dotVector2.y = Vector3.Dot(CameraFollowTarget.Singleton.transform.forward, transform.right) * 10;
                dotVector2.y = Mathf.Clamp(dotVector2.y, -1, 1);
                value.y = dotVector2.y;
            }

            if (value.z == 0)
            {
                value.z = Mathf.MoveTowards(value.z, 0, Time.deltaTime);
            }
        }
        #endregion

        //Если двигатель не вышел в полный режим работы (state On), то обнуляем значение пользовательского ввода
        //If the engine does not go into full operation(state On), we reset the value of user input
        if (HelicopterState != HelicopterState.On)
        {
            value = Vector3.zero;
        }

        //Если вертолет только что столкнулся с препятствием (не вышло "шоковое" время), то обнуляем пользовательский ввод
        if (shocktime > 0)
        {
            value = Vector3.zero;
        }

        #region Stabilization
        //Выравниваем наклон вертолета по осям, если их значения равны 0
        //Align the helicopter tilt along the axes if their values are 0
        if (autoStableTilt == true || shocktime > 0)
        {
            //Если авто стабилизация отключена, но произошло столкновение, то
            //вертолет все равно будет выравнивать положение по осям.
            //Это нужно, что бы вертолет не "бился" бесконечно о препятствие, двигаясь в направлении наклона.

            float stableX = transform.eulerAngles.x;
            float stableY = transform.eulerAngles.y;
            float stableZ = transform.eulerAngles.z;

            if (value.x == 0)
            {
                stableX = Mathf.MoveTowardsAngle(stableX, 0, stableSpeed * Time.deltaTime);
            }

            if (value.z == 0)
            {
                stableZ = Mathf.MoveTowardsAngle(stableZ, 0, stableSpeed * Time.deltaTime);
            }

            rigidbody.MoveRotation(Quaternion.Euler(new Vector3(stableX, stableY, stableZ)));
        }

        #endregion

        #region Set Speed Tilt
        //Корректируем значение ввода по осям исходя из данных tiltSpeed и массы вертолета
        //Adjust the value of the input on the axes based on the tiltSpeed value and the mass rigidBody of the helicopter
        value.x *= tiltSpeed.pitch;// * tmpForce * rigidbody.mass * Time.deltaTime;
        value.y *= tiltSpeed.yaw;// * tmpForce * rigidbody.mass * Time.deltaTime;
        value.z *= tiltSpeed.roll;// * tmpForce * rigidbody.mass * Time.deltaTime;
        #endregion

        #region clamp
        //Ограничиваем наклон вертолета исходя из значения maxTiltAngle
        //Limit helicopter tilt based on maxTiltAngle value
        float roll = GetAngle(transform.localEulerAngles.z) * 100 / maxTiltAngle;

        if (value.z > 0 && roll > 100)
        {
            value.z = 0;
        }

        if (value.z < 0 && roll < -100)
        {
            value.z = 0;
        }

        float yaw = GetAngle(transform.localEulerAngles.y);

        float pitch = GetAngle(transform.localEulerAngles.x) * 100 / maxTiltAngle;

        if (value.x > 0 && pitch > 100)
        {
            value.x = 0;
        }

        if (value.x < 0 && pitch < -100)
        {
            value.x = 0;
        }

        #endregion

        //Применяем обработанное значение
        //Apply the processed value
        rigidbody.AddRelativeTorque(value);
    }

    //Премещение вертолета в пространстве
    //Move the helicopter in space
    private void MoveProcess()
    {
        Vector3 angleDirection = Vector3.zero;

        #region Up force block
        //Корректируем массу вертолета
        //Так-как различные дочерние колайдеры будут влиять на массу вертолета и его поведение, 
        //необходимо корректировать массу, для вычисления точной подьемной силы, что бы добится зависания вертолета
        //Adjust the weight of the helicopter
        // So, as various child coliders will influence the mass of the helicopter and its behavior,
        // it is necessary to adjust the mass in order to calculate the exact lifting force in order to achieve the hovering of the helicopter
        float force = rigidbody.mass * massAdjustment;

        //Корректируем подьемную силу исходя из мощности двигателя, в процентах
        //В итоге получаем подьемную силу достаточную для поддержания вертолета на текущей высоте
        //Adjust the lift on the basis of engine power, in percent
        //As a result, we obtain a lift force sufficient to maintain the helicopter at the current height
        force = force / 100 * currentEnginePower;

        //Увеличиваем или уменьшаем подьемную силу исходя из значения UpForce
        // Increase or decrease the lift based on the value of UpForce
        upForceCurrent = force + currentScrewAngle;

        //Если текущая высоты больше заданной максимальной (maxAltitude), то обнуляем подьемную силу
        //If the current height is greater than the specified maximum (maxAltitude), then reset the lift
        if (currentAltitude > maxAltitude)
        {
            upForceCurrent = 0;
        }
        #endregion

        #region Move force block

        //Если в полете, то расчитываем значения силы в направлении наклона вертолета
        //Здесь так же учитывается displacementIntensity и rigidbody.mass
        // If in flight, we calculate the force values in the direction of helicopter tilt
        // It also takes into account displacementIntensity and rigidbody.mass
        if (onFly && shocktime <= 0)
        {
            float roll = GetAngle(transform.localEulerAngles.z);
            float pitch = GetAngle(transform.localEulerAngles.x);
            angleDirection = transform.TransformDirection(new Vector3(-roll, 0, pitch)) * (displacementIntensity * rigidbody.mass) * Time.deltaTime;
        }

        #endregion

        angleDirection.y = upForceCurrent;
        rigidbody.AddForce(angleDirection);
    }

    //Редактируем высоту вертолета
    //Editing the height of the helicopter
    private void CheckAltitude()
    {
        //calculate the current height
        //Вычисляем текущую высоту, 
        currentAltitude = transform.position.y;

        if (radioAltitude == true)
        {
            Vector3 position = GetGroudCollisionPoint(transform.position);
            currentAltitude = currentAltitude - position.y;
        }

        if (!holdAltitude)
            return;

        //Change the altitude hold value
        //Изменяем значение удержания высоты
        if(currentScrewAngle > 0)
        {
            targetAltitude = currentAltitude + Time.deltaTime;
        }

        if (currentScrewAngle < 0)
        {
            targetAltitude = currentAltitude - Time.deltaTime;
        }

        if(targetAltitude > maxAltitude)
        {
            targetAltitude = maxAltitude;
        }

        //Change the current altitude of the helicopter.
        //Изменяем текущую высоту вертолета.
        float autoLiftValue = (targetAltitude - currentAltitude) * Time.deltaTime;
        rigidbody.AddForce(Vector3.up * autoLiftValue, ForceMode.VelocityChange);

    }

    public void StartEngine()
    {
        if (HelicopterState == HelicopterState.On || HelicopterState == HelicopterState.Start)
        {
            HelicopterState = HelicopterState.Stop;
        }
        else
        {
            if (HelicopterState == HelicopterState.Stop || HelicopterState == HelicopterState.Off)
            {
                HelicopterState = HelicopterState.Start;
            }
        }
    }

    //Данный метод вызывается каждый цикл в Update
    //Здесь обрабатываются данные в зависимости от состояния HelicopterState
    // This method is called each cycle in Update
    // Data is processed here depending on the state of HelicopterState
    private void WorkEngine()
    {
        switch (HelicopterState)
        {
            case HelicopterState.Off:
                break;

            case HelicopterState.On:
                ChangeEnginePower();
                break;

            case HelicopterState.Start:
                ChangeEnginePower();
                if (currentEnginePower >= startEnginePower)
                {
                    HelicopterState = HelicopterState.On;
                }
                break;

            case HelicopterState.Stop:
                ChangeEnginePower();
                if (currentEnginePower <= 0)
                {
                    HelicopterState = HelicopterState.Off;
                }
                break;
        }
    }

    //Вращение роторов и воспроизведение звука
    private void RotorRotation()
    {
        #region Rotation Rotors
        //Вращаем винты исходя из текущей мощности двигателя 
        //Rotate the screws based on the current engine power
        foreach (var item in rotors)
        {
            item.Rotation(currentEnginePower);
        }
        #endregion

        //Назначаем искажение звука работы винтов исходя из текущей мощности двигателя
        // Assign the sound distortion of the screws based on the current engine power

        if (audioSource == null)
        {
            Debug.LogError("AudioSource is null. Set audioSource on copter and set link");
        }
        else
        {
            audioSource.pitch = Mathf.Clamp(currentEnginePower / pitchCorrector, 0, 1.2f);
        }
    }

    //Подгоняем значение HelicopterState к targetPower со скоростью speedChangePower
    //Либо напрямую ставим значение currentPower равное targetPower если скорость равня 0
    //Метод вызывается каждый цикл в определенных состояниях  HelicopterState
    // Adjust the HelicopterState value to the targetPower at speeds of speedChangePower
    // Or directly set the value of currentPower equal to targetPower if the speed is 0
    // The method is called each cycle in certain states HelicopterState
    private void ChangeEnginePower()
    {
        if (speedChangePower > 0)
        {
            currentEnginePower = Mathf.MoveTowards(currentEnginePower, targetEnginePower, speedChangePower * Time.deltaTime);
        }
        else
        {
            currentEnginePower = targetEnginePower;
        }

    }

    //Получаем местоположения препятствия в пространстве под вертолетом
    //Get the locations of the obstacles in the space under the helicopter
    private Vector3 GetGroudCollisionPoint(Vector3 startRayPosition)
    {
        RaycastHit hit;
        Vector3 hitDirection = (startRayPosition + Vector3.down * 100) - startRayPosition;
        if (Physics.Raycast(startRayPosition, hitDirection, out hit, Mathf.Infinity, layerMaskOnDown))
        {
            Debug.DrawRay(startRayPosition, hit.point - startRayPosition, Color.red);
            return hit.point;
        }
        else
        {
            Debug.DrawRay(startRayPosition, hit.point - startRayPosition, Color.red);
            return hitDirection;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        ContactPoint contact = collision.contacts[0];

        CallEvents.eventCopterCollision.Invoke();
        //if (AllarmMessageText.Singleton != null) { AllarmMessageText.Singleton.SetMessage = "Столкновение: " + contact.thisCollider.name + " " + collision.relativeVelocity.magnitude.ToString("f0"); }

        Vector3 direction = contact.normal;
        direction = new Vector3(contact.normal.x, 0, contact.normal.z);

        //Применяем силу в напровлении отталкивания, если коллайдер не является  "LandingCollider"
        //We use force in the repulsion direction if the collider is not a "LandingCollider"
        if (contact.thisCollider != landingCollider)
        {
            rigidbody.AddForce(contact.normal * rigidbody.mass * forceRepulsion);
            shocktime = collisionShocktime;
        }
    }

    private void OnCollisionStay(Collision collision)
    {
        onFly = false;
    }

    private void OnCollisionExit(Collision collision)
    {
        onFly = true;
    }

    private float GetAngle(float value, bool ads = false)
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

    [System.Serializable]
    public struct TiltSpeed
    {
        [Range(0, 5000)]
        public float roll;
        [Range(0, 5000)]
        public float pitch;
        [Range(0, 5000)]
        public float yaw;
    }

    public enum DirectionRotate
    {
        RightClock,
        LeftClock,
    }

    [System.Serializable]
    public class RotorController
    {
        public enum Axis
        {
            X,
            Y,
            Z,
        }

        public string nameRotor = "rotor";
        public Transform transformRotor;
        public Axis axis = Axis.Y;
        public DirectionRotate directionRotate = DirectionRotate.LeftClock;
        public float speed = 35;
        public float maxSpeed = 1000;

        private float nextRotate;

        public void Rotation(float rotorSpeed)
        {
            rotorSpeed = Mathf.Clamp(rotorSpeed, 0, maxSpeed);

            if (directionRotate == DirectionRotate.RightClock)
            {
                nextRotate += rotorSpeed * speed * Time.deltaTime;
            }
            else
            {
                nextRotate -= rotorSpeed * speed * Time.deltaTime;
            }

            nextRotate = nextRotate % 360;

            switch (axis)
            {
                case Axis.Y:
                    transformRotor.localRotation = Quaternion.Euler(transformRotor.localRotation.x, nextRotate, transformRotor.localRotation.z);
                    break;
                case Axis.Z:
                    transformRotor.localRotation = Quaternion.Euler(transformRotor.localRotation.x, transformRotor.localRotation.y, nextRotate);
                    break;
                default:
                    transformRotor.localRotation = Quaternion.Euler(nextRotate, transformRotor.localRotation.y, transformRotor.localRotation.z);
                    break;
            }
        }

    }
}
