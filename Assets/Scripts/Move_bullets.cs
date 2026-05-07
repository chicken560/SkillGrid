using UnityEngine;

public class Move_bullets : MonoBehaviour
{
    // Update is called once per frame
    public float speed = 20f;
    
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
    public void OnCollisionEnter(Collision collision)
    {
    }
}
