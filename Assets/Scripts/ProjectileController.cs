using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float damage;
    [SerializeField] float maxDistance;

    RaycastHit hit;
    float currentDistance;

    void FixedUpdate()
    {
        transform.position += transform.forward * speed * Time.fixedDeltaTime;

        if (Physics.Raycast(transform.position, transform.forward, out hit, 1))
        {
            if (hit.collider.CompareTag("Player") || (hit.collider.CompareTag("Enemy")))
                hit.collider.GetComponent<iDamageable>().Damage(damage);

            Destroy(gameObject);
            return;
        }

        currentDistance += speed * Time.fixedDeltaTime;
        if (currentDistance >= maxDistance)
        {
            Destroy(gameObject);
            return;
        }
    }
}
