using UnityEngine;

public class InteractableClue : MonoBehaviour, IInteractable
{
    [Header("Clue Info")]
    [SerializeField] private string clueId;
    [SerializeField] private string clueName;
    [TextArea(3, 10)]
    [SerializeField] private string description;
    [SerializeField] private Sprite icon;

    [Header("Dialogue")]
    [SerializeField] private string playerDialogue = "이게 뭐지?";

    public string InteractionPrompt => $"[F] {clueName} 조사하기";

    public bool CanInteract(IPlayer player)
    {
        return !player.Inventory.HasItem(clueId);
    }

    public void Interact(IPlayer player)
    {
        ClueItem clue = new ClueItem(clueId, clueName, description);
        player.Inventory.AddItem(clue);

        GameManager.Instance.ClueTracker.RegisterClue(clueId);

        // 대사 표시
        var uiManager = FindAnyObjectByType<UIManager>();
        if (!string.IsNullOrEmpty(playerDialogue))
        {
            uiManager?.ShowDialogue("소년", playerDialogue);
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
