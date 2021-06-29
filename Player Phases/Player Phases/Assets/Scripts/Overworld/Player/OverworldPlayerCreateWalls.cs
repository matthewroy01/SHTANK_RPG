using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DG.Tweening;

public class OverworldPlayerCreateWalls : MonoBehaviour
{
    [Header("Creating walls ability")]
    public GameObject shadow;
    public ParticleSystem passiveParts;
    public ParticleSystem createWallParts;

    public GameObject wall;

    [HideInInspector]
    public ShadowState castingShadow = ShadowState.inactive;

    private OverworldPlayerController controller;

    private void Start()
    {
        if (!TryGetComponent(out controller))
        {
            Debug.LogError("OverworldPlayerMovement could not find component OverworldPlayerController.");
        }

        DestroyWall();
    }

    public void MyUpdate()
    {
        //passiveParts.transform.position = controller.refMovement.shadow.transform.position;

        if (Input.GetMouseButtonDown(0))
        {
            castingShadow = ShadowState.moving;

            passiveParts.Play();
        }

        if (Input.GetMouseButtonUp(0))
        {
            castingShadow = ShadowState.created;

            CreateWall();

            passiveParts.Stop();
        }

        if (!Input.GetMouseButton(0) && Input.GetMouseButtonDown(1))
        {
            castingShadow = ShadowState.inactive;

            DestroyWall();
        }
    }

    private void CreateWall()
    {
        wall.SetActive(true);
        //wall.transform.position = controller.refMovement.shadow.transform.position + (Vector3.up * -1.0f);
        wall.transform.position = new Vector3(Mathf.RoundToInt(wall.transform.position.x), wall.transform.position.y, Mathf.RoundToInt(wall.transform.position.z));
        Instantiate(createWallParts, wall.transform.position + (Vector3.up * 0.5f), createWallParts.transform.rotation);
        //wall.transform.DOMoveY(controller.refMovement.shadow.transform.position.y + 0.5f, 0.5f);
    }

    private void DestroyWall()
    {
        wall.SetActive(false);
    }

    public enum ShadowState { inactive, moving, created };
}