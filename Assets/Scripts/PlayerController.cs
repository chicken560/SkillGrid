using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public Vector2 move;
    public void MoveInput(Vector2 newMoveDirection)
    {
        move = newMoveDirection;
    }
    public void OnMove(InputValue value)
    {
        MoveInput(value.Get<Vector2>());
    }
}
