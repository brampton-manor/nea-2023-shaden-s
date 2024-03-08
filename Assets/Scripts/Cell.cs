using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cell : MonoBehaviour
{
    public int x;
    public int y;
    public Vector3 position;
    public bool isBlocked;
    public bool OnEdge;

    public Cell(int x, int y, Vector3 position)
    {
        this.x = x;
        this.y = y;
        this.position = position;
        this.isBlocked = false;
    }

}
