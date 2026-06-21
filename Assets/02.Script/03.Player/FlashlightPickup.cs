using UnityEngine;

public class FlashlightPickup : MonoBehaviour, IInteractable, ISaveRestorable
{
	[Header("연결")]
	[SerializeField] private Flashlight flashlight;

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)]
	[SerializeField] private string dialogue = "손전등을 발견했다. 이제 어두운 곳도 비춰볼 수 있겠다.";

	public string InteractionPrompt => "[F] 손전등 획득";

	public bool CanInteract(IPlayer player) => true;

	// ★ 추가: ISaveRestorable — 손전등 획득은 다른 단서들과 다른 전용 ID를 사용합니다.
	private const string FLASHLIGHT_RESTORE_ID = "__flashlight_pickup__";
	public string RestoreItemId => FLASHLIGHT_RESTORE_ID;
	public void ApplyAlreadyCollected()
	{
		gameObject.SetActive(false);
	}

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

		var uiManager = GameServices.UI;
		uiManager?.ShowDialogue(speaker, dialogue);

		gameObject.SetActive(false);
	}
}