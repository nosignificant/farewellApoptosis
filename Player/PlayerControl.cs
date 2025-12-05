using UnityEngine;

[RequireComponent(typeof(CharacterController))]

public class PlayerControl : MonoBehaviour
{
    public float speed = 7.5f;
    public float jumpSpeed = 8.0f;
    public float gravity = 20.0f;
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f;

    public Transform cameraTransform;

    CharacterController characterController;
    Vector3 moveDirection = Vector3.zero;
    Vector2 rotation = Vector2.zero;

    // canMove가 false면 이동 입력만 막힙니다.
    private static bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();
        rotation.y = transform.eulerAngles.y;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // 1. 이동 로직 (Movement)
        if (characterController.isGrounded)
        {
            Vector3 forward = transform.TransformDirection(Vector3.forward);
            Vector3 right = transform.TransformDirection(Vector3.right);

            float curSpeedX = canMove ? speed * Input.GetAxis("Vertical") : 0;
            float curSpeedY = canMove ? speed * Input.GetAxis("Horizontal") : 0;

            moveDirection = (forward * curSpeedX) + (right * curSpeedY);

            if (Input.GetButton("Jump") && canMove)
            {
                moveDirection.y = jumpSpeed;
            }
        }

        moveDirection.y -= gravity * Time.deltaTime;

        characterController.Move(moveDirection * Time.deltaTime);


        // 2. 회전 로직 (Rotation)
        // 💡 회전은 canMove 변수의 영향을 받지 않도록 독립적으로 둡니다.
        // 만약 회전도 막고 싶다면 별도의 변수(canRotate)를 쓰거나 조건문을 추가하세요.
        rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
        rotation.x += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);

        transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);

        if (cameraTransform != null)
        {
            cameraTransform.localRotation = Quaternion.Euler(rotation.x, 0f, 0f);
        }
    }

    public static void setPlayerMove(bool onOff)
    {
        canMove = onOff;
    }
}