using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
public class AnimationManager : MonoBehaviour
{
    private Animator _animator;
    private SpriteRenderer _renderer;

    private const string idle = "Idle";
    private const string move = "Move";
    private const string attack = "Attack";
    private const string death = "Death";
    private const string staticState = "Static";
    public const float deathTime = 1.82f;

    public bool isStatic = false;
    private static readonly int Static1 = Animator.StringToHash(staticState);
    private static readonly int Death1 = Animator.StringToHash(death);
    private static readonly int Move = Animator.StringToHash(move);
    private static readonly int Attack1 = Animator.StringToHash(attack);
    private static readonly int Idle1 = Animator.StringToHash(idle);

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
        if (!isStatic)
        {
            ResetTriggers();
            _animator.SetTrigger(Death1);
            Invoke(nameof(Hide), deathTime);
        }
        else
        {
            Hide();
        }
     
     
    }

    [ContextMenu("Idle")]
    public void Idle()
    {
        if (!isStatic)
        {
            ResetTriggers();
            _animator.SetTrigger(Idle1);
        }
    }

    [ContextMenu("Walk")]
    public void Walk()
    {
        if (!isStatic)
        {
            ResetTriggers();
            _animator.SetTrigger(Move);
        }
    }

    [ContextMenu("Attack")]
    public void Attack()
    {
        if (!isStatic)
        {
            ResetTriggers();
            _animator.SetTrigger(Attack1);
        }
    }

    private void ResetTriggers()
    {
        _animator.ResetTrigger(Idle1);
        _animator.ResetTrigger(Attack1);
        _animator.ResetTrigger(Move);
        _animator.ResetTrigger(Death1);
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

    public void Show()
    {
        _renderer.color = Color.white;
    }

    public void SetStatic(bool visualsActive)
    {
        isStatic = !visualsActive;
        _animator.SetBool(Static1, isStatic);
    }
}
