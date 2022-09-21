using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float mouseSensitivity;
    [SerializeField] float sprintSpeed;
    [SerializeField] float walkSpeed;
    [SerializeField] float jumpForce;
    [SerializeField] float smoothTime;
    [SerializeField] GameObject cameraHolder;

    Rigidbody _rigidbody;
    PhotonView PV;

    float verticalLookRotation;
    bool isGrounded;
    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        PV = GetComponent<PhotonView>();
    }

    private void Update()
    {
        if (!PV.IsMine)
            return;

        Look();
        CalculateMove();
        TryToJump();
    }

    private void FixedUpdate()
    {
        _rigidbody.MovePosition(_rigidbody.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    private void Start()
    {
        if (!PV.IsMine)
            Destroy(GetComponentInChildren<Camera>().gameObject);
    }

    private void Look()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);
        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    private void CalculateMove()
    {
        Vector3 moveDir = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDir * (Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed), ref smoothMoveVelocity, smoothTime);
    }

    private void TryToJump()
    {
        if (Input.GetKey(KeyCode.Space) && isGrounded)
        {
            _rigidbody.AddForce(transform.up * jumpForce);
        }
    }

    public void SetIsGrounded(bool _isGrounded)
    {
        isGrounded = _isGrounded;
    }
}
