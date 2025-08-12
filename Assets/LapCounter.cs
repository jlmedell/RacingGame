using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LapCounter : MonoBehaviour
{
    public TMP_Text lapsText;       
    private int playerLaps = 0;
    private int aiLaps = 0;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerLaps++;
        }
        else if (other.CompareTag("AI"))
        {
            aiLaps++;
        }

        lapsText.text = $"Player Laps: {playerLaps}\nAI Laps: {aiLaps}";
    }
}
