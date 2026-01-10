// ==================== UIManager.cs ⭐ 수정됨 (인벤토리 관련 제거) ====================
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class UIManager : MonoBehaviour, IUIManager
{
    [Header("UI References")]
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private TextMeshProUGUI interactionText;
    // ⭐ inventoryPanel 제거 - InventoryUIManager가 담당
    [SerializeField] private Image healthBar;
    [SerializeField] private TextMeshProUGUI healthText;
    [SerializeField] private GameObject timerPanel;
    [SerializeField] private TextMeshProUGUI timerText;
    
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TextMeshProUGUI speakerText;
    [SerializeField] private TextMeshProUGUI dialogueText;

    private Coroutine _timerCoroutine;
    private Coroutine _dialogueCoroutine;

    private void Start()
    {
        HideInteractionPrompt();
        // ⭐ HideInventoryUI() 제거
        HideDialogue();
        timerPanel?.SetActive(false);
    }

    public void ShowInteractionPrompt(string text)
    {
        if (interactionPrompt != null)
        {
            interactionPrompt.SetActive(true);
            if (interactionText != null)
            {
                interactionText.text = text;
            }
        }
    }

    public void HideInteractionPrompt()
    {
        interactionPrompt?.SetActive(false);
    }

    // ⭐ ShowInventoryUI() 제거 - InventoryUIManager가 담당
    // ⭐ HideInventoryUI() 제거 - InventoryUIManager가 담당

    public void UpdateHealthUI(int current, int max)
    {
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)current / max;
        }

        if (healthText != null)
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

    // ==================== 대사 표시 시스템 ====================
    public void ShowDialogue(string speaker, string dialogue)
    {
        if (dialoguePanel != null)
        {
            dialoguePanel.SetActive(true);
            
            if (speakerText != null)
            {
                speakerText.text = speaker;
            }
            
            if (dialogueText != null)
            {
                if (_dialogueCoroutine != null)
                {
                    StopCoroutine(_dialogueCoroutine);
                }
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
        
        // 타이핑 효과
        foreach (char c in dialogue)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(0.03f);
        }
        
        // 대기 시간: 2초
        yield return new WaitForSeconds(2f);
        HideDialogue();
    }

    private void Update()
    {
        // ⭐ I키 인벤토리 관련 코드 제거 - InventoryUIManager가 담당
        
        // 대사 중 스페이스바로 스킵
        if (Input.GetKeyDown(KeyCode.Space) && dialoguePanel != null && dialoguePanel.activeSelf)
        {
            HideDialogue();
        }
    }
    
    // ⭐ 인벤토리 관련 메서드는 IUIManager 인터페이스에서도 제거해야 함
    // 아래는 호환성을 위해 빈 메서드로 남겨둠 (나중에 인터페이스에서 제거)
    public void ShowInventoryUI()
    {
        // InventoryUIManager가 담당
    }
    
    public void HideInventoryUI()
    {
        // InventoryUIManager가 담당
    }
}