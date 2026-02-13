using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour, IUIManager
{
	[Header("UI References")]
	[SerializeField] private GameObject interactionPrompt;
	[SerializeField] private TextMeshProUGUI interactionText;
	[SerializeField] private GameObject inventoryPanel;
	// ❌ healthBar, healthText 제거
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

	private void Start()
	{
		HideInteractionPrompt();
		if (inventoryPanel != null) HideInventoryUI();
		HideDialogue();
		timerPanel?.SetActive(false);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space) &&
			dialoguePanel != null &&
			dialoguePanel.activeSelf)
		{
			HideDialogue();
		}
	}

	#region Interaction Prompt

	public void ShowInteractionPrompt(string text)
	{
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

	#endregion

	#region Inventory

	public void ShowInventoryUI()
	{
		if (inventoryPanel == null) return;
		inventoryPanel.SetActive(true);
		Time.timeScale = 0;
	}

	public void HideInventoryUI()
	{
		if (inventoryPanel == null) return;
		inventoryPanel.SetActive(false);
		Time.timeScale = 1;
	}

	#endregion

	// ❌ #region Health 전체 제거

	#region Timer

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

	#endregion

	#region Dialogue

	public void ShowDialogue(string speaker, string dialogue)
	{
		if (dialoguePanel != null)
		{
			dialoguePanel.SetActive(true);

			if (speakerText != null)
				speakerText.text = speaker;

			if (dialogueText != null)
			{
				if (_dialogueCoroutine != null)
					StopCoroutine(_dialogueCoroutine);
				_dialogueCoroutine = StartCoroutine(TypeDialogue(dialogue));
			}
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

	#endregion
}