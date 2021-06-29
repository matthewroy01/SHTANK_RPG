using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SHTANKCutscenes
{
    [CreateAssetMenu(fileName = "New Speaker", menuName = "Dialogue/Speaker", order = 1)]
    public class Speaker : ScriptableObject
    {
        public new string name;

        public ManagedAudio voiceBlip;

        [Header("Emotion Sprites")]
        public Sprite normal;
        public Sprite happy;
        public Sprite sad;
        public Sprite angry;
        public Sprite sigh;
    }

    public enum Speaker_Emotion { normal, happy, sad, angry, sigh };
}