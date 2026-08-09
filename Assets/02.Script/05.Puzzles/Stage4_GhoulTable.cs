using UnityEngine;
using System.Collections;

public class Stage4_GhoulTable : MonoBehaviour, IInteractable, ISaveableObject
{
	[Header("연결 오브젝트")]
	[SerializeField] private Stage4_ToyFoodPuzzle foodPuzzle;
	[SerializeField] private Stage4_GhoulCreature ghoul;
	[SerializeField] private Transform tableTop;
	[SerializeField] private Transform tableViewPoint;

	[Header("퇴장 문")]
	[SerializeField] private Door exitDoor;

	[Header("목각인형 획득")]
	[SerializeField] private string woodenDollItemId = "wooden_doll_4";
	[SerializeField] private string woodenDollName = "목각인형";
	[TextArea(1, 3)][SerializeField] private string woodenDollDesc = "어딘가에 사용할 수 있을 것 같다.";

	[Header("연출 설정")]
	[SerializeField] private float placeToScreamDelay = 0.6f;
	[SerializeField] private float cameraTransitionDuration = 0.8f;

	[Header("대사")]
	[SerializeField] private string speaker = "소년";
	[TextArea(2, 3)][SerializeField] private string placeDialogue = "...여기 있어.";
	[TextArea(2, 3)][SerializeField] private string dollDialogue = "뭔가를 얻었다.";

	[Header("저장 ID")]
	[SerializeField] private string saveId = "stage4_ghoul_table";

	private bool _sequencePlayed = false;

	// ── ISaveableObject ───────────────────────────────────────

	public string SaveId => saveId;

	[System.Serializable]
	private class GhoulTableState
	{
		public bool sequencePlayed;
	}

	public string SaveState()
		=> JsonUtility.ToJson(new GhoulTableState { sequencePlayed = _sequencePlayed });

	public void LoadState(string json)
	{
		if (string.IsNullOrEmpty(json)) return;
		var state = JsonUtility.FromJson<GhoulTableState>(json);
		_sequencePlayed = state.sequencePlayed;

		// ★ 시퀀스 완료 상태면 접시를 테이블 위로 복원
		if (_sequencePlayed && tableTop != null && foodPuzzle != null)
		{
			var dish = foodPuzzle.GetCompletedDishObject();
			if (dish != null)
			{
				dish.SetActive(true);
				dish.transform.SetParent(null);
				dish.transform.position = tableTop.position + Vector3.up * 0.05f;
			}
		}

		Debug.Log($"[GhoulTable] 상태 복원: sequencePlayed={_sequencePlayed}");
	}

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

		// [수정] Camera.main이 null이면(MainCamera 태그 미설정 등) 아래에서 바로 NRE가 나
		// 4스테이지가 이 시점에 멈췄습니다. null이면 카메라 연출만 건너뛰고 나머지는 진행합니다.
		Camera cam = Camera.main;
		if (cam == null)
		{
			Debug.LogWarning("[Stage4_GhoulTable] Camera.main을 찾을 수 없어 카메라 연출을 건너뜁니다.");
		}

		Vector3 originalPos = cam != null ? cam.transform.position : Vector3.zero;
		Quaternion originalRot = cam != null ? cam.transform.rotation : Quaternion.identity;

		if (cam != null && tableViewPoint != null)
			yield return StartCoroutine(
				TransitionCamera(cam, tableViewPoint.position, tableViewPoint.rotation, cameraTransitionDuration));

		foodPuzzle?.PlaceDishOnTable(tableTop);
		GameServices.UI?.ShowDialogue(speaker, placeDialogue);
		yield return new WaitForSeconds(placeToScreamDelay);

		ghoul?.TriggerScream();

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

		if (cam != null)
			yield return StartCoroutine(TransitionCamera(cam, originalPos, originalRot, cameraTransitionDuration));

		var player = GameServices.Player;
		if (player != null && !string.IsNullOrEmpty(woodenDollItemId))
		{
			ClueRegistrar.RegisterUsableItem(player, woodenDollItemId, woodenDollName, "", woodenDollDesc);
			GameServices.UI?.ShowDialogue(speaker, dollDialogue);
		}

		yield return new WaitForSeconds(1.0f);

		GameManager.Instance?.ChangeState(GameState.Playing);

		if (exitDoor != null)
			exitDoor.UnlockFreeAccess();

		Debug.Log("[Stage4] 아귀 시퀀스 완료");
	}

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