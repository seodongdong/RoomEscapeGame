using UnityEngine;

// UI 매니저 인터페이스

public interface IUIManager
{
    void ShowInteractionPrompt(string text);
    void HideInteractionPrompt();
    void ShowInventoryUI();
    void HideInventoryUI();
    void UpdateHealthUI(int current, int max);
    void StartTimer(float duration);
    void StopTimer();
}
