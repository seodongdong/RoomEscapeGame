using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

/// <summary>
/// TV 플레이어
/// Inspector에서 타입 선택:
/// - CloseUp: 카메라 줌인 + 4단계 반복 (Stage1용)
/// - Static:  지지직 Material + 대사 출력 (일반용)
/// </summary>
public class TVPlayer : MonoBehaviour, IInteractable
{
	public enum TVType { CloseUp, Static }

	[Header("TV Type")]
	[SerializeField] private TVType tvType = TVType.CloseUp;

	// ───────────────────────────────
	// CloseUp 전용
	// ───────────────────────────────
	[Header("CloseUp Settings")]
	[SerializeField] private Transform tvCameraPoint;          // TV 앞 카메라 위치/회전
	[SerializeField] private float cameraTransitionDuration = 0.8f;
	[SerializeField] private AnimationCurve transitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

	[Header("CloseUp - 4단계 대사")]
	[TextArea(2, 4)][SerializeField] private string step1Dialogue = "얘들아, 주방은 위험하니까 거실에서 놀고 있어라.";
	[TextArea(2, 4)][SerializeField] private string step2Dialogue = "△△△△ △△△△△△...";
	[TextArea(2, 4)][SerializeField] private string step3Dialogue = "▒▒▒ ▒▒▒▒▒ ▒▒▒▒...";
	[TextArea(2, 4)][SerializeField] private string step4Dialogue = "...";   // 4단계: 크리처 등장 후

	[Header("CloseUp - 단계별 화면 Material")]
	[SerializeField] private Renderer tvScreenRenderer;        // TV 화면 오브젝트
	[SerializeField] private Material normalMaterial;
	[SerializeField] private Material noisyMaterial;
	[SerializeField] private Material glitchMaterial;
	[SerializeField] private Material offMaterial;

	[Header("CloseUp - 크리처")]
	[SerializeField] private GameObject creature;              // 4단계에 활성화

	// ───────────────────────────────
	// Static 전용
	// ───────────────────────────────
	[Header("Static Settings")]
	[SerializeField] private Renderer staticScreenRenderer;
	[SerializeField] private Material staticMaterial;
	[TextArea(2, 4)][SerializeField] private string staticDialogue = "지지직...";
	[SerializeField] private string staticSpeaker = "소년";

	// ───────────────────────────────
	// 공통
	// ───────────────────────────────
	[Header("공통 - 나레이션 UI (Screen Space)")]
	[SerializeField] private GameObject narrationPanel;         // 화면 하단 패널
	[SerializeField] private TextMeshProUGUI narrationText;

	[Header("공통 - 오디오")]
	[SerializeField] private AudioSource tvAudioSource;
	[SerializeField] private AudioClip staticClip;
	[SerializeField] private AudioClip glitchClip;

	// ───────────────────────────────
	// 내부 상태
	// ───────────────────────────────
	private int _viewCount = 0;
	private bool _isPlaying = false;

	private Camera _mainCamera;
	private Vector3 _originalCamPos;
	private Quaternion _originalCamRot;
	private Transform _originalCamParent;
	private Player _player;

	private void Awake()
	{
		_mainCamera = Camera.main;
		_player = GameServices.Player;

		if (narrationPanel != null)
			narrationPanel.SetActive(false);
	}

	public string InteractionPrompt
	{
		get
		{
			if (_isPlaying) return "";
			if (tvType == TVType.CloseUp)
				return _viewCount < 4 ? "[F] TV 시청하기" : "[F] TV (꺼짐)";
			return "[F] TV 보기";
		}
	}

	public bool CanInteract(IPlayer player)
	{
		if (_isPlaying) return false;
		if (tvType == TVType.CloseUp && _viewCount >= 4) return false;
		return true;
	}

	public void Interact(IPlayer player)
	{


		if (tvType == TVType.CloseUp)
			StartCoroutine(PlayCloseUp());
		else
			StartCoroutine(PlayStatic());
	}

	// ══════════════════════════════════════
	// CloseUp 시퀀스
	// ══════════════════════════════════════
	private IEnumerator PlayCloseUp()
	{
		Debug.Log($"[TVPlayer] PlayCloseUp 시작 - viewCount: {_viewCount + 1}");

		_isPlaying = true;
		_viewCount++;

		// ⭐ UIManager 프롬프트 숨김
		var uiManager = GameServices.UI;
		uiManager?.HideInteractionPrompt();

		// 플레이어 조작 비활성화
		if (_player != null) _player.enabled = false;
		GameManager.Instance?.ChangeState(GameState.Puzzle);

		// 카메라 원위치 저장 → TV 앞으로 이동
		Debug.Log("[TVPlayer] 카메라 이동 시작");
		yield return StartCoroutine(MoveCamera(true));
		Debug.Log("[TVPlayer] 카메라 이동 완료");

		// 단계별 재생
		switch (_viewCount)
		{
			case 1: yield return StartCoroutine(CloseUp_Step1()); break;
			case 2: yield return StartCoroutine(CloseUp_Step2()); break;
			case 3: yield return StartCoroutine(CloseUp_Step3()); break;
			case 4:
				// Step4는 내부에서 카메라 복귀 + 크리처 회전 처리
				yield return StartCoroutine(CloseUp_Step4());

				// 플레이어 복귀
				if (_player != null) _player.enabled = true;
				GameManager.Instance?.ChangeState(GameState.Playing);

				_isPlaying = false;
				Debug.Log("[TVPlayer] PlayCloseUp 완료");
				yield break; // ⭐ 여기서 종료
		}

		// 카메라 복귀 (Step 1~3만)
		Debug.Log("[TVPlayer] 카메라 복귀 시작");
		yield return StartCoroutine(MoveCamera(false));

		// 플레이어 복귀
		if (_player != null) _player.enabled = true;
		GameManager.Instance?.ChangeState(GameState.Playing);

		_isPlaying = false;
		Debug.Log("[TVPlayer] PlayCloseUp 완료");
	}

	private IEnumerator CloseUp_Step1()
	{
		Debug.Log("[TVPlayer] CloseUp_Step1 시작");
		SetScreenMaterial(normalMaterial);

		if (tvAudioSource != null)
			tvAudioSource.Play();

		yield return StartCoroutine(ShowNarration(step1Dialogue));
		Debug.Log("[TVPlayer] CloseUp_Step1 완료");
	}

	private IEnumerator CloseUp_Step2()
	{
		SetScreenMaterial(noisyMaterial);
		PlayAudio(staticClip, 0.3f);
		yield return StartCoroutine(ShowNarration(step2Dialogue));
	}

	private IEnumerator CloseUp_Step3()
	{
		SetScreenMaterial(glitchMaterial);
		PlayAudio(glitchClip, 0.5f);

		// 화면 흔들기
		if (tvScreenRenderer != null)
		{
			float elapsed = 0f;
			Vector3 originalScale = tvScreenRenderer.transform.localScale;
			while (elapsed < 1.5f)
			{
				float x = Random.Range(0.95f, 1.05f);
				float y = Random.Range(0.95f, 1.05f);
				tvScreenRenderer.transform.localScale = new Vector3(
					originalScale.x * x, originalScale.y * y, originalScale.z);
				elapsed += Time.deltaTime;
				yield return null;
			}
			tvScreenRenderer.transform.localScale = originalScale;
		}

		yield return StartCoroutine(ShowNarration(step3Dialogue));
	}

	private IEnumerator CloseUp_Step4()
	{
		// TV 꺼짐
		SetScreenMaterial(offMaterial);

		if (tvAudioSource != null)
			tvAudioSource.Stop();

		yield return new WaitForSeconds(0.5f);

		// ⭐ 크리처 등장 전 카메라 원위치 복귀
		Debug.Log("[TVPlayer] 카메라 복귀 (크리처 등장 전)");
		yield return StartCoroutine(MoveCamera(false));

		// 잠깐 대기
		yield return new WaitForSeconds(0.3f);

		// 크리처 등장
		if (creature != null)
		{
			creature.SetActive(true);

			Stage1TVGate.SetTVWatched();

			var audioManager = GameServices.Audio;
			audioManager?.PlaySFX("creature_appear");

			// ⭐ 카메라 강제 회전 (크리처 방향)
			yield return StartCoroutine(ForceLookAtCreature());
		}

		// ⭐ 플레이어 놀람 연출
		var uiManager = GameServices.UI;
		uiManager?.ShowDialogue("소년", "!!");
		yield return new WaitForSeconds(1f);

		uiManager?.ShowDialogue("소년", "이게 뭐지…?");
		yield return new WaitForSeconds(1.5f);

		// 대사창 닫기
		uiManager?.HideDialogue();
	}

	// ⭐ 크리처 방향으로 카메라 강제 회전
	private IEnumerator ForceLookAtCreature()
	{
		if (creature == null || _mainCamera == null) yield break;

		Quaternion startRot = _mainCamera.transform.rotation;
		Vector3 directionToCreature = creature.transform.position - _mainCamera.transform.position;
		Quaternion targetRot = Quaternion.LookRotation(directionToCreature);

		float duration = 0.5f;
		float elapsed = 0f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / duration;
			_mainCamera.transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
			yield return null;
		}

		_mainCamera.transform.rotation = targetRot;
		Debug.Log("[TVPlayer] 크리처 방향으로 카메라 회전 완료");
	}

	// ══════════════════════════════════════
	// Static 시퀀스
	// ══════════════════════════════════════
	private IEnumerator PlayStatic()
	{
		_isPlaying = true;

		// 지지직 Material 적용
		if (staticScreenRenderer != null && staticMaterial != null)
			staticScreenRenderer.material = staticMaterial;

		PlayAudio(staticClip, 0.4f);

		// 대사 출력
		var uiManager = GameServices.UI;
		uiManager?.ShowDialogue(staticSpeaker, staticDialogue);

		yield return new WaitForSeconds(3f);

		// 원래 Material 복구
		if (staticScreenRenderer != null && normalMaterial != null)
			staticScreenRenderer.material = normalMaterial;

		_isPlaying = false;
	}

	// ══════════════════════════════════════
	// 카메라 전환
	// ══════════════════════════════════════
	private IEnumerator MoveCamera(bool toTV)
	{
		if (tvCameraPoint == null) yield break;

		Vector3 startPos, endPos;
		Quaternion startRot, endRot;

		if (toTV)
		{
			_originalCamParent = _mainCamera.transform.parent;
			_originalCamPos = _mainCamera.transform.localPosition;  // ⭐ local 위치 저장
			_originalCamRot = _mainCamera.transform.localRotation;  // ⭐ local 회전 저장

			startPos = _mainCamera.transform.position;
			startRot = _mainCamera.transform.rotation;

			_mainCamera.transform.SetParent(null);

			endPos = tvCameraPoint.position;
			endRot = tvCameraPoint.rotation;
		}
		else
		{
			startPos = _mainCamera.transform.position;
			startRot = _mainCamera.transform.rotation;

			// ⭐ 부모 먼저 설정
			_mainCamera.transform.SetParent(_originalCamParent);

			// ⭐ local 좌표로 복귀 위치 계산
			endPos = _mainCamera.transform.parent.TransformPoint(_originalCamPos);
			endRot = _mainCamera.transform.parent.rotation * _originalCamRot;
		}

		float elapsed = 0f;
		while (elapsed < cameraTransitionDuration)
		{
			elapsed += Time.deltaTime;
			float t = transitionCurve.Evaluate(elapsed / cameraTransitionDuration);
			_mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
			_mainCamera.transform.rotation = Quaternion.Lerp(startRot, endRot, t);
			yield return null;
		}

		_mainCamera.transform.position = endPos;
		_mainCamera.transform.rotation = endRot;

		// ⭐ 최종적으로 local 위치/회전 정확히 복원
		if (!toTV)
		{
			_mainCamera.transform.localPosition = _originalCamPos;
			_mainCamera.transform.localRotation = _originalCamRot;
		}
	}

	// ══════════════════════════════════════
	// 공통 유틸
	// ══════════════════════════════════════
	private IEnumerator ShowNarration(string text)
	{
		Debug.Log($"[TVPlayer] ShowNarration 시작: {text}");
		Debug.Log($"  narrationPanel: {(narrationPanel != null ? "연결됨" : "NULL")}");
		Debug.Log($"  narrationText: {(narrationText != null ? "연결됨" : "NULL")}");

		if (narrationPanel == null || narrationText == null)
		{
			Debug.LogError("[TVPlayer] NarrationPanel 또는 NarrationText가 null!");
			yield break;
		}

		narrationPanel.SetActive(true);
		Debug.Log("[TVPlayer] narrationPanel 활성화됨");

		narrationText.text = "";

		// 타이핑 효과 (스킵 없음)
		foreach (char c in text)
		{
			narrationText.text += c;
			yield return new WaitForSeconds(0.04f);
		}

		// 스페이스 입력 대기
		Debug.Log("[TVPlayer] 스페이스바 입력 대기 중...");
		while (!Input.GetKeyDown(KeyCode.Space))
		{
			yield return null;
		}

		narrationPanel.SetActive(false);
		narrationText.text = "";

		Debug.Log("[TVPlayer] ShowNarration 완료");
	}

	private void SetScreenMaterial(Material mat)
	{
		if (tvScreenRenderer != null && mat != null)
			tvScreenRenderer.material = mat;
	}

	private void PlayAudio(AudioClip clip, float volume)
	{
		if (tvAudioSource != null && clip != null)
			tvAudioSource.PlayOneShot(clip, volume);
	}
}