using UnityEngine;

public class InteractableClue : MonoBehaviour, IInteractable
{
    [Header("Clue Info")]
    [SerializeField] private string clueId;
    [SerializeField] private string clueName;
    [TextArea(3, 10)]
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;
    
    [Header("Interaction")]
    [SerializeField] private bool showNameInPrompt = false;
    [SerializeField] private bool isCollectable = false; // ⭐ 추가!
    
    [Header("Dialogue")]
    [SerializeField] private string speaker = "";
    [TextArea(2, 5)]
    [SerializeField] private string dialogue = "";

    public string InteractionPrompt
    {
        get
        {
            if (showNameInPrompt)
            {
                return $"[F] {clueName} 조사하기";
            }
            else
            {
                return "[F] 조사하기";
            }
        }
    }

    public bool CanInteract(IPlayer player)
    {
        // ⭐ 수집 가능한 단서만 중복 체크
        if (isCollectable)
        {
            return !player.Inventory.HasItem(clueId);
        }
        return true; // 일반 단서는 항상 조사 가능
    }

    public void Interact(IPlayer player)
    {
        // ⭐ 수집 가능한 단서만 인벤토리 추가
        if (isCollectable)
        {
            ClueItem clue = new ClueItem(clueId, clueName, description);
            player.Inventory.AddItem(clue);
            
            GameManager.Instance.ClueTracker.RegisterClue(clueId);
            
            // 획득 후 비활성화
            gameObject.SetActive(false);
        }
        
        // 대사 표시 (모든 단서)
        var uiManager = FindAnyObjectByType<UIManager>();
        if (!string.IsNullOrEmpty(dialogue))
        {
            uiManager?.ShowDialogue(speaker, dialogue);
        }
        
        // ⭐ 일반 단서는 비활성화 안함 (다시 조사 가능)
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent<Player>(out var player))
        {
            player.SetCurrentInteractable(this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.TryGetComponent<Player>(out var player))
        {
            player.SetCurrentInteractable(null);
        }
    }
}