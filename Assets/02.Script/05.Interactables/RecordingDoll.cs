using UnityEngine;

// 녹음 인형 상호작용 클래스
public class RecordingDoll : MonoBehaviour, IInteractable
{
	[Header("Doll Settings")]
	[SerializeField] private string clueId = "recording_doll";
	[SerializeField] private string deadGirlName = "○○○";
	[SerializeField] private AudioClip[] recordings;

	[Header("Dialogues")]
	[SerializeField] private string dollSpeaker = "인형";
	[SerializeField] private string playerSpeaker = "소년";

	[TextArea(2, 5)]
	[SerializeField] private string dialogue1 = "알라뷰!";

	[TextArea(2, 5)]
	[SerializeField] private string dialogue2 = "○○○이가 누구예요?";

	[TextArea(2, 5)]
	[SerializeField] private string dialogue2_player = "...? 이상한 소리가 들린다.";

	[TextArea(2, 5)]
	[SerializeField] private string dialogue3 = "저 ○○○이 아니예요...";

	[TextArea(3, 10)]
	[SerializeField] private string clueDescription = "이상한 소리가 녹음되어 있는 인형. 누군가가 자신이 아니라고 부정하는 목소리가 들린다.";

	private int _playCount = 0;

	public string InteractionPrompt => "[F] 인형 조사하기";

	public bool CanInteract(IPlayer player)
	{
		return true;
	}

	public void Interact(IPlayer player)
	{
		var audioManager = FindAnyObjectByType<AudioManager>();
		var uiManager = FindAnyObjectByType<UIManager>();

		_playCount++;

		switch (_playCount)
		{
			case 1:
				audioManager?.PlaySFX("doll_voice_1");
				uiManager?.ShowDialogue(dollSpeaker, dialogue1);
				break;

			case 2:
				audioManager?.PlaySFX("doll_voice_2");
				string d2 = dialogue2.Replace("○○○", deadGirlName);
				uiManager?.ShowDialogue(dollSpeaker, d2);

				// 잠시 후 플레이어 대사
				StartCoroutine(ShowDelayedDialogue(uiManager, playerSpeaker, dialogue2_player, 2f));
				break;

			case 3:
				audioManager?.PlaySFX("doll_voice_3");
				string d3 = dialogue3.Replace("○○○", deadGirlName);
				uiManager?.ShowDialogue(dollSpeaker, d3);

				if (!player.Inventory.HasItem(clueId))
				{
					ClueItem clue = new ClueItem(clueId, "녹음 인형", clueDescription);
					player.Inventory.AddItem(clue);
					GameManager.Instance.ClueTracker.RegisterClue(clueId);
				}
				break;

			default:
				int randomSound = Random.Range(0, 3);
				audioManager?.PlaySFX($"doll_creepy_{randomSound}");
				break;
		}
	}

	private System.Collections.IEnumerator ShowDelayedDialogue(IUIManager uiManager, string speaker, string dialogue, float delay)
	{
		yield return new WaitForSeconds(delay);
		uiManager?.ShowDialogue(speaker, dialogue);
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