using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverworldPlayerAnimation : MonoBehaviour
{
    private OverworldPlayerController controller;

    public Animator refAnimator;
    public SpriteRenderer refSpriteRenderer;

    void Start()
    {
        if (!TryGetComponent(out controller))
        {
            Debug.LogError("OverworldPlayerAnimation could not find component OverworldPlayerController.");
        }
    }

    public void MyUpdate()
    {
        if (controller.refMovement.refRigidbody.velocity.x > 0.1f)
        {
            refSpriteRenderer.flipX = false;
        }
        if (controller.refMovement.refRigidbody.velocity.x < -0.1f)
        {
            refSpriteRenderer.flipX = true;
        }

        if (controller.refMovement.grounded)
        {
            if (Mathf.Abs(controller.refMovement.refRigidbody.velocity.x) > 0.1f || Mathf.Abs(controller.refMovement.refRigidbody.velocity.z) > 0.1f)
            {
                refAnimator.SetTrigger("running");
            }
            else
            {
                refAnimator.SetTrigger("idling");
            }
        }
        else
        {
            if (controller.refMovement.refRigidbody.velocity.y > 0.0f)
            {
                refAnimator.SetTrigger("rising");
            }
            else
            {
                refAnimator.SetTrigger("falling");
            }
        }
    }
}
