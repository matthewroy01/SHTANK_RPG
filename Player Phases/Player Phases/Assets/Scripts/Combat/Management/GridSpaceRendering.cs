using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridSpaceRendering : MonoBehaviour
{
    public SpriteRenderer spriteBorder;
    private Color defaultBorder;
    public SpriteRenderer spriteFill;
    private Color defaultFill;

    private float differenceInAlpha;

    private void Start()
    {
        defaultBorder = spriteBorder.color;
        defaultFill = spriteFill.color;

        differenceInAlpha = defaultBorder.a - defaultFill.a;
    }

    public void SetColor(Color color)
    {
        spriteBorder.color = color;
        spriteFill.color = new Color(color.r, color.g, color.b, color.a - differenceInAlpha);
    }

    public void ResetColor()
    {
        spriteBorder.color = defaultBorder;
        spriteFill.color = defaultFill;
    }
}
