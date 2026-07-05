using UnityEngine;
using System.Collections;

/// <summary>
/// 아귀 테이블 상호작용 — 접시 전달 → 아귀 비명 → 문 열림
///
/// [씬 배치]
/// 아귀 테이블 오브젝트에 BoxCollider(IsTrigger) + 이 스크립트 부착
///
/// [Inspector 슬롯]
/// foodPuzzle   : Stage4_ToyFoodPuzzle
/// ghoul        : Stage4_GhoulCreature
/// tableTop     : 접시가 놓일 위치 Transform
/// exitDoor     : 연출 후 열릴 Door 오브젝트 (Door.cs)
/// tableViewPoint: 접시 전달 연출 시 카메라가 바라볼 위치
/// </summary>
public class Stage4_GhoulTable : MonoBehaviour, IInteractable
{
	[Header("연결 오브젝트")]
	[SerializeField] private Stage4_ToyFoodPuzzle foodPuzzle;
	[SerializeField] private Stage4_GhoulCreature ghoul;
	[SerializeField] private Transform tableTop;
	[SerializeField] private Transform tableViewPoint;

	[Header("퇴장 문 (Door.cs 부착된 오브젝트)")]
	[Tooltip("퍼즐 완료 후 열릴 문. Door.cs의 UnlockFreeAccess()를 호출합니다.")]
	[SerializeField] private Door exitDoor;

	[Header("목각인형 획득")]
	[Tooltip("이 스테이지에서 획득할 목각인형 itemId")]
	[SerializeField] private string woodenDollItemId = "wooden_doll_4";
	[Tooltip("목각인형 이름 (인벤토리 표시용)")]
	[SerializeField] private string woodenDollName = "목각인형";
	[Tooltip("목각인형 설명")]
	[TextArea(1, 3)][SerializeField] private string woodenDollDesc = "어딘가에 사용할 수 있을 것 같다.";

	[Header("연출 설정")]
	[SerializeField] private float placeToScreamDelay = 0.6f;
	[SerializeField] private float cameraTransitionDuration = 0.8f;

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 3)][SerializeField] private string placeDialogue = "...여기 있어.";
	[TextArea(2, 3)][SerializeField] private string dollDialogue = "뭔가를 얻었다.";

	private bool _sequencePlayed = false;

	// ── IInteractable ─────────────────────────────────────────

	public string InteractionPrompt
	{
		get
		{
			if (_sequencePlayed) return "";
			if (foodPuzzle != null && foodPuzzle.IsHoldingDish)
				return "[F] 음식 가져다 놓기";
			return "";
		}
	}

	public bool CanInteract(IPlayer player)
		=> !_sequencePlayed && foodPuzzle != null && foodPuzzle.IsHoldingDish;

	public void Interact(IPlayer player)
	{
		if (!CanInteract(player)) return;
		StartCoroutine(GhoulSequence());
	}

	// ── 아귀 시퀀스 ──────────────────────────────────────────

	private IEnumerator GhoulSequence()
	{
		_sequencePlayed = true;
		GameManager.Instance?.ChangeState(GameState.Puzzle);

		Camera cam = Camera.main;
		Vector3 originalPos = cam.transform.position;
		Quaternion originalRot = cam.transform.rotation;

		// 1. 카메라를 테이블 쪽으로 전환
		if (tableViewPoint != null)
			yield return StartCoroutine(
				TransitionCamera(cam, tableViewPoint.position, tableViewPoint.rotation, cameraTransitionDuration));

		// 2. 접시 내려놓기
		foodPuzzle?.PlaceDishOnTable(tableTop);
		GameServices.UI?.ShowDialogue(speaker, placeDialogue);
		yield return new WaitForSeconds(placeToScreamDelay);

		// 3. 아귀 비명 + 화면 흔들림
		ghoul?.TriggerScream();

		// 4. 흔들림 끝날 때까지 대기
		if (ghoul != null)
		{
			float timeout = 5f;
			float waited = 0f;
			while (!ghoul.IsScreamFinished && waited < timeout)
			{
				waited += Time.deltaTime;
				yield return null;
			}
		}

		yield return new WaitForSeconds(0.3f);

		// 5. 카메라 원위치
		yield return StartCoroutine(TransitionCamera(cam, originalPos, originalRot, cameraTransitionDuration));

		// 6. 목각인형 획득
		var player = GameServices.Player;
		if (player != null && !string.IsNullOrEmpty(woodenDollItemId))
		{
			ClueRegistrar.RegisterUsableItem(player, woodenDollItemId, woodenDollName, "", woodenDollDesc);
			GameServices.UI?.ShowDialogue(speaker, dollDialogue);
		}

		yield return new WaitForSeconds(1.0f);

		// 7. 상태 복원
		GameManager.Instance?.ChangeState(GameState.Playing);

		// 8. 문 열기 (자유롭게 여닫을 수 있는 상태로 전환)
		if (exitDoor != null)
			exitDoor.UnlockFreeAccess();

		Debug.Log("[Stage4] 아귀 시퀀스 완료 — 퇴장 문 열림, 목각인형 획득");
	}

	// ── 카메라 전환 헬퍼 ─────────────────────────────────────

	private IEnumerator TransitionCamera(Camera cam, Vector3 targetPos, Quaternion targetRot, float duration)
	{
		Vector3 startPos = cam.transform.position;
		Quaternion startRot = cam.transform.rotation;
		float elapsed = 0f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
			cam.transform.position = Vector3.Lerp(startPos, targetPos, t);
			cam.transform.rotation = Quaternion.Slerp(startRot, targetRot, t);
			yield return null;
		}

		cam.transform.position = targetPos;
		cam.transform.rotation = targetRot;
	}

	// ── Trigger ───────────────────────────────────────────────

	private void OnTriggerEnter(Collider other)
	{
		if (other.TryGetComponent<Player>(out var p))
			p.SetCurrentInteractable(this);
	}

	private void OnTriggerExit(Collider other)
	{
		if (other.TryGetComponent<Player>(out var p))
			p.SetCurrentInteractable(null);
	}
}