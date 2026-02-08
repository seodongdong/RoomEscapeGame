using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 범용 TV 플레이어
/// 비디오테이프 재생
/// </summary>
public class TVPlayer : MonoBehaviour, IInteractable
{
	[Header("Required Tape")]
	[SerializeField] private string requiredTapeId;

	[Header("UI")]
	[SerializeField] private GameObject videoScreen;
	[SerializeField] private TextMeshProUGUI narrationText;
	[SerializeField] private Image thumbnailImage;

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string noTapeDialogue = "비디오테이프가 필요하다.";

	public string InteractionPrompt => "[F] TV에서 비디오 재생";

	public bool CanInteract(IPlayer player)
	{
		return true;
	}

	public void Interact(IPlayer player)
	{
		if (!player.Inventory.HasItem(requiredTapeId))
		{
			var uiManager = FindAnyObjectByType<UIManager>();
			uiManager?.ShowDialogue(speaker, noTapeDialogue);
			return;
		}

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