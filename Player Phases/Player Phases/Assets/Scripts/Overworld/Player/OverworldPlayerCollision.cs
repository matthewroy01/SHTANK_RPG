using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldPlayerCollision : MonoBehaviour
{
    private OverworldPlayerController controller;

    private CombatManager refCombatManager;

    void Start()
    {
        if (!TryGetComponent(out controller))
        {
            Debug.LogError("OverworldPlayerCollision could not find component OverworldPlayerController.");
        }

        refCombatManager = FindObjectOfType<CombatManager>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Overworld_Encounter"))
        {
            // save the collision point and round it to the nearest whole number position
            Vector3 collisionPoint = other.contacts[0].point;
            collisionPoint.x = Mathf.Round(collisionPoint.x);
            collisionPoint.z = Mathf.Round(collisionPoint.z);

            OverworldEnemyController tmp = other.gameObject.GetComponent<OverworldEnemyController>();

            // pass the collision point along to the Combat Initiator to begin combat at that position
            FindObjectOfType<SHTANKManager>().TryBeginCombat(collisionPoint, tmp);
        }
    }
}
