using UnityEngine;
using UnityEditor;

public class ShiftTrackChildren : EditorWindow
{
    float shiftAmount = -0.5f;

    [MenuItem("Tools/Shift Track Children")]
    public static void ShowWindow()
    {
        GetWindow<ShiftTrackChildren>("Shift Track Children");
    }

    void OnGUI()
    {
        shiftAmount = EditorGUILayout.FloatField("Shift Amount (units)", shiftAmount);

        if (GUILayout.Button("Shift All Children Y+"))
        {
            ShiftChildren();
        }
    }

    void ShiftChildren()
    {
        GameObject track = GameObject.Find("Track");
        if (track == null)
        {
            Debug.LogError("No GameObject named 'Track' found in scene.");
            return;
        }

        Undo.RecordObject(track.transform, "Shift Track Children");

        foreach (Transform child in track.transform)
        {
            Undo.RecordObject(child, "Shift Child Position");
            child.position += new Vector3(0, shiftAmount);
            EditorUtility.SetDirty(child);
        }
        Debug.Log("Shifted " + track.transform.childCount + " children by " + shiftAmount + " units on Y axis.");
    }
}