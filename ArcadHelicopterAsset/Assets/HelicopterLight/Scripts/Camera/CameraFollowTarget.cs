using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollowTarget : MonoBehaviour
{
    #region SINGLETON
    public static CameraFollowTarget Singleton
    {
        get
        {
            return GameObject.FindObjectOfType<CameraFollowTarget>();
        }
    }
    #endregion

    public GameObject target;

    [SerializeField]
    protected bool onlyMouseHide = false;

    [Range(0, 200)]
    public float lookSpeed = 100;

    [Header("Ограничения угла поворота по оси X")]
    public Vector2 angleX = new Vector2(45, -45);
    [Header("Ограничения угла поворота по оси Y")]
    public Vector2 angleY = new Vector2(0, 0);
    [Header("Инверсия оси Y")]
    public bool invertY = false;
    
    [Header("Высота положения камеры над целью")]
    public Vector3 offSet = Vector3.up;
    [Header("Чувствительность скролинга")]
    public float speedScrollCamera = 20;
    [Header("Сглаживание вращения")]
    public float speedSmoothSpeed = 5;
    [Header("Ограничения скролинга")]
    public float minScroll, maxScroll;
    [Header("Маска для проверки препятствий позади камеры")]
    public LayerMask BackwardLayerMask;
    [Header("Маска коллизий в направлении камеры")]
    public LayerMask forwardLayerMask;
    
    private float cameraPositionZ;
    private Vector3 cameraColliderPoint;
    private float clampX, clampY;
    private Vector3 lookDirection;

    private float pauseTime = 5;

    private Camera _camera;
    public Camera currentCamera
    {
        get
        {
            if(_camera == null)
            {
                _camera = GetComponent<Camera>();
            }

            return _camera;
        }
    }

    private void Start()
    {
        lookDirection.z = 65f;
    }

    private void LateUpdate()
    {
        GetInput();
        GetForwardCollision();
        Follow();
    }

    private void GetInput()
    {
        if (pauseTime > 0)
        {
            pauseTime--;
            return;
        }

        lookDirection = Vector3.zero;

        if (onlyMouseHide)
        {
            if (Cursor.visible)
                return;
        }

        float scroll = 0;

        if (Input.GetKey(KeyCode.KeypadPlus))
        {
            scroll += Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.KeypadMinus))
        {
            scroll -= Time.deltaTime;
        }

        lookDirection += new Vector3(Input.GetAxis("Mouse Y"), Input.GetAxis("Mouse X"), scroll);

    }

    public Vector3 GetForwardCollision(float maxDistance = 500)
    {
        RaycastHit hitForwardCollision;
        Ray rayForward = new Ray(transform.position, transform.TransformDirection(Vector3.forward * maxDistance));

        if (!Physics.Raycast(rayForward, out hitForwardCollision, maxDistance, forwardLayerMask))
        {
            hitForwardCollision.point = transform.TransformPoint(Vector3.forward * maxDistance);
        }

        Debug.DrawLine(transform.position, hitForwardCollision.point);

        return hitForwardCollision.point;
    }

    private float CheckCollision(Vector3 myPosition, Vector3 targetPosition, float currentDistance, float collisionDistance)
    {
        Ray rayCollision = new Ray(targetPosition, myPosition - targetPosition);
        RaycastHit hitCollision;
        if (Physics.Raycast(rayCollision, out hitCollision, collisionDistance, BackwardLayerMask))
        {//сохраняем z камеры равный расстоянию до обьекта столкновения
            if (hitCollision.distance < currentDistance * -1f)
            {
                currentDistance = hitCollision.distance;
                currentDistance = currentDistance * -1f;
            }
        }
        return currentDistance;
    }

    protected void Look(Vector3 lookDirection)
    {
        if (lookDirection == Vector3.zero)
            return;

        transform.eulerAngles += (Vector3.left * lookDirection.x * ToInt(invertY)) + (Vector3.up * lookDirection.y) + (Vector3.forward * 0);

        clampY = transform.eulerAngles.y;
        clampX = transform.eulerAngles.x;

        if (angleX != Vector2.zero)
        {
            if (transform.eulerAngles.x < 180)
            {
                clampX = Mathf.Clamp(transform.eulerAngles.x, 0, angleX.x);
            }
            else
            {
                clampX = Mathf.Clamp(transform.eulerAngles.x, 360 + angleX.y, 360);
            }
        }

        if (angleY != Vector2.zero)
        {
            if (transform.eulerAngles.y < 180)
            {
                clampY = Mathf.Clamp(transform.eulerAngles.y, 0, angleY.x);
            }
            else
            {
                clampY = Mathf.Clamp(transform.eulerAngles.y, 360 + angleY.y, 360);
            }
        }
        
        Vector3 nextRotation = (Vector3.right * clampX) + (Vector3.up * clampY) + (Vector3.forward * transform.eulerAngles.z);
        transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, nextRotation, Time.deltaTime);
    }

    private int ToInt(bool value)
    {
        if (value)
        {
            return -1;
        }
        return 1;
    }

    private void Follow()
    {
        #region Проверяем Target на null
        if (target == null)
        return;
        #endregion

        if (Cursor.visible == true)
        {
            //transform.position = target.transform.TransformPoint(Vector3.forward * cameraPositionZ + offSet);
           // transform.localEulerAngles = Vector3.zero;
           // return;
        }

        #region Назначаем высоту над целью
        Vector3 tempPosition = target.transform.position + offSet;
        #endregion

        #region Приближаем камеру
        cameraPositionZ = Mathf.Clamp(cameraPositionZ + (lookDirection.z * speedScrollCamera), -maxScroll, -minScroll);
        float tmpPositionZ = cameraPositionZ;
        #endregion

        #region Проверка на столкновения  позади камеры
        cameraColliderPoint = transform.TransformPoint(Vector3.back * 2f);
        tmpPositionZ = CheckCollision(cameraColliderPoint, tempPosition, tmpPositionZ, Vector3.Distance(cameraColliderPoint, tempPosition));
        #endregion

        #region Устанавливаем ограничения по углам
        Look(lookDirection * lookSpeed * Time.deltaTime);
        #endregion

        transform.position = transform.rotation * (Vector3.forward * tmpPositionZ) + tempPosition;

        lookDirection = Vector3.zero;
    }
}
