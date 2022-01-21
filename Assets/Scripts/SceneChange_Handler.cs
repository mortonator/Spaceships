using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneChange_Handler : iInteractable
{
    [SerializeField] int transitionToSceneIndex;
    [SerializeField] string interactionText;
    [SerializeField] int spawnLocationIndex;

    public override string GetInteractionText() => interactionText;
    public override void Interact()
    {
        GameManager.Instance.LoadScene(transitionToSceneIndex, spawnLocationIndex);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out Player_ShipController player))
            player.SetInteraction(this);
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && other.TryGetComponent(out Player_ShipController player))
            player.ResetInteraction();
    }
}
