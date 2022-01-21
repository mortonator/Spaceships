using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShipGunHandler : MonoBehaviour
{
    [SerializeField] ProjectileController projectilePrefab;
    Light glow;

    void Start()
    {
        glow = GetComponent<Light>();
        glow.enabled = false;
    }

    public void Fire()
    {
        glow.enabled = true;
        StartCoroutine(EndFire());
    }
    IEnumerator EndFire()
    {
        yield return new WaitForSeconds(1);
        glow.enabled = false;
        Instantiate(projectilePrefab, transform.position + transform.forward, transform.rotation);
        yield return null;
    }
}
