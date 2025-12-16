using UnityEngine;

public class ClueItem_Enhanced : MonoBehaviour, IInteractable
{
	[Header("Database")]
	[SerializeField] private ClueDatabase database;
	[SerializeField] private string clueId; // ID만 입력하면 됨!

	private ClueDatabase.ClueData _clueData;

	public string InteractionPrompt
	{
		get
		{
			if (_clueData == null) return "[F] 조사하기";
			return $"[F] {_clueData.clueName} 조사하기";
		}
	}

	private void Awake()
	{
		if (database != null)
		{
			_clueData = database.GetClue(clueId);

			if (_clueData == null)
			{
				Debug.LogError($"ClueDatabase에 '{clueId}' 단서가 없습니다!");
			}
		}
	}

	public bool CanInteract(IPlayer player)
	{
		return !player.Inventory.HasItem(clueId);
	}

	public void Interact(IPlayer player)
	{
		if (_clueData == null) return;

		// 단서 아이템 생성
		ClueItem clue = new ClueItem(
			_clueData.clueId,
			_clueData.clueName,
			_clueData.description
		);

		player.Inventory.AddItem(clue);
		GameManager.Instance.ClueTracker.RegisterClue(_clueData.clueId);

		// 대사 표시
		var uiManager = FindAnyObjectByType<UIManager>();
		if (!string.IsNullOrEmpty(_clueData.dialogue))
		{
			uiManager?.ShowDialogue(_clueData.speaker, _clueData.dialogue);
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