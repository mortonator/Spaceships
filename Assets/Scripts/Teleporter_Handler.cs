using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teleporter_Handler : iInteractable
{
    [SerializeField] Transform player;
    [SerializeField] Transform teleportToLocation;
    [Space]
    [SerializeField] string interactionText;
    public override string GetInteractionText() => interactionText;

    public override void Interact()
    {
        player.position = teleportToLocation.position;
        player.rotation = teleportToLocation.rotation;
    }
}
