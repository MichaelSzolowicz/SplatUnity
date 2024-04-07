using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Inker : MonoBehaviour
{
    [SerializeField] protected int team;
    public int Team { get { return team; } }

    // Temporary until I set up brush struct
    public float radius, hardness, strength;

    public Color inkColor;

    protected virtual void Start()
    {
        SetInkColor();
    }

    protected void SetInkColor()
    {
        inkColor = new Color(0,0,0,1);
        inkColor[team] = 1;
    }
}
