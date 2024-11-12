using System;
using System.Collections;
using System.Collections.Generic;
using CNet;
using UnityEngine;

public class TransformService : MonoBehaviour, NetService
{
    public enum TransformServiceType
    {
        Position = 0,
        Rotation = 1,
        Scale = 2,
        Velocity = 3,
        AngularVelocity = 4
    }

    void Awake()
    {
        RegisterService((int)ServiceType.Transform);
    }

    void OnDisable()
    {
        UnregisterService((int)ServiceType.Transform);
    }

    public void RegisterService(int serviceID)
    {
        NetManager.Instance.RegisterService(serviceID, this);
    }

    public void UnregisterService(int serviceID)
    {
        NetManager.Instance.UnregisterService(serviceID);
    }

    public void ReceiveData(NetPacket packet)
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
                SyncedTransform syncedTran = (SyncedTransform)syncedObj;
                if (syncedTran.SyncOn == SyncOn.ClientSide)
                {
                    return;
                }

                int serviceType = packet.ReadByte();
                switch (serviceType)
                {
                    case (int)TransformServiceType.Position:
                        syncedTran.SyncPosition((Vector3)packet.DeserializeStruct<NetVector3>(), NetManager.Instance.IsHost);
                        break;
                    case (int)TransformServiceType.Rotation:
                        syncedTran.SyncRotation((Quaternion)packet.DeserializeStruct<NetQuaternion>(), NetManager.Instance.IsHost);
                        break;
                    case (int)TransformServiceType.Scale:
                        syncedTran.SyncScale((Vector3)packet.DeserializeStruct<NetVector3>(), NetManager.Instance.IsHost);
                        break;
                    case (int)TransformServiceType.Velocity:
                        {
                            if (syncedTran.TryGetComponent(out NetRigidbody netRB))
                            {
                                netRB.SyncVelocity((Vector3)packet.DeserializeStruct<NetVector3>(), NetManager.Instance.IsHost);
                            }
                            else
                            {
                                Debug.LogError("<color=red><b>CNet</b></color>: TransformService cannot sync velocity of NetObject with ID " + netID + " because it doesn't have a NetRigidbody component");
                            }
                            break;
                        }
                    case (int)TransformServiceType.AngularVelocity:
                        {
                            if (syncedTran.TryGetComponent(out NetRigidbody netRB))
                            {
                                netRB.SyncAngularVelocity((Vector3)packet.DeserializeStruct<NetVector3>(), NetManager.Instance.IsHost);
                            }
                            else
                            {
                                Debug.LogError("<color=red><b>CNet</b></color>: TransformService cannot sync angular velocity of NetObject with ID " + netID + " because it doesn't have a NetRigidbody component");
                            }
                            break;
                        }
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
