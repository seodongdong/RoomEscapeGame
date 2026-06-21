using UnityEngine;

/// <summary>
/// 1스테이지: 우선순위 체크 문
/// 기획서: "다른 오브젝트를 클릭하면 '우선 TV를 살펴보자..'"
/// </summary>
public class Stage1_DoorWithPriorityCheck : MonoBehaviour, IInteractable
{
	[Header("Settings")]
	[SerializeField] private bool isLocked = true;
	[SerializeField] private GameObject tvObject; // TV 참조

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string tvPriorityDialogue = "우선 TV를 살펴보자...";
	[TextArea(2, 5)]
	[SerializeField] private string lockedDialogue = "문이 잠겨있다.";

	public string InteractionPrompt => "[F] 문 열기";

	public bool CanInteract(IPlayer player)
	{
		return true;
	}

	public void Interact(IPlayer player)
	{
		// 기존 tv.CanInteract() 방식 대신 아래로 교체
		if (Stage1TVGate.CheckPriorityBlocked(player)) return;

		if (isLocked)
		{
			var uiManager = GameServices.UI;
			uiManager?.ShowDialogue(speaker, lockedDialogue);
		}
		else
		{
			gameObject.SetActive(false);
		}
	}
}