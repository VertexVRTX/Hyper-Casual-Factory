using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public Camera cam;
    public LayerMask boxLayer;
    public float minDragDistance = 0.4f;

    private Box _currentBox;
    private Vector3 _dragStartPos;
    private float _dragPlaneY;

    private readonly Collider[] _overlapBuffer = new Collider[8];

    private void Update()
    {
        if (_currentBox != null && _currentBox.IsHandled)
        {
            _currentBox = null;
        }

        if (Input.GetMouseButtonDown(0) || TouchBegan())
        {
            TryInteractOrStartDrag(GetPointerScreenPos());
        }

        if (_currentBox != null && (Input.GetMouseButton(0) || TouchHeld()))
        {
            if (_currentBox.mechanicState == BoxMechanicState.Frozen)
            {
                _currentBox.InteractHold(Time.deltaTime);
            }
            else if (_currentBox.CanBeDragged() && _currentBox.IsDragging)
            {
                DragCurrentBox(GetPointerScreenPos());
            }
            else if (_currentBox.CanBeDragged() && !_currentBox.IsDragging)
            {
                Ray ray = cam.ScreenPointToRay(GetPointerScreenPos());
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, boxLayer))
                {
                    _currentBox.StartDrag(cam, hit.point);
                }
            }
        }

        if (_currentBox != null && (Input.GetMouseButtonUp(0) || TouchEnded()))
        {
            if (_currentBox.IsDragging)
            {
                EndDrag();
            }
            else
            {
                _currentBox = null;
            }
        }
    }

    private void TryInteractOrStartDrag(Vector3 screenPos)
    {
        if (_currentBox != null && _currentBox.IsDragging) return;

        Ray ray = cam.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, boxLayer))
        {
            Box box = hit.collider.GetComponent<Box>();

            if (box != null && !box.IsHandled)
            {
                _currentBox = box;

                if (box.mechanicState == BoxMechanicState.Sealed)
                {
                    box.InteractClick();
                }

                if (box.CanBeDragged())
                {
                    _dragStartPos = box.transform.position;
                    _dragPlaneY = hit.point.y;
                    box.StartDrag(cam, hit.point);
                }
            }
        }
    }

    private void DragCurrentBox(Vector3 screenPos)
    {
        if (_currentBox == null || _currentBox.IsHandled) return;

        Ray ray = cam.ScreenPointToRay(screenPos);

        float conveyorHeight = GameManager.Instance.Conveyor.spawnPoint.position.y;
        Plane plane = new Plane(Vector3.up, new Vector3(0, conveyorHeight, 0));

        if (plane.Raycast(ray, out float dist))
        {
            Vector3 targetPoint = ray.GetPoint(dist);
            targetPoint.y = Mathf.Max(targetPoint.y, conveyorHeight);

            _currentBox.DragTo(targetPoint);
        }
    }

    private void EndDrag()
    {
        if (_currentBox == null) return;

        Box boxToProcess = _currentBox;
        _currentBox = null;

        boxToProcess.EndDrag();

        int hitCount = Physics.OverlapSphereNonAlloc(boxToProcess.transform.position, 1.0f, _overlapBuffer);
        Container container = null;

        for (int i = 0; i < hitCount; i++)
        {
            container = _overlapBuffer[i].GetComponent<Container>();
            if (container != null) break;
        }

        if (container != null)
        {
            container.TryAccept(boxToProcess);
        }
        else
        {
            boxToProcess.PlayThrowUpTween(() =>
            {
                GameManager.Instance.Conveyor.ReleaseBox(boxToProcess);
            });

            if (ScreenFlashEffect.Instance != null) ScreenFlashEffect.Instance.TriggerFlash();
            GameManager.Instance.OnWrongSort();
        }
    }

    private bool TouchBegan() => Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began;
    private bool TouchHeld() => Input.touchCount > 0 &&
        (Input.GetTouch(0).phase == TouchPhase.Moved || Input.GetTouch(0).phase == TouchPhase.Stationary);
    private bool TouchEnded() => Input.touchCount > 0 &&
        (Input.GetTouch(0).phase == TouchPhase.Ended || Input.GetTouch(0).phase == TouchPhase.Canceled);

    private Vector3 GetPointerScreenPos()
    {
        return Input.touchCount > 0 ? (Vector3)Input.GetTouch(0).position : Input.mousePosition;
    }
}
