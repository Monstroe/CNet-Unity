using System.Collections;
using System.Collections.Generic;
using CNet;
using UnityEngine;

[RequireComponent(typeof(SyncedTransform))]
[RequireComponent(typeof(Rigidbody))]
public class NetRigidbody : MonoBehaviour
{
    [SerializeField] private bool syncVelocity = false;
    [SerializeField] private bool syncAngularVelocity = false;
    [Space]
    [SerializeField] private int syncRate = 10;

    public Vector3 Velocity { get => rb.velocity; set => SyncVelocity(value, syncVelocity, true); }
    public Vector3 AngularVelocity { get => rb.angularVelocity; set => SyncAngularVelocity(value, syncAngularVelocity, true); }

    private SyncedTransform syncedTransform;
    private Rigidbody rb;

    private void Awake()
    {
        syncedTransform = GetComponent<SyncedTransform>();
        rb = GetComponent<Rigidbody>();

        if (!NetManager.Instance.IsHost && syncedTransform.SyncOn == SyncOn.ServerSide)
        {
            rb.isKinematic = true;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(UpdateCoroutine());
    }

    private IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f / syncRate);

            Vector3 newPosition;
            Quaternion newRotation;

            if (rb.isKinematic)
            {
                newPosition = syncedTransform.Position + Velocity * (1f / syncRate);
                newRotation = syncedTransform.Rotation * Quaternion.Euler(AngularVelocity * (1f / syncRate));
            }
            else
            {
                newPosition = syncedTransform.Position;
                newRotation = syncedTransform.Rotation;
            }

            syncedTransform.Position = newPosition;
            syncedTransform.Rotation = newRotation;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (NetManager.Instance.Connected)
        {
            if (syncVelocity)
            {
                transform.position += rb.velocity * Time.deltaTime;
            }

            if (syncAngularVelocity)
            {
                transform.rotation *= Quaternion.Euler(rb.angularVelocity * Time.deltaTime);
            }
        }
    }

    internal void SyncVelocity(Vector3 velocity, bool sync, bool force = false)
    {
        if (!syncVelocity && !force)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: NetRigidbody is not set to sync velocity");
            return;
        }

        if (sync)
        {
            using (NetPacket packet = new NetPacket(NetManager.Instance.System, PacketProtocol.UDP))
            {
                packet.Write((short)ServiceType.Transform);
                packet.Write((short)syncedTransform.NetID);
                packet.Write((byte)TransformService.TransformServiceType.Velocity);
                packet.SerializeStruct((NetVector3)velocity);
                NetManager.Instance.Send(packet, PacketProtocol.UDP);
            }
        }

        if (!sync || NetManager.Instance.IsHost || syncedTransform.SyncOn == SyncOn.ClientSide)
        {
            rb.velocity = velocity;
        }
    }

    internal void SyncAngularVelocity(Vector3 angularVelocity, bool sync, bool force = false)
    {
        if (!syncAngularVelocity && !force)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: NetRigidbody is not set to sync angular velocity");
            return;
        }

        if (sync)
        {
            using (NetPacket packet = new NetPacket(NetManager.Instance.System, PacketProtocol.UDP))
            {
                packet.Write((short)ServiceType.Transform);
                packet.Write((short)syncedTransform.NetID);
                packet.Write((byte)TransformService.TransformServiceType.AngularVelocity);
                packet.SerializeStruct((NetVector3)angularVelocity);
                NetManager.Instance.Send(packet, PacketProtocol.UDP);
            }
        }

        if (!sync || NetManager.Instance.IsHost || syncedTransform.SyncOn == SyncOn.ClientSide)
        {
            rb.angularVelocity = angularVelocity;
        }
    }

    public void AddForce(Vector3 force)
    {
        AddForce(force, ForceMode.Force);
    }

    public void AddForce(Vector3 force, ForceMode mode)
    {
        if (NetManager.Instance.IsHost || syncedTransform.SyncOn == SyncOn.ClientSide)
        {
            rb.AddForce(force, mode);
        }
        else
        {
            // SEND ADDFORCE PACKET
        }
    }

    public void MovePostion(Vector3 pos)
    {
        if (!NetManager.Instance.IsHost)
        {
            rb.MovePosition(pos);
        }
    }
}
