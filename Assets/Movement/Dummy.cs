using NUnit.Framework;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float fastMoveX = 16f;
    public float fastMoveY = 16f;
    public float speed = 5f;

    [SerializeField] MoveLinear linear;

    // -------------------------
    bool isFastMoving;
    Vector2 direction;
    bool hasToMove;

    public void OnMove(InputValue value)
    {
        direction = value.Get<Vector2>();
        if (direction != Vector2.zero)
            hasToMove = true;
    }

    public void OnFastMove(InputValue value)
    {
        isFastMoving = value.Get<float>() != 0f;
    }

    void FixedUpdate()
    {
        if (!hasToMove) return;

        if (isFastMoving)
        {
            linear.MoveTo(transform.position + new Vector3(direction.x * fastMoveX, direction.y * fastMoveY, 0f), speed); 
        } else
        {
            linear.MoveTo(transform.position + (Vector3)direction, speed);
        }
    }
}
