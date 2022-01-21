using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipController : MonoBehaviour
{
    [SerializeField] EngineHandler[] rearEngines;
    [SerializeField] EngineHandler[] forwardEngines;
    [Space]
    [SerializeField] Light[] frontLights;
    [SerializeField] ShipGunHandler[] guns;
    [Space]
    [SerializeField] Rigidbody rb;
    [Space]
    [SerializeField] float engineSrength;
    [SerializeField] float turnSpeed;
    [SerializeField] float engineVolume = 1;

    [HideInInspector] public bool canUseWeapons = false;

    public int Acceleration { get; private set; }
    float currentDrive;
    int gunIndex;
    Vector3 turn;
    float drive;
    bool frontLightsOn=false;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="turn">pitch, yaw, roll</param>
    /// <param name="drive"></param>
    public void SetMove(Vector3 _turn, float _drive)
    {
        turn = _turn;
        drive= _drive;
    }
    public void ToggleFrontLights()
    {
        frontLightsOn = !frontLightsOn;

        for (int i = frontLights.Length - 1; i >= 0; i--)
            frontLights[i].enabled = frontLightsOn;
    }

    private void FixedUpdate()
    {
        Move();

        if (canUseWeapons)
            FireGuns();
    }
    void Move()
    {
        if (drive != currentDrive)
        {
            currentDrive = drive;
            ChangeEngineVisuals();
        }

        rb.AddForce((transform.forward * engineSrength * drive), ForceMode.Acceleration);
        rb.angularVelocity += transform.TransformDirection(turn * turnSpeed);
        Acceleration = (int)(transform.forward * engineSrength * drive).sqrMagnitude;
    }
    void ChangeEngineVisuals()
    {
        int index;
        if (currentDrive == 0)
        {
            for (index = rearEngines.Length - 1; index >= 0; index--)
            {
                rearEngines[index].Light.range = 0;
                if (rearEngines[index].Audio.isPlaying == true)
                    rearEngines[index].Audio.Pause();
            }
            for (index = forwardEngines.Length - 1; index >= 0; index--)
            {
                forwardEngines[index].Light.range = 0;
                if (forwardEngines[index].Audio.isPlaying == true)
                    forwardEngines[index].Audio.Pause();
            }
        }
        else if (currentDrive > 0)
        {
            for (index = rearEngines.Length - 1; index >= 0; index--)
            {
                rearEngines[index].Light.range = 5 * currentDrive;
                rearEngines[index].Audio.volume = engineVolume * currentDrive;
                if (rearEngines[index].Audio.isPlaying == false)
                    rearEngines[index].Audio.Play();
            }
            for (index = forwardEngines.Length - 1; index >= 0; index--)
            {
                forwardEngines[index].Light.range = 0;
                if (forwardEngines[index].Audio.isPlaying == true)
                    forwardEngines[index].Audio.Pause();
            }
        }
        else
        {
            for (index = rearEngines.Length - 1; index >= 0; index--)
            {
                rearEngines[index].Light.range = 0;
                if (rearEngines[index].Audio.isPlaying == true)
                    rearEngines[index].Audio.Pause();
            }
            for (index = forwardEngines.Length - 1; index >= 0; index--)
            {
                forwardEngines[index].Light.range = 5 * -currentDrive;
                forwardEngines[index].Audio.volume = engineVolume * -currentDrive;
                if (forwardEngines[index].Audio.isPlaying == false)
                    forwardEngines[index].Audio.Play();
            }
        }
    }

    void FireGuns()
    {
        guns[gunIndex].Fire();

        gunIndex++;
        if (gunIndex >= guns.Length)
            gunIndex = 0;
    }
}
