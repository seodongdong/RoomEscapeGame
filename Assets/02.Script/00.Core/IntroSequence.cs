using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// 인트로 연출
/// 기획서: 비디오 화면 → 깨어남 → 소녀 첫 만남
/// </summary>
public class IntroSequence : MonoBehaviour
{
	[Header("References")]
	[SerializeField] private Player player;
	[SerializeField] private Girl girl;
	[SerializeField] private Transform doorPosition;

	[Header("Cameras")]
	[SerializeField] private Camera introCamera;
	[SerializeField] private Camera playerCamera;

	[Header("Fade")]
	[SerializeField] private Image fadeImage;
	[SerializeField] private float fadeDuration = 2f;

	[Header("Dialogue")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 5)]
	[SerializeField] private string wakeUpDialogue = "...여기가 어디지?";


	private IUIManager _uiManager;

	private void Start()
	{
		_uiManager = GameServices.UI;
		StartCoroutine(PlayIntro());
	}

	private IEnumerator PlayIntro()
	{
		// 플레이어 조작 비활성화
		if (player != null)
		{
			player.enabled = false;
		}

		// 상태 변경
		GameManager.Instance.ChangeState(GameState.MainMenu);

		// 1. 페이드 인
		yield return StartCoroutine(FadeIn());

		// 2. 깨어남
		_uiManager?.ShowDialogue(speaker, wakeUpDialogue);
		yield return new WaitForSeconds(3f);

		// 3. 카메라 전환
		if (introCamera != null && playerCamera != null)
		{
			introCamera.gameObject.SetActive(false);
			playerCamera.gameObject.SetActive(true);
		}

		// 4. 플레이어 활성화
		if (player != null)
		{
			player.enabled = true;
		}

		// 5. 잠시 대기 후 소녀 등장
		yield return new WaitForSeconds(4f);  // 딜레이 늘리기
		girl?.FirstMeeting();

		// 6. 게임 시작
		yield return new WaitForSeconds(5f);
		GameManager.Instance.ChangeState(GameState.Playing);
	}

	private IEnumerator FadeIn()
	{
		if (fadeImage == null) yield break;

		float elapsed = 0f;
		Color color = fadeImage.color;

		while (elapsed < fadeDuration)
		{
			elapsed += Time.deltaTime;
			color.a = 1f - (elapsed / fadeDuration);
			fadeImage.color = color;
			yield return null;
		}

		color.a = 0f;
		fadeImage.color = color;
		fadeImage.gameObject.SetActive(false);
	}

	private IEnumerator FadeOut()
	{
		if (fadeImage == null) yield break;

		fadeImage.gameObject.SetActive(true);
		float elapsed = 0f;
		Color color = fadeImage.color;

		while (elapsed < fadeDuration)
		{
			elapsed += Time.deltaTime;
			color.a = elapsed / fadeDuration;
			fadeImage.color = color;
			yield return null;
		}

		color.a = 1f;
		fadeImage.color = color;
	}
}