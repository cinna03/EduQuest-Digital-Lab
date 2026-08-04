using UnityEngine;

namespace OrbitScout.View
{
    public class PlanetOrbit : MonoBehaviour
    {
        public Transform orbitCenter;
        public float orbitRadius = 0.2f;
        public float degreesPerSecond = 18f;
        public float startAngleDegrees;

        float angleRadians;

        void Start()
        {
            angleRadians = startAngleDegrees * Mathf.Deg2Rad;
            ApplyPosition();
        }

        void Update()
        {
            angleRadians += degreesPerSecond * Mathf.Deg2Rad * Time.deltaTime;
            ApplyPosition();
        }

        void ApplyPosition()
        {
            if (orbitCenter == null)
                return;

            Vector3 offset = new Vector3(Mathf.Cos(angleRadians), 0f, Mathf.Sin(angleRadians)) * orbitRadius;
            transform.position = orbitCenter.position + offset;
            transform.LookAt(orbitCenter.position);
        }
    }
}
