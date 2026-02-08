using UnityEngine;

/// <summary>
/// 대사 시스템 인터페이스
/// 대사 표시/숨김 및 활성 상태 확인
/// </summary>
public interface IDialogueSystem
{
	bool IsDialogueActive { get; }

	void ShowDialogue(string speaker, string text, float duration);
	void HideDialogue();
}