using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Door_Handler : iInteractable
{
    [SerializeField] string interactionText;
    [SerializeField] Animator anim;

    public override string GetInteractionText() => interactionText;

    public override void Interact()
    {
        anim.SetBool("IsOpen", !anim.GetBool("IsOpen"));
    }
}
