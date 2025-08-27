using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

[RequireComponent(typeof(Collider2D))]
public class LapCounter : MonoBehaviour
{
    public TMP_Text lapsText;      

    // Global lap counts/events so other systems (HUD/AI) can react
    public static int PlayerLaps { get; private set; }
    public static int AILaps { get; private set; }
    public static event Action PlayerLapIncremented;
    public static event Action AILapIncremented;

    // Reset static counters (useful for scene changes)
    public static void ResetCounters()
    {
        PlayerLaps = 0;
        AILaps = 0;
    }

    private int playerLaps = 0;
    private int aiLaps = 0;

    private void Start()
    {
        // Reset static counters when this LapCounter starts
        ResetCounters();
        playerLaps = 0;
        aiLaps = 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerLaps++;
            PlayerLaps++;
            Debug.Log($"PLAYER LAP COMPLETED! Local: {playerLaps}, Global: {PlayerLaps}");
            PlayerLapIncremented?.Invoke();
        }
        else if (other.CompareTag("AI"))
        {
            aiLaps++;
            AILaps++;
            Debug.Log($"AI LAP COMPLETED! Local: {aiLaps}, Global: {AILaps}");
            AILapIncremented?.Invoke();
        }

        if (lapsText != null)
        {
            lapsText.text = $"Player Laps: {playerLaps}\nAI Laps: {aiLaps}";
        }
    }
}
