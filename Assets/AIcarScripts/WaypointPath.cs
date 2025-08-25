//Attach to any GameObject that has child Transforms as waypoints.
using UnityEngine;

[ExecuteAlways]
[AddComponentMenu("AI/Waypoint Path")]
public class WaypointPath : MonoBehaviour
{
    [Header("Path Settings")]
    public bool loop = true;
    [Tooltip("Visual size in Scene view only")] public float gizmoSize = 0.2f;
    [Tooltip("Show child indices in Scene view")] public bool labelIndices = true;
    [Tooltip("Color for gizmos")] public Color lineColor = new Color(1f, 0.92f, 0.016f, 1f); // yellow
    public Color pointColor = new Color(1f, 0f, 1f, 1f); // magenta

    /// <summary>Total number of child waypoints.</summary>
    public int Count => transform.childCount;
    public int Next(int index) => NextIndex(index);

    /// <summary>Get the i-th waypoint Transform (clamped or wrapped based on loop).</summary>
    public Transform Get(int index)
    {
        if (Count == 0) return transform;
        if (loop)
        {
            int i = (index % Count + Count) % Count;
            return transform.GetChild(i);
        }
        else
        {
            int i = Mathf.Clamp(index, 0, Count - 1);
            return transform.GetChild(i);
        }
    }

    /// <summary>Return the next index according to loop setting.</summary>
    public int NextIndex(int index)
    {
        if (Count == 0) return 0;
        return loop ? (index + 1) % Count : Mathf.Min(index + 1, Count - 1);
    }

    /// <summary>Find index of the closest waypoint to a world position.</summary>
    public int NearestIndex(Vector2 worldPos)
    {
        if (Count == 0) return 0;
        int best = 0; float bestD = float.PositiveInfinity;
        for (int i = 0; i < Count; i++)
        {
            float d = ((Vector2)transform.GetChild(i).position - worldPos).sqrMagnitude;
            if (d < bestD) { bestD = d; best = i; }
        }
        return best;
    }

    // ── Gizmos for editing ───────────────────────────────────────────────────
    private void OnDrawGizmos()
    {
        int n = Count; if (n == 0) return;
        for (int i = 0; i < n; i++)
        {
            Vector3 a = transform.GetChild(i).position;
            Vector3 b = transform.GetChild((i + 1) % n).position;
            Gizmos.color = pointColor; Gizmos.DrawWireSphere(a, gizmoSize);
            if (loop || i < n - 1) { Gizmos.color = lineColor; Gizmos.DrawLine(a, b); }
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (!labelIndices) return; int n = Count; if (n == 0) return;
        var style = new GUIStyle(UnityEditor.EditorStyles.boldLabel) { normal = { textColor = Color.white }, fontSize = 11 };
        for (int i = 0; i < n; i++)
        {
            var p = transform.GetChild(i).position + Vector3.up * 0.2f;
            UnityEditor.Handles.Label(p, i.ToString(), style);
        }
    }
#endif
}
