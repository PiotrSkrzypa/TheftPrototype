using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public partial class SecurityCamera : MonoBehaviour
{

    #region Properties
    [SerializeField] GameEvents GameEvents;

    [Header("Camera Rotation")]
    [SerializeField] Transform CameraPivot;
    [SerializeField] Transform ArmPivot;
    [SerializeField] float DefaultPitch = 20f;
    [SerializeField] float MovementYawAngle = 60f;
    [SerializeField] float PatrolRotationSpeed = 6f;
    [SerializeField] float MaxRotationSpeed = 15f;
    [SerializeField] bool FollowSuspiciousTargets = true;

    [Header("Detection")]
    [SerializeField] float DetectionHalfAngle = 30f;
    [SerializeField] float DetectionRange = 20f;
    [SerializeField] SphereCollider DetectionTrigger;
    [SerializeField] Light DetectionLight;
    [SerializeField] Color Colour_NothingDetected = Color.green;
    [SerializeField] Color Colour_FullyDetected = Color.red;
    [SerializeField] float DetectionBuildRate = 0.5f;
    [SerializeField] float DetectionDecayRate = 0.5f;
    [SerializeField] [Range(0f, 1f)] float SuspicionThreshold = 0.5f;
    [SerializeField] List<string> DetectableTags;
    [SerializeField] LayerMask DetectionLayerMask = ~0;

    [Header("Events")]
    [SerializeField] UnityEvent<GameObject> OnDetected = new UnityEvent<GameObject>();
    [SerializeField] UnityEvent OnAllClear = new UnityEvent();

#if UNITY_EDITOR
    [Header("DEBUG")]
    [Tooltip("Enable this to see debug gizmos")]
    [SerializeField] bool DebugMode = false; 
    [SerializeField] int DetectionResolution = 3;
#endif 
    #endregion

    public GameObject CurrentlyDetectedTarget { get; private set; }
    public bool HasDetectedTarget { get; private set; } = false;

    float CurrentAngle = 0f;
    float CosDetectionHalfAngle;
    bool SweepClockwise = true;

    Dictionary<GameObject, PotentialTarget> AllTargets = new Dictionary<GameObject, PotentialTarget>();

    void Start()
    {
        ConfigureCameraLight();
        DetectionTrigger.radius = DetectionRange;

        CosDetectionHalfAngle = Mathf.Cos(Mathf.Deg2Rad * DetectionHalfAngle);
    }

    private void ConfigureCameraLight()
    {
        DetectionLight.color = Colour_NothingDetected;
        DetectionLight.range = DetectionRange;
        DetectionLight.spotAngle = DetectionHalfAngle * 2f;
        DetectionLight.innerSpotAngle = DetectionHalfAngle * 2f;
    }

    void Update()
    {
        RefreshTargetInfo();
        HandleCameraRotation();

#if UNITY_EDITOR
        if (DebugMode)
        {
            DrawDebugCone();
        }
#endif

    }

    private void OnTriggerEnter(Collider other)
    {
        TryAddPotentialTarget(other);

    }

    private void TryAddPotentialTarget(Collider other)
    {
        if (!DetectableTags.Contains(other.tag))
            return;

        AllTargets[other.gameObject] = new PotentialTarget() { LinkedGO = other.gameObject };
    }

    private void OnTriggerExit(Collider other)
    {
        TryRemovePotentialTarget(other);
    }

    private void TryRemovePotentialTarget(Collider other)
    {
        if (!DetectableTags.Contains(other.tag))
            return;

        AllTargets.Remove(other.gameObject);
    }

    private void HandleCameraRotation()
    {
        Quaternion desiredPivotRotation = ArmPivot.transform.rotation;
        Quaternion desiredCameraRotation = CameraPivot.transform.rotation;

        if (FollowSuspiciousTargets && CurrentlyDetectedTarget != null && AllTargets[CurrentlyDetectedTarget].DetectionLevel >= SuspicionThreshold)
        {
            CalculateRotationTowardsSuspicion(ref desiredPivotRotation, ref desiredCameraRotation);
        }
        else
        {
            CalculatePatrolRotation(out desiredPivotRotation, out desiredCameraRotation);
        }
        ArmPivot.transform.rotation = Quaternion.RotateTowards(ArmPivot.transform.rotation,
                                                                 desiredPivotRotation,
                                                                 MaxRotationSpeed * Time.deltaTime);
        CameraPivot.transform.rotation = Quaternion.RotateTowards(CameraPivot.transform.rotation,
                                                                 desiredCameraRotation,
                                                                 MaxRotationSpeed * Time.deltaTime);
    }

    private void CalculateRotationTowardsSuspicion(ref Quaternion desiredPivotRotation, ref Quaternion desiredCameraRotation)
    {
        if (AllTargets[CurrentlyDetectedTarget].InFOV)
        {
            var vecToTarget = (CurrentlyDetectedTarget.transform.position -
                                   ArmPivot.transform.position).normalized;
            var vecToTargetXZ = new Vector3(vecToTarget.x, 0f, vecToTarget.z).normalized;
            desiredCameraRotation = Quaternion.LookRotation(vecToTarget, Vector3.up);
            desiredPivotRotation = Quaternion.LookRotation(vecToTargetXZ, Vector3.up);
        }
    }

    private void CalculatePatrolRotation(out Quaternion desiredPivotRotation, out Quaternion desiredCameraRotation)
    {
        CurrentAngle += PatrolRotationSpeed * Time.deltaTime * ( SweepClockwise ? 1f : -1f );
        if (Mathf.Abs(CurrentAngle) >= ( MovementYawAngle * 0.5f ))
            SweepClockwise = !SweepClockwise;

        desiredPivotRotation = ArmPivot.transform.parent.rotation * Quaternion.Euler(0f, CurrentAngle, 0f);
        desiredCameraRotation = CameraPivot.transform.parent.rotation * Quaternion.Euler(DefaultPitch, 0f, 0f);
    }


    void RefreshTargetInfo()
    {
        float highestDetectionLevel = 0f;
        CurrentlyDetectedTarget = null;

        foreach (var target in AllTargets)
        {
            var targetInfo = target.Value;

            CheckTargetVisibility(targetInfo);
            HandleTargetDetectionLevel(targetInfo);

            if (targetInfo.DetectionLevel > highestDetectionLevel)
            {
                highestDetectionLevel = targetInfo.DetectionLevel;
                CurrentlyDetectedTarget = targetInfo.LinkedGO;
            }
        }

        if (CurrentlyDetectedTarget != null)
            DetectionLight.color = Color.Lerp(Colour_NothingDetected, Colour_FullyDetected, highestDetectionLevel);
        else
        {
            DetectionLight.color = Colour_NothingDetected;

            if (HasDetectedTarget)
            {
                HasDetectedTarget = false;
                OnAllClear.Invoke();
            }
        }
    }

    private bool CheckTargetVisibility(PotentialTarget targetInfo)
    {
        bool isVisible = false;
        Vector3 vecToTarget = (targetInfo.LinkedGO.transform.position -
                                   CameraPivot.transform.position).normalized;
        if (Vector3.Dot(CameraPivot.transform.forward, vecToTarget) >= CosDetectionHalfAngle)
        {
            RaycastHit hitInfo;
            if (Physics.Raycast(CameraPivot.transform.position, vecToTarget,
                                out hitInfo, DetectionRange, DetectionLayerMask, QueryTriggerInteraction.Ignore))
            {
                if (hitInfo.collider.gameObject == targetInfo.LinkedGO)
                    isVisible = true;
            }
        }
        targetInfo.InFOV = isVisible;
        return isVisible;
    }

    private void HandleTargetDetectionLevel(PotentialTarget targetInfo)
    {
        if (targetInfo.InFOV)
        {
            targetInfo.DetectionLevel = Mathf.Clamp01(targetInfo.DetectionLevel + DetectionBuildRate * Time.deltaTime);

            if (targetInfo.DetectionLevel >= 1f && !targetInfo.OnDetectedEventSent)
            {
                HasDetectedTarget = true;
                targetInfo.OnDetectedEventSent = true;
                OnDetected.Invoke(targetInfo.LinkedGO);
                GameEvents.OnTheftFailed.Invoke();
            }
        }
        else
        {
            targetInfo.DetectionLevel = Mathf.Clamp01(targetInfo.DetectionLevel - DetectionDecayRate * Time.deltaTime);
            if (targetInfo.DetectionLevel < SuspicionThreshold)
            {
                targetInfo.OnDetectedEventSent = false;
            }
        }
    }

#if UNITY_EDITOR
    private void DrawDebugCone()
    {
        Vector3 origin = CameraPivot.transform.position;
        Quaternion baseRotation = CameraPivot.transform.rotation;
        float halfAngleRad = Mathf.Deg2Rad * DetectionHalfAngle;

        // Generate square grid from -1 to 1 in both axes
        for (int y = 0; y < DetectionResolution; y++)
        {
            float v = Mathf.Lerp(-1f, 1f, y / (float)(DetectionResolution - 1));
            for (int x = 0; x < DetectionResolution; x++)
            {
                float h = Mathf.Lerp(-1f, 1f, x / (float)(DetectionResolution - 1));

                // Skip directions outside the unit circle (preserves circular shape)
                if (h * h + v * v > 1f)
                    continue;

                // Compute angle offset in tangent of cone
                Vector3 localDir = new Vector3(h, v, 1f / Mathf.Tan(halfAngleRad)).normalized;

                // Rotate to world space
                Vector3 worldDir = baseRotation * localDir;

                if (Physics.Raycast(origin, worldDir, out RaycastHit hit, DetectionRange, DetectionLayerMask))
                {
                    if (DetectableTags.Contains(hit.collider.tag))
                    {
                        Debug.DrawRay(origin, worldDir * hit.distance, Color.red);
                    }
                    else
                    {
                        Debug.DrawRay(origin, worldDir * hit.distance, Color.green);
                    }
                }
            }
        }
    }
#endif
}