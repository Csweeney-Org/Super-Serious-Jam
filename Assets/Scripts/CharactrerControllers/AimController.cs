using UnityEngine;

namespace Assets.Scripts.CharactrerControllers
{
    public class AimController : MonoBehaviour
    {
        [SerializeField, Tooltip("Spinning speed of visual aiming indicator")]
        private float indicatorSpeed = 120f;
        [SerializeField, Tooltip("Can be used to disable aiming indicator for enemy AI")] 
        private bool displayIndicator = true;
        [SerializeField, Tooltip("Default is (-1.0) for clockwise rotation, (1.0) for counter-clockwise")] 
        private float rotationDir = -1f;

        public Vector3 CurrentAimDirection => transform.forward;

        // Keeps rotating around the attached entity at a constant speed
        // CurrentAimDirection can be queried by other scripts at any time

        private void Update()
        {
            if (displayIndicator)
                transform.Rotate(0, rotationDir * indicatorSpeed * Time.deltaTime, 0);                
        }
        
        // Debug feature for the editor, delete in later stages
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * 3f);
        }

        private void ApplyAimAssist()
        {
            //TODO: Add slow down based on dot product to enemy
        }
    }
}