using UnityEngine;

public class BuildingCamera : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Camera _cam;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 20f;

    [Header("Zoom")]
    [SerializeField] private float _zoomSpeed = 10f;
    [SerializeField] private float _minZoom = 5f;
    [SerializeField] private float _maxZoom = 50f;

    private float _initialZoom;
    private Vector3 _initialPos;
    public bool controlsActive;

    private void OnEnable()
    {
        _initialZoom = _cam.orthographicSize;
        _initialPos = _cam.transform.position;
        controlsActive = true;
    }

    private void OnDisable()
    {
        _cam.orthographicSize = _initialZoom;
        _cam.transform.position = _initialPos;
        controlsActive = false;
    }

    void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    void HandleMovement()
    {
        if (!controlsActive) { return; }
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 move = new Vector3(h, v, 0).normalized;

        transform.position += move * _moveSpeed * Time.deltaTime;
    }

    void HandleZoom()
    {
        if (!controlsActive) { return; }
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            _cam.orthographicSize -= scroll * _zoomSpeed;
            _cam.orthographicSize = Mathf.Clamp(_cam.orthographicSize, _minZoom, _maxZoom);
        }
    }
}
