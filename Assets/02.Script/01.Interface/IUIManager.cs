using UnityEngine;

public interface IUIManager
{
	// 상호작용 프롬프트
	void ShowInteractionPrompt(string text);
	void HideInteractionPrompt();

	// 인벤토리
	void ShowInventoryUI();
	void HideInventoryUI();

	// ❌ UpdateHealthUI 제거

	// 타이머 (5스테이지)
	void StartTimer(float duration);
	void StopTimer();

	// 대사 시스템
	void ShowDialogue(string speaker, string dialogue);
	void HideDialogue();
}