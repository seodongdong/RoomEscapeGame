using UnityEngine;

// 대사 시스템 인터페이스

public interface IDialogueSystem
{
    void ShowDialogue(string speaker, string text, float duration);
    void HideDialogue();
    bool IsDialogueActive { get; }      // 대사 활성화 여부 확인
}
