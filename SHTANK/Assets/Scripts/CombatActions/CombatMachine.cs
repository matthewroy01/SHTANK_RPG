using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CombatMachine : MonoBehaviour
{
    public List<char> instructionStack;
    public List<Instruction> instructionTest;

    public CombatAction testAction;

    // Start is called before the first frame update
    void Start()
    {
        //Instruction instruction = Instruction.INST_ATTACK;
        //instructionStack.Add(System.Convert.ToChar(instruction));
        //
        //Debug.Log(instructionStack[0]);
        //
        //Instruction newInstruction = (Instruction)instructionStack[0];
        //Debug.Log(newInstruction);
        InterpretAction();
    }

    void InterpretAction()
    {
        for(int i = 0; i < testAction.InstructionList.Count; i++)
        {
            Instruction task = (Instruction)testAction.InstructionList[i];

            switch(task)
            {
                case Instruction.INST_ATTACK:
                    {
                        Debug.Log("Attack aSction");
                        break;
                    }
                case Instruction.INST_MOVE:
                    {
                        Debug.Log("Movement action");
                        break;
                    }
                default:
                    {
                        Debug.Log("Instruction did not match any case");
                        break;
                    }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

}
