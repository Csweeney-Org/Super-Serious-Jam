using System.ComponentModel;
using UnityEngine;

namespace Assets.Scripts.SpinPhysics
{
    public class Spinner : MonoBehaviour
    {
        [field: SerializeField, Description("Angular speed in degrees per second")]
        public float SpinSpeed { get; private set; }
        private Vector3 RotationVector = Vector3.zero;

        private void Awake()
        {
            SetSpinSpeed(SpinSpeed);//Forces initial update of the rotation vector
        }

        public void SetSpinSpeed(float newSpeed)
        {
            SpinSpeed = newSpeed;
            RotationVector = new Vector3(0, newSpeed, 0);
        }

        private void Update()
        {
            this.transform.Rotate(RotationVector * Time.deltaTime);
        }
    }
}