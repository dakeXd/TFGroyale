using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{
    public float explosionArea;
    public float explosionTime, explosionTimeDMG;

    [NonSerialized] public int damage;
    [NonSerialized] public bool blueSide;
    public LayerMask explosionLayer;
    public GameObject visuals;
    public Transform raycaster;

    void Start()
    {
        StartCoroutine(ExplosionCR());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator ExplosionCR()
    {
        visuals.SetActive(true);
        yield return new WaitForSeconds(explosionTimeDMG);
        var inArea = Physics2D.RaycastAll(raycaster.position, blueSide ? Vector2.right : Vector2.left, explosionArea, explosionLayer);
        foreach(var item in inArea)
        {
            if (item.transform.CompareTag("BlockUnit"))
            {
                var minion = item.transform.gameObject.GetComponentInParent<MinionController>();
                if (minion.blueSide != blueSide)
                {
                    minion.Damage(damage, 0);
                }
            }
            else if (item.transform.CompareTag("Building"))
            {
                var tower = item.transform.gameObject.GetComponentInParent<Tower>();
                if (tower.blueSide != blueSide){
                    tower.Damage(damage * 2, 0);
                }
            }
        }
        yield return new WaitForSeconds(explosionTime - explosionTimeDMG);
        visuals.SetActive(false);
        Destroy(gameObject);
    }
}
