using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CharacterManager : MonoBehaviour
{
    protected PhaseManager refPhaseManager;
    protected CombatGrid refCombatGrid;

    protected PlayerManager refPlayerManager;
    protected AllyManager refAllyManager;
    protected EnemyManager refEnemyManager;

    [Header("Base Character Prefab")]
    public GameObject characterPrefab;

    protected void AssignCharacterValues(GameObject obj, CharacterDefinition def, Character_Affiliation affiliation, PartyMember member = null)
    {
        Character refCharacter = obj.GetComponent<Character>();
        MovementDialogueProcessor refMovementDialogueProcessor = obj.GetComponent<MovementDialogueProcessor>();
        SpriteRenderer refSpriteRenderer = obj.GetComponentInChildren<SpriteRenderer>();

        obj.name = def.characterName;

        if (refCharacter != null)
        {
            // max health and current health
            refCharacter.healthMax = def.healthMax;
            if (member != null)
            {
                refCharacter.healthCurrent = member.currentHealth;
            }
            else
            {
                refCharacter.healthCurrent = refCharacter.healthMax;
            }

            // attack and defense modifiers
            refCharacter.attack = def.attack;
            refCharacter.defense = def.defense;

            // stagger
            refCharacter.stagger = def.stagger;

            // nashbalm
            refCharacter.nashbalm = def.nashbalm;

            // movement range
            refCharacter.movementRangeDefault = def.movementRange;
            refCharacter.movementRangeCurrent = refCharacter.movementRangeDefault;

            // moveset
            refCharacter.moveset = def.moveset;
            refCharacter.movesetData = new MovesetData(refCharacter.moveset, 2);

            // affiliation
            refCharacter.affiliation = affiliation;

            // navigable terrain types
            refCharacter.terrainTypes = def.terrainTypes;

            // character portrait for UI
            refCharacter.portrait = def.portrait;

            // ability definitions for UI
            refCharacter.abilityUIDefinition = FindObjectOfType<CharacterUI>().InitializeAbilityUI(refCharacter);

            // character description for tooltips
            refCharacter.characterDescription = def.characterDescription;

            // for player characters, add the movement dialogue
            refCharacter.refMovementDialogueProcessor = refCharacter.GetComponent<MovementDialogueProcessor>();

            // character effect UI (for previewing and displaying damage)
            refCharacter.refCharacterEffectUI = refCharacter.GetComponent<EffectUI>();
        }

        if (refMovementDialogueProcessor != null)
        {
            // movement dialogue text
            refMovementDialogueProcessor.dialogue = def.movementDialogue;

            // movement dialogue sound
            refMovementDialogueProcessor.speechBlip = def.movementDialogueSound;
        }

        if (refSpriteRenderer != null)
        {
            // temporary sprite
            refSpriteRenderer.sprite = def.sprite;
        }
    }

    protected void AssignPassive(Character parent, CharacterDefinition def)
    {
        if (def.passiveFunctionality != null)
        {
            parent.passive = Instantiate(def.passiveFunctionality, parent.transform);
            parent.passive.myCharacter = parent;
        }
    }

    protected GridSpace GetSpawnSpace(int targetX, int targetY, List<GridSpace_TerrainType> validTerrainTypes, Character_Affiliation affiliation)
    {
        List<GridSpace> search = refCombatGrid.GetBreadthFirst(refCombatGrid.grid[targetX, targetY], refCombatGrid.gridWidth * refCombatGrid.gridHeight, TerrainTypePresets.all, affiliation);

        for (int i = 0; i < search.Count; ++i)
        {
            if (validTerrainTypes.Contains(search[i].GetTerrainType()) && search[i].character == null && IsOnCheckerboard(search[i].coordinate))
            {
                return search[i];
            }
        }

        return refCombatGrid.grid[targetX, targetY];
    }

    protected bool IsOnCheckerboard(Vector2Int coordinate)
    {
        if (coordinate.x % 2 == 0)
        {
            if (coordinate.y % 2 == 1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else if (coordinate.x % 2 == 1)
        {
            if (coordinate.y % 2 == 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    public void DestroyCharacters<T>(List<T> characters)
    {
        for (int i = 0; i < characters.Count; ++i)
        {
            Destroy((characters[i] as Character).gameObject);
        }

        characters.Clear();
    }

    public abstract void AddCharacter(CharacterDefinition def, GridSpace gridSpace);
}