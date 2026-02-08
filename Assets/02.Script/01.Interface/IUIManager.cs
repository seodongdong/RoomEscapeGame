using UnityEngine;
/// <summary>
/// UI 관리 인터페이스
/// 모든 UI 표시 관리 (프롬프트, 인벤토리, 체력, 타이머, 대사)
/// </summary>
public interface IUIManager
{
	// 상호작용 프롬프트
	void ShowInteractionPrompt(string text);
	void HideInteractionPrompt();

	// 인벤토리
	void ShowInventoryUI();
	void HideInventoryUI();

	// 체력
	void UpdateHealthUI(int current, int max);

	// 타이머 (5스테이지)
	void StartTimer(float duration);
	void StopTimer();

	// 대사 시스템
	void ShowDialogue(string speaker, string dialogue);
	void HideDialogue();
}