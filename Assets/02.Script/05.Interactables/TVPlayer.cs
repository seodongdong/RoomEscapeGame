using UnityEngine;
using TMPro;
using System.Collections;

public class TVPlayer : MonoBehaviour, IInteractable
{
    [SerializeField] private string requiredTapeId;
    [SerializeField] private GameObject videoScreen;
    [SerializeField] private TextMeshProUGUI narrationText;

    public string InteractionPrompt => "[F] TV에서 비디오 재생";

    public bool CanInteract(IPlayer player)
    {
        return player.Inventory.HasItem(requiredTapeId);
    }

    public void Interact(IPlayer player)
    {
        var tape = player.Inventory.GetItem(requiredTapeId);
        if (tape != null)
        {
            StartCoroutine(PlayVideoCoroutine(tape.Description));
        }
    }

    private IEnumerator PlayVideoCoroutine(string narration)
    {
        videoScreen?.SetActive(true);
        
        if (narrationText != null)
        {
            narrationText.text = "";
            foreach (char c in narration)
            {
                narrationText.text += c;
                yield return new WaitForSeconds(0.05f);
            }
        }

        yield return new WaitForSeconds(3f);
        videoScreen?.SetActive(false);
    }
}