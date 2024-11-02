using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class MinionController : Poolable
{

    public Minion stats;
    public AnimationManager animator;
    public bool blueSide;
    private int _actualLife;
    public readonly float Speed = 1.7f; 
    public readonly float allyDetectDistnace = 0.5f;
    //private Vector3 _lastPos;
    private bool _dead, _moving, _attacking;
    //public Dynamite dynamitePrefab;
    private readonly float arrowTime = 0.5f;
    private readonly float dynamiteTime = 0.4f;
    [NonSerialized] public bool visualsActive = true;

    private GameInstance _instance;
    /*
    void Start()
    {
        animator.SetAnimator(blueSide ? stats.bodyBlueSide : stats.bodyRedSide);
        _actualLife = stats.maxLife;
        _dead = _moving = _attacking = false;
        //_lastPos = transform.position;
    }*/
    
    public override void Active()
    {
        base.Active();
        _dead = _moving = _attacking = false;
        visualsActive = true;
        
        GetComponentInChildren<BoxCollider2D>().enabled = true;
    }

    public override void Deactivate()
    {
        base.Deactivate();
        transform.localScale = Vector3.one;
    }

    public void UpdatePooledParameters()
    {
        _actualLife = stats.maxLife;
        _instance = GetComponentInParent<GameInstance>();
        animator.Show();
        animator.SetAnimator(blueSide ? stats.bodyBlueSide : stats.bodyRedSide);
        animator.SetStatic(visualsActive);
    }

    
    // Update is called once per frame
    void Update()
    {
        if (_dead)
            return;

        CheckEnemies();
        WalkAction();
        
    }

    public void WalkAction()
    {
        if (_attacking || _dead)
            return;
        Transform closest;
        if (CheckCanWalk(out closest))
        {
            Movement(closest);
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

    private Vector3 pos, enemyPos;
    public bool CheckCanWalk(out Transform closest)
    {
        closest = null;
        if (_attacking || _dead)
            return false;
        pos = transform.position;
        /*
        Tower enemyTower = blueSide ?_instance.redSide.tower : _instance.blueSide.tower;
        if (Vector2.Distance(enemyTower.transform.position, transform.position) < allyDetectDistnace)
            return false;
        */
        //Only check allys
        var minionCollection = blueSide ? _instance.blueSideMinions : _instance.redSideMinions;
        MinionController minion;
        float minDistance = Mathf.Infinity;
        for (int i = 0; i < minionCollection.Count; i++)
        {
            minion = minionCollection[i];
            if(minion.IsDead() || minion == this)
                continue;
            enemyPos = minion.transform.position;
            if (blueSide && enemyPos.x > pos.x || !blueSide && enemyPos.x < pos.x)
            {
                var distance = Vector2.Distance(pos, enemyPos);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = minion.transform;
                }
                if (distance <= allyDetectDistnace)
                {
                    return false;
                }
            }
        }
     
        
        /*
        foreach (var minion in _instance.redSideMinions)
        {
            if(minion.IsDead() || minion == this)
                continue;
            var minionPos = minion.transform.position;
            if (blueSide && minionPos.x > pos.x || !blueSide && minionPos.x < pos.x)
            {
                if (Vector2.Distance(pos, minionPos) < allyDetectDistnace)
                    return false;
            }
        }*/
        
        /*
        //Vector2 rayStart = new Vector2(transform.position.x, transform.position.y + 0.75f);
        //var hitAlly = Physics2D.RaycastAll(rayStart, blueSide ? Vector2.right : Vector2.left, allyDetectDistnace, attackObjetives);
        int hits = Physics2D.RaycastNonAlloc(rayStart, blueSide ? Vector2.right : Vector2.left, m_Results, allyDetectDistnace, attackObjetives);
        //Debug.DrawRay(rayStart, blueSide ? Vector2.right : Vector2.left);
        for (int i = 0; i < hits ; i++)
        {
            var hit_t = m_Results[i].transform;
          
            if (hit_t.CompareTag("BlockUnit"))
            {
                var minion = hit_t.gameObject.GetComponentInParent<MinionController>();
                if (minion.blueSide != blueSide)
                {
                    return false;
                }
                if (blueSide)
                {
                    if (hit_t.position.x > transform.position.x)
                    {
                        return false;
                    }
                }
                else
                {
                    if (hit_t.position.x < transform.position.x)
                    {
                        return false;
                    }
                }
            }
            else if (hit_t.CompareTag("Building"))
            {
                if (hit_t.gameObject.GetComponent<Tower>().blueSide != blueSide)
                    return false;
            }else if (hit_t.CompareTag("Block"))
            {
                return false;
            }
        }
        */
        return true;
    }
    public bool CheckEnemies()
    {
        if (_attacking || _dead)
            return false;
        
        pos = transform.position;
        float closestDistance = Mathf.Infinity;
        MinionController closestT = null;
        var minionCollection = blueSide ? _instance.redSideMinions : _instance.blueSideMinions;
        float distance;
        for (int i = 0; i < minionCollection.Count; i++)
        {
            if(minionCollection[i].IsDead())
                continue;
            enemyPos = minionCollection[i].transform.position;
            distance = Vector2.Distance(pos, enemyPos);
            if (distance < closestDistance && distance < stats.range)
            {
                closestDistance = distance;
                closestT = minionCollection[i];
            }
        }

        if (closestT == null)
        {
            Tower enemyTower = blueSide ?_instance.redSide.tower : _instance.blueSide.tower;
            if (Mathf.Abs((enemyTower.transform.position.x + (blueSide ? 0.3f : -0.3f))- pos.x) < stats.range)
            {
                switch (stats.attackType)
                {
                    case Attack.Melee:
                        
                        StartCoroutine(MeleeSingleAttack(enemyTower));
                        break;
                    case Attack.Arrow:
                        StartCoroutine(ArrowAttack());
                        break;
                    case Attack.TNT:
                        StartCoroutine(TNTAttack());
                        break;
                      
                }

                return true;
            }
            return false;
        }
        else
        {
            switch (stats.attackType)
            {
                case Attack.Melee:
                    StartCoroutine(MeleeSingleAttack(closestT));
                    break;
                case Attack.Arrow:
                    StartCoroutine(ArrowAttack());
                    break;
                case Attack.TNT:
                    StartCoroutine(TNTAttack());
                    break;
            }

            return true;
        }
        
        

        /*
        Vector2 rayStart = new Vector2(transform.position.x, transform.position.y + 0.75f);
        //var hit = Physics2D.RaycastAll(rayStart, blueSide ? Vector2.right : Vector2.left, stats.range, attackObjetives);
        //int hits = Physics2D.RaycastNonAlloc(rayStart, blueSide ? Vector2.right : Vector2.left, m_Results, stats.range, attackObjetives);
        var hits = Physics2D.OverlapCircleNonAlloc(rayStart, stats.range, m_ResultsC, attackObjetives);
        //Debug.DrawRay(rayStart, blueSide ? Vector2.right : Vector2.left, Color.red);
   
        //Siempre se detectara a su misma
        if(hits<= 1)
        {
            return false;
        }
        if (_attacking)
            return true;

      */
        //colls.Sort((a, b) => Vector2.Distance(transform.position, a.transform.position).CompareTo(Vector2.Distance(transform.position, b.transform.position)));


        /*
        for (int i = 0; i < hits; i++)
        {
            var hit_t = m_ResultsC[i].transform;
            if (hit_t.CompareTag("BlockUnit"))
            {
                var minion = hit_t.gameObject.GetComponentInParent<MinionController>();
                if (minion.IsDead())
                    continue;
                if (minion.blueSide == blueSide)
                    continue;
            } else if (hit_t.CompareTag("Building"))
            {
                var tower = hit_t.gameObject.GetComponentInParent<Tower>();
                if (tower.IsDestroyed())
                    continue;
                if (tower.blueSide == blueSide)
                    continue;
            }
            float distance = Vector2.Distance(transform.position, hit_t.position);
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestIndex = i;
                closestT = hit_t;
            }
        }

        if (closestIndex == -1 || closestT == null)
        {
            return false;
        }*/
        /*
        if (closestT.CompareTag("BlockUnit"))
        {
            switch (stats.attackType)
            {
                case Attack.Melee:
                    var minion = closestT.gameObject.GetComponentInParent<MinionController>();
                    StartCoroutine(MeleeSingleAttack(minion));
                    break;
                case Attack.Arrow:
                    StartCoroutine(ArrowAttack());
                    break;
                case Attack.TNT:
                    StartCoroutine(TNTAttack());
                    break;
            }
        }
        else if (closestT.CompareTag("Building"))
        {
            switch (stats.attackType)
            {
                case Attack.Melee:
                    var tower = closestT.transform.gameObject.GetComponentInParent<Tower>();
                    StartCoroutine(MeleeSingleAttack(tower));
                    break;
                case Attack.Arrow:
                    StartCoroutine(ArrowAttack());
                    break;
                case Attack.TNT:
                    StartCoroutine(TNTAttack());
                    break;
            }

        }

        return true;*/
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
            yield return new WaitForSeconds(AnimationManager.deathTime);
            //Destroy(gameObject);
            Remove();
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
    private void Movement(Transform closest)
    {

        if (_attacking || _dead)
            return;
        Vector2 traslation = new Vector2(Speed * Time.deltaTime, 0);
        if (!blueSide)
            traslation *= -1;
        //rb.MovePosition((Vector2)transform.position + traslation);
        transform.Translate(traslation);
        if(closest == null)
            return;
        if (Vector2.Distance(transform.position, closest.position) < allyDetectDistnace)
        {
            transform.position = new Vector3(closest.position.x + (blueSide ? -allyDetectDistnace : allyDetectDistnace),
                transform.position.y, transform.position.z);
        }
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
    }�*/
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
        var arrow = (Arrow)GameManager.Instance.ArrowPooler.GetItem(); //(Instantiate(arrowPrefab, transform.position, transform.rotation, null);
        arrow.transform.position = transform.position;
        arrow.transform.rotation = transform.rotation;
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
        var arrow = (Dynamite )GameManager.Instance.DynamitePooler.GetItem(); 
        arrow.transform.position = transform.position;
        arrow.transform.rotation = transform.rotation;
        arrow.transform.localScale = transform.localScale;
        arrow.damage = stats.attack;
        arrow.blueSide = blueSide;
        arrow.maxDistance = stats.range;
        yield return new WaitForSeconds(stats.speed - dynamiteTime);
        _attacking = false;

    }
}
