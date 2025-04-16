using System;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [Header("Other Controllers")]
    public InputController inputController;

    [Header("Game Objects")]
    public Rigidbody rb;
    public WheelData[] wheels;

    [Header("Car Settings")]
    public CarDriveType carDriveType;
    [Range(350f, 1000f)]
    public float carMass = 350f;
    [Range(35f, 100f)]
    public float wheelMass = 35f;
    [Range(0.5f, 2f)]
    public float frontWheelTraction = 1f;
    [Range(0.5f, 2f)]
    public float backWheelTraction = 1f;

    public float turnSenstivity = 30f;

    [Header("Drive Settings")]
    [Range(7f, 12f)]
    public float maxSpeed = 10f;
    [Range(5f, 15f)]
    public float accelerationSpeed = 5f;
    [Range(1500f, 3000f)]
    public float motorForce = 1500f;

    [Header("Brakes Settings")]
    [Range(0f, 10f)]
    public float frontBrakeSensitivity = 5f;
    [Range(0f, 10f)]
    public float backBrakeSensitivity = 5f;
    [Range(4000f, 10000f)]
    public float brakePower = 5000f;

    [Header("Drift Settings")]
    [Range(0f, 1f)]
    public float frontDriftFactor = 0.55f;
    [Range(0f, 1f)]
    public float backDriftFactor = 0.45f;
    public float driftDuration = 1f;

    // Private Variables
    private Vector2 movementInput = Vector2.zero;

    private float currentSpeed = 0f;
    private bool isBraking = false;

    private float driftTimer;
    private bool isDrifting = false;

    private void Start()
    {
        SetDefaultValues();
        SetWheelDefaultStiffness();

        ApplyInputs();
    }
    private void SetDefaultValues()
    {
        rb.mass = carMass;

        foreach (var wheel in wheels)
        {
            wheel.wheelCollider.mass = wheelMass;
        }
    }
    private void SetWheelDefaultStiffness()
    {
        foreach (var wheel in wheels)
        {
            float defaultStiffness = wheel.wheelAxelType == WheelAxelType.FRONT ? frontWheelTraction : backWheelTraction;
            wheel.wheelDefaultStiffness = frontWheelTraction;
        }
    }

    private void ApplyInputs()
    {
        InputControls inputControls = inputController.inputControls;

        inputControls.Car.Movement.performed += ctx => movementInput = ctx.ReadValue<Vector2>();
        inputControls.Car.Movement.canceled += ctx => movementInput = Vector2.zero;

        inputControls.Car.Brake.performed += ctx =>
        {
            isBraking = true;

            driftTimer = driftDuration;
            isDrifting = true;
        };
        inputControls.Car.Brake.canceled += ctx => isBraking = false;
    }

    private void FixedUpdate()
    {
        ApplyDrive();
        ApplySteering();
        ApplyBrakes();
        ApplyDrift();
    }

    private void Update()
    {
        UpdateSpeed();
        UpdateDrift();
    }

    private void LateUpdate()
    {
        ApplyWheelAnimation();
    }
    private void ApplyDrive()
    {
        float speed = movementInput.y * accelerationSpeed * Time.fixedDeltaTime * 100;
        float motorTorqueValue = motorForce * speed;

        foreach (var wheel in wheels)
        {
            if (carDriveType == CarDriveType.FRONT_WHEEL)
            {
                if (wheel.wheelAxelType == WheelAxelType.FRONT)
                {
                    wheel.wheelCollider.motorTorque = motorTorqueValue;
                }
            }
            else if (carDriveType == CarDriveType.REAR_WHEEL)
            {
                if (wheel.wheelAxelType == WheelAxelType.REAR)
                {
                    wheel.wheelCollider.motorTorque = motorTorqueValue;
                }
            }
            else
            {
                wheel.wheelCollider.motorTorque = motorTorqueValue;
            }
        }
    }
    private void ApplySteering()
    {
        float targetSteeringAngle = movementInput.x * turnSenstivity;

        foreach (var wheel in wheels)
        {
            if (carDriveType == CarDriveType.FRONT_WHEEL)
            {
                if (wheel.wheelAxelType == WheelAxelType.FRONT)
                {
                    wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, targetSteeringAngle, 0.5f);
                }
            }
            else if (carDriveType == CarDriveType.REAR_WHEEL)
            {
                if (wheel.wheelAxelType == WheelAxelType.REAR)
                {
                    wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, targetSteeringAngle, 0.5f);
                }
            }
            else
            {
                wheel.wheelCollider.steerAngle = Mathf.Lerp(wheel.wheelCollider.steerAngle, targetSteeringAngle, 0.5f);
            }
        }
    }
    private void ApplyBrakes()
    {

        foreach (var wheel in wheels)
        {
            float brakeSensitivity = wheel.wheelAxelType == WheelAxelType.FRONT ? frontBrakeSensitivity : backBrakeSensitivity;
            float newBrakeTorque = brakePower * brakeSensitivity;
            float currentBrakeTorque = isBraking ? newBrakeTorque : 0f;

            wheel.wheelCollider.brakeTorque = currentBrakeTorque;
        }
    }
    private void ApplyDrift()
    {
        foreach (var wheel in wheels)
        {
            WheelFrictionCurve sideWaysFriction = wheel.wheelCollider.sidewaysFriction;
            if (isDrifting)
            {
                float driftFactor = wheel.wheelAxelType == WheelAxelType.FRONT ? frontDriftFactor : backDriftFactor;
                sideWaysFriction.stiffness *= (1 - driftFactor);
            }
            else
            {
                sideWaysFriction.stiffness = wheel.wheelDefaultStiffness;
            }
            wheel.wheelCollider.sidewaysFriction = sideWaysFriction;
        }
    }

    private void UpdateSpeed()
    {
        rb.linearVelocity = Vector3.ClampMagnitude(rb.linearVelocity, maxSpeed);
        currentSpeed = rb.linearVelocity.magnitude;
    }
    private void UpdateDrift()
    {
        driftTimer -= Time.deltaTime;
        if (driftTimer < 0)
        {
            isDrifting = false;
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
    public WheelAxelType wheelAxelType;
    public Transform wheelTransform;
    public WheelCollider wheelCollider;
    public float wheelDefaultStiffness = 1f;
}

public enum WheelAxelType
{
    FRONT,
    REAR,
}

public enum CarDriveType
{
    FRONT_WHEEL,
    REAR_WHEEL,
    ALL_WHEEL,
}