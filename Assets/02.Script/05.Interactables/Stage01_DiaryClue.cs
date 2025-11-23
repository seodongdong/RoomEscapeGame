using UnityEngine;

public class Stage1_DiaryClue : MonoBehaviour, IInteractable
{
    [Header("Clue Info")]
    [SerializeField] private string clueId = "";
    [SerializeField] private string clueName = "";
    [TextArea(3, 10)]
    [SerializeField] private string description = "";
    
    [Header("Interaction")]
    [SerializeField] private bool showNameInPrompt = true;
    [SerializeField] private bool isCollectable = true; // ? 중요 단서는 true!
    
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
        if (isCollectable)
        {
            return !player.Inventory.HasItem(clueId);
        }
        return true;
    }

    public void Interact(IPlayer player)
    {
        if (isCollectable)
        {
            ClueItem clue = new ClueItem(clueId, clueName, description);
            player.Inventory.AddItem(clue);
            GameManager.Instance.ClueTracker.RegisterClue(clueId);
            
            gameObject.SetActive(false);
        }
        
        var uiManager = FindAnyObjectByType<UIManager>();
        if (!string.IsNullOrEmpty(dialogue))
        {
            uiManager?.ShowDialogue(speaker, dialogue);
        }
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
