using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class NetRigidbody : MonoBehaviour
{
    private Rigidbody rb;

    private void Awake()
    {
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
        if (NetManager.Instance.IsHost)
        {
            rb.AddForce(force);
        }
    }
}
