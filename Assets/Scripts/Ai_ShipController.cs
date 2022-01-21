using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ai_ShipController : MonoBehaviour, iDamageable
{
    [SerializeField] ShipController ship;
    [SerializeField] Player_ShipController playerShip;
    [Space]
    [SerializeField] float steeringSpeed;
    [SerializeField] float minSpeed;
    [SerializeField] float maxSpeed;
    [SerializeField] float recoverSpeedMin;
    [SerializeField] float recoverSpeedMax;
    [Space]
    [SerializeField] LayerMask groundCollisionMask;
    [SerializeField] float groundCollisionDistance;
    [SerializeField] float groundAvoidanceAngle;
    [SerializeField] float groundAvoidanceMinSpeed;
    [SerializeField] float groundAvoidanceMaxSpeed;
    [Space]
    [SerializeField] float pitchUpThreshold;
    [SerializeField] float fineSteeringAngle;
    [SerializeField] float rollFactor;
    [Space]
    [SerializeField] bool canUseCannon;
    [SerializeField] float bulletSpeed;
    [SerializeField] float cannonRange;
    [SerializeField] float cannonMaxFireAngle;
    [SerializeField] float cannonBurstLength;
    [SerializeField] float cannonBurstCooldown;
    [Space]
    [SerializeField] bool startDocked;
    [SerializeField] int maxHealth;

    Animator anim;
    float health;
    bool isDead;

    Rigidbody rb;
    Vector3 lastInput;
    bool isRecoveringSpeed;

    float missileDelayTimer;
    float missileCooldownTimer;

    bool cannonFiring;
    float cannonBurstTimer;
    float cannonCooldownTimer;

    List<Vector3> dodgeOffsets;
    const float dodgeUpdateInterval = 0.25f;
    float dodgeTimer;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        dodgeOffsets = new List<Vector3>();

        health = maxHealth;
        anim = GetComponent<Animator>();
        anim.SetBool("IsDock", startDocked);
    }

    void FixedUpdate()
    {
        if (!isDead) 
            UpdateNavigation();
    }

    #region Ai Navitation
    void UpdateNavigation()
    {
        Vector3 steering;
        float throttle;

        Quaternion velocityRot = Quaternion.LookRotation(rb.velocity.normalized);
        Ray ray = new Ray(rb.position, velocityRot * Quaternion.Euler(groundAvoidanceAngle, 0, 0) * Vector3.forward);

        if (Physics.Raycast(ray, groundCollisionDistance + rb.angularVelocity.z, groundCollisionMask.value))
        {
            steering = AvoidGround();
            throttle = CalculateThrottle(groundAvoidanceMinSpeed, groundAvoidanceMaxSpeed);
        }
        else
        {
            Vector3 targetPosition;

            targetPosition = GetTargetPosition();

            if (rb.velocity.z < recoverSpeedMin || isRecoveringSpeed)
            {
                isRecoveringSpeed = rb.velocity.z < recoverSpeedMax;

                steering = RecoverSpeed();
                throttle = 1;
            }
            else
            {
                steering = CalculateSteering(Time.fixedDeltaTime, targetPosition);
                throttle = CalculateThrottle(minSpeed, maxSpeed);
            }
        }

        ship.SetMove(steering, throttle);

        CalculateWeapons(Time.fixedDeltaTime);
    }

    Vector3 AvoidGround()
    {
        //roll level and pull up
        float roll = rb.rotation.eulerAngles.z;
        if (roll > 180f) roll -= 360f;
        return new Vector3(-1, 0, Mathf.Clamp(-roll * rollFactor, -1, 1));
    }

    Vector3 RecoverSpeed()
    {
        //roll and pitch level
        float roll = rb.rotation.eulerAngles.z;
        float pitch = rb.rotation.eulerAngles.x;
        if (roll > 180f) roll -= 360f;
        if (pitch > 180f) pitch -= 360f;
        return new Vector3(Mathf.Clamp(-pitch, -1, 1), 0, Mathf.Clamp(-roll * rollFactor, -1, 1));
    }

    Vector3 GetTargetPosition()
    {
        if (playerShip == null)
        {
            return rb.position;
        }

        Vector3 targetPosition = playerShip.transform.position;

        if (Vector3.Distance(targetPosition, rb.position) < cannonRange)
        {
            return Utilities.FirstOrderIntercept(rb.position, rb.velocity, bulletSpeed, targetPosition, playerShip.rb.velocity);
        }

        return targetPosition;
    }

    Vector3 CalculateSteering(float dt, Vector3 targetPosition)
    {
        if (playerShip != null && playerShip.health == 0)
        {
            return new Vector3();
        }

        Vector3 error = targetPosition - rb.position;
        error = Quaternion.Inverse(rb.rotation) * error;   //transform into local space

        Vector3 errorDir = error.normalized;
        Vector3 pitchError = new Vector3(0, error.y, error.z).normalized;
        Vector3 rollError = new Vector3(error.x, error.y, 0).normalized;

        Vector3 targetInput = new Vector3();

        float pitch = Vector3.SignedAngle(Vector3.forward, pitchError, Vector3.right);
        if (-pitch < pitchUpThreshold) pitch += 360f;
        targetInput.x = pitch;

        if (Vector3.Angle(Vector3.forward, errorDir) < fineSteeringAngle)
        {
            targetInput.y = error.x;
        }
        else
        {
            float roll = Vector3.SignedAngle(Vector3.up, rollError, Vector3.forward);
            targetInput.z = roll * rollFactor;
        }

        targetInput.x = Mathf.Clamp(targetInput.x, -1, 1);
        targetInput.y = Mathf.Clamp(targetInput.y, -1, 1);
        targetInput.z = Mathf.Clamp(targetInput.z, -1, 1);

        Vector3 input = Vector3.MoveTowards(lastInput, targetInput, steeringSpeed * dt);
        lastInput = input;

        return input;
    }

    float CalculateThrottle(float minSpeed, float maxSpeed)
    {
        float input = 0;

        if (rb.velocity.z < minSpeed)
        {
            input = 1;
        }
        else if (rb.velocity.z > maxSpeed)
        {
            input = -1;
        }

        return input;
    }

    void CalculateWeapons(float dt)
    {
        if (playerShip == null) return;

        if (canUseCannon)
        {
            CalculateCannon(dt);
        }
    }


    void CalculateCannon(float dt)
    {
        if (playerShip.health == 0)
        {
            cannonFiring = false;
            return;
        }

        if (cannonFiring)
        {
            cannonBurstTimer = Mathf.Max(0, cannonBurstTimer - dt);

            if (cannonBurstTimer == 0)
            {
                cannonFiring = false;
                cannonCooldownTimer = cannonBurstCooldown;
                ship.canUseWeapons = false;
            }
        }
        else
        {
            cannonCooldownTimer = Mathf.Max(0, cannonCooldownTimer - dt);

            Vector3 targetPosition = Utilities.FirstOrderIntercept(rb.position, rb.velocity, bulletSpeed, playerShip.transform.position, playerShip.rb.velocity);

            Vector3 error = targetPosition - rb.position;
            float range = error.magnitude;
            Vector3 targetDir = error.normalized;
            float targetAngle = Vector3.Angle(targetDir, rb.rotation * Vector3.forward);

            if (range < cannonRange && targetAngle < cannonMaxFireAngle && cannonCooldownTimer == 0)
            {
                cannonFiring = true;
                cannonBurstTimer = cannonBurstLength;
                ship.canUseWeapons = true;
            }
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

public static class Utilities
{
    //first-order intercept using absolute target position
    public static Vector3 FirstOrderIntercept(
        Vector3 shooterPosition,
        Vector3 shooterVelocity,
        float shotSpeed,
        Vector3 targetPosition,
        Vector3 targetVelocity
    )
    {
        Vector3 targetRelativePosition = targetPosition - shooterPosition;
        Vector3 targetRelativeVelocity = targetVelocity - shooterVelocity;
        float t = FirstOrderInterceptTime(
            shotSpeed,
            targetRelativePosition,
            targetRelativeVelocity
        );
        return targetPosition + t * (targetRelativeVelocity);
    }//first-order intercept using relative target position
    public static float FirstOrderInterceptTime(
        float shotSpeed,
        Vector3 targetRelativePosition,
        Vector3 targetRelativeVelocity
    )
    {
        float velocitySquared = targetRelativeVelocity.sqrMagnitude;
        if (velocitySquared < 0.001f)
        {
            return 0f;
        }

        float a = velocitySquared - shotSpeed * shotSpeed;

        //handle similar velocities
        if (Mathf.Abs(a) < 0.001f)
        {
            float t = -targetRelativePosition.sqrMagnitude / (2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition));
            return Mathf.Max(t, 0f); //don't shoot back in time
        }

        float b = 2f * Vector3.Dot(targetRelativeVelocity, targetRelativePosition);
        float c = targetRelativePosition.sqrMagnitude;
        float determinant = b * b - 4f * a * c;

        if (determinant > 0f)
        { //determinant > 0; two intercept paths (most common)
            float t1 = (-b + Mathf.Sqrt(determinant)) / (2f * a),
                    t2 = (-b - Mathf.Sqrt(determinant)) / (2f * a);
            if (t1 > 0f)
            {
                if (t2 > 0f)
                    return Mathf.Min(t1, t2); //both are positive
                else
                {
                    return t1; //only t1 is positive
                }
            }
            else
            {
                return Mathf.Max(t2, 0f); //don't shoot back in time
            }
        }
        else if (determinant < 0f)
        { //determinant < 0; no intercept path
            return 0f;
        }
        else
        { //determinant = 0; one intercept path, pretty much never happens
            return Mathf.Max(-b / (2f * a), 0f); //don't shoot back in time
        }
    }
}