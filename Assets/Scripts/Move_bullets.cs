using UnityEngine;

public class Move_bullets : MonoBehaviour
{
    // Update is called once per frame
    public float speed = 20f;
    private Rigidbody rb;
    private GameObject Bullets;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }
    void Update()
    {
        rb.linearVelocity = transform.forward * speed;
    }
    
}
