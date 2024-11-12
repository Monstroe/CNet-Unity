using UnityEngine;

[RequireComponent(typeof(NetRigidbody))]
[RequireComponent(typeof(NetTransform))]
public class TestPlayer : MonoBehaviour
{
    [SerializeField] private float speed = 10000.0f;
    [SerializeField] private float jumpSpeed = 20.0f;
    [SerializeField] private float sensitivity = 2.0f;
    [SerializeField] private float lookXLimit = 80.0f;
    [SerializeField] private NetTransform playerCamera;

    private float rotationX = 0;
    private NetRigidbody netRB;
    private NetTransform netTransform;

    void Awake()
    {
        netRB = GetComponent<NetRigidbody>();
        netTransform = GetComponent<NetTransform>();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Movement
        float moveX = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float moveZ = Input.GetAxis("Vertical") * speed * Time.deltaTime;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;
        Vector3 currentVelocity = netRB.Velocity;

        currentVelocity.x = move.x;
        currentVelocity.y = netRB.Velocity.y;
        currentVelocity.z = move.z;

        if (Input.GetButton("Jump") && Physics.Raycast(transform.position, Vector3.down, 1.1f))
        {
            netRB.AddForce(Vector3.up * jumpSpeed);
        }

        netRB.Velocity = currentVelocity;


        // Rotation
        float mouseX = Input.GetAxis("Mouse X") * sensitivity;
        float mouseY = -Input.GetAxis("Mouse Y") * sensitivity;

        netTransform.Rotate(0, mouseX, 0);

        rotationX += mouseY;
        rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);
        playerCamera.LocalRotation = Quaternion.Euler(rotationX, 0, 0);
    }

}
