using UnityEngine;

public partial class SecurityCamera
{
    class PotentialTarget
    {
        public GameObject LinkedGO;
        public bool InFOV;
        public float DetectionLevel;
        public bool OnDetectedEventSent;
    }
}