using System.Collections;
using CNet;
using UnityEngine;

[RequireComponent(typeof(SyncedTransform))]
[RequireComponent(typeof(Rigidbody))]
public class NetRigidbody : MonoBehaviour
{
    public bool IsSyncingVelocity { get => syncVelocity; set => syncVelocity = value; }
    public bool IsSyncingAngularVelocity { get => syncAngularVelocity; set => syncAngularVelocity = value; }

    public Vector3 Velocity { get => rb.velocity; set => SyncVelocity(value, syncVelocity, true); }
    public Vector3 AngularVelocity { get => rb.angularVelocity; set => SyncAngularVelocity(value, syncAngularVelocity, true); }

    [SerializeField] private bool syncVelocity = false;
    [SerializeField] private bool syncAngularVelocity = false;
    [Space]
    [SerializeField] private int syncRate = 10;
    [Space]
    [SerializeField] private bool clientSidePrediction = false;

    private SyncedTransform syncedTransform;
    private Rigidbody rb;

    void Awake()
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
        if (NetManager.Instance.IsHost)
        {
            StartCoroutine(UpdateCoroutine());
        }
    }

    private IEnumerator UpdateCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f / syncRate);

            syncedTransform.Position = transform.position;
            syncedTransform.Rotation = transform.rotation;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (NetManager.Instance.Connected && !NetManager.Instance.IsHost && syncedTransform.SyncOn == SyncOn.ServerSide && clientSidePrediction)
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

    public void SyncVelocity(Vector3 velocity, bool sync, bool force = false)
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

    public void SyncAngularVelocity(Vector3 angularVelocity, bool sync, bool force = false)
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

    public void AddForce(float x, float y, float z)
    {
        AddForce(new Vector3(x, y, z));
    }

    public void AddForce(float x, float y, float z, ForceMode mode)
    {
        AddForce(new Vector3(x, y, z), mode);
    }

    /*public void MovePostion(Vector3 pos)
    {
        if (!NetManager.Instance.IsHost)
        {
            rb.MovePosition(pos);
        }
    }*/
}
