using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 2½ºÅ×ÀÌÁö: Á¦´Ü(°ü) »çÅÁ ÆÛÁñ
///
/// [±âÈ¹¼­ ³»¿ë]
/// - °ü Å¬¸¯ ¡æ ÆÛÁñ È­¸éÀ¸·Î ÀüÈ¯ (CameraPuzzleBase »ó¼Ó)
/// - ÆÛÁñ È­¸é: ¸®¼Â Çâ·Î, ¿©ÀÚ¾ÆÀÌ »çÁø 16Àå, »çÅÁ 5°³
/// - ¸ÊÀÇ ¹æ¼® À§ °¢¸ñÀÎÇü À§Ä¡¿Í µ¿ÀÏÇÑ À§Ä¡¿¡ °°Àº »öÀÇ »çÅÁÀ» »çÁø À§¿¡ ¿Ã·ÁµÎ¸é µÊ
/// - »çÅÁÀ» ¿Ã·ÁµÎ¸é ¿©ÀÚ¾ÆÀÌ ¾ó±¼ÀÌ ¹«Ç¥Á¤¡æ¿ô´Â Ç¥Á¤À¸·Î ¹Ù²ñ
/// - Æ²¸° À§Ä¡¿¡ »çÅÁÀ» ¿Ã¸®¸é ÀÚµ¿ ¸®¼Â
/// - 5°³ ´Ù ¸ÂÀ¸¸é ¸¶Áö¸· »çÅÁÀ» ¹ÞÀº 5¹øÂ° »çÁø¸¸ È°Â¦ ¿ô´Â Ç¥Á¤À¸·Î ¹Ù²ñ
/// - ÆÛÁñ µµÁß ³ª°¡¸é ÀüºÎ ¸®¼Â
/// - ÇØ°á ÈÄ: Å©¸®Ã³(ÀÎ¿µ)°¡ ¸Ê µÚÂÊ Áß¾ÓÀ¸·Î ÀÌµ¿ ¡æ ÀÛÀº ¹æ ÁøÀÔ °¡´É
///
/// [Á¡ÇÁ½ºÄÉ¾î]
/// ÆÛÁñ ¿Ï·á ÈÄ °ü ¾Õ¿¡¼­ µÚ¸¦ µ¹¾Æº¸¸é ¹æ¼®¿¡ »ç¶÷ ÀÎ¿µ°ú Å« ¹Ú¼ö¼Ò¸®
/// (º°µµ Stage2_JumpscareTrigger·Î Ã³¸®)
/// </summary>
public class Stage2_AltarCandyPuzzle : CameraPuzzleBase
{
	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
	// »çÅÁ ½½·Ô µ¥ÀÌÅÍ
	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

	[System.Serializable]
	public class CandySlotData
	{
		[Header("½½·Ô Á¤º¸")]
		public int slotIndex;           // 0~15: ¹æ¼® ¹øÈ£ (ÆÛÁñ È­¸é»ó À§Ä¡)
		public bool isCorrectSlot;      // ÀÌ ½½·ÔÀÌ Á¤´äÀÎÁö ¿©ºÎ

		[Header("Á¤´ä »çÅÁ »ö")]
		public Color correctCandyColor; // ÀÌ ½½·Ô¿¡ ¿Ã·Á¾ß ÇÏ´Â »çÅÁ »ö»ó

		[Header("UI ¿¬°á")]
		public Image photoImage;        // ¹æ¼® À§ ¿©ÀÚ¾ÆÀÌ »çÁø (16Àå Áß 1Àå)
		public GameObject candyVisual;  // »çÅÁ ºñÁÖ¾ó ¿ÀºêÁ§Æ®

		[Header("»çÁø Ç¥Á¤ ½ºÇÁ¶óÀÌÆ®")]
		public Sprite neutralFaceSprite;    // ¹«Ç¥Á¤
		public Sprite smileFaceSprite;      // ¿ô´Â Ç¥Á¤ (»çÅÁ ¿Ã·ÈÀ» ¶§)
		public Sprite bigSmileFaceSprite;   // È°Â¦ ¿ô´Â Ç¥Á¤ (¸¶Áö¸· Á¤´ä)
	}

	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
	// Inspector ¼³Á¤
	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

	[Header("»çÅÁ ½½·Ô (16°³ ¹æ¼®)")]
	[SerializeField] private List<CandySlotData> slots = new List<CandySlotData>();

	[Header("¼±ÅÃ °¡´ÉÇÑ »çÅÁ 5°³")]
	[SerializeField] private List<CandyButton> candyButtons = new List<CandyButton>();

	[Header("¸®¼Â Çâ·Î")]
	[SerializeField] private Button resetButton;

	[Header("³ª°¡±â ¹öÆ°")]
	[SerializeField] private Button exitButton;

	[Header("Å©¸®Ã³ ¿¬°á")]
	[SerializeField] private Stage2_ShadowCreature shadowCreature;

	[Header("ÆÛÁñ ¿Ï·á ´ë»ç")]
	[SerializeField] private string speaker = "¼Ò³â";
	[TextArea(2, 4)]
	[SerializeField] private string solveDialogue = "...¹Ú¼ö¼Ò¸®°¡ µé¸°´Ù.";
	[TextArea(2, 4)]
	[SerializeField] private string wrongPlacementDialogue = "¾Æ´Ñ °Í °°´Ù...";
	[TextArea(2, 4)]
	[SerializeField] private string exitPuzzleDialogue = "ÆÛÁñÀ» ³ª°¬´Ù. ´Ù½Ã °üÀ» »ìÆìºÁ¾ß ÇÒ °Í °°´Ù.";

	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
	// ³»ºÎ »óÅÂ
	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

	// ÇöÀç ¹èÄ¡ »óÅÂ: slotIndex ¡æ ¹èÄ¡µÈ »çÅÁ »ö
	private Dictionary<int, Color> _placements = new Dictionary<int, Color>();

	// ÇöÀç ¼±ÅÃµÈ »çÅÁ »ö (nullÀÌ¸é ¹Ì¼±ÅÃ)
	private Color? _selectedCandyColor = null;

	// Á¤´ä ½½·Ôµé: isCorrectSlot == trueÀÎ ½½·Ôµé
	private List<CandySlotData> _correctSlots;

	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
	// ÃÊ±âÈ­
	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

	protected override void Awake()
	{
		base.Awake();

		// Á¤´ä ½½·Ô Ä³½Ì
		_correctSlots = slots.FindAll(s => s.isCorrectSlot);

		// ¸®¼Â ¹öÆ°
		if (resetButton != null)
			resetButton.onClick.AddListener(ResetPuzzle);

		// ³ª°¡±â ¹öÆ°
		if (exitButton != null)
			exitButton.onClick.AddListener(ExitPuzzle);

		// »çÅÁ ¹öÆ° ÃÊ±âÈ­
		foreach (var candy in candyButtons)
		{
			if (candy != null)
				candy.Initialize(this);
		}
	}

	protected override void OnPuzzleStarted()
	{
		base.OnPuzzleStarted();
		ResetAllVisuals();
		_selectedCandyColor = null;
		Debug.Log("[AltarPuzzle] ÆÛÁñ ½ÃÀÛ - »çÅÁÀ» ¼±ÅÃÇÏ°í ¹æ¼® À§¿¡ ¿Ã·Áº¸¼¼¿ä.");
	}

	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
	// »çÅÁ ¼±ÅÃ (CandyButton¿¡¼­ È£Ãâ)
	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

	/// <summary>
	/// »çÅÁ »ö»óÀ» ¼±ÅÃÇÕ´Ï´Ù. ÀÌ¹Ì ¼±ÅÃµÈ »öÀ» ´Ù½Ã ´©¸£¸é ¼±ÅÃ ÇØÁ¦.
	/// </summary>
	public void SelectCandy(Color color)
	{
		if (_selectedCandyColor.HasValue && _selectedCandyColor.Value == color)
		{
			_selectedCandyColor = null;
			Debug.Log("[AltarPuzzle] »çÅÁ ¼±ÅÃ ÇØÁ¦");
		}
		else
		{
			_selectedCandyColor = color;
			Debug.Log($"[AltarPuzzle] »çÅÁ ¼±ÅÃ: {color}");
		}

		// ¼±ÅÃ »óÅÂ ½Ã°¢ ÇÇµå¹é
		UpdateCandyButtonVisuals();
	}

	private void UpdateCandyButtonVisuals()
	{
		foreach (var candy in candyButtons)
		{
			if (candy == null) continue;

			bool isSelected = _selectedCandyColor.HasValue &&
							  ApproxColorEqual(_selectedCandyColor.Value, candy.CandyColor);
			candy.SetSelected(isSelected);
		}
	}

	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
	// ½½·Ô Å¬¸¯ (¹æ¼® À§ »çÁø ¹öÆ°¿¡¼­ È£Ãâ)
	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

	/// <summary>
	/// »çÁø(¹æ¼® ½½·Ô) Å¬¸¯ ½Ã È£ÃâµË´Ï´Ù.
	/// ¼±ÅÃµÈ »çÅÁÀÌ ÀÖÀ¸¸é ¹èÄ¡¸¦ ½ÃµµÇÕ´Ï´Ù.
	/// </summary>
	public void OnSlotClicked(int slotIndex)
	{
		if (!_selectedCandyColor.HasValue)
		{
			Debug.Log("[AltarPuzzle] ¸ÕÀú »çÅÁÀ» ¼±ÅÃÇÏ¼¼¿ä.");
			return;
		}

		PlaceCandy(slotIndex, _selectedCandyColor.Value);
	}

	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
	// »çÅÁ ¹èÄ¡ ·ÎÁ÷
	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

	private void PlaceCandy(int slotIndex, Color color)
	{
		var slot = slots.Find(s => s.slotIndex == slotIndex);
		if (slot == null)
		{
			Debug.LogWarning($"[AltarPuzzle] slotIndex {slotIndex}¿¡ ÇØ´çÇÏ´Â ½½·ÔÀÌ ¾ø½À´Ï´Ù.");
			return;
		}

		// ÀÌ¹Ì ¹èÄ¡µÈ ½½·ÔÀÌ¸é ±³Ã¼ (±âÁ¸ °Í Á¦°Å ÈÄ Àç¹èÄ¡)
		if (_placements.ContainsKey(slotIndex))
		{
			RemoveCandyFromSlot(slot);
		}

		// Á¤´ä ½½·ÔÀÎÁö È®ÀÎ
		bool isCorrect = slot.isCorrectSlot && ApproxColorEqual(color, slot.correctCandyColor);

		if (!isCorrect)
		{
			// Æ²¸° ¹èÄ¡ ¡æ Áï½Ã ¸®¼Â
			ShowFeedback(wrongPlacementDialogue);
			StartCoroutine(WrongPlacementFlash(slot, color));
			return;
		}

		// Á¤´ä ¹èÄ¡
		_placements[slotIndex] = color;

		// »çÁø Ç¥Á¤ º¯°æ (¿ô´Â Ç¥Á¤)
		bool isLastCorrect = (_placements.Count == _correctSlots.Count);
		if (slot.photoImage != null)
		{
			slot.photoImage.sprite = isLastCorrect && slot.bigSmileFaceSprite != null
				? slot.bigSmileFaceSprite
				: slot.smileFaceSprite;
		}

		// »çÅÁ ºñÁÖ¾ó È°¼ºÈ­
		if (slot.candyVisual != null)
		{
			slot.candyVisual.SetActive(true);
			var renderer = slot.candyVisual.GetComponent<Renderer>();
			if (renderer != null)
				renderer.material.color = color;
			var image = slot.candyVisual.GetComponent<Image>();
			if (image != null)
				image.color = color;
		}

		Debug.Log($"[AltarPuzzle] ½½·Ô {slotIndex} Á¤´ä ¹èÄ¡! ({_placements.Count}/{_correctSlots.Count})");

		// Á¤´ä °³¼ö ÃæÁ· ½Ã ÇØ°á È®ÀÎ
		CheckSolution();
	}

	private IEnumerator WrongPlacementFlash(CandySlotData slot, Color color)
	{
		// Àá±ñ Æ²¸° »ö Ç¥½Ã ÈÄ ¿ø·¡ Ç¥Á¤À¸·Î º¹±Í
		if (slot.photoImage != null && slot.neutralFaceSprite != null)
		{
			slot.photoImage.sprite = slot.neutralFaceSprite;
		}
		yield return new WaitForSecondsRealtime(0.5f);
	}

	private void RemoveCandyFromSlot(CandySlotData slot)
	{
		_placements.Remove(slot.slotIndex);

		if (slot.candyVisual != null)
			slot.candyVisual.SetActive(false);

		if (slot.photoImage != null && slot.neutralFaceSprite != null)
			slot.photoImage.sprite = slot.neutralFaceSprite;
	}

	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
	// ¸®¼Â
	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

	public void ResetPuzzle()
	{
		_placements.Clear();
		_selectedCandyColor = null;
		ResetAllVisuals();
		UpdateCandyButtonVisuals();
		Debug.Log("[AltarPuzzle] ÆÛÁñ ¸®¼Â (Çâ·Î)");
	}

	private void ResetAllVisuals()
	{
		foreach (var slot in slots)
		{
			if (slot.candyVisual != null)
				slot.candyVisual.SetActive(false);

			if (slot.photoImage != null && slot.neutralFaceSprite != null)
				slot.photoImage.sprite = slot.neutralFaceSprite;
		}
	}

	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
	// Á¤´ä ÆÇÁ¤
	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

	protected override bool IsSolutionCorrect()
	{
		// Á¤´ä ½½·Ô ¼ö¸¸Å­ ¹èÄ¡µÇ¾ú´ÂÁö È®ÀÎ
		if (_placements.Count != _correctSlots.Count) return false;

		foreach (var correctSlot in _correctSlots)
		{
			if (!_placements.ContainsKey(correctSlot.slotIndex)) return false;
			if (!ApproxColorEqual(_placements[correctSlot.slotIndex], correctSlot.correctCandyColor)) return false;
		}
		return true;
	}

	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
	// ÆÛÁñ ÇØ°á
	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

	protected override void SolvePuzzle()
	{
		// ¸¶Áö¸· »çÁøÀ» È°Â¦ ¿ô´Â Ç¥Á¤À¸·Î º¯°æ
		foreach (var correctSlot in _correctSlots)
		{
			if (correctSlot.photoImage != null && correctSlot.bigSmileFaceSprite != null)
				correctSlot.photoImage.sprite = correctSlot.bigSmileFaceSprite;
		}

		// È¿°úÀ½
		var audioManager = FindAnyObjectByType<AudioManager>();
		audioManager?.PlaySFX("puzzle_solve");

		// ¹Ú¼ö ¼Ò¸® (Á¡ÇÁ½ºÄÉ¾î º¹¼±)
		audioManager?.PlaySFX("applause");

		// ´ë»ç Ãâ·Â
		ShowFeedback(solveDialogue);

		Debug.Log("[AltarPuzzle] ÆÛÁñ ÇØ°á! Å©¸®Ã³°¡ ÀÌµ¿ÇÕ´Ï´Ù.");

		// base.SolvePuzzle() ¡æ Ä«¸Þ¶ó º¹±Í + »óÅÂ Playing º¹±Í
		base.SolvePuzzle();

		// Å©¸®Ã³ ÀÌµ¿ (ÀÛÀº ¹æ ¾Õ ¡æ ¸Ê µÚÂÊ Áß¾Ó)
		if (shadowCreature != null)
			shadowCreature.MoveToFinalPosition();
	}

	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
	// ³ª°¡±â (ÆÛÁñ µµÁß ³ª°¡¸é ÀüºÎ ¸®¼Â)
	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

	public override void ExitPuzzle()
	{
		// ±âÈ¹¼­: "ÆÛÁñ µµÁß ³ª°¡¸é ÀüºÎ ¸®¼ÂµÊ"
		ResetPuzzle();
		ShowFeedback(exitPuzzleDialogue);
		base.ExitPuzzle();
	}

	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡
	// À¯Æ¿
	// ¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡¦¡

	private void ShowFeedback(string message)
	{
		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue(speaker, message);
	}

	/// <summary>
	/// Color ºñ±³ ½Ã float ¿ÀÂ÷ Çã¿ë (0.01f)
	/// </summary>
	private bool ApproxColorEqual(Color a, Color b)
	{
		return Mathf.Abs(a.r - b.r) < 0.01f &&
			   Mathf.Abs(a.g - b.g) < 0.01f &&
			   Mathf.Abs(a.b - b.b) < 0.01f;
	}
}