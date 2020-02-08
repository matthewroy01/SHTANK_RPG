using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridColorProcessor : MonoBehaviour
{
    private CombatGrid refCombatGrid;
    private AbilityProcessor refAbilityProcessor;
    private PlayerSelector refPlayerSelector;

    [Header("Grid Colors")]
    public Color colorMovement;
    public Color colorAbilityPreview;
    public Color colorRangedStartingSpaces;
    public Color colorDefaultState;

    // default state < possible movement spaces < possible starting spaces for ranged abilities < ability previews

    void Start()
    {
        refCombatGrid = FindObjectOfType<CombatGrid>();
        refAbilityProcessor = FindObjectOfType<AbilityProcessor>();
        refPlayerSelector = FindObjectOfType<PlayerSelector>();
    }

    void Update()
    {
        ProcessColors();
    }

    private void ProcessColors()
    {
        for (int i = 0; i < refCombatGrid.grid.GetLength(0); ++i)
        {
            for (int j = 0; j < refCombatGrid.grid.GetLength(1); ++j)
            {
                // reset grid space color to the default state
                refCombatGrid.grid[i, j].obj.GetComponent<Renderer>().material.color = colorDefaultState;
            }
        }

        for (int i = 0; i < refCombatGrid.grid.GetLength(0); ++i)
        {
            for (int j = 0; j < refCombatGrid.grid.GetLength(1); ++j)
            {
                // color for movement spaces
                CheckListForGridSpace(refPlayerSelector.currentPlayer.movementSpaces, refCombatGrid.grid[i, j], colorMovement, i, j);

                // color for ranged starting spaces
                CheckListForGridSpace(refAbilityProcessor.GetStartingSpaces(), refCombatGrid.grid[i, j], colorRangedStartingSpaces, i, j);

                // color for ability preview
                CheckListForGridSpace(refAbilityProcessor.GetGridSpaces(), refCombatGrid.grid[i, j], colorAbilityPreview, i, j);
            }
        }
    }

    private void CheckListForGridSpace(List<GridSpace> checkAgainst, GridSpace toCheck, Color color, int i, int j)
    {
        if (checkAgainst.Contains(toCheck))
        {
            toCheck.obj.GetComponent<Renderer>().material.color = Color.Lerp(Color.Lerp(color, Color.white, 0.5f), color, Mathf.Sin(Time.time * 10.0f + (0.5f * (i - j))));
        }
    }
}
