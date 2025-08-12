using UnityEngine;

public class PlayerCarController : MonoBehaviour
{
    public float moveSpeed = 100f;     //units per second
    public float turnSpeed = 200f;   //degrees per second

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        float moveInput = Mathf.Max(0, Input.GetAxisRaw("Vertical"));
        float turnInput = Input.GetAxisRaw("Horizontal");

        transform.Rotate(0, 0, -turnInput * turnSpeed * Time.deltaTime);
        Vector3 forward = transform.up;
        transform.position += forward * moveInput * moveSpeed * Time.deltaTime;
    }
}
