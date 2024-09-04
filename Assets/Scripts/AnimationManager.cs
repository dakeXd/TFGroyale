using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationManager : MonoBehaviour
{
    private Animator _animator;
    private SpriteRenderer _renderer;

    private readonly string idle = "Idle";
    private readonly string move = "Move";
    private readonly string attack = "Attack";
    private readonly string death = "Death";
    public readonly float deathTime = 1.82f;
    void Awake()
    {
        _animator = gameObject.GetComponent<Animator>();
        _renderer = gameObject.GetComponent<SpriteRenderer>();
    }

    public void SetAnimator(RuntimeAnimatorController newAnimator)
    {
        _animator.runtimeAnimatorController = newAnimator;
    }

    [ContextMenu("Death")]
    public void Death()
    {
        ResetTriggers();
        _animator.SetTrigger(death);
        Invoke(nameof(Hide), deathTime);
    }

    [ContextMenu("Idle")]
    public void Idle()
    {
        ResetTriggers();
        _animator.SetTrigger(idle);
    }

    [ContextMenu("Walk")]
    public void Walk()
    {
        ResetTriggers();
        _animator.SetTrigger(move);
    }

    [ContextMenu("Attack")]
    public void Attack()
    {
        ResetTriggers();
        _animator.SetTrigger(attack);
    }

    private void ResetTriggers()
    {
        _animator.ResetTrigger(idle);
        _animator.ResetTrigger(attack);
        _animator.ResetTrigger(move);
        _animator.ResetTrigger(death);
    }

    [ContextMenu("Damage")]
    public void Damage()
    {
        _renderer.color = Color.red;
        Invoke(nameof(EndDamage), 0.25f);
    }

    private void EndDamage()
    {
        _renderer.color = Color.white;
    }
    private void Hide()
    {
        _renderer.color = new Color(0f, 0f, 0f, 0f);
    }
}
