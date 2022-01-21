using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_ShipController : MonoBehaviour, iDamageable
{
    [SerializeField] ShipController ship;
    [SerializeField] Cinemachine.CinemachineFreeLook freeLook;
    [SerializeField] Transform camTrans;
    [Space]
    [SerializeField] int maxHealth;
    [Space]
    [SerializeField] TMPro.TMP_Text speedText;
    [SerializeField] TMPro.TMP_Text interactText;

    const float lerpSpeed = 0.6f;

    public Rigidbody rb { get; private set; }
    iInteractable interactable;

    Vector3 rotations; // pitch, yaw, roll
    Vector3 position;
    float drive;

    public float health { get; private set; }
    bool isDead;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        health = maxHealth;
        ship.canUseWeapons = false;
    }
    void FixedUpdate()
    {
        ship.SetMove(rotations, drive);

        speedText.text = "Acceleration: " + ship.Acceleration + "\n Speed: " + (int)((position - transform.position).sqrMagnitude * 10000);
        position = transform.position;
    }

    #region Interact
    public void SetInteraction(iInteractable _interactable)
    {
        interactText.text = "Square: " + _interactable.GetInteractionText();
        interactable = _interactable;
    }
    public void ResetInteraction()
    {
        interactText.text = string.Empty;
        interactable = null;
    }
    #endregion

    #region Inputs
    public void Input_Drive(InputAction.CallbackContext c)
    {
        drive =  c.ReadValue<float>();
    }
    public void Input_Rotate_Pitch(InputAction.CallbackContext c)
    {
        rotations.x =  c.ReadValue<float>();
    }
    public void Input_Rotate_Roll(InputAction.CallbackContext c)
    {
        rotations.z =  -c.ReadValue<float>();
    }
    public void Input_Rotate_Yaw(InputAction.CallbackContext c)
    {
        rotations.y =  c.ReadValue<float>();
    }
    public void Input_ResetCamera(InputAction.CallbackContext c)
    {
        if(c.started)
        {
            camTrans.localEulerAngles = new Vector3(camTrans.eulerAngles.x, 0, camTrans.eulerAngles.z);
            freeLook.m_YAxis.Value = 0.55f;
        }
    }
    public void Input_Interact(InputAction.CallbackContext c)
    {
        if (c.started)
        {
            interactable.Interact();
        }
    }
    public void Input_FireWeapons(InputAction.CallbackContext c)
    {
        if (c.started)
        {
            ship.canUseWeapons = !ship.canUseWeapons;
        }
    }
    public void Input_LightToggle(InputAction.CallbackContext c)
    {
        if (c.started)
        {
            ship.ToggleFrontLights();
        }
    }
    #endregion

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
