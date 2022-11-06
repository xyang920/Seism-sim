//https://forum.unity.com/threads/how-to-make-camera-move-in-a-way-similar-to-editor-scene.524645/

using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float navigationSpeed = 2.4f;
    [SerializeField] private float shiftMultiplier = 2f;
    [SerializeField] private float sensitivity = 1.0f;
    [SerializeField] private float panSensitivity = 0.5f;
    [SerializeField] private float mouseWheelZoomSpeed = 1.0f;
    private Camera cam;
    private Vector3 anchorPoint;
    private Quaternion anchorRot;

    private bool isPanning;

    private void Awake()
    {
        cam = GetComponent<Camera>();
    }

    private void Update()
    {
        MousePanning();
        if (isPanning)
        { return; }

        if (Input.GetMouseButton(1))
        {
            Vector3 move = Vector3.zero;
            float speed = navigationSpeed * (Input.GetKey(KeyCode.LeftShift) ? shiftMultiplier : 1f) * Time.deltaTime * 9.1f;
            if (Input.GetKey(KeyCode.W))
                move += Vector3.forward * speed;
            if (Input.GetKey(KeyCode.S))
                move -= Vector3.forward * speed;
            if (Input.GetKey(KeyCode.D))
                move += Vector3.right * speed;
            if (Input.GetKey(KeyCode.A))
                move -= Vector3.right * speed;
            if (Input.GetKey(KeyCode.E))
                move += Vector3.up * speed;
            if (Input.GetKey(KeyCode.Q))
                move -= Vector3.up * speed;
            transform.Translate(move);
        }
        if (Input.GetMouseButtonDown(1))
        {
            anchorPoint = new Vector3(Input.mousePosition.y, -Input.mousePosition.x);
            anchorRot = transform.rotation;
        }

        if (Input.GetMouseButton(1))
        {
            Quaternion rot = anchorRot;
            Vector3 dif = anchorPoint - new Vector3(Input.mousePosition.y, -Input.mousePosition.x);
            rot.eulerAngles += dif * sensitivity;
            transform.rotation = rot;
        }

        MouseWheeling();
    }

    //Zoom with mouse wheel
    private void MouseWheeling()
    {
        float speed = 10 * (mouseWheelZoomSpeed * (Input.GetKey(KeyCode.LeftShift) ? shiftMultiplier : 1f) * Time.deltaTime * 9.1f);

        Vector3 pos = transform.position;
        if (Input.GetAxis("Mouse ScrollWheel") < 0)
        {
            pos = pos - (transform.forward * speed);
            transform.position = pos;
        }
        if (Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            pos = pos + (transform.forward * speed);
            transform.position = pos;
        }
    }

    private float pan_x;
    private float pan_y;
    private Vector3 panComplete;

    private void MousePanning()
    {
        pan_x = -Input.GetAxis("Mouse X") * panSensitivity;
        pan_y = -Input.GetAxis("Mouse Y") * panSensitivity;
        panComplete = new Vector3(pan_x, pan_y, 0);

        if (Input.GetMouseButtonDown(2))
        {
            isPanning = true;
        }

        if (Input.GetMouseButtonUp(2))
        {
            isPanning = false;
        }

        if (isPanning)
        {
            transform.Translate(panComplete);
        }
    }
}