using UnityEngine;

public class GirlRescueTrigger : MonoBehaviour, IInteractable
{
    [Header("References")]
    [SerializeField] private Transform girlTransform;
    [SerializeField] private GameObject boxVisual;
    
    [Header("Dialogue")]
    [SerializeField] private string speaker = "";
    [TextArea(2, 5)]
    [SerializeField] private string dialogue = "";
    
    private bool _isOpened;

    public string InteractionPrompt => "[F] 제설함 열기";

    public bool CanInteract(IPlayer player)
    {
        return !_isOpened;
    }

    public void Interact(IPlayer player)
    {
        _isOpened = true;
        
        boxVisual?.SetActive(false);
        girlTransform.gameObject.SetActive(true);
        
        Debug.Log("제설함 상자에서 소녀를 발견했습니다!");
        
        var uiManager = FindAnyObjectByType<UIManager>();
        if (!string.IsNullOrEmpty(dialogue))
        {
            uiManager?.ShowDialogue(speaker, dialogue);
        }
        
        var chaseSequence = FindAnyObjectByType<ChaseSequence>();
        chaseSequence?.StartChase();
    }
}