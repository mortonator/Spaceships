using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class iInteractable : MonoBehaviour
{
    public abstract void Interact();
    public abstract string GetInteractionText();
}
