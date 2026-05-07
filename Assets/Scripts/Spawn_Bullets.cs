using UnityEngine;

public class Spawn_bullets : MonoBehaviour
{
    public GameObject bulletPrefab; // Prefab of the bullet to spawn
    public Transform spawnPoint;
    public float bulletSpeed = 20f; // Speed at which the bullet will travel
    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        { 
                GameObject bullet = Instantiate(bulletPrefab, spawnPoint.position + spawnPoint.forward, spawnPoint.rotation);
                Rigidbody rb = bullet.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.linearVelocity = spawnPoint.forward * bulletSpeed;
                }   
        }
    }

    
}
