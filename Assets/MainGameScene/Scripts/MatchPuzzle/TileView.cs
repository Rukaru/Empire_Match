using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class TileView : MonoBehaviour
{
    [SerializeField] SpriteRenderer sr;
    Color defaultColor;

    void Awake()
    {
        if (sr == null) sr = GetComponent<SpriteRenderer>();
        defaultColor = sr.color;
    }

    public void GlowSelect()
    {
        StopAllCoroutines();
        sr.color = Color.white;
    }

    public void ResetColor()
    {
        StopAllCoroutines();
        sr.color = defaultColor;
    }
}
