using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class PlayerControl : MonoBehaviour
{
    [Header("이동 설정")]
    public float moveSpeed = 10.0f;
    public float jumpForce = 5.0f;
    public LayerMask groundLayer;

    // [추가됨] 중력 배수 설정 (기본 1.0, 2.5 정도면 묵직하게 떨어짐)
    [Header("중력 설정")]
    public float gravityMultiplier = 2.5f;

    [Header("엔더 진주 설정")]
    public Throw pearlPrefab;
    public Transform throwPoint;

    [Header("던지기 설정")]
    public float minThrowForce = 10f;
    public float maxThrowForce = 40f;
    public float maxChargeTime = 2.0f;

    [Header("안전 장치")]
    public float minHoldTime = 1.0f;
    public float maxTeleportRange = 30.0f;

    [Header("공중 제한 설정")]
    public int maxAirThrows = 2;
    private int currentAirThrows = 0;

    [Header("시선 설정")]
    public float lookSpeed = 2.0f;
    public float lookXLimit = 60.0f;
    public Transform cameraTransform;

    Rigidbody rb;
    Vector2 rotation = Vector2.zero;
    bool isGrounded = false;
    float currentChargeTime = 0f;
    bool isCharging = false;
    private static bool canMove = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (cameraTransform == null) cameraTransform = Camera.main.transform;
        if (throwPoint == null) throwPoint = cameraTransform;

        rotation.y = transform.eulerAngles.y;
        if (cameraTransform != null) rotation.x = cameraTransform.localEulerAngles.x;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.linearDamping = 0f;
    }

    void Update()
    {
        if (!canMove) return;

        RotationLogic();
        CheckGround();
        HandleThrow();

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Vector3 vel = rb.linearVelocity;
            vel.y = jumpForce;
            rb.linearVelocity = vel;
        }
    }

    void FixedUpdate()
    {
        MoveLogicSnappy();
        ApplyExtraGravity(); // [추가됨] 추가 중력 적용 함수 호출
    }

    // [추가됨] 유니티 기본 중력에 더해 추가적인 힘을 가함
    void ApplyExtraGravity()
    {
        // 땅에 없을 때만 더 강하게 당김
        if (!isGrounded)
        {
            // 기본 중력 * (배수 - 1) 만큼의 힘을 추가로 가함
            // ForceMode.Acceleration을 사용하여 질량과 무관하게 적용
            Vector3 extraGravityForce = Physics.gravity * (gravityMultiplier - 1.0f);
            rb.AddForce(extraGravityForce, ForceMode.Acceleration);
        }
    }

    void HandleThrow()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (isGrounded || currentAirThrows < maxAirThrows)
            {
                isCharging = true;
                currentChargeTime = 0f;
            }
        }

        if (isCharging && Input.GetMouseButton(0))
        {
            currentChargeTime += Time.deltaTime;
        }

        if (isCharging && Input.GetMouseButtonUp(0))
        {
            if (currentChargeTime >= minHoldTime)
            {
                ThrowPearl();
            }

            isCharging = false;
            currentChargeTime = 0f;
        }
    }

    void ThrowPearl()
    {
        if (pearlPrefab == null) return;

        if (!isGrounded)
        {
            currentAirThrows++;
        }

        float ratio = Mathf.Clamp01(currentChargeTime / maxChargeTime);
        float finalForce = Mathf.Lerp(minThrowForce, maxThrowForce, ratio);

        Throw pearl = Instantiate(pearlPrefab, throwPoint.position + throwPoint.forward, Quaternion.identity);

        Vector3 throwDir = cameraTransform.forward;
        pearl.Initialize(this, throwDir * finalForce, maxTeleportRange);
    }

    public void TeleportTo(Vector3 position)
    {
        rb.MovePosition(position);

        Vector3 vel = rb.linearVelocity;
        vel.y = 0;
        rb.linearVelocity = vel;
    }

    void MoveLogicSnappy()
    {
        if (canMove)
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical");
            float currentSpeed = moveSpeed;

            Vector3 camFwd = cameraTransform.forward;
            Vector3 camRight = cameraTransform.right;
            camFwd.y = 0; camRight.y = 0;
            camFwd.Normalize(); camRight.Normalize();

            Vector3 moveDir = (camFwd * v + camRight * h).normalized;
            if (Input.GetKey(KeyCode.LeftShift)) currentSpeed += moveSpeed * 1.2f;
            Vector3 targetVel = moveDir * currentSpeed;

            // 기존 속도의 Y값을 유지
            targetVel.y = rb.linearVelocity.y;

            rb.linearVelocity = targetVel;
        }
        else { rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0); }
    }

    void RotationLogic()
    {
        if (Player.isPlayerLockOn)
        {
            rotation.y = cameraTransform.eulerAngles.y;

            float currentX = cameraTransform.localEulerAngles.x;
            if (currentX > 180) currentX -= 360;
            rotation.x = currentX;
            transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);
            cameraTransform.localRotation = Quaternion.Euler(rotation.x, 0f, 0f);

            return;
        }
        rotation.y += Input.GetAxis("Mouse X") * lookSpeed;
        rotation.x += -Input.GetAxis("Mouse Y") * lookSpeed;
        rotation.x = Mathf.Clamp(rotation.x, -lookXLimit, lookXLimit);

        transform.rotation = Quaternion.Euler(0f, rotation.y, 0f);
        if (cameraTransform != null)
            cameraTransform.localRotation = Quaternion.Euler(rotation.x, 0f, 0f);
    }

    void CheckGround()
    {
        bool wasGrounded = isGrounded;
        isGrounded = Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 1.1f, groundLayer);

        if (isGrounded && !wasGrounded)
        {
            currentAirThrows = 0;
        }
        else if (isGrounded)
        {
            currentAirThrows = 0;
        }
    }

    public static void setPlayerMove(bool onOff)
    {
        canMove = onOff;
        if (onOff == false)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
    }
}