using UnityEngine;

public class Footsteps : MonoBehaviour
{
    public AudioSource footsteps;
    public AudioSource running;
    public AudioSource jump;

    void Update()
    {
        if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.D)){
            footsteps.enabled = true;
        }
        else
        {
            footsteps.enabled = false;
        }
        if(Input.GetKey(KeyCode.LeftShift)){
            running.enabled = true;
            footsteps.enabled = false;
        }
        else
        {
            running.enabled = false;
        }
        if(Input.GetKey(KeyCode.Space)) {
            jump.enabled = true;
            running.enabled = false;
            footsteps.enabled = false;
        }
        else
        {
            jump.enabled = false;
        }
    }
}
