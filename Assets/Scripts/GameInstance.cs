using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInstance : MonoBehaviour
{
    public bool visualsActive = false;
    public List<GameObject> visuals;
    public SideManager blueSide, redSide;
    public InputDriver input1, input2;


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    [ContextMenu("Update visuals")]
    public void UpdateVisuals()
    {
        foreach(var item in visuals)
        {
            item.SetActive(visualsActive);
        }
    }
}
