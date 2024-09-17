using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Tower : MonoBehaviour
{
    public bool blueSide;
    public UnityAction onDamage, onDestroy;
    [NonSerialized] public int life;
    public SpriteRenderer sprite;
    public Sprite destroyed, completed;
    private bool _destroyed;

    private void Start()
    {
       Reset();
    }
    public void Damage(int amount, float timeOffset = 0, MinionController enemy = null)
    {
        if (_destroyed)
            return;
        StartCoroutine(ActualDmg(amount, timeOffset, enemy));
    }

    private IEnumerator ActualDmg(int amount, float timeOffset, MinionController enemy = null)
    {
        yield return new WaitForSeconds(timeOffset);
        if (_destroyed)
            yield break;
        if (enemy != null)
        {
            if (enemy.IsDead())
                yield break;
        }
        sprite.color = Color.red;
        
        life -= amount;
        life = life < 0 ? 0 : life;
        onDamage?.Invoke();
        if (life <= 0)
        {
            _destroyed = true;
            sprite.sprite = destroyed;
            GetComponent<BoxCollider2D>().enabled = false;
            onDestroy?.Invoke();
        }
        yield return new WaitForSeconds(0.15f);
        sprite.color = Color.white;

    }

    public void Reset()
    {
        _destroyed = false;
        sprite.sprite = completed;
        GetComponent<BoxCollider2D>().enabled = true;
    }

    public bool IsDestroyed()
    {
        return _destroyed;
    }
}
