using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSpectator : MonoBehaviour
{
    [SerializeField]
    protected bool onlyMouseHide = false;

    [Header("Ограничения угла поворота по оси X")]
    public Vector2 angleX = new Vector2(45, -45);
    [Header("Ограничения угла поворота по оси Y")]
    public Vector2 angleY = new Vector2(0, 0);

    [Header("Скорость вращения")]
    public float speedLook = 60;
    [Header("Инверсия оси Y")]
    public float speedMove = 30;

    [Header("Скорость движения")]
    public bool invertY = false;

    CharacterController characterController;
    Vector3 lookDirection;
    Vector3 moveDirection;
    private float clampX, clampY;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    private void GetInput()
    {
        lookDirection = Vector3.zero;
        moveDirection = Vector3.zero;

        if (onlyMouseHide)
        {
            if (Cursor.visible)
                return;
        }

        lookDirection.x = Input.GetAxis("Mouse Y") * speedLook * Time.deltaTime;
        lookDirection.y = Input.GetAxis("Mouse X") * speedLook * Time.deltaTime;

        moveDirection.x = Input.GetAxis("Roll") * speedMove * Time.deltaTime;
        moveDirection.z = Input.GetAxis("Pitch") * speedMove * Time.deltaTime;
        moveDirection.y = Input.GetAxis("Yaw") * speedMove * Time.deltaTime;
    }

    private void Look(Vector3 lookDirection)
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

    private void Move(Vector3 moveDirection)
    {
        characterController.Move(transform.TransformDirection( moveDirection));
    }

    private void LateUpdate()
    {
        GetInput();
        Look(lookDirection);
        Move(moveDirection);
    }

    private int ToInt(bool value)
    {
        if (value)
        {
            return -1;
        }
        return 1;
    }

}
