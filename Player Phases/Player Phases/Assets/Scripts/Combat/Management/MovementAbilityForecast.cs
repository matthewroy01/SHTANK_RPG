using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementAbilityForecast : MonoBehaviour
{
    public SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer.color = new Color(Color.white.r, Color.white.g, Color.white.b, 0.0f);
    }

    public void DisplayForecast(Vector3 position, Character character)
    {
        if (character != null)
        {
            SpriteRenderer tmp = character.GetComponentInChildren<SpriteRenderer>();
            if (tmp != null)
            {
                spriteRenderer.sprite = tmp.sprite;
                spriteRenderer.transform.position = new Vector3(position.x, 0.5f, position.z);
                spriteRenderer.color = new Color(Color.white.r, Color.white.g, Color.white.b, 0.5f + (Mathf.Sin(Time.deltaTime) * 0.12f));

                return;
            }
        }

        spriteRenderer.color = new Color(Color.white.r, Color.white.g, Color.white.b, 0.0f);
    }
}
