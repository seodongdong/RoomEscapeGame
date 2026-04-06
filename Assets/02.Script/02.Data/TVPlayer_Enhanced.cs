using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.UI;

public class TVPlayer_Enhanced : MonoBehaviour, IInteractable
{
	[Header("Required Tape")]
	[SerializeField] private string requiredTapeId;

	[Header("UI References")]
	[SerializeField] private GameObject videoScreen;
	[SerializeField] private TextMeshProUGUI narrationText;
	[SerializeField] private Image thumbnailImage; // 선택사항

	[Header("Database")]
	[SerializeField] private VideoTapeDatabase database;

	public string InteractionPrompt => "[F] TV에서 비디오 재생";

	public bool CanInteract(IPlayer player)
	{
		return player.Inventory.HasItem(requiredTapeId);
	}

	public void Interact(IPlayer player)
	{
		var tape = player.Inventory.GetItem(requiredTapeId);
		if (tape != null)
		{
			// Database에서 비디오 데이터 가져오기
			var videoData = database?.GetVideo(requiredTapeId);

			if (videoData != null)
			{
				StartCoroutine(PlayVideoCoroutine(videoData));
			}
			else
			{
				// Database 없으면 아이템 Description 사용
				StartCoroutine(PlayVideoCoroutine(tape.Description, null));
			}
		}
	}

	private IEnumerator PlayVideoCoroutine(VideoTapeDatabase.VideoData videoData)
	{
		videoScreen?.SetActive(true);

		// 썸네일 이미지 표시
		if (thumbnailImage != null && videoData.thumbnailImage != null)
		{
			thumbnailImage.sprite = videoData.thumbnailImage;
		}

		// 나레이션 타이핑
		if (narrationText != null)
		{
			narrationText.text = "";
			foreach (char c in videoData.narration)
			{
				narrationText.text += c;
				yield return new WaitForSeconds(0.05f);
			}
		}

		yield return new WaitForSeconds(3f);
		videoScreen?.SetActive(false);
	}

	private IEnumerator PlayVideoCoroutine(string narration, Sprite thumbnail)
	{
		videoScreen?.SetActive(true);

		if (thumbnailImage != null && thumbnail != null)
		{
			thumbnailImage.sprite = thumbnail;
		}

		if (narrationText != null)
		{
			narrationText.text = "";
			foreach (char c in narration)
			{
				narrationText.text += c;
				yield return new WaitForSeconds(0.05f);
			}
		}

		yield return new WaitForSeconds(3f);
		videoScreen?.SetActive(false);
	}
}