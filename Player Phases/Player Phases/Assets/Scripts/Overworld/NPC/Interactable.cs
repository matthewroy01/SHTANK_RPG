using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SHTANKCutscenes
{
    public class Interactable : MonoBehaviour
    {
        public Interactable_Type type;
        public CutsceneDefinition definition;
    }

    public enum Interactable_Type { NPC, inanimate };
}