using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Instruction
{
    INST_MOVE = 0x21,
    INST_ATTACK = 0x35
};

[CreateAssetMenu(fileName = "New Combat Action", menuName = "Combat Action")]
public class CombatAction : ScriptableObject
{
    public new string name;
    [TextArea(3, 10)]
    public string description;

    public List<Instruction> InstructionList;
}