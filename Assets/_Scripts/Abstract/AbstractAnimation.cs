using System.Collections;
using UnityEngine;

public abstract class AbstractAnimation : SaiMonoBehaviour
{
    [SerializeField] protected Animator animator;
    protected override void LoadComponents()
    {
        base.LoadComponents();
        this.LoadAnimator();
    }
    protected void LoadAnimator()
    {
        if (animator != null) return;
        animator = transform.GetComponentInChildren<Animator>();
        Debug.Log(transform.name + ": LoadAnimator", gameObject);
    }
    private bool HasParameter(Animator animator, string paramName)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
            {
                return true;
            }
        }
        return false;
    }
    private bool HasParameter(string paramName)
    {
        foreach (AnimatorControllerParameter param in this.animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    protected void PlayAnimation(string animationName, bool state)
    {
        if (HasParameter(animationName))
        {
            this.animator.SetBool(animationName, state);
        }
    }
    protected void ActivateTrigger(string triggerName)
    {
        if (!HasParameter(animator, triggerName))
        {
            return;
        }

        animator.SetTrigger(triggerName);
    }

}
