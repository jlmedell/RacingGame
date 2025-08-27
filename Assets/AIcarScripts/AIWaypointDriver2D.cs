using UnityEngine;

[RequireComponent(typeof(SimpleCarMotor2D))]
public class AIWaypointDriver2D : MonoBehaviour
{
    public WaypointPath path;
    public float lookAhead = 0f; // for now default to 0 to get more accurate pathfinding 
    public float passRadius = 1.2f;
    public float cornerSlowdown = 0.6f; // 0..1
    [Tooltip("Increase AI max speed by this amount every time it completes a lap")]
    public float speedIncreasePerLap = 0f;

    [Range(0f,15f)] public float angleDeadZone = 3f;   // degrees

    // change to true to invert steering (debugging)
    public bool invertSteer = false;

    SimpleCarMotor2D motor; int current;

    void Awake() { motor = GetComponent<SimpleCarMotor2D>(); }
    void OnEnable() { LapCounter.AILapIncremented += HandleAILap; }
    void OnDisable() { LapCounter.AILapIncremented -= HandleAILap; }

    void HandleAILap()
    {
        if (motor != null && speedIncreasePerLap != 0f)
            motor.maxSpeed += speedIncreasePerLap;
    }
    void Start() { if (path && path.Count > 0) current = path.NearestIndex(transform.position); }

    void FixedUpdate()
    {
        if (!path || path.Count == 0) { motor.SetInputs(0, 0); return; }

        // choose a lookahead target a few segments ahead
        Vector2 pos = transform.position;
        int idx = current; Vector2 target = path.Get(idx).position;
        float d = 0; int guard = 0;
        while (d < lookAhead && guard++ < path.Count + 5)
        {
            Vector2 next = path.Get(path.Next(idx)).position;
            d += Vector2.Distance(target, next);
            idx = path.Next(idx); target = next;
        }

        Vector2 fwd = transform.up;
        Vector2 to = (target - pos).normalized;
        float ang = Vector2.SignedAngle(fwd, to);
        float steer = Mathf.Clamp(ang / 45f, -1f, 1f);

        // attempt to make a steering deadzone in order to travel in straightaways
        if (Mathf.Abs(ang) < angleDeadZone) steer = 0f;

        // to invert steering
        if (invertSteer)
            steer = -steer;

        float throttle = Mathf.Lerp(cornerSlowdown, 1f, 1f - Mathf.Clamp01(Mathf.Abs(ang) / 90f));
        motor.SetInputs(steer, throttle);

        if (Vector2.Distance(pos, path.Get(current).position) <= passRadius)
            current = path.Next(current);
    }
}