using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerHitAnimator : MonoBehaviour
{
    public Animator animator;
    public InputActionProperty hitAction;
    public string triggerName = "HitTrigger";

    void OnEnable()
    {
        if (hitAction.action != null)
        {
            hitAction.action.performed += OnHit;
            hitAction.action.Enable();
        }
    }

    void OnDisable()
    {
        if (hitAction.action != null) {
            hitAction.action.performed -= OnHit;
            hitAction.action.Disable();
        }
    }
    
    private void OnHit(InputAction.CallbackContext ctx) {
        if (!animator) return;
        animator.ResetTrigger(triggerName);
        animator.SetTrigger(triggerName);
    }
}
