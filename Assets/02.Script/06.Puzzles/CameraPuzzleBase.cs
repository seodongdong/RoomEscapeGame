using UnityEngine;
using System.Collections;

// 카메라 기반 퍼즐의 기본 동작을 정의하는 추상 클래스
public abstract class CameraPuzzleBase : MonoBehaviour, IPuzzle
{
    [Header("Puzzle Settings")]
    [SerializeField] protected string puzzleId;
    [SerializeField] protected bool isSolved;
    
    [Header("Camera Settings")]
    [SerializeField] protected Transform puzzleCameraPosition; // 카메라 이동 위치
    [SerializeField] protected float cameraTransitionDuration = 1f;
    [SerializeField] protected AnimationCurve cameraTransitionCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("UI")]
    [SerializeField] protected GameObject puzzleUI; // 3D 위에 띄울 UI
    
    protected Camera _mainCamera;
    protected Transform _originalCameraParent;
    protected Vector3 _originalCameraPosition;
    protected Quaternion _originalCameraRotation;
    protected Player _player;
    
    public string PuzzleId => puzzleId;
    public bool IsSolved => isSolved;
    
    public event System.Action OnPuzzleSolved;

    // 카메라 및 플레이어 초기화
    protected virtual void Awake()
    {
        _mainCamera = Camera.main;
        _player = FindAnyObjectByType<Player>();
    }

    // 퍼즐 시작 메서드
    public virtual void StartPuzzle()
    {
        if (isSolved) return;
        
        // 게임 상태 변경
        GameManager.Instance.StateManager.ChangeState(GameState.Puzzle);
        
        // 플레이어 조작 비활성화
        if (_player != null)
        {
            _player.enabled = false;
        }
        
        // 원래 카메라 위치 저장
        _originalCameraParent = _mainCamera.transform.parent;
        _originalCameraPosition = _mainCamera.transform.position;
        _originalCameraRotation = _mainCamera.transform.rotation;
        
        // 카메라를 부모에서 분리
        _mainCamera.transform.SetParent(null);
        
        // 카메라 전환 시작
        StartCoroutine(TransitionCamera(true));
    }

    // 카메라 전환 코루틴
    protected virtual IEnumerator TransitionCamera(bool toPuzzle)
    {
        Vector3 startPos = _mainCamera.transform.position;
        Quaternion startRot = _mainCamera.transform.rotation;
        
        Vector3 endPos;
        Quaternion endRot;
        
        if (toPuzzle)
        {
            // 퍼즐 카메라 위치로
            endPos = puzzleCameraPosition.position;
            endRot = puzzleCameraPosition.rotation;
        }
        else
        {
            // 원래 위치로
            endPos = _originalCameraPosition;
            endRot = _originalCameraRotation;
        }
        
        float elapsed = 0f;
        
        while (elapsed < cameraTransitionDuration)
        {
            elapsed += Time.unscaledDeltaTime; // Time.timeScale 영향 안받음
            float t = cameraTransitionCurve.Evaluate(elapsed / cameraTransitionDuration);
            
            _mainCamera.transform.position = Vector3.Lerp(startPos, endPos, t);
            _mainCamera.transform.rotation = Quaternion.Slerp(startRot, endRot, t);
            
            yield return null;
        }
        
        _mainCamera.transform.position = endPos;
        _mainCamera.transform.rotation = endRot;
        
        if (toPuzzle)
        {
            // 퍼즐 UI 표시
            if (puzzleUI != null)
            {
                puzzleUI.SetActive(true);
            }
            
            OnPuzzleStarted();
        }
        else
        {
            // 카메라를 다시 플레이어 자식으로
            _mainCamera.transform.SetParent(_originalCameraParent);
            _mainCamera.transform.localPosition = Vector3.up * 0.6f;
            _mainCamera.transform.localRotation = Quaternion.identity;
            
            OnPuzzleExited();
        }
    }

    // 하위 클래스에서 오버라이드 가능
    protected virtual void OnPuzzleStarted()
    {
        Debug.Log($"퍼즐 시작: {puzzleId}");
    }

    protected virtual void OnPuzzleExited()
    {
        Debug.Log($"퍼즐 종료: {puzzleId}");
    }

    public virtual void CheckSolution()
    {
        if (IsSolutionCorrect())
        {
            SolvePuzzle();
        }
    }

    protected abstract bool IsSolutionCorrect();

    protected virtual void SolvePuzzle()
    {
        isSolved = true;
        
        // UI 숨김
        if (puzzleUI != null)
        {
            puzzleUI.SetActive(false);
        }
        
        OnPuzzleSolved?.Invoke();
        Debug.Log($"퍼즐 해결: {puzzleId}");
        
        // 카메라 복귀
        ExitPuzzle();
    }

    public virtual void ExitPuzzle()
    {
        // UI 숨김
        if (puzzleUI != null)
        {
            puzzleUI.SetActive(false);
        }
        
        // 카메라 복귀 애니메이션
        StartCoroutine(ExitPuzzleCoroutine());
    }

    protected virtual IEnumerator ExitPuzzleCoroutine()
    {
        yield return StartCoroutine(TransitionCamera(false));
        
        // 게임 상태 복귀
        GameManager.Instance.StateManager.ChangeState(GameState.Playing);
        
        // 플레이어 조작 활성화
        if (_player != null)
        {
            _player.enabled = true;
        }
    }
}