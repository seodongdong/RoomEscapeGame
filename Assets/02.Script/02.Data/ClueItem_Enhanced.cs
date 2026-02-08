using UnityEngine;

/// <summary>
/// Database 기반 단서 아이템
/// ID만 입력하면 Database에서 자동 로드
/// </summary>
public class ClueItem_Enhanced : MonoBehaviour, IInteractable
{
	[Header("Database")]
	[SerializeField] private ClueDatabase database;
	[SerializeField] private string clueId;

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
				Debug.LogError($"[ClueItem] Database에 '{clueId}' 단서가 없습니다!");
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

		ClueItem clue = new ClueItem(
			_clueData.clueId,
			_clueData.clueName,
			_clueData.description
		);

		player.Inventory.AddItem(clue);
		GameManager.Instance.ClueTracker.RegisterClue(_clueData.clueId);

		var uiManager = FindAnyObjectByType<UIManager>();
		if (!string.IsNullOrEmpty(_clueData.dialogue))
		{
			uiManager?.ShowDialogue(_clueData.speaker, _clueData.dialogue);
		}

		gameObject.SetActive(false);
	}
}