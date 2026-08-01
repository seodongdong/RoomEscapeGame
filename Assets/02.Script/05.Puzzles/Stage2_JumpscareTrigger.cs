using UnityEngine;
using System.Collections;

/// <summary>
/// 2스테이지: 점프스케어 트리거 (선택 연출)
///
/// [이번 수정 — 필수]
/// puzzleReference의 타입이 Stage2_AltarCandyPuzzle로 고정되어 있어서,
/// 2스테이지 퍼즐을 Stage2_LightSequencePuzzle로 교체하면 Inspector에서
/// 연결이 끊기고 트리거가 영원히 발동하지 않게 됩니다.
/// MonoBehaviour + IPuzzle 방식으로 바꿔 어떤 퍼즐이든 연결할 수 있게 했습니다.
/// (씬의 다른 스크립트들이 쓰는 것과 동일한 패턴입니다.)
///
/// [동작]
/// 퍼즐 해결 후 플레이어가 이 트리거에 진입하면 방석 위에 인영이 잠깐
/// 나타나고 박수 SFX가 재생됩니다. 1회만 발동합니다.
///
/// [기획서 v2 기준 참고]
/// v2 기획서의 2스테이지 연출에는 이 점프스케어가 명시되어 있지 않습니다.
/// 쓰지 않을 거라면 씬에서 이 오브젝트를 비활성화하면 되고,
/// 스크립트 자체는 지우지 않아도 다른 곳에 영향을 주지 않습니다.
/// </summary>
public class Stage2_JumpscareTrigger : MonoBehaviour
{
	[Header("퍼즐 연결 (해결 후에만 발동)")]
	[Tooltip("IPuzzle을 구현한 퍼즐 컴포넌트. Stage2_LightSequencePuzzle 등.")]
	[SerializeField] private MonoBehaviour puzzleObject;

	[Header("점프스케어 오브젝트")]
	[SerializeField] private GameObject jumpscareCreatureVisual;
	[SerializeField] private float visibleDuration = 1.5f;

	[Header("오디오")]
	[SerializeField] private string jumpscareSFX = "applause_loud";

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 4)]
	[SerializeField] private string jumpscareDialogue = "!!";

	private IPuzzle _puzzle;
	private bool _hasTriggered = false;

	private void Awake()
	{
		_puzzle = puzzleObject as IPuzzle;

		if (_puzzle == null && puzzleObject != null)
			Debug.LogError($"[Stage2 Jumpscare] {puzzleObject.name}은 IPuzzle을 구현하지 않습니다!");

		if (jumpscareCreatureVisual != null)
			jumpscareCreatureVisual.SetActive(false);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (_hasTriggered) return;
		if (!other.CompareTag("Player")) return;
		if (_puzzle == null || !_puzzle.IsSolved) return;

		_hasTriggered = true;
		StartCoroutine(PlayJumpscare());
	}

	private IEnumerator PlayJumpscare()
	{
		if (jumpscareCreatureVisual != null)
			jumpscareCreatureVisual.SetActive(true);

		GameServices.Audio?.PlaySFX(jumpscareSFX);
		GameServices.UI?.ShowDialogue(speaker, jumpscareDialogue);

		Debug.Log("[Stage2 Jumpscare] 방석 인영 등장!");

		yield return new WaitForSeconds(visibleDuration);

		if (jumpscareCreatureVisual != null)
			jumpscareCreatureVisual.SetActive(false);
	}
}