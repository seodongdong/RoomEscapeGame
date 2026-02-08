using UnityEngine;
using System.Collections;

/// <summary>
/// 대사 시스템
/// </summary>
public class DialogueSystem : MonoBehaviour, IDialogueSystem
{
	private IUIManager _uiManager;
	private bool _isDialogueActive;

	public bool IsDialogueActive => _isDialogueActive;

	private void Start()
	{
		_uiManager = FindAnyObjectByType<UIManager>();
	}

	public void ShowDialogue(string speaker, string text, float duration)
	{
		_isDialogueActive = true;
		_uiManager?.ShowDialogue(speaker, text);

		StartCoroutine(HideAfterDelay(duration));
	}

	public void HideDialogue()
	{
		_isDialogueActive = false;
		_uiManager?.HideDialogue();
	}

	private IEnumerator HideAfterDelay(float delay)
	{
		yield return new WaitForSeconds(delay);
		HideDialogue();
	}
}