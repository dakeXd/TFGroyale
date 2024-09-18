using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : Poolable
{
    public readonly float  speed = 5f;
    public int damage;
    public bool blueSide;
    private bool _enabled;
    public float maxDistance;
    private float _elapsedDistance;
    

    public override void Active()
    {
        base.Active();
        _enabled = true;
        _elapsedDistance = 0;
    }
    
    
    public override void Deactivate()
    {
        base.Deactivate();
        _enabled = false;
    }

    private void FixedUpdate()
    {
        if (!enabled)
            return;

        if (_elapsedDistance >= maxDistance)
        {
            Remove();
        }
        int mult = blueSide ? 1 : -1;
        transform.Translate(new Vector3(speed * mult * Time.fixedDeltaTime, 0, 0));
        _elapsedDistance += Mathf.Abs( speed * mult * Time.fixedDeltaTime);
       
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!_enabled)
            return;
        if (collision.CompareTag("BlockUnit"))
        {
            var minion = collision.gameObject.GetComponentInParent<MinionController>();
            if (minion.blueSide == blueSide)
                return;
            minion.Damage(damage, 0.1f);
            Remove();
        }else if (collision.CompareTag("Building"))
        {
            var tower = collision.gameObject.GetComponentInParent<Tower>();
            if (tower.blueSide == blueSide)
                return;
            tower.Damage(damage, 0.1f);
            Remove();
        }
    }
}
