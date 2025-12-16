using UnityEngine;

public abstract class CreatureBase : MonoBehaviour
{
	[Header("Creature Settings")]
	[SerializeField] protected int stageNumber;
	[SerializeField] protected float moveSpeed = 2f;
	[SerializeField] protected bool canJumpscare = true;

	[Header("Hint Settings")]
	[SerializeField] protected bool providesHint;
	[SerializeField] protected Transform hintTarget; // 단서 위치

	[Header("Detection")]
	[SerializeField] protected float detectionRange = 10f;
	[SerializeField] protected float jumpscareRange = 2f;

	protected Player _player;
	protected bool _hasJumpscared;
	protected bool _isActive = true;

	protected virtual void Start()
	{
		_player = FindAnyObjectByType<Player>();
	}

	protected virtual void Update()
	{
		if (!_isActive || _player == null) return;

		float distanceToPlayer = Vector3.Distance(transform.position, _player.transform.position);

		// 점프스케어 범위
		if (distanceToPlayer < jumpscareRange && !_hasJumpscared)
		{
			TriggerJumpscare();
		}
		// 감지 범위 내
		else if (distanceToPlayer < detectionRange)
		{
			UpdateBehavior();
		}

		// 힌트 제공
		if (providesHint && hintTarget != null)
		{
			ProvideHint();
		}
	}

	// 각 크리처별 고유 행동 (하위 클래스에서 구현)
	protected abstract void UpdateBehavior();

	// 단서 위치 힌트 제공
	protected virtual void ProvideHint()
	{
		// 단서 방향으로 이동하거나 바라보기
		Vector3 directionToHint = (hintTarget.position - transform.position).normalized;
		transform.forward = Vector3.Lerp(transform.forward, directionToHint, Time.deltaTime);
	}

	// 점프스케어
	protected virtual void TriggerJumpscare()
	{
		if (!canJumpscare || _hasJumpscared) return;

		_hasJumpscared = true;

		// 효과음 재생
		var audioManager = FindAnyObjectByType<AudioManager>();
		audioManager?.PlaySFX("jumpscare");

		Debug.Log($"[{gameObject.name}] 점프스케어!");

		// 잠시 후 사라지기
		StartCoroutine(DisappearAfterJumpscare());
	}

	private System.Collections.IEnumerator DisappearAfterJumpscare()
	{
		yield return new WaitForSeconds(1f);
		_isActive = false;
		gameObject.SetActive(false);
	}

	// 크리처 활성화/비활성화
	public void SetActive(bool active)
	{
		_isActive = active;
		gameObject.SetActive(active);
	}
}