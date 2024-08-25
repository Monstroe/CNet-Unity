using System;
using System.Collections;
using System.Collections.Generic;
using CNet;
using UnityEngine;

public class TransformService : NetService
{
    public enum TransformServiceType
    {
        Position = 0,
        Rotation = 1,
        Scale = 2
    }

    public override void ReceiveData(NetPacket packet)
    {
        try
        {
            int netID = packet.ReadShort();

            if (!NetManager.Instance.NetObjects.TryGetValue(netID, out SyncedObject syncedObj))
            {
                Debug.LogError("<color=red><b>CNet</b></color>: TransformService could not find NetObject with ID " + netID);
                return;
            }

            if (syncedObj is SyncedTransform)
            {
                int serviceType = (int)packet.ReadByte();
                switch (serviceType)
                {
                    case (int)(TransformServiceType.Position):
                        ((SyncedTransform)syncedObj).SyncPosition((Vector3)packet.DeserializeStruct<NetVector3>());
                        break;
                    case (int)(TransformServiceType.Rotation):
                        ((SyncedTransform)syncedObj).SyncRotation((Quaternion)packet.DeserializeStruct<NetQuaternion>());
                        break;
                    case (int)(TransformServiceType.Scale):
                        ((SyncedTransform)syncedObj).SyncScale((Vector3)packet.DeserializeStruct<NetVector3>());
                        break;
                }
            }
            else
            {
                Debug.LogError("<color=red><b>CNet</b></color>: TransformService can not read NetObject with ID " + netID + " because it isn't of type 'SyncedTransform'");
            }
        }
        catch (Exception e)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: TransformService error - " + e.Message);
        }
    }
}
