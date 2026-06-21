using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour, InputSystem_Actions.IPlayerActions
{
    public SpinCharacterController playerCharacter;
    private InputSystem_Actions playerControls;
    private void Awake()
    {
        playerControls = new InputSystem_Actions();
        playerControls.Player.SetCallbacks(this);
    }

    private void OnEnable()
    {
        playerControls.Enable();
    }
    private void OnDisable()
    {
        playerControls.Disable();
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
    }

    public void OnNext(InputAction.CallbackContext context)
    {
    }

    public void OnPrevious(InputAction.CallbackContext context)
    {
    }

    public Vector2 MovementDirection = Vector2.zero;
    public void OnMove(InputAction.CallbackContext ctx)
    {
        MovementDirection = ctx.ReadValue<Vector2>().normalized;
    }

    public void FixedUpdate()
    {
        playerCharacter.ApplyForce(new Vector3(MovementDirection.x, 0, MovementDirection.y), magnitude: Time.fixedDeltaTime * 10f);
    }
    public void OnValidate()
    {
#if UNITY_EDITOR
        //This unity editor preprocessor check prevents any misfires during build and publish steps
        //We check if the object is in a valid scene to prevent errors fired on the prefab definition, which itself does not know how to resolve a playerCharacter
        if (gameObject.scene.IsValid() && playerCharacter == null) Debug.LogError($"Player Input Controller does not have a linked player controller!!");
#endif
    }
}