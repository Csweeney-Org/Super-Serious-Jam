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
    /// <summary>
    /// Helper method to ApplyForce, but this is specifically for player/AI controlled movement inputs 
    /// where the magnitude of the force should be the characters acceleration
    /// </summary>
    /// <param name="direction"></param>
    public void ApplyMovementForce(Vector3 direction)
    {
        if (isToppled)
            return; 
        
        // Adjust movement speed dynamically based on TotalWeight
        float dynamicMaxSpeed = Mathf.Max(1f, maxSpeed - (Inventory.TotalWeight * WeightSpeedPenalty));

        if (rigidBody.linearVelocity.magnitude < maxSpeed || Vector3.Dot(direction, rigidBody.linearVelocity) < 0)
        {
            //Prevent further player input driven force unless it is in a direction that will result in slower velocity
            ApplyForce(direction, Speed);
        }
        else
        {
            //This might get spammy, enable it when you need to debug
            //Debug.Log($"Movement has been input for controller {gameObject.name} but further movement would exceed maximum speed. Ignoring");
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
