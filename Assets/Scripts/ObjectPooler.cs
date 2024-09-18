using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    public Poolable prefab;
    public List<Poolable> pooledItems;

    private void Awake()
    {
        pooledItems = new List<Poolable>(128);
    }

    // Start is called before the first frame update
    public Poolable GetItem()
    {
        if (pooledItems.Count > 0)
        {
            Poolable item = pooledItems[^1];
            pooledItems.RemoveAt(pooledItems.Count-1);
            item.Active();
            return item;
        }
        else
        {
            //Debug.Log("Creating new poolable");
            var item = Instantiate(prefab, transform);
            item.pooler = this;
            item.Active();
            return item;
        }
    }

    public void ReturnItem(Poolable item)
    {
        item.Deactivate();
        pooledItems.Add(item);
    }
}

public abstract class Poolable : MonoBehaviour
{
    public ObjectPooler pooler;

    public virtual void Active()
    {
        gameObject.SetActive(true);
    }

    public virtual void Deactivate()
    {
        gameObject.SetActive(false);
        transform.parent = pooler.transform;
    }

    public virtual void Remove()
    {
        pooler.ReturnItem(this);
    }
}
