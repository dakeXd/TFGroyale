using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dynamite : Poolable
{
    public readonly float speed = 5f;
    [NonSerialized]  public int damage;
    [NonSerialized]  public bool blueSide;
    private bool _enabled;
    [NonSerialized]  public float maxDistance;
    private float _elapsedDistance;
    //public Explosion explosion;


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
            Explosion();
        }
        int mult = blueSide ? 1 : -1;
        transform.Translate(new Vector3(speed * mult * Time.fixedDeltaTime, 0, 0));
        _elapsedDistance += speed * mult * Time.fixedDeltaTime;

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
            Explosion();
        }else if (collision.CompareTag("Building"))
        {
            var tower = collision.gameObject.GetComponentInParent<Tower>();
            if (tower.blueSide == blueSide)
                return;
            Explosion();
        }
    }

    public void Explosion()
    {
        if (!_enabled)
            return;
        //var item = Instantiate(explosion, transform.position, transform.rotation);
        var item = (Explosion)GameManager.Instance.ExplosionPooler.GetItem(); 
        item.transform.position = transform.position;
        item.transform.rotation = transform.rotation;
        item.transform.localScale = transform.localScale;
        item.blueSide = blueSide;
        item.damage = damage;
        Remove();
    }
}
