using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.ThirdPerson;

public class Player_CharacterController : MonoBehaviour
{
    public static Player_CharacterController player;

    [SerializeField] InputHandler inputs;
    [SerializeField] ThirdPersonCharacter m_Character;
    [SerializeField] Transform m_Cam;
    [SerializeField] Transform m_Target;
    [Space]
    [SerializeField] int maxHealth;
    [Space]
    public Player_UiCharacter ui;

    Vector3 m_CamForward;
    Vector3 m_Move;

    float interactDistance = 8;
    RaycastHit hit;
    iInteractable interactable;

    float health;
    bool isDead;

    void OnEnable()
    {
        player = this;
    }

    void FixedUpdate()
    {
        Move();
        GetInteract();
    }
    void Move()
    {
        // calculate camera relative direction to move:
        m_CamForward = Vector3.Scale(m_Cam.forward, new Vector3(1, 0, 1)).normalized;
        m_Move = inputs.move_input.y * m_CamForward + inputs.move_input.x * m_Cam.right;

        // walk speed multiplier
        if (!inputs.run) 
            m_Move *= 0.5f;

        // pass all parameters to the character control script
        m_Character.Move(m_Move, inputs.crouch, inputs.m_Jump);
        inputs.m_Jump = false;
    }

    public void FireGun()
    {

    }

     void GetInteract()
    {
        if (Physics.Raycast(m_Cam.position + (m_Cam.forward * m_Cam.localPosition.z), m_Cam.forward, out hit, interactDistance) && hit.collider.CompareTag("Interact"))
        {
            interactable = hit.collider.GetComponent<iInteractable>();

            if (interactable != null)
                ui.SetInteractText("Square: " + interactable.GetInteractionText());
        }
        else
        {
            interactable = null;
            ui.SetInteractText("");
        }
    }
    public void Interact()
    {
        if (interactable != null)
            interactable.Interact();
    }


    public void Damage(float dam)
    {
        if (isDead) return;

        health -= dam;

        if (health <= 0)
            Die();
    }
    void Die()
    {
        isDead = true;
    }
}
