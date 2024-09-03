using System.Collections;
using System.Collections.Generic;
using CNet;
using UnityEngine;

public class SyncedTransform : SyncedObject
{
    [SerializeField] private SyncOn syncOn;
    [Space]
    [SerializeField] private bool syncPosition;
    [SerializeField] private bool syncRotation;
    [SerializeField] private bool syncScale;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        /*if (NetManager.Instance.IsHost || syncOn == SyncOn.ClientSide)
        {
            if (syncPosition)
            {
                SendPosition(transform.position);
            }

            if (syncRotation)
            {
                SendRotation(transform.rotation);
            }

            if (syncScale)
            {
                SendScale(transform.localScale);
            }
        }*/
    }

    public void SyncPosition(Vector3 pos)
    {
        if (!syncPosition)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: SyncedTransform is not set to sync position");
            return;
        }

        transform.position = pos;

        if (NetManager.Instance.IsHost)
        {
            SendPosition(pos);
        }
    }

    private void SendPosition(Vector3 pos)
    {
        using (NetPacket packet = new NetPacket(NetManager.Instance.System, PacketProtocol.UDP))
        {
            packet.Write((short)ServiceType.Transform);
            packet.Write((short)NetID);
            packet.Write((byte)TransformService.TransformServiceType.Position);
            packet.SerializeStruct<NetVector3>((NetVector3)pos);
            NetManager.Instance.Send(packet, PacketProtocol.UDP);
        }
    }

    public void SyncRotation(Quaternion rot)
    {
        if (!syncRotation)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: SyncedTransform is not set to sync rotation");
            return;
        }

        transform.rotation = rot;

        if (NetManager.Instance.IsHost)
        {
            SendRotation(rot);
        }
    }

    private void SendRotation(Quaternion rot)
    {
        using (NetPacket packet = new NetPacket(NetManager.Instance.System, PacketProtocol.UDP))
        {
            packet.Write((short)ServiceType.Transform);
            packet.Write((short)NetID);
            packet.Write((byte)TransformService.TransformServiceType.Rotation);
            packet.SerializeStruct<NetQuaternion>((NetQuaternion)rot);
            NetManager.Instance.Send(packet, PacketProtocol.UDP);
        }
    }

    public void SyncScale(Vector3 scl)
    {
        if (!syncScale)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: SyncedTransform is not set to sync scale");
            return;
        }

        transform.localScale = scl;

        if (NetManager.Instance.IsHost)
        {
            SendScale(scl);
        }
    }

    private void SendScale(Vector3 scl)
    {
        using (NetPacket packet = new NetPacket(NetManager.Instance.System, PacketProtocol.UDP))
        {
            packet.Write((short)ServiceType.Transform);
            packet.Write((short)NetID);
            packet.Write((byte)TransformService.TransformServiceType.Scale);
            packet.SerializeStruct<NetVector3>((NetVector3)scl);
            NetManager.Instance.Send(packet, PacketProtocol.UDP);
        }
    }
}
