using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class OverworldEnemyMovement : MonoBehaviour
{
    private Vector3 skuttlebugHome;

    public LayerMask groundLayerMask;
    public GameObject shadow;

    [Header("Chasing and distances")]
    public float minDistanceToPlayer;
    public float maxDistanceFromHome;
    public float chaseSpeed;

    [HideInInspector]
    public Rigidbody refRigidbody;

    private OverworldEnemyState state;

    private OverworldPlayerController refPlayer;

    [HideInInspector]
    public bool runningAI = false;
    private Coroutine aiCoroutine;

    private void Start()
    {
        state = OverworldEnemyState.idling;

        if (!TryGetComponent(out refRigidbody))
        {
            Debug.LogError("OverworldEnemyMovement could not find component Rigidbody.");
        }

        refPlayer = FindObjectOfType<OverworldPlayerController>();
    }

    private void Awake()
    {
        skuttlebugHome = transform.position;
    }

    private void Update()
    {
        MoveShadow();
    }

    public void MyUpdate()
    {
        if (!runningAI)
        {
            aiCoroutine = StartCoroutine(DoAI());
        }
    }

    private IEnumerator DoAI()
    {
        runningAI = true;

        while (true)
        {
            switch (state)
            {
                case OverworldEnemyState.idling:
                {
                    if (Vector3.Distance(transform.position, refPlayer.transform.position) < minDistanceToPlayer)
                    {
                        transform.DOJump(transform.position + Vector3.up, 1, 1, 0.5f).SetEase(Ease.Linear);
                        yield return new WaitForSecondsRealtime(0.75f);
                        state = OverworldEnemyState.chasing;
                    }

                    refRigidbody.velocity = Vector3.zero;
                    break;
                }
                case OverworldEnemyState.chasing:
                {
                    if (Vector3.Distance(transform.position, skuttlebugHome) > maxDistanceFromHome)
                    {
                        state = OverworldEnemyState.fleeing;
                        refRigidbody.velocity = Vector3.zero;
                        yield return new WaitForSecondsRealtime(1.0f);
                    }

                    refRigidbody.velocity = (refPlayer.transform.position - transform.position).normalized * chaseSpeed;
                    break;
                }
                case OverworldEnemyState.fleeing:
                {
                    if (Vector3.Distance(new Vector3(transform.position.x, 0.0f, transform.position.z), new Vector3(skuttlebugHome.x, 0.0f, skuttlebugHome.z)) < 0.5f)
                    {
                        state = OverworldEnemyState.idling;
                    }

                    refRigidbody.velocity = (skuttlebugHome - transform.position).normalized * (chaseSpeed * 0.75f);
                    break;
                }
            }

            yield return new WaitForEndOfFrame();
        }
    }

    private void MoveShadow()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, Vector3.down, out hit, groundLayerMask);

        if (hit.transform)
        {
            shadow.SetActive(true);

            shadow.transform.position = new Vector3(hit.point.x, hit.point.y, hit.point.z);

            float distance = Vector3.Distance(transform.position, hit.point);
            if (distance != 0)
            {
                float dividedDistance = 1.0f / distance;
                Mathf.Clamp(dividedDistance, 0.001f, 1.0f);
                shadow.transform.localScale = Vector3.ClampMagnitude(Vector3.one * dividedDistance, 3.464f);
            }
        }
        else
        {
            // don't display a shadow
            shadow.SetActive(false);
        }
    }

    public void SetState(OverworldEnemyState newState)
    {
        state = newState;
    }

    public void StopAI()
    {
        if (aiCoroutine != null)
        {
            StopCoroutine(aiCoroutine);
            runningAI = false;
        }
    }
}

public enum OverworldEnemyState { idling, fleeing, chasing };