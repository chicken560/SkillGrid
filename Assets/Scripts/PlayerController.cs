using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    void Update()
    {
        Vector3 moveInput = Vector3.zero;
        if (Keyboard.current.wKey.isPressed)
            moveInput.z += 1;
        if (Keyboard.current.sKey.isPressed)
            moveInput.z -= 1;
        if (Keyboard.current.aKey.isPressed)
            moveInput.x -= 1;
        if (Keyboard.current.dKey.isPressed)
            moveInput.x += 1;
        if (Keyboard.current.spaceKey.isPressed)
        {   // Jump logic can be implemented here
            moveInput.y += jumpForce * Time.deltaTime;
        }
            moveInput = moveInput.normalized * moveSpeed * Time.deltaTime;
        transform.Translate(moveInput);
    }
}
