using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class AbilityForecast : MonoBehaviour
{
    private List<Coroutine> running = new List<Coroutine>();
    private List<Character> runningCharacters = new List<Character>();

    public void DisplayForecast(CombatGrid combatGrid)
    {
        if (combatGrid != null)
        {
            List<GridSpace> dirty = new List<GridSpace>(combatGrid.dirty.ToArray());
            List<Character> newCharacters = GetCharactersFromDirtyGridSpaces(dirty);

            // if the previously displayed characters aren't the same as the newly provided ones, we should update the display
            // this prevents flipping an ability that can't really be flipped constantly updating the forecast
            if (!CheckIfListsAreTheSame(newCharacters))
            {
                // stop the existing display coroutines
                for (int i = 0; i < running.Count; ++i)
                {
                    if (running[i] != null)
                    {
                        StopCoroutine(running[i]);
                    }
                }

                foreach(Character character in runningCharacters)
                {
                    character.refCharacterEffectUI.Reset();
                }

                running.Clear();
                runningCharacters.Clear();
                runningCharacters.AddRange(newCharacters);

                // check if the dirty grid spaces have characters and or need things displayed on them
                for (int i = 0; i < dirty.Count; ++i)
                {
                    CheckDirtyGridSpace(dirty[i]);
                }
            }
        }
    }

    private List<Character> GetCharactersFromDirtyGridSpaces(List<GridSpace> dirty)
    {
        List<Character> characters = new List<Character>();

        for (int i = 0; i < dirty.Count; ++i)
        {
            // if the grid space has a character, add it to the list
            if (dirty[i].character != null)
            {
                characters.Add(dirty[i].character);
            }
        }

        return characters;
    }

    private void CheckDirtyGridSpace(GridSpace gridSpace)
    {
        if (gridSpace.character != null)
        {
            List<Effect> effects = new List<Effect>(gridSpace.effects.ToArray());

            for (int i = 0; i < effects.Count; ++i)
            {
                // if the effect shouldn't be displayed, remove it from the list
                if (!CheckEffect(effects[i], gridSpace.character))
                {
                    effects.RemoveAt(i);
                    --i;
                }
            }

            // start displaying effects and save the coroutine to a list to be stopped later
            running.Add(StartCoroutine(gridSpace.character.refCharacterEffectUI.DisplayEffectLoop(new List<Effect>(effects), 1.0f, false)));
        }
    }

    private bool CheckIfListsAreTheSame(List<Character> newCharacters)
    {
        // if the lists are different sizes, they aren't the same
        if (newCharacters.Count != runningCharacters.Count)
        {
            return false;
        }

        // check if the lists have the same contents
        for (int i = 0; i < newCharacters.Count; ++i)
        {
            if (!runningCharacters.Contains(newCharacters[i]))
            {
                return false;
            }
        }

        return true;
    }

    private bool CheckEffect(Effect effect, Character character)
    {
        // check if an effect should be displayed based on various logic
        switch(effect.id)
        {
            case Effect_ID.damage:
            {
                if (effect.source == character)
                {
                    return false;
                }
                break;
            }
            case Effect_ID.healing:
            {
                break;
            }
            case Effect_ID.aggro:
            {
                if (character.affiliation == Character_Affiliation.player)
                {
                    return false;
                }
                break;
            }
            case Effect_ID.frosty:
            {
                break;
            }
            case Effect_ID.aggroDispel:
            {
                if (character.affiliation == Character_Affiliation.enemy)
                {
                    return false;
                }
                break;
            }
            case Effect_ID.attackUp:
            {
                break;
            }
        }

        return true;
    }
}
