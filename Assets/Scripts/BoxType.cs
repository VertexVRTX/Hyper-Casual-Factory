using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum BoxType { Red, Blue, Green, Yellow, BonusScore, BonusTime }
public enum BoxMechanicState { Normal, Sealed, Frozen, Glass }

[RequireComponent(typeof(Rigidbody))]
public class Box : MonoBehaviour
{
    public BoxType Type;
    public MeshRenderer Renderer;
    public Collider boxCollider;

    [HideInInspector] public bool IsDragging;
    [HideInInspector] public bool IsHandled;
    [HideInInspector] public bool isOnConveyorAsTrigger;

    [Header("Bonus Values")]
    public int bonusTimeAmount = 5;
    public int bonusScoreAmount = 50;

    public bool IsBonus => Type == BoxType.BonusScore || Type == BoxType.BonusTime;

    [Header("Mechanics Setup")]
    public BoxMechanicState mechanicState = BoxMechanicState.Normal;

    [Header("Visual Overlays")]
    public GameObject tapeVisual;
    public GameObject iceVisual;
    public GameObject glassVisual;

    [Header("Glass Box Settings")]
    public float maxAllowedSpeed = 15f;
    private Vector3 _lastFramePosition;
    private bool _isFirstDragFrame;

    private float _freezeHoldTimer = 0f;
    private const float UNFREEZE_DURATION = 0.5f;

    private Vector3 _dragOffset;
    private Camera _cam;

    private Rigidbody _rb;
    private float _lockedDragY;

    private void Awake()
    {
        if (boxCollider == null) boxCollider = GetComponent<Collider>();
        _rb = GetComponent<Rigidbody>();
    }

    public void Init(BoxType type, Material mat)
    {
        Type = type;
        Renderer.material = mat;
        IsDragging = false;
        IsHandled = false;
        transform.localScale = Vector3.one;

        ResetPhysicsState();
    }

    public void InitMechanic(BoxMechanicState state)
    {
        mechanicState = state;
        _freezeHoldTimer = 0f;

        if (tapeVisual != null) tapeVisual.SetActive(mechanicState == BoxMechanicState.Sealed);
        if (iceVisual != null) iceVisual.SetActive(mechanicState == BoxMechanicState.Frozen);
        if (glassVisual != null) glassVisual.SetActive(mechanicState == BoxMechanicState.Glass);
    }

    public bool CanBeDragged()
    {
        if (IsHandled) return false;

        return mechanicState == BoxMechanicState.Normal || mechanicState == BoxMechanicState.Glass;
    }

    public void InteractClick()
    {
        if (IsHandled) return;

        if (mechanicState == BoxMechanicState.Sealed)
        {
            mechanicState = BoxMechanicState.Normal;
            if (tapeVisual != null) tapeVisual.SetActive(false);

            if (GameManager.Instance != null && GameManager.Instance.Conveyor != null)
            {
                GameManager.Instance.Conveyor.ApplyMicroSlowdown(0.4f);
            }

            transform.DOKill();
            transform.DOPunchScale(Vector3.one * 0.2f, 0.15f);
            AudioManager.Instance?.PlaySFX(AudioManager.Instance.tapeUnwrapSound);
        }
    }

    public void InteractHold(float deltaTime)
    {
        if (IsHandled) return;

        if (mechanicState == BoxMechanicState.Frozen)
        {
            if (GameManager.Instance != null && GameManager.Instance.Conveyor != null)
            {
                GameManager.Instance.Conveyor.ApplyMicroSlowdown(0.1f);
            }

            _freezeHoldTimer += deltaTime;

            if (_freezeHoldTimer >= UNFREEZE_DURATION)
            {
                mechanicState = BoxMechanicState.Normal;
                if (iceVisual != null) iceVisual.SetActive(false);

                if (_rb != null)
                {
                    _rb.velocity = Vector3.zero;
                    _rb.angularVelocity = Vector3.zero;
                }

                transform.DOKill();
                transform.DOPunchScale(Vector3.one * 0.25f, 0.2f);
                AudioManager.Instance?.PlaySFX(AudioManager.Instance.iceMeltSound);
            }
        }
    }

    public void StartDrag(Camera cam, Vector3 hitWorldPoint)
    {
        if (IsHandled) return;

        _cam = cam;
        IsDragging = true;
        _dragOffset = transform.position - hitWorldPoint;

        _lastFramePosition = transform.position;
        _isFirstDragFrame = true;

        _lockedDragY = transform.position.y;

        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.isKinematic = true;
        }

        transform.DOKill();
        transform.DOScale(1.15f, 0.15f);
    }

    public void DragTo(Vector3 worldPoint)
    {
        if (IsHandled) return;

        Vector3 targetPosition = worldPoint + _dragOffset;
        targetPosition.y = Mathf.Max(targetPosition.y, _lockedDragY);

        if (mechanicState == BoxMechanicState.Glass)
        {
            if (!_isFirstDragFrame)
            {
                float currentSpeed = (targetPosition - _lastFramePosition).magnitude / Time.deltaTime;

                if (currentSpeed > maxAllowedSpeed)
                {
                    BreakGlassBox();
                    return;
                }
            }
            else
            {
                _isFirstDragFrame = false;
            }
        }

        _lastFramePosition = targetPosition;

        transform.position = targetPosition;
    }

    private void BreakGlassBox()
    {
        IsDragging = false;
        IsHandled = true;

        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }

        if (CameraShaker.Instance != null)
        {
            CameraShaker.Instance.ShakeOnWrong();
        }

        GameManager.Instance.OnWrongSort();
        AudioManager.Instance?.PlaySFX(AudioManager.Instance.glassBreakSound);

        transform.DOKill();
        transform.DOShakeScale(0.2f, 0.5f, 20)
            .OnComplete(() =>
            {
                transform.DOScale(Vector3.zero, 0.1f).OnComplete(() =>
                {
                    GameManager.Instance.Conveyor.ReleaseBox(this);
                });
            });
    }

    public void EndDrag()
    {
        if (IsHandled) return;

        IsDragging = false;

        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.velocity = Vector3.zero;
        }

        transform.DOScale(1f, 0.15f);
    }

    public void PlayCorrectTween(Vector3 targetPos, System.Action onComplete)
    {
        IsHandled = true;
        IsDragging = false;

        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }

        if (_rb != null)
        {
            _rb.velocity = Vector3.zero;
            _rb.isKinematic = true;
        }

        transform.DOMove(targetPos, 0.25f).SetEase(Ease.InBack);
        transform.DOScale(0f, 0.25f).SetDelay(0.05f)
            .OnComplete(() => onComplete?.Invoke());
    }

    public void PlayThrowUpTween(System.Action onComplete = null)
    {
        IsHandled = true;
        IsDragging = false;

        if (boxCollider != null)
        {
            boxCollider.enabled = false;
        }

        if (_rb != null)
        {
            _rb.useGravity = false;
            _rb.Sleep();
            _rb.isKinematic = true;
        }

        Vector3 targetUpPos = transform.position + Vector3.up * 10f;

        Sequence throwSeq = DOTween.Sequence();
        throwSeq.Join(transform.DOMove(targetUpPos, 0.5f).SetEase(Ease.OutQuad));
        throwSeq.Join(transform.DORotate(new Vector3(Random.Range(180, 360), Random.Range(180, 360), 0), 0.5f, RotateMode.FastBeyond360));
        throwSeq.Append(transform.DOScale(Vector3.zero, 0.15f));

        throwSeq.OnComplete(() =>
        {
            onComplete?.Invoke();
        });
    }

    public void PlayWrongTween(Vector3 targetPos, System.Action onComplete = null)
    {
        PlayThrowUpTween(onComplete);
    }

    public void ResetPhysicsState()
    {
        if (_rb == null) _rb = GetComponent<Rigidbody>();

        if (_rb != null)
        {
            _rb.isKinematic = false;
            _rb.useGravity = true;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _rb.constraints = RigidbodyConstraints.FreezeRotation;
        }

        if (boxCollider != null)
        {
            boxCollider.enabled = true;
            boxCollider.isTrigger = false;
        }
    }
}
