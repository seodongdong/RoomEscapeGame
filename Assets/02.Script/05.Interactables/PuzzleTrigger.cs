using UnityEngine;

public class PuzzleTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private PuzzleBase puzzle;
    
    public string InteractionPrompt => "[F] ?? ????";

    public bool CanInteract(IPlayer player)
    {
        return !puzzle.IsSolved;
    }

    public void Interact(IPlayer player)
    {
        puzzle.StartPuzzle();
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