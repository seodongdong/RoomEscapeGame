using System.Collections;
using UnityEngine;

// 대사 시스템 매니저 클래스

public class DialogueSystem : MonoBehaviour, IDialogueSystem
{
    // UI 매니저 참조
    private IUIManager _uiManager;
    private bool _isDialogueActive;

    // 대사 활성화 여부
    public bool IsDialogueActive => _isDialogueActive;

    private void Start()
    {
        // UI 매니저 찾기
        _uiManager = FindAnyObjectByType<UIManager>();
    }

    public void ShowDialogue(string speaker, string text, float duration)
    {
        // 대사 활성화
        _isDialogueActive = true;
        _uiManager?.ShowDialogue(speaker, text);

        StartCoroutine(HideAfterDelay(duration));
    }

    public void HideDialogue()
    {
        // 대사 비활성화
        _isDialogueActive = false;
        _uiManager?.HideDialogue();
    }

    // 대사 자동 숨기기 코루틴
    private IEnumerator HideAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        HideDialogue();
    }
}
