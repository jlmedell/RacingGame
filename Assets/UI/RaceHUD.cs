using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RaceHUD : MonoBehaviour
{
	[Header("Style")]
	public Color playerColor = new Color(0.2f, 0.9f, 1f, 1f);
	public Color aiColor = new Color(1f, 0.25f, 0.8f, 1f);
	public Color barBackground = new Color(1f, 1f, 1f, 0.08f);
	public int fontSize = 20;
	public float barHeight = 10f;

	// References
	Transform player;
	Transform ai;
	PlayerCarController playerController;
	SimpleCarMotor2D aiMotor;

	// UI
	Canvas canvas;
	TMP_Text lapsText;
	TMP_Text playerSpeedText;
	TMP_Text aiSpeedText;
	Image playerBarFill;
	Image aiBarFill;
	TMP_Text playerLabel;
	TMP_Text aiLabel;
	TMP_Text playerPlaceText;
	TMP_Text aiPlaceText;
	RectTransform playerCard;
	RectTransform aiCard;

	// Local sprite fallback
	static Sprite whiteSprite;

	// Track reference for simple position ordering
	WaypointPath waypointPath;

	// Speed sampling
	Vector3 lastPlayerPos;
	Vector3 lastAIPos;
	float playerSpeed;
	float aiSpeed;

	void Awake()
	{
		DontDestroyOnLoad(gameObject);
		BuildUI();
		FindActors();
	}

	void OnEnable()
	{
		LapCounter.PlayerLapIncremented += UpdateLapText;
		LapCounter.AILapIncremented += UpdateLapText;
	}

	void Start()
	{
		// Initial update of lap text
		UpdateLapText();
	}

	void OnDisable()
	{
		LapCounter.PlayerLapIncremented -= UpdateLapText;
		LapCounter.AILapIncremented -= UpdateLapText;
	}

	void Update()
	{
		if (player == null || ai == null) FindActors();

		// Compute speeds from position delta to avoid engine-specific rb API differences
		float dt = Mathf.Max(0.0001f, Time.deltaTime);
		if (player != null)
		{
			playerSpeed = (player.position - lastPlayerPos).magnitude / dt;
			lastPlayerPos = player.position;
		}
		if (ai != null)
		{
			aiSpeed = (ai.position - lastAIPos).magnitude / dt;
			lastAIPos = ai.position;
		}

		UpdateBarsAndText();
	}

	void BuildUI()
	{
		// Canvas
		var go = new GameObject("HUD Canvas");
		go.transform.SetParent(transform);
		canvas = go.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		go.AddComponent<GraphicRaycaster>();

		Sprite uiSprite = GetWhiteSprite();

		// Laps (hidden)
		lapsText = null;

		// Player group (top-left)
		CreateSpeedGroup(go.transform, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(20, -20),
			"PLAYER", playerColor, uiSprite, out playerCard, out playerBarFill, out playerSpeedText, out playerLabel, out playerPlaceText, rightAlign: false);

		// AI group (top-right)
		CreateSpeedGroup(go.transform, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-20, -20),
			"AI", aiColor, uiSprite, out aiCard, out aiBarFill, out aiSpeedText, out aiLabel, out aiPlaceText, rightAlign: true);

		UpdateLapText();
	}

	GameObject CreateTMP(string text, int size, TextAlignmentOptions align)
	{
		var tgo = new GameObject("TMP_Text");
		var tmp = tgo.AddComponent<TextMeshProUGUI>();
		tmp.text = text;
		tmp.fontSize = size;
		tmp.alignment = align;
		tmp.raycastTarget = false;
		return tgo;
	}

	GameObject CreateTMPWithShadow(string text, int size, TextAlignmentOptions align)
	{
		var container = new GameObject("TMP_Shadowed");
		container.AddComponent<RectTransform>();
		var shadow = new GameObject("Shadow");
		shadow.transform.SetParent(container.transform, false);
		var s = shadow.AddComponent<TextMeshProUGUI>();
		s.text = text; s.fontSize = size; s.alignment = align; s.raycastTarget = false; s.color = new Color(0f, 0f, 0f, 0.6f);
		var srt = shadow.GetComponent<RectTransform>(); srt.anchoredPosition = new Vector2(1.5f, -1.5f);

		var main = new GameObject("Main");
		main.transform.SetParent(container.transform, false);
		var m = main.AddComponent<TextMeshProUGUI>();
		m.text = text; m.fontSize = size; m.alignment = align; m.raycastTarget = false;
		return container; // return container so caller can parent it; read TMP via GetComponentInChildren
	}

	static Sprite GetWhiteSprite()
	{
		if (whiteSprite != null) return whiteSprite;
		var tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
		tex.SetPixels(new Color[] { Color.white, Color.white, Color.white, Color.white });
		tex.Apply();
		whiteSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100f);
		whiteSprite.name = "HUD_WhiteSprite";
		whiteSprite.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
		return whiteSprite;
	}

	void CreateSpeedGroup(Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPos,
		string title, Color accent, Sprite sprite, out RectTransform card, out Image fillImage, out TMP_Text speedText, out TMP_Text titleText, out TMP_Text placeText, bool rightAlign = false)
	{
		// Card container
		var group = new GameObject(title + " Card");
		var rt = group.AddComponent<RectTransform>();
		rt.SetParent(parent, false);
		rt.anchorMin = anchorMin; rt.anchorMax = anchorMax; rt.pivot = pivot;
		rt.sizeDelta = new Vector2(280, 50);
		rt.anchoredPosition = anchoredPos;
		card = rt;

		// Card background
		var cardBgGO = new GameObject(title + " CardBG");
		var cardBg = cardBgGO.AddComponent<Image>();
		cardBg.sprite = sprite; cardBg.type = Image.Type.Simple; cardBg.color = new Color(0f, 0f, 0f, 0.18f);
		var rtCardBg = cardBgGO.GetComponent<RectTransform>();
		rtCardBg.SetParent(group.transform, false);
		rtCardBg.anchorMin = new Vector2(0f, 0f);
		rtCardBg.anchorMax = new Vector2(1f, 1f);
		rtCardBg.offsetMin = new Vector2(0f, 0f);
		rtCardBg.offsetMax = new Vector2(0f, 0f);
		var outline = cardBgGO.AddComponent<Outline>();
		outline.effectColor = new Color(1f, 1f, 1f, 0.08f);
		outline.effectDistance = new Vector2(1f, -1f);

		// Will add text over the bar after creating it
		titleText = null;
		placeText = null;

		// Bar BG
		var bgGO = new GameObject(title + " Bar BG");
		var bg = bgGO.AddComponent<Image>();
		bg.color = barBackground; bg.sprite = sprite; bg.type = Image.Type.Simple;
		var rtBg = bgGO.GetComponent<RectTransform>();
		rtBg.SetParent(group.transform, false);
		rtBg.anchorMin = new Vector2(0f, 0f);
		rtBg.anchorMax = new Vector2(1f, 0f);
		rtBg.pivot = new Vector2(0.5f, 0f);
		rtBg.sizeDelta = new Vector2(-16f, barHeight + 6f);
		rtBg.anchoredPosition = new Vector2(0, 6f);
		rtBg.SetSiblingIndex(0);

		// Tick marks every 10%
		int ticks = 10;
		for (int i = 1; i < ticks; i++)
		{
			var tick = new GameObject(title + " Tick " + i);
			var imgT = tick.AddComponent<Image>();
			imgT.sprite = sprite; imgT.type = Image.Type.Simple; imgT.color = new Color(1f, 1f, 1f, 0.12f);
			var rtT = tick.GetComponent<RectTransform>();
			rtT.SetParent(rtBg, false);
			rtT.anchorMin = new Vector2(i / (float)ticks, 0f);
			rtT.anchorMax = new Vector2(i / (float)ticks, 1f);
			rtT.sizeDelta = new Vector2(1f, -6f);
		}

		// Bar Fill
		var fillGO = new GameObject(title + " Bar Fill");
		var fill = fillGO.AddComponent<Image>();
		fill.color = accent; fill.sprite = sprite; fill.type = Image.Type.Filled; fill.fillMethod = Image.FillMethod.Horizontal; fill.fillOrigin = (int)Image.OriginHorizontal.Left; fill.fillAmount = 0f;
		var rtFill = fillGO.GetComponent<RectTransform>();
		rtFill.SetParent(rtBg, false);
		rtFill.anchorMin = new Vector2(0f, 0.5f);
		rtFill.anchorMax = new Vector2(1f, 0.5f);
		rtFill.pivot = new Vector2(0f, 0.5f);
		rtFill.sizeDelta = new Vector2(-8f, barHeight);

		// Speed text (hidden, no number on bar)
		speedText = null;

		// Laps text (left side above the bar)
		string initialLapText = "LAPS: 0";
		// Always use the current lap count for the initial lap text
		if (title == "PLAYER") initialLapText = $"LAPS: {LapCounter.PlayerLaps}";
		else if (title == "AI") initialLapText = $"LAPS: {LapCounter.AILaps}";
		var lapsGO = CreateTMPWithShadow(initialLapText, fontSize - 2, TextAlignmentOptions.MidlineLeft);
		var rtLaps = lapsGO.GetComponent<RectTransform>();
		rtLaps.SetParent(rtBg, false);
		rtLaps.anchorMin = new Vector2(0.9f, -1f);
		rtLaps.anchorMax = new Vector2(0.9f, -1f);
		rtLaps.pivot = new Vector2(0f, 0f);
		rtLaps.anchoredPosition = new Vector2(8, -2);
		placeText = lapsGO.transform.Find("Main").GetComponent<TMP_Text>(); // Get the main text, not the shadow
		Debug.Log($"HUD: Created {title} lap text - placeText is null: {placeText == null}");

		// Title (right side above the bar, closer to laps text)
		var titleGO = CreateTMPWithShadow(title, fontSize - 2, TextAlignmentOptions.MidlineLeft);
		var rtTitle = titleGO.GetComponent<RectTransform>();
		rtTitle.SetParent(rtBg, false);
		rtTitle.anchorMin = new Vector2(0.2f, -1f);
		rtTitle.anchorMax = new Vector2(0.2f, -1f);
		rtTitle.pivot = new Vector2(0f, 0f);
		rtTitle.anchoredPosition = new Vector2(8, -2);
		titleText = titleGO.transform.Find("Main").GetComponent<TMP_Text>(); // Get the main text, not the shadow
		Debug.Log($"HUD: Created {title} title text - titleText is null: {titleText == null}");

		fillImage = fill;
	}

	void FindActors()
	{
		var pObj = GameObject.FindGameObjectWithTag("Player");
		if (pObj != null)
		{
			player = pObj.transform;
			playerController = pObj.GetComponent<PlayerCarController>();
			lastPlayerPos = player.position;
		}
		var aiObj = GameObject.FindGameObjectWithTag("AI");
		if (aiObj != null)
		{
			ai = aiObj.transform;
			aiMotor = aiObj.GetComponent<SimpleCarMotor2D>();
			lastAIPos = ai.position;
		}
		if (waypointPath == null)
		{
			waypointPath = FindObjectOfType<WaypointPath>();
		}
	}

	void UpdateLapText()
	{
		Debug.Log($"HUD: UpdateLapText called! Player: {LapCounter.PlayerLaps}, AI: {LapCounter.AILaps}");
		
		if (lapsText != null)
		{
			lapsText.text = $"Laps: P {LapCounter.PlayerLaps} | AI {LapCounter.AILaps}";
		}
		
		// Also update the individual lap counters in each speedometer card
		UpdateBarsAndText();
	}

	void UpdateBarsAndText()
	{
		// Player
		float playerMax = (playerController != null) ? Mathf.Max(0.01f, playerController.moveSpeed) : Mathf.Max(0.01f, playerSpeed);
		float pFill = Mathf.Clamp01(playerSpeed / playerMax);
		if (playerBarFill != null)
		{
			playerBarFill.fillAmount = pFill;
			playerBarFill.color = BarGradient(playerColor, pFill);
		}
		// No speed text display anymore

		// AI
		float aiMax = (aiMotor != null) ? Mathf.Max(0.01f, aiMotor.maxSpeed) : Mathf.Max(0.01f, aiSpeed);
		float aFill = Mathf.Clamp01(aiSpeed / aiMax);
		if (aiBarFill != null)
		{
			aiBarFill.fillAmount = aFill;
			aiBarFill.color = BarGradient(aiColor, aFill);
		}
		// No speed text display anymore

		// Update lap counts in each card
		if (playerPlaceText != null)
		{
			playerPlaceText.text = "LAPS: " + LapCounter.PlayerLaps;
			Debug.Log($"HUD: Successfully updated PLAYER lap text to: LAPS: {LapCounter.PlayerLaps}");
		}
		else
		{
			Debug.LogError("HUD: playerPlaceText is NULL!");
		}
		
		if (aiPlaceText != null)
		{
			aiPlaceText.text = "LAPS: " + LapCounter.AILaps;
			Debug.Log($"HUD: Successfully updated AI lap text to: LAPS: {LapCounter.AILaps}");
		}
		else
		{
			Debug.LogError("HUD: aiPlaceText is NULL!");
		}
	}

	Color BarGradient(Color baseColor, float t)
	{
		// Blend from base to warm red near max
		Color warm = new Color(1f, 0.45f, 0.2f, 1f);
		return Color.Lerp(baseColor, warm, Mathf.SmoothStep(0f, 1f, t));
	}

}


