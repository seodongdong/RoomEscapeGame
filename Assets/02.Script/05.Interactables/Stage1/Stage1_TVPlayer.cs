using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// 1스테이지: TV 상호작용
/// 기획서: 4단계 반복 시스템
/// 1. 정상 홈비디오
/// 2. 미세한 노이즈
/// 3. 소리 끊김, 화면 왜곡
/// 4. TV 강제 종료 + 크리처 등장
/// </summary>
public class Stage1_TVPlayer : MonoBehaviour, IInteractable
{
	[Header("Video Clips")]
	[SerializeField] private string[] videoDialogues; // 홈비디오 대사들

	[Header("UI")]
	[SerializeField] private GameObject videoScreen;
	[SerializeField] private TextMeshProUGUI narrationText;

	[Header("Creature")]
	[SerializeField] private GameObject creature;

	[Header("Effects")]
	[SerializeField] private AudioSource tvAudioSource;
	[SerializeField] private Material normalMaterial;
	[SerializeField] private Material noisyMaterial;
	[SerializeField] private Material glitchMaterial;

	private int _viewCount = 0;
	private bool _isPlaying = false;

	public string InteractionPrompt => _viewCount < 4 ? "[F] TV 시청하기" : "[F] TV (꺼짐)";

	public bool CanInteract(IPlayer player)
	{
		return !_isPlaying;
	}

	public void Interact(IPlayer player)
	{
		if (_viewCount >= 4)
		{
			// 기획서: "TV 강제 종료"
			var uiManager = FindAnyObjectByType<UIManager>();
			uiManager?.ShowDialogue("소년", "TV가 꺼져있다.");
			return;
		}

		StartCoroutine(PlayVideoSequence());
	}

	private IEnumerator PlayVideoSequence()
	{
		_isPlaying = true;
		videoScreen?.SetActive(true);

		_viewCount++;

		switch (_viewCount)
		{
			case 1:
				// 정상 홈비디오
				yield return StartCoroutine(PlayNormalVideo());
				break;

			case 2:
				// 미세한 노이즈
				yield return StartCoroutine(PlayNoisyVideo());
				break;

			case 3:
				// 소리 끊김, 화면 왜곡
				yield return StartCoroutine(PlayGlitchVideo());
				break;

			case 4:
				// TV 강제 종료 + 크리처 등장
				yield return StartCoroutine(TriggerCreatureEvent());
				break;
		}

		videoScreen?.SetActive(false);
		_isPlaying = false;
	}

	private IEnumerator PlayNormalVideo()
	{
		videoScreen.GetComponent<Renderer>().material = normalMaterial;

		// 기획서: "가정적이고 다정한 아빠의 시점"
		// "얘들아 주방은 위험하니까 거실에서 놀고있어라."
		narrationText.text = "";
		string dialogue = "얘들아, 주방은 위험하니까 거실에서 놀고 있어라.";

		foreach (char c in dialogue)
		{
			narrationText.text += c;
			yield return new WaitForSeconds(0.05f);
		}

		yield return new WaitForSeconds(3f);
	}

	private IEnumerator PlayNoisyVideo()
	{
		videoScreen.GetComponent<Renderer>().material = noisyMaterial;

		// 노이즈 효과음
		tvAudioSource?.PlayOneShot(tvAudioSource.clip, 0.3f);

		yield return StartCoroutine(PlayNormalVideo());
	}

	private IEnumerator PlayGlitchVideo()
	{
		videoScreen.GetComponent<Renderer>().material = glitchMaterial;

		// 소리 끊김
		tvAudioSource?.Stop();

		narrationText.text = "□□□ □□□□□...";
		yield return new WaitForSeconds(2f);

		// 화면 왜곡
		videoScreen.transform.localScale = new Vector3(1.2f, 0.8f, 1f);
		yield return new WaitForSeconds(1f);
		videoScreen.transform.localScale = Vector3.one;
	}

	private IEnumerator TriggerCreatureEvent()
	{
		// TV 강제 종료
		videoScreen?.SetActive(false);
		tvAudioSource?.Stop();

		yield return new WaitForSeconds(0.5f);

		// 기획서: "돌아보면 크리처 등장"
		creature?.SetActive(true);

		var audioManager = FindAnyObjectByType<AudioManager>();
		audioManager?.PlaySFX("creature_appear");

		Debug.Log("[TV] 크리처 등장!");
	}
}