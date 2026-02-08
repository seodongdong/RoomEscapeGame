using UnityEngine;

/// <summary>
/// 액자 (가족사진 등)
/// </summary>
public class PhotoFrame : MonoBehaviour, IInteractable
{
	[Header("Photo Info")]
	[SerializeField] private string photoId;
	[SerializeField] private string photoName;
	[TextArea(3, 10)]
	[SerializeField] private string description;
	[SerializeField] private Sprite photoImage;

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string dialogue;

	[Header("Display")]
	[SerializeField] private bool showInPopup;
	[SerializeField] private GameObject photoPopup;
	[SerializeField] private UnityEngine.UI.Image popupImage;

	public string InteractionPrompt => "[F] 사진 보기";

	public bool CanInteract(IPlayer player)
	{
		return true;
	}

	public void Interact(IPlayer player)
	{
		// 팝업 표시
		if (showInPopup && photoPopup != null)
		{
			photoPopup.SetActive(true);
			if (popupImage != null && photoImage != null)
			{
				popupImage.sprite = photoImage;
			}
		}

		// 대사 표시
		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue(speaker, dialogue);

		// 단서로 추가
		if (!player.Inventory.HasItem(photoId))
		{
			ClueItem photo = new ClueItem(photoId, photoName, description);
			player.Inventory.AddItem(photo);  // ✅ 수정됨
			GameManager.Instance.ClueTracker.RegisterClue(photoId);
		}
	}
}