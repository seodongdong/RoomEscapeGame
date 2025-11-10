using UnityEngine;

// 찢어진 일기장 단서 상호작용 클래스
public class Stage1_DiaryClue : MonoBehaviour, IInteractable
{
	[Header("Clue Info")]
	[SerializeField] private string clueId = "diary_page_1";
	[SerializeField] private string clueName = "찢어진 일기장";
	[TextArea(3, 10)]
	[SerializeField] private string description = "누군가 그린 그림 일기. 스파게티처럼 보이는 음식과 탁한 색의 우울한 표정을 한 아저씨가 그려져 있다.";

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string dialogue = "찢어진 일기장이다... 누가 그린 걸까?";

	public string InteractionPrompt => $"[F] {clueName} 조사하기";

	public bool CanInteract(IPlayer player)
	{
		return !player.Inventory.HasItem(clueId);
	}

	// 상호작용 메서드
	public void Interact(IPlayer player)
	{
		// 단서 아이템 생성 및 인벤토리에 추가
		ClueItem clue = new ClueItem(clueId, clueName, description);

		// 인벤토리에 단서 추가 및 단서 추적기 등록
		player.Inventory.AddItem(clue);
		GameManager.Instance.ClueTracker.RegisterClue(clueId);

		// 대화 표시
		var uiManager = FindAnyObjectByType<UIManager>();
		if (!string.IsNullOrEmpty(dialogue))
		{
			uiManager?.ShowDialogue(speaker, dialogue);
		}

		gameObject.SetActive(false);
	}

	// 플레이어가 트리거 영역에 들어올 때 현재 상호작용 가능한 객체 설정
	private void OnTriggerEnter(Collider other)
	{
		// 플레이어인지 확인 후 상호작용 가능 객체 설정
		if (other.TryGetComponent<Player>(out var player))
		{
			player.SetCurrentInteractable(this);
		}
	}

	// 플레이어가 트리거 영역에서 나갈 때 현재 상호작용 가능한 객체 해제
	private void OnTriggerExit(Collider other)
	{
		// 플레이어인지 확인 후 상호작용 가능 객체 해제
		if (other.TryGetComponent<Player>(out var player))
		{
			player.SetCurrentInteractable(null);
		}
	}
}