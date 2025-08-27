using UnityEngine;

// Ensures a HUD exists in any scene at runtime without manual setup
public class RaceHUDBootstrap : MonoBehaviour
{
	[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
	static void EnsureHUD()
	{
		var existing = Object.FindObjectOfType<RaceHUD>();
		if (existing != null) return;
		var go = new GameObject("RaceHUD");
		go.AddComponent<RaceHUD>();
	}
}


