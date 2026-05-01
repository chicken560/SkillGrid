using UnityEngine;

public class Footsteps : MonoBehaviour
{
    public AudioSource footsteps;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)){
            footsteps.enabled = true;
        }
        else
        {
            footsteps.enabled = false;
        }
    }
}
