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
        [SerializeField, Tooltip("Maximal distance for aimassist")]
        private float MaxAimAssistDistance = 200f;
        [SerializeField, Tooltip("Activate Aimassist")]
        private bool aimAssistActive = true;

        private float effectiveSpeed;
        

        public Vector3 CurrentAimDirection => transform.forward;

        // Keeps rotating around the attached entity at a constant speed
        // CurrentAimDirection can be queried by other scripts at any time

        private void Update()
        {
            if (aimAssistActive)
                effectiveSpeed = ApplyAimAssist(indicatorSpeed);
            else
                effectiveSpeed = indicatorSpeed;
                
            if (displayIndicator)
                transform.Rotate(0, rotationDir * ApplyAimAssist(indicatorSpeed) * Time.deltaTime, 0);    
            print($"Current speed is: {ApplyAimAssist(indicatorSpeed)}");

        }

        public bool EvaluateTarget(SpinCharacterController shooter, SpinCharacterController target, float alignmentThreshold)
        {
            if (target == null || shooter == null)
                return false;
            
            // Relative velocity to target
            Vector3 relativeVelocity = target.rigidBody.linearVelocity - shooter.rigidBody.linearVelocity;

            // Estimate travel time of projectile 
            float distance = Vector3.Distance(shooter.transform.position, target.transform.position);
            //TODO: Add reference to launch speed defined in Projectile instead of magic number (carfull division by 0)
            float timeToImpact = distance / 10f;

            // Predict new target position
            Vector3 predictedTargetPos = target.transform.position + (relativeVelocity * timeToImpact);

            // Flatten Vectors to XZ Plane (Ignore Height)
            Vector3 shooterPosFlat = new Vector3(shooter.transform.position.x, 0, shooter.transform.position.z);
            Vector3 targetPosFlat = new Vector3(predictedTargetPos.x, 0, predictedTargetPos.z);
            Vector3 aimDirFlat = new Vector3(CurrentAimDirection.x, 0, CurrentAimDirection.z).normalized;

            Vector3 directionToTarget = (targetPosFlat - shooterPosFlat).normalized;

            float alignment = Vector3.Dot(aimDirFlat, directionToTarget);
            if (alignment < alignmentThreshold) return false;

            // Check line of sight
            Vector3 rayStart = shooter.transform.position + (Vector3.up * 0.5f);
            Vector3 rayDir = (target.transform.position - shooter.transform.position).normalized;
            //TODO: Raycast function 
            if (Physics.Raycast(rayStart, rayDir, out RaycastHit hit, distance))
            {
                SpinCharacterController hitUnit = hit.collider.GetComponentInParent<SpinCharacterController>();
                if (hitUnit != target) 
                    return false;
            }
            return true;
        }
        
        // Debug feature for the editor, delete in later stages
        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawRay(transform.position, transform.forward * 3f);
        }

        private float ApplyAimAssist(float rotationSpeed)
        {
            Vector3 rayStart = transform.position + (Vector3.up * 0.5f);
            SpinCharacterController selfUnit = GetComponentInParent<SpinCharacterController>();

            //TODO: Remove magic number (thickness of raycast)
            float assistRadius = 1.5f; 

            if (Physics.SphereCast(rayStart, assistRadius, transform.forward, out RaycastHit hit, MaxAimAssistDistance))
            {
                SpinCharacterController hitUnit = hit.collider.GetComponentInParent<SpinCharacterController>();

                // Ensure it only triggers for characters
                if (hitUnit != null && hitUnit != selfUnit)
                    return rotationSpeed * 0.5f; 
                
            }
            
            // Assign normal speed if anything except a character is hit
            return rotationSpeed;
        }
    }
}