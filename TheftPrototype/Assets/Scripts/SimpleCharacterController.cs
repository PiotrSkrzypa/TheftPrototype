using System;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class SimpleCharacterController : MonoBehaviour
{
    [SerializeField] GameEvents _gameEvents;
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Mouse Look")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float mouseSensitivity = 100f;


    private CharacterController _controller;
    private float _xRotation = 0f;
    private Vector3 _velocity;
    private bool _isGrounded;

    void Start()
    {
        _controller = GetComponent<CharacterController>();

        if (cameraTransform == null)
            cameraTransform = Camera.main.transform;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        _gameEvents.OnTheftComplete += DisableController;
        _gameEvents.OnTheftFailed += DisableController;
    }

    private void OnDestroy()
    {
        _gameEvents.OnTheftComplete -= DisableController;
        _gameEvents.OnTheftFailed -= DisableController;
    }

    private void DisableController()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        enabled = false;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        _xRotation -= mouseY;
        _xRotation = Mathf.Clamp(_xRotation, -90f, 90f);

        cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        _isGrounded = _controller.isGrounded;
        if (_isGrounded && _velocity.y < 0)
            _velocity.y = 0f;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 horizontalMovement = transform.right * x + transform.forward * z;
        horizontalMovement *= moveSpeed;

        if (Input.GetButtonDown("Jump") && _isGrounded)
            _velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        _velocity.y += gravity * Time.deltaTime;

        Vector3 movement = (horizontalMovement + _velocity) * Time.deltaTime;
        _controller.Move(movement);
    }
}