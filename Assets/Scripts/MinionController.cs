using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MinionController : MonoBehaviour
{

    public Minion stats;
    public AnimationManager animator;
    public Rigidbody2D rb;
    public bool blueSide;
    private int _actualLife;
    public readonly float Speed = 0.9f; 
    public readonly float allyDetectDistnace = 0.5f;
    //private Vector3 _lastPos;
    public LayerMask attackObjetives;
    private bool _dead, _moving, _attacking;
    public Arrow arrowPrefab;
    public Dynamite dynamitePrefab;
    private readonly float arrowTime = 0.5f;
    private readonly float dynamiteTime = 0.4f;
    void Start()
    {
        animator.SetAnimator(blueSide ? stats.bodyBlueSide : stats.bodyRedSide);
        _actualLife = stats.maxLife;
        _dead = _moving = _attacking = false;
        //_lastPos = transform.position;
    }

    
    // Update is called once per frame
    void FixedUpdate()
    {
        if (_dead)
            return;

        WalkAction();
        CheckEnemies();
    }

    public void WalkAction()
    {
        if (_attacking || _dead)
            return;
        if (CheckCanWalk())
        {
            Movement();
            if (!_moving)
            {
                _moving = true;
                animator.Walk();
            }
        }
        else
        {
            if (_moving)
            {
                _moving = false;
                animator.Idle();
            }
        }
    }

    public bool CheckCanWalk()
    {
        if (_attacking || _dead)
            return false;
        Vector2 rayStart = new Vector2(transform.position.x, 0.5f);
        var hitAlly = Physics2D.RaycastAll(rayStart, blueSide ? Vector2.right : Vector2.left, allyDetectDistnace, attackObjetives);
        foreach (var allyHit in hitAlly)
        {
            if (allyHit.transform.CompareTag("BlockUnit"))
            {
                var minion = allyHit.transform.gameObject.GetComponentInParent<MinionController>();
                if (minion.blueSide != blueSide)
                {
                    return false;
                }
                if (blueSide)
                {
                    if (allyHit.transform.position.x > transform.position.x)
                    {
                        return false;
                    }
                }
                else
                {
                    if (allyHit.transform.position.x < transform.position.x)
                    {
                        return false;
                    }
                }
            }
            else if (allyHit.transform.CompareTag("Building"))
            {
                if (allyHit.transform.gameObject.GetComponent<Tower>().blueSide != blueSide)
                    return false;
            }
        }
        return true;
    }
    public bool CheckEnemies()
    {

        Vector2 rayStart = new Vector2(transform.position.x, 0.5f);
        var hit = Physics2D.RaycastAll(rayStart, blueSide ? Vector2.right : Vector2.left, stats.range, attackObjetives);
        //Debug.DrawRay(rayStart, blueSide ? Vector2.right : Vector2.left, Color.red);
        
        //Siempre se detectara a su misma
        if(hit.Length <= 1)
        {
            return false;
        }
        if (_attacking)
            return true;
        List<RaycastHit2D> colls = new List<RaycastHit2D>(hit);
        colls.Sort((a, b) => Vector2.Distance(transform.position, a.transform.position).CompareTo(Vector2.Distance(transform.position, b.transform.position)));
        for (int i = 0; i < colls.Count; i++)
        {
            //si son unidades enemigas vivas atacarlas, si no seguir buscando un objetivo
            if (colls[i].transform.CompareTag("BlockUnit"))
            {
                var minion = colls[i].transform.gameObject.GetComponentInParent<MinionController>();
                if (minion.IsDead())
                    continue;
                if (minion.blueSide == blueSide)
                    continue;
                switch (stats.attackType)
                {
                    case Attack.Melee:
                        StartCoroutine(MeleeSingleAttack(minion));
                        break;
                    case Attack.Arrow:
                        StartCoroutine(ArrowAttack());
                        break;
                    case Attack.TNT:
                        StartCoroutine(TNTAttack());
                        break;
                }
             
                break;
            }
            else if (colls[i].transform.CompareTag("Building"))
            {
                var tower = colls[i].transform.gameObject.GetComponentInParent<Tower>();
                if (tower.IsDestroyed())
                    continue;
                if (tower.blueSide == blueSide)
                    continue;
                switch (stats.attackType)
                {
                    case Attack.Melee:
                        StartCoroutine(MeleeSingleAttack(tower));
                        break;
                    case Attack.Arrow:
                        StartCoroutine(ArrowAttack());
                        break;
                    case Attack.TNT:
                        StartCoroutine(TNTAttack());
                        break;
                }

                break;
            }
        }

        return true;
    }





    public void Damage(int amount, float timeOffset = 0, MinionController enemy = null)
    {
        /*
          if (timeOffset == 0)
              ActualDmg(amount);
          else
              Invoke(nameof(ActualDmg), timeOffset);
        */
        StartCoroutine(ActualDmg(amount, timeOffset, enemy));
       
    }

    private IEnumerator ActualDmg(int amount, float timeOffset, MinionController enemy = null)
    {
        yield return new WaitForSeconds(timeOffset);
        if(enemy != null)
        {
            if (enemy.IsDead())
                yield break;
        }
        animator.Damage();
        _actualLife -= amount;
        if (_actualLife <= 0)
        {
         
            yield return new WaitForSeconds(0.05f);
            _dead = true;
            Death();
            yield return new WaitForSeconds(animator.deathTime);
            Destroy(gameObject);
        }
    }

    private void Death()
    {
        animator.Death();
        GetComponentInChildren<BoxCollider2D>().enabled = false;
    }

    public bool IsDead()
    {
        return _dead;
    }
    private void Movement()
    {

        if (_attacking || _dead)
            return;
        Vector2 traslation = new Vector2(Speed * Time.fixedDeltaTime, 0);
        if (!blueSide)
            traslation *= -1;
        rb.MovePosition((Vector2)transform.position + traslation);
    
    }

    /*
    private void CheckWalking()
    {
        if ((transform.position - _lastPos).magnitude < 0.0001f)
        {
            if (_moving)
            {
                _moving = false;
                animator.Idle();
            }
        }
        else
        {
            if (!_moving)
            {
                _moving = true;
                animator.Walk();
            }
        }
        _lastPos = transform.position;
    }
    */

    
    /*
    private void OnTriggerStay2D(Collider2D collision)
    {

       
        if (collision.CompareTag("BlockUnit")){
            _trigger = true;
            MeleeSingleAttack();
        }
        //TODO Building
    }*/

    /*
    private void DrawRect()
    {
        Vector2 size = new Vector2(stats.range, 1);
        Vector2 offset = new Vector2(blueSide ? transform.position.x : (transform.position.x - stats.range), transform.position.y + 0.5f);
        var rect = new Rect(offset, size);
        Debug.DrawLine(new Vector3(rect.x, rect.y), new Vector3(rect.x + rect.width, rect.y), Color.green);
        Debug.DrawLine(new Vector3(rect.x, rect.y), new Vector3(rect.x, rect.y + rect.height), Color.red);
        Debug.DrawLine(new Vector3(rect.x + rect.width, rect.y + rect.height), new Vector3(rect.x + rect.width, rect.y), Color.green);
        Debug.DrawLine(new Vector3(rect.x + rect.width, rect.y + rect.height), new Vector3(rect.x, rect.y + rect.height), Color.red);
    }ç*/
    private IEnumerator MeleeSingleAttack(MinionController minion)
    {
        //Si se esta atacando parar
        if (_attacking || _dead)
            yield break;
        _attacking = true;
        _moving = false;
        animator.Attack();
        minion.Damage(stats.attack, stats.speed / 3, this);
        yield return new WaitForSeconds(stats.speed);
        _attacking = false;
    }

    private IEnumerator MeleeSingleAttack(Tower tower)
    {
        //Si se esta atacando parar
        if (_attacking || _dead)
            yield break;
        _attacking = true;
        _moving = false;
        animator.Attack();
        tower.Damage(stats.attack, stats.speed / 3, this);
        yield return new WaitForSeconds(stats.speed);
        _attacking = false;
    }

    private IEnumerator ArrowAttack()
    {
        //Si se esta atacando parar
        if (_attacking || _dead)
            yield break;
        _attacking = true;
        _moving = false;
        animator.Attack();
        yield return new WaitForSeconds(arrowTime);
        if (_dead)
            yield break;
        var arrow = Instantiate(arrowPrefab, transform.position, transform.rotation, null);
        arrow.transform.localScale = transform.localScale;
        arrow.damage = stats.attack;
        arrow.blueSide = blueSide;
        arrow.maxDistance = stats.range;
        yield return new WaitForSeconds(stats.speed - arrowTime);
        _attacking = false;
   
    }

    private IEnumerator TNTAttack()
    {
        //Si se esta atacando parar
        if (_attacking || _dead)
            yield break;
        _attacking = true;
        _moving = false;
        animator.Attack();
        yield return new WaitForSeconds(dynamiteTime);
        if (_dead)
            yield break;
        var arrow = Instantiate(dynamitePrefab, transform.position, transform.rotation, null);
        arrow.transform.localScale = transform.localScale;
        arrow.damage = stats.attack;
        arrow.blueSide = blueSide;
        arrow.maxDistance = stats.range;
        yield return new WaitForSeconds(stats.speed - dynamiteTime);
        _attacking = false;

    }
}
