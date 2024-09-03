using System.Collections;
using System.Collections.Generic;
using CNet;
using UnityEngine;

public class SyncedTransform : SyncedObject
{
    public SyncOn SyncOn { get => syncOn; set => syncOn = value; }
    public Vector3 Position { get => transform.position; set => SyncPosition(value, true); }
    public Quaternion Rotation { get => transform.rotation; set => SyncRotation(value, true); }
    public Vector3 Scale { get => transform.localScale; set => SyncScale(value, true); }

    [SerializeField] private SyncOn syncOn;
    [Space]
    [SerializeField] private bool syncPosition;
    [SerializeField] private bool syncRotation;
    [SerializeField] private bool syncScale;

    internal void SyncPosition(Vector3 pos, bool sync)
    {
        if (!syncPosition)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: SyncedTransform is not set to sync position");
            return;
        }

        if (sync)
        {
            using (NetPacket packet = new NetPacket(NetManager.Instance.System, PacketProtocol.UDP))
            {
                packet.Write((short)ServiceType.Transform);
                packet.Write((short)NetID);
                packet.Write((byte)TransformService.TransformServiceType.Position);
                packet.SerializeStruct((NetVector3)pos);
                NetManager.Instance.Send(packet, PacketProtocol.UDP);
            }
        }

        if (!sync || NetManager.Instance.IsHost || syncOn == SyncOn.ClientSide)
        {
            transform.position = pos;
        }
    }

    internal void SyncRotation(Quaternion rot, bool sync)
    {
        if (!syncRotation)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: SyncedTransform is not set to sync rotation");
            return;
        }

        if (sync)
        {
            using (NetPacket packet = new NetPacket(NetManager.Instance.System, PacketProtocol.UDP))
            {
                packet.Write((short)ServiceType.Transform);
                packet.Write((short)NetID);
                packet.Write((byte)TransformService.TransformServiceType.Rotation);
                packet.SerializeStruct((NetQuaternion)rot);
                NetManager.Instance.Send(packet, PacketProtocol.UDP);
            }
        }

        if (!sync || NetManager.Instance.IsHost || syncOn == SyncOn.ClientSide)
        {
            transform.rotation = rot;
        }
    }

    internal void SyncScale(Vector3 scl, bool sync)
    {
        if (!syncScale)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: SyncedTransform is not set to sync scale");
            return;
        }

        if (sync)
        {
            using (NetPacket packet = new NetPacket(NetManager.Instance.System, PacketProtocol.UDP))
            {
                packet.Write((short)ServiceType.Transform);
                packet.Write((short)NetID);
                packet.Write((byte)TransformService.TransformServiceType.Scale);
                packet.SerializeStruct((NetVector3)scl);
                NetManager.Instance.Send(packet, PacketProtocol.UDP);
            }
        }

        if (!sync || NetManager.Instance.IsHost || syncOn == SyncOn.ClientSide)
        {
            transform.localScale = scl;
        }
    }
}
