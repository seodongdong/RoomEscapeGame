using UnityEngine;

public class PuzzleTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private MonoBehaviour puzzleComponent; // ← 변경!
    [SerializeField] private string promptText = "퍼즐";
    
    private IPuzzle _puzzle;
    
    private void Awake()
    {
        _puzzle = puzzleComponent as IPuzzle;
        
        if (_puzzle == null)
        {
            Debug.LogError($"{gameObject.name}: Puzzle component가 IPuzzle을 구현하지 않습니다!");
        }
    }
    
    public string InteractionPrompt => $"[F] {promptText} 조사하기";

    public bool CanInteract(IPlayer player)
    {
        return _puzzle != null && !_puzzle.IsSolved;
    }

    public void Interact(IPlayer player)
    {
        _puzzle?.StartPuzzle();
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
