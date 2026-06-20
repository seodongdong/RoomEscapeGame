using UnityEngine;

/// <summary>
/// 3스테이지 미로 입장 직후 TV장에 배치되는 손전등 획득 오브젝트.
///
/// [기획서 기준]
/// "미로 입장 직후 TV장에서 발견. 이후 O키 또는 L키로 게임 내내 사용 가능"
///
/// [동작]
/// F키 상호작용 → Flashlight.Acquire() 호출 + 대사 출력 + 오브젝트 비활성화.
/// 인벤토리(IInventory)에는 등록하지 않음 — 손전등은 토글형 장비이며
/// 기획서의 "사용 가능 단서(인벤토리 등록)" 분류와는 다른 개념이기 때문.
/// </summary>
public class FlashlightPickup : MonoBehaviour, IInteractable
{
	[Header("연결")]
	[Tooltip("Player 자식의 Flashlight 컴포넌트. 비워두면 Start()에서 자동 탐색.")]
	[SerializeField] private Flashlight flashlight;

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)]
	[SerializeField] private string dialogue = "손전등을 발견했다. 이제 어두운 곳도 비춰볼 수 있겠다.";

	public string InteractionPrompt => "[F] 손전등 획득";

	public bool CanInteract(IPlayer player) => true;

	private void Start()
	{
		if (flashlight == null)
			flashlight = FindAnyObjectByType<Flashlight>();

		if (flashlight == null)
			Debug.LogError("[FlashlightPickup] Flashlight 컴포넌트를 찾을 수 없습니다!");
	}

	public void Interact(IPlayer player)
	{
		flashlight?.Acquire();

		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue(speaker, dialogue);

		gameObject.SetActive(false);
	}
}