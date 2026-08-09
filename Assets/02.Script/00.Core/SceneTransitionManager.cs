using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 씬 전환 관리
/// 페이드 효과 포함
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
	private static SceneTransitionManager _instance;
	public static SceneTransitionManager Instance => _instance;

	[Header("Fade Settings")]
	[SerializeField] private Image fadeImage;
	[SerializeField] private float fadeDuration = 1f;

	[Header("Loading")]
	[SerializeField] private GameObject loadingPanel;
	[SerializeField] private Slider loadingBar;

	private void Awake()
	{
		if (_instance != null && _instance != this)
		{
			Destroy(gameObject);
			return;
		}

		_instance = this;
		DontDestroyOnLoad(gameObject);

		// [수정] fadeImage가 씬에 배치된 Canvas를 참조할 경우, 이 매니저(DontDestroyOnLoad)와
		// 별개로 파괴되어 두 번째 씬 전환부터 fadeImage == null이 되는 문제가 있었음.
		// → Fade용 Canvas를 이 매니저의 자식으로 만들어 함께 영속되도록 보장한다.
		EnsurePersistentFadeCanvas();
	}

	/// <summary>
	/// Fade용 Canvas/Image가 씬 전환에도 파괴되지 않도록 보장한다.
	/// - Inspector에 fadeImage가 미리 연결돼 있으면: 그 Canvas를 이 매니저의 자식으로 재부모화
	/// - 연결돼 있지 않으면: 런타임에 새로 생성
	/// </summary>
	private void EnsurePersistentFadeCanvas()
	{
		if (fadeImage != null)
		{
			Transform canvasRoot = fadeImage.canvas != null ? fadeImage.canvas.transform : fadeImage.transform;
			canvasRoot.SetParent(transform, worldPositionStays: false);
			fadeImage.gameObject.SetActive(false);
			return;
		}

		GameObject canvasGO = new GameObject("FadeCanvas_Runtime");
		canvasGO.transform.SetParent(transform, false);

		Canvas canvas = canvasGO.AddComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = 999;

		CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
		scaler.referenceResolution = new Vector2(1920, 1080);

		canvasGO.AddComponent<GraphicRaycaster>();

		GameObject imageGO = new GameObject("FadeImage_Runtime");
		imageGO.transform.SetParent(canvasGO.transform, false);

		RectTransform rt = imageGO.AddComponent<RectTransform>();
		rt.anchorMin = Vector2.zero;
		rt.anchorMax = Vector2.one;
		rt.offsetMin = Vector2.zero;
		rt.offsetMax = Vector2.zero;

		fadeImage = imageGO.AddComponent<Image>();
		fadeImage.color = new Color(0f, 0f, 0f, 0f);
		fadeImage.raycastTarget = true;
		imageGO.SetActive(false);
	}

	/// <summary>
	/// 씬 전환 (페이드 효과)
	/// </summary>
	public void LoadScene(string sceneName)
	{
		StartCoroutine(LoadSceneCoroutine(sceneName));
	}

	/// <summary>
	/// 씬 전환 (인덱스)
	/// </summary>
	public void LoadScene(int sceneIndex)
	{
		StartCoroutine(LoadSceneCoroutine(sceneIndex));
	}

	private IEnumerator LoadSceneCoroutine(string sceneName)
	{
		// 페이드 아웃
		yield return StartCoroutine(FadeOut());

		// 로딩 패널 표시
		if (loadingPanel != null)
		{
			loadingPanel.SetActive(true);
		}

		// 씬 로드
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
		asyncLoad.allowSceneActivation = false;

		// 로딩 바 업데이트
		while (!asyncLoad.isDone)
		{
			float progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);

			if (loadingBar != null)
			{
				loadingBar.value = progress;
			}

			if (asyncLoad.progress >= 0.9f)
			{
				asyncLoad.allowSceneActivation = true;
			}

			yield return null;
		}

		// 로딩 패널 숨김
		if (loadingPanel != null)
		{
			loadingPanel.SetActive(false);
		}

		// 페이드 인
		yield return StartCoroutine(FadeIn());
	}

	private IEnumerator LoadSceneCoroutine(int sceneIndex)
	{
		yield return StartCoroutine(FadeOut());

		SceneManager.LoadScene(sceneIndex);

		yield return StartCoroutine(FadeIn());
	}

	private IEnumerator FadeOut()
	{
		if (fadeImage == null) yield break;

		fadeImage.gameObject.SetActive(true);
		float elapsed = 0f;
		Color color = fadeImage.color;

		while (elapsed < fadeDuration)
		{
			elapsed += Time.deltaTime;
			color.a = elapsed / fadeDuration;
			fadeImage.color = color;
			yield return null;
		}

		color.a = 1f;
		fadeImage.color = color;
	}

	private IEnumerator FadeIn()
	{
		if (fadeImage == null) yield break;

		float elapsed = 0f;
		Color color = fadeImage.color;

		while (elapsed < fadeDuration)
		{
			elapsed += Time.deltaTime;
			color.a = 1f - (elapsed / fadeDuration);
			fadeImage.color = color;
			yield return null;
		}

		color.a = 0f;
		fadeImage.color = color;
		fadeImage.gameObject.SetActive(false);
	}

	/// <summary>
	/// 즉시 씬 전환 (페이드 없음)
	/// </summary>
	public void LoadSceneImmediate(string sceneName)
	{
		SceneManager.LoadScene(sceneName);
	}
}