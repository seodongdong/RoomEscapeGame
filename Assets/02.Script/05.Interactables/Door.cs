using UnityEngine;

public class Door : MonoBehaviour, IInteractable
{
    [SerializeField] private bool isLocked = true;
    [SerializeField] private string requiredKeyId;
    [SerializeField] private bool requiresGirl;
    
    [Header("Dialogue")]
    [SerializeField] private string lockedDialogue = "잠겨있다...";
    [SerializeField] private string openDialogue = "문이 열렸다!";
    
    public string InteractionPrompt
    {
        get
        {
            if (isLocked)
            {
                return requiresGirl 
                    ? "[F] 문 열기 (소녀가 필요합니다)" 
                    : "[F] 문 열기 (열쇠가 필요합니다)";
            }
            return "[F] 문 열기";
        }
    }

    public bool CanInteract(IPlayer player)
    {
        if (!isLocked) return true;
        
        bool hasKey = string.IsNullOrEmpty(requiredKeyId) || 
                      player.Inventory.HasItem(requiredKeyId);
        
        bool hasGirl = !requiresGirl;
        
        return hasKey && hasGirl;
    }

    public void Interact(IPlayer player)
    {
        var uiManager = FindAnyObjectByType<UIManager>();
        
        if (CanInteract(player))
        {
            isLocked = false;
            Debug.Log("문이 열렸습니다!");
            
            if (!string.IsNullOrEmpty(openDialogue))
            {
                uiManager?.ShowDialogue("소년", openDialogue);
            }
        }
        else
        {
            Debug.Log("문이 잠겨있습니다.");
            
            if (!string.IsNullOrEmpty(lockedDialogue))
            {
                uiManager?.ShowDialogue("소년", lockedDialogue);
            }
        }
    }
}