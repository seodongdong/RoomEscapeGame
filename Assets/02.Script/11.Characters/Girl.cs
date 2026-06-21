using UnityEngine;
using System.Collections;

/// <summary>
/// 소녀 캐릭터
/// 기획서: 대문 앞 등장 → "그 문은 안 열려." → "나를 구해줘…" → 사라짐
/// </summary>
public class Girl : MonoBehaviour
{
	[Header("첫 등장 대사")]
	[SerializeField] private string speaker = "소녀";
	[TextArea(2, 5)]
	[SerializeField] private string dialogue1 = "그 문은 안 열려.";
	[TextArea(2, 5)]
	[SerializeField] private string dialogue2 = "나를 구해줘…";

	[Header("등장 위치")]
	[SerializeField] private Transform appearPosition;  // 대문 앞 위치

	[Header("연출 타이밍")]
	[SerializeField] private float dialogue1Duration = 3f;   // 첫 대사 유지 시간
	[SerializeField] private float betweenDelay = 1f;        // 대사 사이 간격
	[SerializeField] private float dialogue2Duration = 3f;   // 두 번째 대사 유지 시간
	[SerializeField] private float disappearDelay = 0.5f;    // 사라지기 전 대기

	[Header("페이드 연출 (선택)")]
	[SerializeField] private bool useFade = true;
	[SerializeField] private float fadeDuration = 0.8f;

	[Header("5스테이지 추격전 설정")]
	[SerializeField] private bool shouldFollow = false;
	[SerializeField] private float followDistance = 2f;
	[SerializeField] private float followSpeed = 3f;

	private Player _player;
	private IUIManager _uiManager;
	private bool _hasMetPlayer = false;
	private Renderer[] _renderers;
	public bool IsRescued { get; private set; } = false;

	private void Awake()
	{
		// Start()에 있던 것을 Awake()로 이동
		_renderers = GetComponentsInChildren<Renderer>(true); // true = 비활성 자식도 포함
	}

	private void Start()
	{
		_player = GameServices.Player;
		_uiManager = GameServices.UI;

		gameObject.SetActive(false);
	}

	private void Update()
	{
		if (shouldFollow && _player != null)
			FollowPlayer();
	}

	// ── 첫 만남 ───────────────────────────────────
	public void FirstMeeting()
	{
		Debug.Log("[Girl] FirstMeeting 호출됨");
		if (_hasMetPlayer)
		{
			Debug.Log("[Girl] 이미 만남 처리됨 - return");
			return;
		}
		_hasMetPlayer = true;

		gameObject.SetActive(true);
		Debug.Log("[Girl] SetActive(true) 완료");

		if (useFade) SetAlpha(0f);
		Debug.Log($"[Girl] useFade={useFade}, appearPosition={appearPosition}");

		StartCoroutine(FirstMeetingSequence());
		Debug.Log("[Girl] 코루틴 시작됨");
	}

	private IEnumerator FirstMeetingSequence()
	{
		Debug.Log("[Girl] FirstMeetingSequence 시작");

		if (appearPosition != null)
		{
			transform.position = appearPosition.position;
			Debug.Log($"[Girl] 위치 이동: {appearPosition.position}");
		}
		else
			Debug.LogWarning("[Girl] appearPosition이 null!");

		if (useFade)
		{
			Debug.Log("[Girl] FadeIn 시작");
			yield return StartCoroutine(FadeIn());
			Debug.Log("[Girl] FadeIn 완료");
		}
		else
			gameObject.SetActive(true);

		Debug.Log("[Girl] 첫 번째 대사 출력");
		_uiManager?.ShowDialogue(speaker, dialogue1);
		yield return new WaitForSeconds(dialogue1Duration);
		_uiManager?.HideDialogue();

		yield return new WaitForSeconds(betweenDelay);

		Debug.Log("[Girl] 두 번째 대사 출력");
		_uiManager?.ShowDialogue(speaker, dialogue2);
		yield return new WaitForSeconds(dialogue2Duration);
		_uiManager?.HideDialogue();

		yield return new WaitForSeconds(disappearDelay);

		Debug.Log("[Girl] 사라짐 시작");
		if (useFade)
			yield return StartCoroutine(FadeOut());
		else
			gameObject.SetActive(false);
	}

	// ── 페이드 인/아웃 ────────────────────────────
	private IEnumerator FadeIn()
	{

		// 투명하게 시작
		SetAlpha(0f);

		float elapsed = 0f;
		while (elapsed < fadeDuration)
		{
			elapsed += Time.deltaTime;
			SetAlpha(Mathf.Clamp01(elapsed / fadeDuration));
			yield return null;
		}
		SetAlpha(1f);
	}

	private IEnumerator FadeOut()
	{
		float elapsed = 0f;
		while (elapsed < fadeDuration)
		{
			elapsed += Time.deltaTime;
			SetAlpha(Mathf.Clamp01(1f - (elapsed / fadeDuration)));
			yield return null;
		}

		SetAlpha(0f);
		gameObject.SetActive(false);
	}

	private void SetAlpha(float alpha)
	{
		if (_renderers == null || _renderers.Length == 0)
		{
			Debug.LogWarning("[Girl] Renderer가 없습니다!");
			return;
		}

		foreach (var r in _renderers)
		{
			if (r == null) continue;
			foreach (var mat in r.materials)
			{
				if (mat.HasProperty("_BaseColor"))
				{
					var col = mat.GetColor("_BaseColor");
					col.a = alpha;
					mat.SetColor("_BaseColor", col);
				}
				else if (mat.HasProperty("_Color"))
				{
					var col = mat.GetColor("_Color");
					col.a = alpha;
					mat.SetColor("_Color", col);
				}
			}
		}
	}

	// ── 5스테이지 추격전 따라다니기 ──────────────
	public void StartFollowing()
	{
		IsRescued = true;
		shouldFollow = true;
		gameObject.SetActive(true);
	}

	private void FollowPlayer()
	{
		float distance = Vector3.Distance(transform.position, _player.transform.position);
		if (distance > followDistance)
		{
			Vector3 direction = (_player.transform.position - transform.position).normalized;
			transform.position += direction * followSpeed * Time.deltaTime;
			transform.LookAt(_player.transform);
		}
	}

	public void StopFollowing()
	{
		shouldFollow = false;
	}
}