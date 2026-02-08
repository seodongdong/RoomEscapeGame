using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Database 기반 TV 플레이어
/// </summary>
public class TVPlayer_Enhanced : MonoBehaviour, IInteractable
{
	[Header("Required Tape")]
	[SerializeField] private string requiredTapeId;

	[Header("UI")]
	[SerializeField] private GameObject videoScreen;
	[SerializeField] private TextMeshProUGUI narrationText;
	[SerializeField] private Image thumbnailImage;

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
			var videoData = database?.GetVideo(requiredTapeId);

			if (videoData != null)
			{
				StartCoroutine(PlayVideoCoroutine(videoData));
			}
			else
			{
				StartCoroutine(PlayVideoCoroutine(tape.Description, null));
			}
		}
	}

	private IEnumerator PlayVideoCoroutine(VideoTapeDatabase.VideoData videoData)
	{
		videoScreen?.SetActive(true);

		if (thumbnailImage != null && videoData.thumbnailImage != null)
		{
			thumbnailImage.sprite = videoData.thumbnailImage;
		}

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