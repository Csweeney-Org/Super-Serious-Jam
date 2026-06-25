using UnityEngine;
using Assets.Scripts.SpinPhysics;

public class SpinCharacterController : MonoBehaviour, ICollidable
{
    public Rigidbody rigidBody;
    public ThrowableInventory Inventory;

    [Header("Initialization Properties")]
    [field: SerializeField] public float maxSpeed { get; private set; }
    [field: SerializeField] public float maxTurnRate { get; private set; }
    [field: SerializeField] public Vector3 Velocity { get; private set; } = Vector3.zero;

    [Header("Live Dynamic Properties")]
    [field: SerializeField] public float Mass { get; private set; }
    [field: SerializeField] public float Speed { get; private set; }
    [field: SerializeField] public float Inertia { get; private set; }

    [Header("Topple Meter Properties")] // Tunable for balancing 
    [field: SerializeField] public float MaxToppleHealth { get; private set; } = 100f;
    [field: SerializeField] public float CurrentToppleHealth { get; private set; } = 100f;
    [field: SerializeField] public float RegenDelay { get; private set; } = 10f;
    [field: SerializeField] public float RegenRate { get; private set; } = 10f;
    [field: SerializeField] public float WeightDefenseFactor { get; private set; } = 0.05f; 
    [field: SerializeField] public float WeightSpeedPenalty { get; private set; } = 0.2f;

    [Header("Visual Integrations")]
    [field: SerializeField] public Spinner CharacterSpinner { get; private set; }
    [field: SerializeField] public float MaxVisualSpinSpeed { get; private set; } = 720f;

    private float timeSinceLastHit = 0f;
    private bool isToppled = false;

    private void Start()
    {
        rigidBody.freezeRotation = true;
    }

/// DAMAGE & HEALTH/TOPPLE ///
    private void Update()
    {
        HandleToppleRegeneration();
        UpdateVisualSpin();
    }

    private void HandleToppleRegeneration()
    {
        if (isToppled)
            return;
        
        if (CurrentToppleHealth < MaxToppleHealth)
        {
            // Start timer for regen delay (should be independent from FPS)
            timeSinceLastHit += Time.deltaTime;
            if (timeSinceLastHit >= RegenDelay)
            {
                CurrentToppleHealth += RegenRate * Time.deltaTime;
                // Prevents overhealing of more than max health
                CurrentToppleHealth = Mathf.Min(CurrentToppleHealth, MaxToppleHealth);
            }
        }
    }

    private void UpdateVisualSpin()
    {
        if (CharacterSpinner == null)
            return;
        
        if (isToppled)
        {
            CharacterSpinner.SetSpinSpeed(0f);
            return;
        }

        float healthPercentage = CurrentToppleHealth / MaxToppleHealth;
        CharacterSpinner.SetSpinSpeed(MaxVisualSpinSpeed * healthPercentage);
    }

    /// <summary>
    /// To be called byother scripts to apply damage to entity.
    /// Takes damage value as input, calculates damagereduction from weight and applies damage.
    /// Calls topple or death if no health remains
    /// </summary>
    /// <param name="incomingDamage"></param>
    public void TakeDamage(float incomingDamage)
    {
        if (isToppled)
            return;
        
        // Reset regen timer
        timeSinceLastHit = 0f;

        // Damage calculation factors in weight as damage reduction (1f + weight since it initializes with 0 weight)
        float damageReductionModifier = 1f + (Inventory.TotalWeight * WeightDefenseFactor);
        float actualDamage = incomingDamage / damageReductionModifier; //TODO: change to multiplication to avoid negative values -> devision by 0

        CurrentToppleHealth -= actualDamage;
        Debug.Log($"{gameObject.name} took {actualDamage:F1} damage! Topple Meter: {CurrentToppleHealth:F1}");

        if (CurrentToppleHealth <= 0f)
        {
            // Safeguard against negative values in other components
            CurrentToppleHealth = 0f;
            Topple();
        }
    }

    private void Topple()
    {
        isToppled = true;
        Debug.Log($"{gameObject.name} HAS TOPPLED!");
        //TODO: Disable Movement and open UI / start animation
    }

/// MOVEMENT ///
    /// <summary>
    /// 
    /// </summary>
    /// <param name="direction">Assumes pre-normalized for performance</param>
    /// <param name="magnitude"></param>
    public void ApplyForce(Vector3 direction, float magnitude = 1f)
    {
        rigidBody.AddForce(direction * magnitude, ForceMode.Force);
    }

    public void ApplyMovementForce(Vector3 direction)
    {
        if (isToppled) return; 

        float bumperRadius = 0.5f; 
        float lookAheadDistance = 0.5f; 

        Vector3 castStart = transform.position + (Vector3.up * 0.5f);

        // Should detect walls to prevent sticking to them if movement angle is slightly towards wall
        if (Physics.SphereCast(castStart, bumperRadius, direction, out RaycastHit hit, lookAheadDistance))
        {
            SpinCharacterController hitUnit = hit.collider.GetComponentInParent<SpinCharacterController>();
            if (hitUnit == null && !hit.collider.isTrigger) 
            {
                Vector3 wallNormal = hit.normal;
                wallNormal.y = 0; 
                wallNormal.Normalize();

                if (Vector3.Dot(direction, wallNormal) < 0)
                {
                    direction = Vector3.ProjectOnPlane(direction, wallNormal).normalized;
                }
            }
        }

        float dynamicMaxSpeed = Mathf.Max(1f, maxSpeed - (Inventory.TotalWeight * WeightSpeedPenalty));

        Vector3 desiredVelocity = direction.normalized * dynamicMaxSpeed;
        Vector3 steeringForce = desiredVelocity - rigidBody.linearVelocity;
        steeringForce.y = 0;

        if (steeringForce.sqrMagnitude > 0.1f)
        {
            ApplyForce(steeringForce.normalized, Speed);
        }
    }

/// TRIGGERS ///
    public void OnTriggerEnter(Collider other)
    {
        // Handle collision logic here
    }
    public void OnTriggerExit(Collider other)
    {
        // Handle collision exit logic here
    }

/// VALITDATE (ENSURE EDITOR REFERENCES) ///
    public void OnValidate()
    {
        Inventory ??= GetComponent<ThrowableInventory>();
        CharacterSpinner ??= GetComponentInChildren<Spinner>();
    }
}
