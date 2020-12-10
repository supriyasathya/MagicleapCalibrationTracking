using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdjustSpriteAlpha : MonoBehaviour {

    public float alpha;

    private void Start()
    {
        setAlpha(alpha);
    }

    public void setAlpha(float alpha)
    {
        SpriteRenderer[] children = GetComponentsInChildren<SpriteRenderer>(true);
        Color newColor;
        foreach (SpriteRenderer child in children)
        {
            newColor = child.color;
            newColor.a = alpha;
            child.color = newColor;
        }
    }
}
