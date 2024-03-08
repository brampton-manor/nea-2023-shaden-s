using UnityEngine;
using Photon.Pun;
using UnityEngine.ProBuilder.Shapes;

public class ExplosiveBarrel : Destructible, IDamageable
{
    [SerializeField] private float explosionRadius = 5f;
    [SerializeField] private float explosionForce = 1000f;
    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private GameObject destroyedBarrel;
    [SerializeField] private AudioClip explosionSound;

    public string barrelID;

    private bool hasExploded = false;

    void Awake()
    {
        barrelID = GenerateUniqueID(4);
    }

    string GenerateUniqueID(int length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new System.Random();
        var uniqueID = new char[length];
        for (int i = 0; i < length; i++) uniqueID[i] = chars[random.Next(chars.Length)];
        return new string(uniqueID);
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
        if (hasExploded) return; // Prevent recursion or multiple calls
        hasExploded = true;

        // Play explosion sound
        AudioSource.PlayClipAtPoint(explosionSound, transform.position);

        // Show explosion effect
        Instantiate(explosionEffect, transform.position, Quaternion.identity);
        Instantiate(destroyedBarrel, transform.position, Quaternion.identity);

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
                damageable.TakeDamage(150); // Consider adding distance-based damage reduction
            }
        }

        Destroy(gameObject);
    }
}
