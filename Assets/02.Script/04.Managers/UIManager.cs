using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// UI 통합 매니저
///
/// [수정사항]
/// - 다이얼로그/인벤토리 등 UI가 활성화될 때 InteractionPrompt 자동 숨김
/// - UI가 비활성화될 때 InteractionPrompt 복원 (Player Raycast가 자연스럽게 처리)
/// - _isUIOpen 플래그로 프롬프트 표시 차단
/// </summary>
public class UIManager : MonoBehaviour, IUIManager
{
	[Header("UI References")]
	[SerializeField] private GameObject interactionPrompt;
	[SerializeField] private TextMeshProUGUI interactionText;
	[SerializeField] private GameObject inventoryPanel;
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

	/// <summary>
	/// 다른 UI(인벤토리, 다이어리, 뷰어 등)가 열려있는지 여부.
	/// true일 때는 ShowInteractionPrompt 무시.
	/// </summary>
	private bool _isAnyUIOpen = false;

	private void Start()
	{
		HideInteractionPrompt();
		if (inventoryPanel != null) HideInventoryUI();
		HideDialogue();
		timerPanel?.SetActive(false);
	}

	private void Update()
	{
		// 스페이스바로 대사 스킵
		if (Input.GetKeyDown(KeyCode.Space) &&
			dialoguePanel != null &&
			dialoguePanel.activeSelf)
		{
			HideDialogue();
		}
	}

	// ─────────────────────────────────────────────
	// Interaction Prompt
	// ─────────────────────────────────────────────

	public void ShowInteractionPrompt(string text)
	{
		// 다른 UI가 열려있으면 프롬프트 표시 차단
		if (_isAnyUIOpen) return;

		if (interactionPrompt != null)
		{
			interactionPrompt.SetActive(true);
			if (interactionText != null)
				interactionText.text = text;
		}
	}

	public void HideInteractionPrompt()
	{
		interactionPrompt?.SetActive(false);
	}

	// ─────────────────────────────────────────────
	// UI Open/Close 상태 알림 (인벤토리, 다이어리, 뷰어 등에서 호출)
	// ─────────────────────────────────────────────

	/// <summary>
	/// 다른 UI가 열릴 때 호출 → InteractionPrompt 숨기고 표시 차단
	/// </summary>
	public void NotifyUIOpened()
	{
		_isAnyUIOpen = true;
		HideInteractionPrompt();
	}

	/// <summary>
	/// 다른 UI가 닫힐 때 호출 → InteractionPrompt 차단 해제
	/// (실제 표시는 Player Raycast가 다음 프레임에 처리)
	/// </summary>
	public void NotifyUIClosed()
	{
		_isAnyUIOpen = false;
	}

	// ─────────────────────────────────────────────
	// Inventory (UIManager 자체 패널 - 레거시 지원)
	// ─────────────────────────────────────────────

	public void ShowInventoryUI()
	{
		if (inventoryPanel == null) return;
		inventoryPanel.SetActive(true);
		Time.timeScale = 0;
		NotifyUIOpened();
	}

	public void HideInventoryUI()
	{
		if (inventoryPanel == null) return;
		inventoryPanel.SetActive(false);
		Time.timeScale = 1;
		NotifyUIClosed();
	}

	// ─────────────────────────────────────────────
	// Timer
	// ─────────────────────────────────────────────

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

				if (remaining <= 30f)
					timerText.color = Color.red;
			}

			yield return null;
		}
	}

	// ─────────────────────────────────────────────
	// Dialogue
	// ─────────────────────────────────────────────

	public void ShowDialogue(string speaker, string dialogue)
	{
		if (dialoguePanel == null) return;

		dialoguePanel.SetActive(true);

		// 대화창이 열리면 프롬프트 숨기기
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

		// 대화창 닫히면 프롬프트 차단 해제
		// (다른 UI가 열려있지 않은 경우에만)
		if (!_isAnyUIOpen)
		{
			// Player Raycast가 다음 프레임에 자연스럽게 ShowInteractionPrompt 호출
		}
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