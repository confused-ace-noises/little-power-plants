using UnityEngine;

public class Follow : MonoBehaviour
{
    [SerializeField] Transform toFollow;

    // Update is called once per frame
    void LateUpdate()
    {
        transform.position = toFollow.position;
        transform.rotation = toFollow.rotation;
    }
}
