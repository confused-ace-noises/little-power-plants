using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    [Header("Movement Settings")]
    public float fastMoveX = 16f;
    public float fastMoveY = 16f;
    public float speed = 5f;
    public float fastMoveMult = 10f;
    public float stopStrength = 5f;

    [SerializeField] new Rigidbody2D rigidbody;

    // -------------------------
    Vector2 direction;
    bool isFastMoving;
    bool wantsToStop;
    bool needsUpdate;

    public void OnMove(InputValue value)
    {
        direction = value.Get<Vector2>();
        if (direction != Vector2.zero)
            needsUpdate = true;
    }

    public void OnFastMove(InputValue value)
    {
        if (!isFastMoving)
        {
            isFastMoving = value.Get<float>() != 0f;
        }
    }

    public void OnStop(InputValue value)
    {
        if (!wantsToStop)
        {
            wantsToStop = value.Get<float>() != 0f;
        }
    }

    void FixedUpdate()
    {
        if (wantsToStop)
        {
            Vector2 stopForce = stopStrength * Time.fixedDeltaTime * -rigidbody.linearVelocity;
            rigidbody.AddForce(stopForce, ForceMode2D.Impulse);

            // stop if below a certain speed-
            if (rigidbody.linearVelocity.magnitude < 0.1f)
            {
                rigidbody.AddForce(-rigidbody.linearVelocity, ForceMode2D.Impulse);
                wantsToStop = false;
            } else
            {
                // override player movement
                needsUpdate = false;
            }
        }

        rigidbody.linearVelocity *= 0.9666f; 

        if (!needsUpdate) return;


        rigidbody.AddForce(speed * Time.deltaTime * direction, ForceMode2D.Impulse);
    }
}
