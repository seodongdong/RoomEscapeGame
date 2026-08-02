using UnityEngine;

/// <summary>
/// 3스테이지: 종이인형 크리처
/// 기획서: "처음엔 평범한 인형이였다가 뒤로 갈수록 점점 기괴하게"
/// (고개 돌아감, 눈 떨어짐, 피/먼지로 더러워짐, 옷이 찢어짐 등)
///
/// [이번 수정 — 버그 2건]
/// 1. UpdateBehavior에서 _player가 null일 때 LookAt이 NullReferenceException을
///    던지며 매 프레임 로그를 채우던 문제 → null 체크 추가.
///    Y축만 회전하도록 바꿔서 인형이 위아래로 기울어지는 것도 막았습니다.
/// 2. UpdateDollState에서 dollModel이 비어 있으면 NRE가 나던 문제 → null 체크 추가.
///
/// [Stage3_SlidingPuzzle 연동]
/// 그림을 하나 완성할 때마다 NextState()가 호출되어 인형이 점점 기괴해집니다.
/// SlidingPuzzle의 dollCreature 슬롯에 이 컴포넌트를 연결하세요.
///
/// 파일 위치는 기존과 동일하게 02.Script/10.Enemy/ 아래에 덮어써 주세요.
/// </summary>
public class Stage3_DollCreature : CreatureBase
{
	[System.Serializable]
	public class DollState
	{
		public GameObject dollModel;
		[TextArea(1, 2)]
		public string description;
	}

	[Header("Doll States")]
	[Tooltip("그림 개수만큼 준비하세요. 인덱스가 커질수록 기괴한 모델.")]
	[SerializeField] private DollState[] dollStates;

	[Header("응시 설정")]
	[SerializeField] private bool lookAtPlayer = true;

	private int _currentStateIndex = 0;

	public int CurrentStateIndex => _currentStateIndex;

	protected override void Start()
	{
		base.Start();
		UpdateDollState(0);
	}

	protected override void UpdateBehavior()
	{
		// 가만히 앉아서 플레이어를 바라봅니다.
		if (!lookAtPlayer || _player == null) return;

		Vector3 lookDir = _player.transform.position - transform.position;
		lookDir.y = 0f; // Y축 회전만 (인형이 앞뒤로 기울지 않도록)
		if (lookDir.sqrMagnitude > 0.001f)
			transform.rotation = Quaternion.LookRotation(lookDir);
	}

	/// <summary>다음 단계 모델로 전환. 슬라이딩 퍼즐의 그림 하나 완성마다 호출됩니다.</summary>
	public void NextState()
	{
		if (dollStates == null || dollStates.Length == 0) return;

		_currentStateIndex = Mathf.Min(_currentStateIndex + 1, dollStates.Length - 1);
		UpdateDollState(_currentStateIndex);
	}

	/// <summary>저장 복원용 — 특정 단계로 바로 설정합니다.</summary>
	public void SetState(int index)
	{
		if (dollStates == null || dollStates.Length == 0) return;
		_currentStateIndex = Mathf.Clamp(index, 0, dollStates.Length - 1);
		UpdateDollState(_currentStateIndex);
	}

	private void UpdateDollState(int index)
	{
		if (dollStates == null || dollStates.Length == 0) return;

		foreach (var state in dollStates)
		{
			if (state?.dollModel != null)
				state.dollModel.SetActive(false);
		}

		if (index < 0 || index >= dollStates.Length) return;

		var current = dollStates[index];
		if (current?.dollModel != null)
		{
			current.dollModel.SetActive(true);
			Debug.Log($"[PaperDoll] 단계 {index}: {current.description}");
		}
	}
}