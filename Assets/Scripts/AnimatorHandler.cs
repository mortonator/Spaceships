using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class AnimatorHandler : MonoBehaviour
{
    [SerializeField] ThirdPersonCharacter character;

    void OnAnimatorMove()
    {
        character.AnimatorMove();
    }
}
