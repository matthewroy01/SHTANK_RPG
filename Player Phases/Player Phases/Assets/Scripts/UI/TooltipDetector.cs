﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipDetector : MonoBehaviour
{
    private TooltipController refTooltipController;
    private RectTransform rectTransform;

    [HideInInspector]
    public StatusUIDefinition statusDef;

    private void Start()
    {
        refTooltipController = FindObjectOfType<TooltipController>();
        rectTransform = GetComponent<RectTransform>();
    }

    public bool CheckForMouseOver()
    {
        Vector2 mousePos = Input.mousePosition;

        // if we mouse over this UI object, display a tooltip
        if (RectTransformUtility.RectangleContainsScreenPoint(rectTransform, mousePos))
        {
            return true;
        }

        return false;
    }
}