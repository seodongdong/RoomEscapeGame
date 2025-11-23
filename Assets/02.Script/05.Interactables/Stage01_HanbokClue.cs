using UnityEngine;

public class Stage1_HanbokClue : MonoBehaviour, IInteractable
{
    [Header("Clue Info")]
    [SerializeField] private string clueId = "";
    [SerializeField] private string clueName = "";
    [SerializeField] private string girlName = ""; // 죽은 딸의 이름
    
    [Header("Interaction")]
    [SerializeField] private bool showNameInPrompt = true; // ⭐ 중요 단서는 true!
    
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
                return "[F] 선물 박스 열기";
            }
            else
            {
                return "[F] 조사하기";
            }
        }
    }

    public bool CanInteract(IPlayer player)
    {
        return !player.Inventory.HasItem(clueId);
    }

    public void Interact(IPlayer player)
    {
        string description = $"깨끗한 한복이다. 이름 자수에 '{girlName}'이라고 적혀있다.";
        ClueItem clue = new ClueItem(clueId, clueName, description);
        
        player.Inventory.AddItem(clue);
        GameManager.Instance.ClueTracker.RegisterClue(clueId);
        
        var uiManager = FindAnyObjectByType<UIManager>();
        if (!string.IsNullOrEmpty(dialogue))
        {
            // {girlName} 치환
            string finalDialogue = dialogue.Replace("○○○", girlName);
            uiManager?.ShowDialogue(speaker, finalDialogue);
        }
        
        gameObject.SetActive(false);
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
