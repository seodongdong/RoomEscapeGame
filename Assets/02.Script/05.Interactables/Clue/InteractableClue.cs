using UnityEngine;

/// <summary>
/// 단서 오브젝트 (문서/물리 분리)
/// - Document: 맵에 남음, 전체화면 뷰어
/// - Physical: 인벤토리 추가, 맵에서 사라짐
/// - Environment: 맵에 남음, 확대 뷰
/// </summary>
public class InteractableClue : MonoBehaviour, IInteractable
{
	[Header("Clue Info")]
	[SerializeField] private string clueId;
	[SerializeField] private string clueName;
	[TextArea(3, 10)]
	[SerializeField] private string description;
	[SerializeField] private Sprite icon;

	[Header("Clue Type")]
	[SerializeField] private ClueType clueType = ClueType.Document;

	[Header("Document Settings (문서류)")]
	[SerializeField] private Sprite[] documentPages;    // 문서 페이지 이미지들
	[SerializeField] private bool canReread = true;     // 재열람 가능 여부

	[Header("Environment Settings (환경 단서)")]
	[SerializeField] private Sprite environmentImage;   // 확대 이미지
	[SerializeField] private string environmentHint;    // 플레이어가 기억할 힌트

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string dialogue;

	private bool _isRegistered = false;

	public string InteractionPrompt
	{
		get
		{
			switch (clueType)
			{
				case ClueType.Document:
					return $"[F] {clueName} 읽기";
				case ClueType.Physical:
					return $"[F] {clueName} 획득";
				case ClueType.Environment:
					return $"[F] {clueName} 살펴보기";
				default:
					return "[F] 조사하기";
			}
		}
	}

	public bool CanInteract(IPlayer player)
	{
		// Physical은 이미 획득했으면 상호작용 불가
		if (clueType == ClueType.Physical)
		{
			return !player.Inventory.HasItem(clueId);
		}
		// Document/Environment는 항상 가능
		return true;
	}

	public void Interact(IPlayer player)
	{
		// 처음 상호작용 시 단서 등록
		if (!_isRegistered)
		{
			_isRegistered = true;
			GameManager.Instance.ClueTracker.RegisterClue(clueId);
		}

		switch (clueType)
		{
			case ClueType.Document:
				OpenDocumentViewer();
				break;

			case ClueType.Physical:
				PickupPhysicalItem(player);
				break;

			case ClueType.Environment:
				OpenEnvironmentViewer();
				break;
		}
	}

	// 문서 뷰어 열기
	private void OpenDocumentViewer()
	{
		// canReread가 false면 한 번만 열람 가능
		if (!canReread && _isRegistered)
		{
			var uiManager = FindAnyObjectByType<UIManager>();
			uiManager?.ShowDialogue(speaker, "이미 확인한 문서다.");
			return;
		}

		var documentViewer = FindAnyObjectByType<DocumentViewerUI>();
		if (documentViewer != null)
		{
			documentViewer.OpenDocument(clueName, documentPages, dialogue);
		}
		else
		{
			var uiManager = FindAnyObjectByType<UIManager>();
			uiManager?.ShowDialogue(speaker, dialogue);
		}
	}

	// 물리 아이템 획득
	private void PickupPhysicalItem(IPlayer player)
	{
		ClueItem item = new ClueItem(clueId, clueName, description);
		player.Inventory.AddItem(item);

		var uiManager = FindAnyObjectByType<UIManager>();
		if (!string.IsNullOrEmpty(dialogue))
		{
			uiManager?.ShowDialogue(speaker, dialogue);
		}

		// 맵에서 사라짐
		gameObject.SetActive(false);
	}

	// Environment 타입일 때 ObjectViewer3D로 위임
	private void OpenEnvironmentViewer()
	{
		// ObjectViewer3D 컴포넌트가 있으면 그쪽에서 처리
		// InteractableClue 대신 ObjectViewer3D를 직접 붙여서 사용 권장

		var uiManager = FindAnyObjectByType<UIManager>();
		if (!string.IsNullOrEmpty(dialogue))
		{
			uiManager?.ShowDialogue(speaker, dialogue);
		}
		// SetActive(false) 없음 - 맵에 남음
	}
}