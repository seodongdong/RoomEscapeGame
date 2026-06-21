using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// UI 통합 매니저
/// 담당: 상호작용 프롬프트 / 대사 / 타이머
///
/// [이번 수정]
/// - I키 토글 제거 → Player.cs + InventoryUI_Complete에서 처리
/// - ShowInventoryUI / HideInventoryUI 레거시 유지
///   (IUIManager 인터페이스 구현 의무 때문)
/// </summary>
public class UIManager : MonoBehaviour, IUIManager
{
	[Header("UI References")]
	[SerializeField] private GameObject interactionPrompt;
	[SerializeField] private TextMeshProUGUI interactionText;
	[SerializeField] private GameObject inventoryPanel;    // 레거시 슬롯 (미사용 가능)
	[SerializeField] private GameObject timerPanel;
	[SerializeField] private TextMeshProUGUI timerText;

	[Header("Dialogue UI")]
	[SerializeField] private GameObject dialoguePanel;
	[SerializeField] private TextMeshProUGUI speakerText;
	[SerializeField] private TextMeshProUGUI dialogueText;

	[Header("Settings")]
	[SerializeField] private float typingSpeed = 0.03f;
	[SerializeField] private float autoHideDelay = 2f;

	private Coroutine _timerCoroutine;
	private Coroutine _dialogueCoroutine;
	private bool _isAnyUIOpen = false;

	private void Start()
	{
		HideInteractionPrompt();
		if (inventoryPanel != null) inventoryPanel.SetActive(false);
		HideDialogue();
		timerPanel?.SetActive(false);
	}

	private void Update()
	{
		// Space → 대사 스킵
		if (Input.GetKeyDown(KeyCode.Space) &&
			dialoguePanel != null &&
			dialoguePanel.activeSelf)
		{
			HideDialogue();
		}
		// ★ I키 제거 — Player.cs + InventoryUI_Complete에서 처리
	}

	// ─── Interaction Prompt ───────────────────────────────────

	public void ShowInteractionPrompt(string text)
	{
		if (_isAnyUIOpen) return;
		if (interactionPrompt == null) return;

		interactionPrompt.SetActive(true);
		if (interactionText != null)
			interactionText.text = text;
	}

	public void HideInteractionPrompt()
	{
		interactionPrompt?.SetActive(false);
	}

	// ─── UI 열림/닫힘 알림 ────────────────────────────────────

	public void NotifyUIOpened()
	{
		_isAnyUIOpen = true;
		HideInteractionPrompt();
	}

	public void NotifyUIClosed()
	{
		_isAnyUIOpen = false;
	}

	// ─── Inventory (IUIManager 구현 — 레거시) ─────────────────

	public void ShowInventoryUI()
	{
		inventoryPanel?.SetActive(true);
		NotifyUIOpened();
	}

	public void HideInventoryUI()
	{
		inventoryPanel?.SetActive(false);
		NotifyUIClosed();
	}

	// ─── Timer ────────────────────────────────────────────────

	public void StartTimer(float duration)
	{
		timerPanel?.SetActive(true);

		if (_timerCoroutine != null)
			StopCoroutine(_timerCoroutine);

		_timerCoroutine = StartCoroutine(TimerCoroutine(duration));
	}

	public void StopTimer()
	{
		if (_timerCoroutine != null)
		{
			StopCoroutine(_timerCoroutine);
			_timerCoroutine = null;
		}
		timerPanel?.SetActive(false);
	}

	private IEnumerator TimerCoroutine(float duration)
	{
		float remaining = duration;

		while (remaining > 0)
		{
			remaining -= Time.deltaTime;

			if (timerText != null)
			{
				int minutes = Mathf.FloorToInt(remaining / 60);
				int seconds = Mathf.FloorToInt(remaining % 60);
				timerText.text = $"{minutes:00}:{seconds:00}";

				timerText.color = remaining <= 30f ? Color.red : Color.white;
			}

			yield return null;
		}
	}

	// ─── Dialogue ─────────────────────────────────────────────

	public void ShowDialogue(string speaker, string dialogue)
	{
		if (dialoguePanel == null) return;

		dialoguePanel.SetActive(true);
		HideInteractionPrompt();

		if (speakerText != null)
			speakerText.text = speaker;

		if (dialogueText != null)
		{
			if (_dialogueCoroutine != null)
				StopCoroutine(_dialogueCoroutine);
			_dialogueCoroutine = StartCoroutine(TypeDialogue(dialogue));
		}
	}

	public void HideDialogue()
	{
		if (_dialogueCoroutine != null)
		{
			StopCoroutine(_dialogueCoroutine);
			_dialogueCoroutine = null;
		}
		dialoguePanel?.SetActive(false);
	}

	private IEnumerator TypeDialogue(string dialogue)
	{
		dialogueText.text = "";

		foreach (char c in dialogue)
		{
			dialogueText.text += c;
			yield return new WaitForSeconds(typingSpeed);
		}

		yield return new WaitForSeconds(autoHideDelay);
		HideDialogue();
	}
}