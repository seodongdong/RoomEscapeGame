using UnityEngine;
using System.Collections;

/// <summary>
/// 대문 (엔딩 분기)
/// 기획서: 소녀 구출 + 캠코더 수집 여부에 따라 엔딩 결정
/// </summary>
public class ExitDoor : MonoBehaviour, IInteractable
{
	[Header("References")]
	[SerializeField] private Girl girlReference;

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string escapeDialogue = "빨리 나가자!";

	public string InteractionPrompt => "[F] 대문 열기";

	public bool CanInteract(IPlayer player)
	{
		return true;
	}

	public void Interact(IPlayer player)
	{
		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue(speaker, escapeDialogue);

		// 소녀 구출 여부
		bool girlRescued = girlReference != null && girlReference.gameObject.activeSelf;

		// 캠코더 수집 여부
		bool hasCamcorder = player.Inventory.HasItem("camcorder");

		StartCoroutine(TriggerEndingDelayed(girlRescued, hasCamcorder, 2f));
	}

	private IEnumerator TriggerEndingDelayed(bool girlRescued, bool hasCamcorder, float delay)
	{
		yield return new WaitForSeconds(delay);

		var endingType = GameManager.Instance.EndingManager.CheckEndingConditions(
			FindAnyObjectByType<Player>().Inventory,
			girlRescued,
			hasCamcorder
		);

		GameManager.Instance.EndingManager.TriggerEnding(endingType);
	}
}