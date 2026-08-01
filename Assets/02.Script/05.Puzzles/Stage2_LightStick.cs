using UnityEngine;
using System.Collections;

/// <summary>
/// 2스테이지 기억 퍼즐의 "각목" 컴포넌트.
///
/// [두 가지 역할 — role로 구분]
/// Map        : 방석 위에 랜덤 배치되어 순서대로 깜빡임. 클릭 불가.
/// PuzzleInput: 퍼즐 화면에 일렬로 배치. 색만 표시되고 깜빡이지 않음. 클릭으로 입력.
///
/// 두 역할 모두 colorId로 짝을 이룹니다. 맵에서 본 색 순서를
/// 퍼즐 화면에서 같은 색 순서로 클릭하면 정답입니다.
///
/// [씬 설정]
/// - 각목 모델에 이 스크립트 + Collider 부착
/// - bodyRenderer: 각목 몸통 렌더러 (색이 칠해질 부분)
/// - blinkLight  : 깜빡일 Light (Map 역할일 때만 필요, 없어도 됨)
/// - Map 역할 각목과 PuzzleInput 역할 각목은 별개의 오브젝트입니다.
///   (맵 = 방석 위 / 퍼즐 = 관 앞 일렬)
/// </summary>
public class Stage2_LightStick : MonoBehaviour
{
	public enum Role { Map, PuzzleInput }

	[Header("역할")]
	[SerializeField] private Role role = Role.Map;

	[Header("시각 요소")]
	[Tooltip("각목 몸통 렌더러. 여기에 색이 칠해집니다.")]
	[SerializeField] private Renderer bodyRenderer;
	[Tooltip("깜빡임용 Light. 없으면 emissive만 사용합니다.")]
	[SerializeField] private Light blinkLight;
	[Tooltip("깜빡일 때 emissive 강도")]
	[SerializeField] private float emissionIntensity = 2.5f;
	[Tooltip("Light 사용 시 켜질 때 밝기")]
	[SerializeField] private float lightIntensity = 3f;

	[Header("효과음")]
	[SerializeField] private string blinkSFX = "";
	[SerializeField] private string clickSFX = "puzzle_click";

	// ── 런타임 ────────────────────────────────────────────────
	private int _colorId = -1;
	private Color _color = Color.white;
	private bool _clickEnabled = false;
	private Stage2_LightSequencePuzzle _puzzle;
	private MaterialPropertyBlock _mpb;

	private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
	private static readonly int LegacyColorId = Shader.PropertyToID("_Color");
	private static readonly int EmissionColorId = Shader.PropertyToID("_EmissionColor");

	public int ColorId => _colorId;

	// ── 초기화 ────────────────────────────────────────────────

	private void Awake()
	{
		_mpb = new MaterialPropertyBlock();
		if (bodyRenderer == null) bodyRenderer = GetComponentInChildren<Renderer>();
		if (blinkLight != null) blinkLight.enabled = false;
	}

	/// <summary>퍼즐이 단계 시작 시 호출. 색과 ID를 주입합니다.</summary>
	public void Setup(Stage2_LightSequencePuzzle puzzle, int colorId, Color color)
	{
		_puzzle = puzzle;
		_colorId = colorId;
		_color = color;

		ApplyColor(color, false);

		if (blinkLight != null)
		{
			blinkLight.color = color;
			blinkLight.enabled = false;
		}
	}

	/// <summary>퍼즐 화면 각목의 클릭 입력 허용 여부.</summary>
	public void SetClickEnabled(bool enabled)
	{
		_clickEnabled = enabled && role == Role.PuzzleInput;
	}

	// ── 깜빡임 (Map 역할) ─────────────────────────────────────

	/// <summary>지정 시간 동안 한 번 밝게 빛납니다.</summary>
	public void Blink(float duration)
	{
		if (role != Role.Map) return;
		StopAllCoroutines();
		StartCoroutine(BlinkRoutine(duration));
	}

	private IEnumerator BlinkRoutine(float duration)
	{
		SetGlow(true);
		if (!string.IsNullOrEmpty(blinkSFX))
			GameServices.Audio?.PlaySFX(blinkSFX);

		yield return new WaitForSeconds(duration);

		SetGlow(false);
	}

	/// <summary>불빛 완전 정지 (3단계 클리어 후).</summary>
	public void StopBlinking()
	{
		StopAllCoroutines();
		SetGlow(false);
	}

	private void SetGlow(bool on)
	{
		if (blinkLight != null)
		{
			blinkLight.enabled = on;
			blinkLight.intensity = lightIntensity;
		}
		ApplyColor(_color, on);
	}

	private void ApplyColor(Color color, bool emissive)
	{
		if (bodyRenderer == null) return;

		bodyRenderer.GetPropertyBlock(_mpb);
		_mpb.SetColor(BaseColorId, color);
		_mpb.SetColor(LegacyColorId, color);
		_mpb.SetColor(EmissionColorId, emissive ? color * emissionIntensity : Color.black);
		bodyRenderer.SetPropertyBlock(_mpb);
	}

	// ── 클릭 입력 (PuzzleInput 역할) ─────────────────────────

	private void OnMouseDown()
	{
		if (!_clickEnabled) return;
		if (GameManager.Instance != null &&
			GameManager.Instance.CurrentState != GameState.Puzzle) return;

		if (!string.IsNullOrEmpty(clickSFX))
			GameServices.Audio?.PlaySFX(clickSFX);

		StartCoroutine(ClickFeedback());
		_puzzle?.OnStickClicked(this);
	}

	private IEnumerator ClickFeedback()
	{
		// 눌린 느낌만 짧게 — 정오답 정보는 주지 않습니다 (기획서: 입력 도중 판정 없음)
		Vector3 origin = transform.localPosition;
		Vector3 pressed = origin + Vector3.down * 0.03f;

		float t = 0f;
		while (t < 0.08f)
		{
			t += Time.unscaledDeltaTime;
			transform.localPosition = Vector3.Lerp(origin, pressed, t / 0.08f);
			yield return null;
		}
		t = 0f;
		while (t < 0.08f)
		{
			t += Time.unscaledDeltaTime;
			transform.localPosition = Vector3.Lerp(pressed, origin, t / 0.08f);
			yield return null;
		}
		transform.localPosition = origin;
	}
}