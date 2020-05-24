using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Passive : MonoBehaviour
{
    // combat begining and ending
    public virtual void BeginCombat() { }
    public virtual void EndCombat() { }

    // selection and deselection
    public virtual void Selected() { }
    public virtual void Deselected() { }

    // turn begining and ending
    public virtual void BeginTurn() { }
    public virtual void EndTurn() { }

    // using abilities
    public virtual void AbilityUse1() { }
    public virtual void AbilityUse2() { }
    public virtual void AbilityUse3() { }
    public virtual void AbilityUse4() { }

    // effect types
    public virtual void ReceiveDamage() { }
    public virtual void ReceiveHealing() { }
    public virtual void ReceiveStatus() { }

    // misc
    public virtual void StoreGridSpace(GridSpace toStore) { }
}
