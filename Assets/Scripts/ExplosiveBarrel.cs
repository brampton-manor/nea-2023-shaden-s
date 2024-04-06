using UnityEngine;
using Photon.Pun;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UIElements;

public class ExplosiveBarrel : Destructible, IDamageable
{
    [SerializeField] GameObject explosionEffect;
    [SerializeField] GameObject destroyedBarrel;
    [SerializeField] AudioClip explosionSound;
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionForce = 1000f;
    private bool hasExploded = false;

    public string barrelID;
    void Awake()
    {
        barrelID = "Barrel_" + transform.position.x + "_" + transform.position.z;
    }

    public override void TakeDamage(float damage)
    {
        if (!hasExploded)
        {
            Die();
        }
    }

    public override void Die()
    {
        if (hasExploded) return; 
        hasExploded = true;

        NetworkManager.Instance.ExplodeBarrelRPC(transform.position, barrelID);

        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider nearbyObject in colliders)
        {
            Rigidbody rb = nearbyObject.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.AddExplosionForce(explosionForce, transform.position, explosionRadius);
            }

            IDamageable damageable = nearbyObject.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.TakeDamage(75); 
            }
        }
    }

    public void SyncExplosion()
    {
        AudioSource.PlayClipAtPoint(explosionSound, transform.position);
        Instantiate(explosionEffect, transform.position, Quaternion.identity);
        Instantiate(destroyedBarrel, transform.position, Quaternion.identity); 
        Destroy(gameObject);
    }

}
