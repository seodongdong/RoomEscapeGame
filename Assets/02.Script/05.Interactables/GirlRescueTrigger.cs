using UnityEngine;

/// <summary>
/// 5스테이지: 제설함 상자
/// 기획서: "상자 안에 갇힌 누나를 구한다"
/// </summary>
public class GirlRescueTrigger : MonoBehaviour, IInteractable
{
	[Header("References")]
	[SerializeField] private Transform girlTransform;
	[SerializeField] private GameObject boxVisual;
	[SerializeField] private string requiredKeyId = "basement_key";

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string noKeyDialogue = "열쇠가 필요하다.";
	[TextArea(2, 5)]
	[SerializeField] private string rescueDialogue = "...! 이 안에 누군가 있다!";

	private bool _isOpened;

	public string InteractionPrompt => "[F] 제설함 상자 열기";

	public bool CanInteract(IPlayer player)
	{
		return !_isOpened;
	}

	public void Interact(IPlayer player)
	{
		var uiManager = FindAnyObjectByType<UIManager>();

		// 열쇠 필요
		if (!player.Inventory.HasItem(requiredKeyId))
		{
			uiManager?.ShowDialogue(speaker, noKeyDialogue);
			return;
		}

		_isOpened = true;

		boxVisual?.SetActive(false);
		girlTransform.gameObject.SetActive(true);

		uiManager?.ShowDialogue(speaker, rescueDialogue);

		// 기획서: "상자 열면 컷씬으로 등장한다"
		var chaseSequence = FindAnyObjectByType<ChaseSequence>();
		chaseSequence?.StartChase();
	}
}