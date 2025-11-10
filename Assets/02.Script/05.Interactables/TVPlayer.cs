using UnityEngine;
using TMPro;
using System.Collections;

public class TVPlayer : MonoBehaviour, IInteractable
{
	// 재생에 필요한 비디오 테이프 ID
	[SerializeField] private string requiredTapeId;
	// 비디오 화면과 내레이션 텍스트 UI
	[SerializeField] private GameObject videoScreen;
	[SerializeField] private TextMeshProUGUI narrationText;

	public string InteractionPrompt => "[F] TV에서 비디오 재생";

	public bool CanInteract(IPlayer player)
	{
		return player.Inventory.HasItem(requiredTapeId);
	}

	// 상호작용 처리
	public void Interact(IPlayer player)
	{
		// 인벤토리에서 비디오 테이프 아이템 가져오기
		var tape = player.Inventory.GetItem(requiredTapeId);
		if (tape != null)
		{
			StartCoroutine(PlayVideoCoroutine(tape.Description));
		}
	}

	// 비디오 재생 및 내레이션 표시 코루틴
	private IEnumerator PlayVideoCoroutine(string narration)
	{
		// 비디오 화면 활성화
		videoScreen?.SetActive(true);

		// 내레이션 타이핑 효과
		if (narrationText != null)
		{
			narrationText.text = "";
			foreach (char c in narration)
			{
				narrationText.text += c;
				yield return new WaitForSeconds(0.05f);
			}
		}

		// 3초 대기 후 비디오 화면 비활성화
		yield return new WaitForSeconds(3f);
		videoScreen?.SetActive(false);
	}
}
