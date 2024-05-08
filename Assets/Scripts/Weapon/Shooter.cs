using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shooter : MonoBehaviour
{
    public Player instigator;

    [SerializeField]
    protected Projectile projectilePrefab;

    [SerializeField]
    protected float fireRate;
    [SerializeField]
    protected float launchStrength;
    [SerializeField]
    protected float xVariance, yVariance;

    public void StartShooting()
    {
        StartCoroutine(ShootOnLoop());
    }

    public void StopShooting()
    {
        StopAllCoroutines();
    }

    public IEnumerator ShootOnLoop()
    {
        while(true)
        {
            Shoot();
            yield return new WaitForSeconds(fireRate);
        }
    }

    public void Shoot()
    {
        Projectile newProjectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
        newProjectile.Instigator = instigator;

        Rigidbody rigidbody = newProjectile.GetComponent<Rigidbody>();
        if(rigidbody)
        {
            Vector3 direction = transform.forward;
            direction.x += Random.Range(0f, xVariance);
            direction.y += Random.Range(0f, yVariance);
            rigidbody.AddForce(direction.normalized * launchStrength, ForceMode.Impulse);
        }
    }
}
