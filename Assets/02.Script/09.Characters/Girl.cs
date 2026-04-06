using UnityEngine;
using System.Collections;

/// <summary>
/// 소녀 캐릭터 (누나)
/// 기획서: 첫 만남 대사, 추격전 시 플레이어 따라다니기
/// </summary>
public class Girl : MonoBehaviour
{
	[Header("Dialogue")]
	[SerializeField] private string speaker = "소녀";
	[TextArea(2, 5)]
	[SerializeField] private string firstMeetingDialogue = "그 문은 안 열려.";
	[TextArea(2, 5)]
	[SerializeField] private string pleaDialogue = "나를 구해줘...";

	[Header("Follow Settings")]
	[SerializeField] private bool shouldFollow;
	[SerializeField] private float followDistance = 2f;
	[SerializeField] private float followSpeed = 3f;

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

	#region First Meeting

	/// <summary>
	/// 첫 만남 (인트로에서 호출)
	/// 기획서: "그 문은 안 열려." → "나를 구해줘..."
	/// </summary>
	public void FirstMeeting()
	{
		if (_hasMetPlayer) return;

		_hasMetPlayer = true;
		StartCoroutine(FirstMeetingSequence());
	}

	private IEnumerator FirstMeetingSequence()
	{
		gameObject.SetActive(true);

		// 첫 대사
		_uiManager?.ShowDialogue(speaker, firstMeetingDialogue);
		yield return new WaitForSeconds(3f);

		// 두 번째 대사
		_uiManager?.ShowDialogue(speaker, pleaDialogue);
		yield return new WaitForSeconds(3f);

		// 사라짐
		gameObject.SetActive(false);
	}

	#endregion

	#region Follow Player

	/// <summary>
	/// 플레이어 따라다니기 (5스테이지 추격전)
	/// </summary>
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

	/// <summary>
	/// 추격전 시작 시 호출
	/// 기획서: "누나...! 같이 가자!"
	/// </summary>
	public void StartFollowing()
	{
		shouldFollow = true;
		gameObject.SetActive(true);

		var uiManager = FindAnyObjectByType<UIManager>();
		uiManager?.ShowDialogue("소년", "누나...! 같이 가자!");
	}

	/// <summary>
	/// 추격전 종료 시 호출
	/// </summary>
	public void StopFollowing()
	{
		shouldFollow = false;
	}

	#endregion
}