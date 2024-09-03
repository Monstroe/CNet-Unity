using System.Collections;
using System.Collections.Generic;
using CNet;
using UnityEngine;

[RequireComponent(typeof(SyncedTransform))]
[RequireComponent(typeof(Rigidbody))]
public class NetRigidbody : MonoBehaviour
{
    public Vector3 velocity;
    public Vector3 angularVelocity;
    private SyncedTransform syncedTransform;
    private Rigidbody rb;

    private void Awake()
    {
        syncedTransform = GetComponent<SyncedTransform>();
        rb = GetComponent<Rigidbody>();

        if (!NetManager.Instance.IsHost)
        {
            rb.isKinematic = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void AddForce(Vector3 force)
    {
        AddForce(force, ForceMode.Force);
    }

    public void AddForce(Vector3 force, ForceMode mode)
    {
        if (NetManager.Instance.IsHost)
        {
            rb.AddForce(force, mode);
        }
        else
        {
            // SEND ADDFORCE PACKET
        }
    }

    public void AddForce(float x, float y, float z)
    {
        AddForce(new Vector3(x, y, z), ForceMode.Force);
    }

    public void AddForce(float x, float y, float z, ForceMode mode)
    {
        AddForce(new Vector3(x, y, z), mode);
    }
}
