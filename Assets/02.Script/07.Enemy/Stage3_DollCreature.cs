using UnityEngine;

/// <summary>
/// 3스테이지: 종이인형 크리처
/// 기획서: "처음엔 평범한 인형이였다가 뒤로 갈수록 점점 기괴하게"
/// (고개 돌아감, 눈 떨어짐, 피/먼지로 더러워짐, 옷이 찢어짐 등)
/// </summary>
public class Stage3_DollCreature : CreatureBase
{
	[System.Serializable]
	public class DollState
	{
		public GameObject dollModel;
		public string description;
	}

	[Header("Doll States")]
	[SerializeField] private DollState[] dollStates; // 4개 (각 퍼즐마다)

	private int _currentStateIndex = 0;

	protected override void Start()
	{
		base.Start();
		UpdateDollState(0);
	}

	protected override void UpdateBehavior()
	{
		// 가만히 앉아있음
		transform.LookAt(_player.transform);
	}

	public void NextState()
	{
		_currentStateIndex++;
		if (_currentStateIndex < dollStates.Length)
		{
			UpdateDollState(_currentStateIndex);
		}
	}

	private void UpdateDollState(int index)
	{
		// 모든 상태 비활성화
		foreach (var state in dollStates)
		{
			state.dollModel.SetActive(false);
		}

		// 현재 상태만 활성화
		if (index < dollStates.Length)
		{
			dollStates[index].dollModel.SetActive(true);
			Debug.Log($"[PaperDoll] {dollStates[index].description}");
		}
	}
}