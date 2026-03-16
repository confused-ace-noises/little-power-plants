using UnityEngine;

public class MoveLinear : MonoBehaviour
{
    float speed = 5f;
    Vector3 moveTo;
    bool moving = false;

    public void MoveTo(Vector3 globalPosition, float speed)
    {
        moveTo = globalPosition;
        moving = true;
        this.speed = speed;
    } 

    void FixedUpdate()
    {
        if (!moving) return;

        transform.position = Vector3.MoveTowards(
            transform.position,
            moveTo,
            speed * Time.fixedDeltaTime
        );

        // if there's a floating point error, it's close enough.
        if (Vector3.Distance(transform.position, moveTo) < 0.001f)
        {
            transform.position = moveTo;
            moving = false;
        }
    }    
}
