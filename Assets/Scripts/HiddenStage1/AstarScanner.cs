using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

public class AstarScanner : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        AstarPath.active.Scan();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
