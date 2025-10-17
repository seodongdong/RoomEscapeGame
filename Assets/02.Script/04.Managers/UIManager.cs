using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

// UI 매니저 클래스

public class UIManager : MonoBehaviour, IUIManager
{
    [Header("UI References")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI interactionText;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private Image healthBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private TextMeshProUGUI timerText;

    private Coroutine _timerCoroutine;

    private void Start()
    {
        HideInteractionPrompt();            // 시작 시 상호작용 프롬프트 숨기기
        HideInventoryUI();                    // 시작 시 인벤토리 숨기기
        timerPanel?.SetActive(false);       // 타이머 패널 숨기기
    }

    public void ShowInteractionPrompt(string text)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);  // 프롬프트 활성화
            if (interactionText != null)        // 텍스트 설정
            {
                interactionText.text = text;    // 상호작용 텍스트 설정
            }
        }
    }

    public void HideInteractionPrompt()
    {
        interactionPrompt?.SetActive(false);
    }

    public void ShowInventoryUI()
    {
        inventoryPanel?.SetActive(true);
        Time.timeScale = 0;     // 게임 일시정지
    }

    public void HideInventoryUI()
    {
        inventoryPanel?.SetActive(false);
        Time.timeScale = 1;     // 게임 재개
    }

    public void UpdateHealthUI(int current, int max)
    {
        if (healthBar != null)              // 체력 바가 할당되어 있는지 확인
        {
            healthBar.fillAmount = (float)current / max;
        }

        if (healthText != null)             // 체력 텍스트가 할당되어 있는지 확인
        {
            healthText.text = $"{current} / {max}";
        }
    }

    public void StartTimer(float duration)
    {
        if (timerPanel != null)
        {
            timerPanel.SetActive(true);
        }

        if (_timerCoroutine != null)
        {
            StopCoroutine(_timerCoroutine);
        }

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

    // 타이머 코루틴 
    // duration 초 동안 타이머를 작동시키며, 30초 이하일 때 텍스트 색상을 빨간색으로 변경
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
                {
                    timerText.color = Color.red;
                }
            }
            yield return null;
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            // 인벤토리 토글
            if (inventoryPanel != null && inventoryPanel.activeSelf)
            {
                HideInventoryUI();
            }
            else
            {
                ShowInventoryUI();
            }
        }
    }
}
