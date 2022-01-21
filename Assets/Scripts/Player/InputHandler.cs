using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputHandler : MonoBehaviour
{
    [SerializeField] Cinemachine.CinemachineInputProvider cinemachineInput;

    PlayerInput input;
    Player_CharacterController player;

    [HideInInspector] public Vector2 move_input;
    [HideInInspector] public bool m_Jump;
    [HideInInspector] public bool crouch;
    [HideInInspector] public bool run;

    const float lerpSpeed = 0.8f;

    void Awake()
    {
        input = GetComponent<PlayerInput>();
        player = GetComponent<Player_CharacterController>();
    }

    public void SetEnable(bool setTo)
    {
        input.enabled = setTo;
        cinemachineInput.enabled = setTo;
    }

    public void Input_Move(InputAction.CallbackContext c)
    {
        move_input = Vector2.Lerp(move_input, c.ReadValue<Vector2>(), lerpSpeed);
    }
    public void Input_Jump(InputAction.CallbackContext c)
    {
        if (c.started)
        {
            m_Jump = true;
            crouch = false;
        }
    }
    public void Input_Crouch(InputAction.CallbackContext c)
    {
        if (c.started)
        {
            crouch = !crouch;
            run = false;
        }
    }
    public void Input_Run(InputAction.CallbackContext c)
    {
        if (c.started)
        {
            run = !run;
            crouch = false;
        }
    }
    public void Input_Fire(InputAction.CallbackContext c)
    {
        if (c.started)
            player.FireGun();
    }
    public void Input_Interact(InputAction.CallbackContext c)
    {
        if (c.started)
            player.Interact();
    }
}
