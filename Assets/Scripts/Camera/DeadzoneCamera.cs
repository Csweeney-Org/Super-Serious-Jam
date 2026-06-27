using UnityEngine;

public class DeadzoneCamera : MonoBehaviour
{
    [Header("Targeting")]
    [Tooltip("Drag your Player GameObject here")]
    public Transform PlayerTarget;
    
    [Tooltip("Toggle this off if you want the camera to be completely static again")]
    public bool FollowPlayer = true;

    [Header("Deadzone Properties")]
    [Tooltip("How far the player can move left/right before the camera shifts")]
    public float HorizontalDeadzone = 5f;
    [Tooltip("How far the player can move up/down before the camera shifts")]
    public float VerticalDeadzone = 3f;
    [Tooltip("How smoothly the camera catches up (higher = faster snap)")]
    public float SmoothSpeed = 10f;

    private Vector3 initialOffset;
    private Vector3 currentFocusPoint;

    private void Start()
    {
        if (PlayerTarget == null)
        {
            Debug.LogWarning("DeadzoneCamera has no PlayerTarget assigned!");
            return;
        }

        initialOffset = transform.position - PlayerTarget.position;
        currentFocusPoint = PlayerTarget.position;
    }

    private void LateUpdate()
    {
        if (!FollowPlayer || PlayerTarget == null) return;

        float distanceX = PlayerTarget.position.x - currentFocusPoint.x;
        float distanceZ = PlayerTarget.position.z - currentFocusPoint.z;

        if (Mathf.Abs(distanceX) > HorizontalDeadzone)
            currentFocusPoint.x += Mathf.Sign(distanceX) * (Mathf.Abs(distanceX) - HorizontalDeadzone);
        

        if (Mathf.Abs(distanceZ) > VerticalDeadzone)
            currentFocusPoint.z += Mathf.Sign(distanceZ) * (Mathf.Abs(distanceZ) - VerticalDeadzone);


        Vector3 targetCameraPosition = currentFocusPoint + initialOffset;

        transform.position = Vector3.Lerp(transform.position, targetCameraPosition, SmoothSpeed * Time.deltaTime);
    }

    private void OnDrawGizmos()
    {
        if (PlayerTarget == null) return;

        Gizmos.color = Color.yellow;
        
        Vector3 center = Application.isPlaying ? currentFocusPoint : PlayerTarget.position;
        
        Vector3 size = new Vector3(HorizontalDeadzone * 2, 0.1f, VerticalDeadzone * 2);
        Gizmos.DrawWireCube(center, size);
    }
}