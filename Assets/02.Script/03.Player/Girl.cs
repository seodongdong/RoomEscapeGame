using UnityEngine;
using System.Collections;

public class Girl : MonoBehaviour
{
	[Header("Dialogue")]
	[SerializeField] private string speaker = "소녀";
	[TextArea(2, 5)]
	[SerializeField] private string firstMeetingDialogue = "그 문은 안 열려.";
	[TextArea(2, 5)]
	[SerializeField] private string pleaDialogue = "나를 구해줘...";

	[Header("Stage Hints")]
	[SerializeField] private Transform[] hintTargets; // 스테이지별 힌트 위치

	[Header("Follow Settings")]
	[SerializeField] private bool shouldFollow;
	[SerializeField] private float followDistance = 2f;
	[SerializeField] private float followSpeed = 3f;

	[Header("Appearance")]
	[SerializeField] private float appearDuration = 2f;

	private Player _player;
	private IUIManager _uiManager;
	private bool _hasMetPlayer;

	private void Start()
	{
		_player = FindAnyObjectByType<Player>();
		_uiManager = FindAnyObjectByType<UIManager>();

		// 처음엔 비활성화
		gameObject.SetActive(false);
	}

	private void Update()
	{
		if (shouldFollow && _player != null)
		{
			FollowPlayer();
		}
	}

	// 첫 만남 (인트로에서 호출)
	public void FirstMeeting()
	{
		if (_hasMetPlayer) return;

		_hasMetPlayer = true;
		StartCoroutine(FirstMeetingSequence());
	}

	private IEnumerator FirstMeetingSequence()
	{
		// 소녀 등장
		gameObject.SetActive(true);

		// 첫 대사: "그 문은 안 열려."
		_uiManager?.ShowDialogue(speaker, firstMeetingDialogue);
		yield return new WaitForSeconds(3f);

		// 두 번째 대사: "나를 구해줘..."
		_uiManager?.ShowDialogue(speaker, pleaDialogue);
		yield return new WaitForSeconds(3f);

		// 사라짐
		gameObject.SetActive(false);
	}

	// 스테이지별 힌트 제공
	public void ShowHint(int stageIndex)
	{
		if (hintTargets == null || stageIndex >= hintTargets.Length) return;

		Transform target = hintTargets[stageIndex];
		if (target != null)
		{
			gameObject.SetActive(true);
			transform.LookAt(target);

			StartCoroutine(HintSequence());
		}
	}

	private IEnumerator HintSequence()
	{
		yield return new WaitForSeconds(appearDuration);
		gameObject.SetActive(false);
	}

	// 추격전 시 플레이어 따라다니기
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

	// 추격전 시작 시 호출
	public void StartFollowing()
	{
		shouldFollow = true;
		gameObject.SetActive(true);
	}

	// 추격전 종료 시 호출
	public void StopFollowing()
	{
		shouldFollow = false;
	}
}