using UnityEngine;
using System.Collections;

public class Stage4_GhoulCreature : CreatureBase
{
	[Header("Ghoul Settings")]
	[SerializeField] private Transform tablePosition;
	[SerializeField] private Animator animator;

	[Header("화면 흔들림 설정")]
	[SerializeField] private float shakeDuration = 1.2f;
	[SerializeField] private float shakeMagnitude = 0.08f;
	[SerializeField] private float shakeFrequency = 25f;

	private bool _hasScreamed = false;
	public bool IsScreamFinished { get; private set; } = false;

	protected override void Start()
	{
		base.Start();
		if (tablePosition != null)
			transform.position = tablePosition.position;
	}

	protected override void UpdateBehavior()
	{
		if (_player != null)
			transform.LookAt(_player.transform);
	}

	public void TriggerScream()
	{
		if (_hasScreamed) return;
		_hasScreamed = true;
		IsScreamFinished = false;

		GameServices.Audio?.PlaySFX("ghoul_scream");
		animator?.SetTrigger("Scream");

		StartCoroutine(ShakeCameraAndFinish());
		Debug.Log("[Ghoul] 비명!!!");
	}

	private IEnumerator ShakeCameraAndFinish()
	{
		Camera cam = Camera.main;
		if (cam == null) { IsScreamFinished = true; yield break; }

		Vector3 originalPos = cam.transform.localPosition;
		float elapsed = 0f;

		while (elapsed < shakeDuration)
		{
			elapsed += Time.deltaTime;
			float strength = shakeMagnitude * (1f - elapsed / shakeDuration);
			float x = Mathf.Sin(elapsed * shakeFrequency) * strength;
			float y = Mathf.Cos(elapsed * shakeFrequency * 0.7f) * strength;
			cam.transform.localPosition = originalPos + new Vector3(x, y, 0f);
			yield return null;
		}

		cam.transform.localPosition = originalPos;
		IsScreamFinished = true;
		Debug.Log("[Ghoul] 화면 흔들림 종료");
	}
}