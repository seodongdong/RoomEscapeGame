using UnityEngine;
using System.Collections;

/// <summary>
/// 힌트 시스템
/// 소녀가 단서 위치를 가리킴
/// </summary>
public class HintSystem : MonoBehaviour
{
	[System.Serializable]
	public class Hint
	{
		public string hintId;
		[TextArea(2, 5)]
		public string hintText;
		public Transform hintLocation; // 힌트 대상 위치
		public bool isAutoTrigger;
		public float autoTriggerDelay = 10f;
	}

	[Header("References")]
	[SerializeField] private Girl girlReference;

	[Header("Hints")]
	[SerializeField] private Hint[] hints;

	[Header("Settings")]
	[SerializeField] private float girlAppearDuration = 2f;
	[SerializeField] private bool enableAutoHints = true;

	private void Start()
	{
		if (enableAutoHints)
		{
			StartAutoHints();
		}
	}

	private void StartAutoHints()
	{
		foreach (var hint in hints)
		{
			if (hint.isAutoTrigger)
			{
				StartCoroutine(AutoTriggerHint(hint));
			}
		}
	}

	private IEnumerator AutoTriggerHint(Hint hint)
	{
		yield return new WaitForSeconds(hint.autoTriggerDelay);

		ShowHint(hint.hintId);
	}

	/// <summary>
	/// 힌트 표시
	/// </summary>
	public void ShowHint(string hintId)
	{
		var hint = System.Array.Find(hints, h => h.hintId == hintId);

		if (hint == null)
		{
			Debug.LogWarning($"[HintSystem] 힌트를 찾을 수 없음: {hintId}");
			return;
		}

		StartCoroutine(ShowHintSequence(hint));
	}

	private IEnumerator ShowHintSequence(Hint hint)
	{
		// 대사 표시
		var uiManager = FindAnyObjectByType<UIManager>();
		if (!string.IsNullOrEmpty(hint.hintText))
		{
			uiManager?.ShowDialogue("", hint.hintText);
		}

		// 소녀가 힌트 위치 가리키기
		if (girlReference != null && hint.hintLocation != null)
		{
			girlReference.gameObject.SetActive(true);
			girlReference.transform.position = hint.hintLocation.position + Vector3.up * 2f;
			girlReference.transform.LookAt(hint.hintLocation);

			// 일정 시간 후 사라짐
			yield return new WaitForSeconds(girlAppearDuration);
			girlReference.gameObject.SetActive(false);
		}
	}

	/// <summary>
	/// 인덱스로 힌트 표시
	/// </summary>
	public void ShowHintByIndex(int index)
	{
		if (index >= 0 && index < hints.Length)
		{
			ShowHint(hints[index].hintId);
		}
	}

	/// <summary>
	/// 다음 힌트 표시
	/// </summary>
	private int _currentHintIndex = 0;
	public void ShowNextHint()
	{
		if (_currentHintIndex < hints.Length)
		{
			ShowHint(hints[_currentHintIndex].hintId);
			_currentHintIndex++;
		}
	}

	/// <summary>
	/// 힌트 리셋
	/// </summary>
	public void ResetHints()
	{
		_currentHintIndex = 0;
		StopAllCoroutines();
	}

#if UNITY_EDITOR
	private void OnDrawGizmos()
	{
		if (hints == null) return;

		Gizmos.color = Color.cyan;
		foreach (var hint in hints)
		{
			if (hint.hintLocation != null)
			{
				Gizmos.DrawWireSphere(hint.hintLocation.position, 0.5f);
				Gizmos.DrawLine(hint.hintLocation.position, hint.hintLocation.position + Vector3.up * 2f);
			}
		}
	}
#endif
}