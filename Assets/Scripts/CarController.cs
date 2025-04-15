using System;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Other Controllers")]
    public InputController inputController;

    [Header("Game Objects")]
    public Rigidbody rb;
    public WheelData[] wheels;

    public float turnSenstivity = 30f;

    [Header("Car Settings")]
    [Range(7f, 12f)]
    public float maxSpeed = 10f;
    [Range(5f, 15f)]
    public float accelerationSpeed = 5f;
    [Range(1500f, 3000f)]
    public float motorForce = 1500f;

    [Header("Brakes Settings")]
    [Range(4f, 10f)]
    public float brakeSensitivity = 5f;
    [Range(4000f, 10000f)]
    public float brakePower = 5000f;

    // Private Variables
    private Vector2 movementInput = Vector2.zero;
    private float currentSpeed = 0f;
    private bool isBraking = false;

    private void Start()
    {
        InputControls inputControls = inputController.inputControls;

        inputControls.Car.Movement.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        inputControls.Car.Movement.canceled += ctx => movementInput = Vector2.zero;

        inputControls.Car.Brake.performed += ctx => isBraking = true;
        inputControls.Car.Brake.canceled += ctx => isBraking = false;
    }

    private void FixedUpdate()
    {
        ApplyDrive();
        ApplySteering();
        ApplyBrakes();
    }
    private void Update()
    {
        ApplySpeedLimit();
    }
    private void LateUpdate()
    {
        ApplyWheelAnimation();
    }
    private void ApplyDrive()
    {
        currentSpeed = movementInput.y * accelerationSpeed * Time.fixedDeltaTime * 100;
        float motorTorqueValue = motorForce * currentSpeed;

        foreach (var wheel in wheels)
        {
            if (wheel.axelType == AxelType.FRONT)
            {
                wheel.wheelCollider.motorTorque = motorTorqueValue;
            }
        }
    }
    private void ApplySpeedLimit()
    {
        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed);
    }
    private void ApplySteering()
    {
        float targetSteeringAngle = movementInput.x * turnSenstivity;

        foreach (var wheel in wheels)
        {
            if (wheel.axelType == AxelType.FRONT)
            {
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, targetSteeringAngle, 0.5f);
            }
        }
    }
    private void ApplyBrakes()
    {
        float newBrakeTorque = brakePower * brakeSensitivity;
        float currentBrakeTorque = isBraking ? newBrakeTorque : 0f;

        foreach (var wheel in wheels)
        {
            if (wheel.axelType == AxelType.FRONT)
            {
                wheel.wheelCollider.brakeTorque = currentBrakeTorque;
            }
        }
    }

    private void ApplyWheelAnimation()
    {
        foreach (var wheel in wheels)
        {
            Quaternion rotation;
            Vector3 position;
            wheel.wheelCollider.GetWorldPose(out position, out rotation);

            if (wheel.wheelTransform != null)
            {
                wheel.wheelTransform.position = position;
                wheel.wheelTransform.rotation = rotation;
            }
        }
    }
}

[Serializable]
public class WheelData
{
    public AxelType axelType;
    public Transform wheelTransform;
    public WheelCollider wheelCollider;
}

public enum AxelType
{
    FRONT,
    REAR,
}