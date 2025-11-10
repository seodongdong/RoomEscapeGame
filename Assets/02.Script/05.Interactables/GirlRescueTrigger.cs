using UnityEngine;

// 소녀 구출 트리거 클래스

public class GirlRescueTrigger : MonoBehaviour, IInteractable
{
	[Header("References")]
	[SerializeField] private Transform girlTransform;
	[SerializeField] private GameObject boxVisual;

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string dialogue = "...! 이 안에 누군가 있다!";

	private bool _isOpened;

	public string InteractionPrompt => "[F] 제설함 상자 열기";

	public bool CanInteract(IPlayer player)
	{
		return !_isOpened;
	}

	public void Interact(IPlayer player)
	{
		_isOpened = true;

		boxVisual?.SetActive(false);
		girlTransform.gameObject.SetActive(true);

		Debug.Log("제설함 상자에서 소녀를 발견했습니다!");

		var uiManager = FindAnyObjectByType<UIManager>();
		if (!string.IsNullOrEmpty(dialogue))
		{
			uiManager?.ShowDialogue(speaker, dialogue);
		}

		var chaseSequence = FindAnyObjectByType<ChaseSequence>();
		chaseSequence?.StartChase();
	}
}