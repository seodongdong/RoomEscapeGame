using UnityEngine;

/// <summary>
/// 범용 단서 오브젝트
/// Inspector에서 모든 값 편집 가능
/// </summary>
public class InteractableClue : MonoBehaviour, IInteractable
{
	[Header("Clue Info")]
	[SerializeField] private string clueId;
	[SerializeField] private string clueName;
	[TextArea(3, 10)]
	[SerializeField] private string description;
	[SerializeField] private Sprite icon;

	[Header("Interaction")]
	[SerializeField] private bool showNameInPrompt = true;
	[SerializeField] private bool isCollectable = true;

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string dialogue;

	public string InteractionPrompt
	{
		get
		{
			if (showNameInPrompt && !string.IsNullOrEmpty(clueName))
			{
				return $"[F] {clueName} 조사하기";
			}
			return "[F] 조사하기";
		}
	}

	public bool CanInteract(IPlayer player)
	{
		if (isCollectable)
		{
			return !player.Inventory.HasItem(clueId);
		}
		return true;
	}

	public void Interact(IPlayer player)
	{
		if (isCollectable)
		{
			ClueItem clue = new ClueItem(clueId, clueName, description);
			player.Inventory.AddItem(clue);
			GameManager.Instance.ClueTracker.RegisterClue(clueId);

			gameObject.SetActive(false);
		}

		var uiManager = FindAnyObjectByType<UIManager>();
		if (!string.IsNullOrEmpty(dialogue))
		{
			uiManager?.ShowDialogue(speaker, dialogue);
		}
	}
}