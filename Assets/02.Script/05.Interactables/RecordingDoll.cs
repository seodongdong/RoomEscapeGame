using UnityEngine;
using System.Collections;

/// <summary>
/// 녹음 인형
/// - 계속 상호작용 시 1→2→3→1→2→3 반복
/// - 상호작용 프롬프트: Raycast로 콜라이더에 닿을 때만 표시
/// </summary>
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
	[SerializeField] private string dialogue2 = "{0}이가 누구예요?";

	[TextArea(2, 5)]
	[SerializeField] private string dialogue2_player = "...? 이상한 소리가 들린다.";

	[TextArea(2, 5)]
	[SerializeField] private string dialogue3 = "저 {0}이 아니예요...";

	[TextArea(3, 10)]
	[SerializeField] private string clueDescription = "이상한 소리가 녹음되어 있는 인형. 누군가가 자신이 아니라고 부정하는 목소리가 들린다.";

	// ⭐ 1~3 반복 카운트 (1-based, 모듈러로 순환)
	private int _playCount = 0;

	// ⭐ 프롬프트는 Player Raycast가 처리하므로
	// OnTriggerEnter/Exit 제거 → Raycast 방식 사용
	public string InteractionPrompt => "[F] 인형 조사하기";

	public bool CanInteract(IPlayer player)
	{
		return true;
	}

	public void Interact(IPlayer player)
	{
		if (Stage1TVPriorityManager.CheckPriorityBlocked(player)) return;

		var audioManager = FindAnyObjectByType<AudioManager>();
		var uiManager = FindAnyObjectByType<UIManager>();

		_playCount++;

		// ⭐ 1→2→3→1→2→3 순환 (1-based 모듈러)
		int step = ((_playCount - 1) % 3) + 1;

		switch (step)
		{
			case 1:
				audioManager?.PlaySFX("doll_voice_1");
				uiManager?.ShowDialogue(dollSpeaker, dialogue1);
				break;

			case 2:
				audioManager?.PlaySFX("doll_voice_2");
				string d2 = string.Format(dialogue2, deadGirlName);
				uiManager?.ShowDialogue(dollSpeaker, d2);
				StartCoroutine(ShowDelayedDialogue(uiManager, playerSpeaker, dialogue2_player, 2f));
				break;

			case 3:
				audioManager?.PlaySFX("doll_voice_3");
				string d3 = string.Format(dialogue3, deadGirlName);
				uiManager?.ShowDialogue(dollSpeaker, d3);

				// 처음 3회 도달 시 단서 등록 (이후엔 이미 등록됨)
				if (!player.Inventory.HasItem(clueId))
				{
					ClueItem clue = new ClueItem(clueId, "녹음 인형", clueDescription);
					player.Inventory.AddItem(clue);
					GameManager.Instance.ClueTracker.RegisterClue(clueId);
					Debug.Log("[RecordingDoll] 단서 등록: recording_doll");
				}
				break;
		}
	}

	private IEnumerator ShowDelayedDialogue(IUIManager uiManager, string speaker, string dialogue, float delay)
	{
		yield return new WaitForSeconds(delay);
		uiManager?.ShowDialogue(speaker, dialogue);
	}
}