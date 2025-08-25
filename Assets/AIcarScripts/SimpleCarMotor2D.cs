using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SimpleCarMotor2D : MonoBehaviour
{
    public float maxSpeed = 5f;
    public float acceleration = 15f;
    public float braking = 40f;
    public float turnRate = 200f;
    public float grip = 6f;

    Rigidbody2D rb; float steer; float throttle;

    void Awake() { rb = GetComponent<Rigidbody2D>(); rb.gravityScale = 0; rb.angularDamping = 0.5f; }
    public void SetInputs(float s, float t) { steer = Mathf.Clamp(s, -1, 1); throttle = Mathf.Clamp(t, -1, 1); }

    void FixedUpdate()
    {
        Vector2 fwd = transform.up;
        float acc = throttle >= 0 ? acceleration : braking;
        rb.AddForce(fwd * (acc * throttle));

        if (rb.linearVelocity.magnitude > maxSpeed) rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;

        float steerStrength = Mathf.Clamp01(Mathf.Abs(Vector2.Dot(rb.linearVelocity, fwd)) / Mathf.Max(0.1f, maxSpeed * 0.5f));
        rb.angularVelocity = (-steer * turnRate * steerStrength);

        Vector2 right = new Vector2(fwd.y, -fwd.x);
        float side = Vector2.Dot(rb.linearVelocity, right);
        rb.AddForce(-right * side * grip);
    }
}