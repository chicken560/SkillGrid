using UnityEngine;

public class Spawn_bullets : MonoBehaviour
{
    public GameObject bulletPrefab; // Prefab of the bullet to spawn
    public Transform spawnPoint; // Point from where the bullet will be spawned

    private void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        { 
                Instantiate(bulletPrefab, spawnPoint.position, spawnPoint.rotation);
        }
    }

    
}
