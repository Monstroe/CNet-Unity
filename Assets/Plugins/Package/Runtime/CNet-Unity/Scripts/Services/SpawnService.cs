using System;
using System.Collections;
using System.Collections.Generic;
using CNet;
using UnityEngine;

public class SpawnService : NetService
{
    internal Queue<Action<GameObject>> spawnedPrefabActionCache = new Queue<Action<GameObject>>();

    public override void ReceiveData(NetPacket packet)
    {
        try
        {
            int netPrefabID = packet.ReadShort();
            if (netPrefabID < 0 || netPrefabID >= NetManager.Instance.NetPrefabs.Count)
            {
                Debug.LogError("<color=red><b>CNet</b></color>: SpawnService could not find NetPrefab with ID " + netPrefabID);
                return;
            }
            GameObject prefab = NetManager.Instance.NetPrefabs[netPrefabID].gameObject;

            Vector3 pos = (Vector3)packet.DeserializeStruct<NetVector3>();
            Quaternion rot = (Quaternion)packet.DeserializeStruct<NetQuaternion>();

            int parentID = packet.ReadShort();
            SyncedObject parent = null;
            if (parentID >= 0 && !NetManager.Instance.NetObjects.TryGetValue(parentID, out parent))
            {
                Debug.LogError("<color=red><b>CNet</b></color>: SpawnService could not find parent NetObject with ID " + parentID);
                return;
            }

            int netID;

            if (NetManager.Instance.IsHost)
            {
                if (packet.UnreadLength > 0)
                {
                    Debug.LogError("<color=red><b>CNet</b></color>: SpawnService was passed a NetID even though this client is the host");
                    return;
                }

                netID = NetManager.Instance.GenerateNetID();
                packet.Write((short)netID);
                SpawnPrefab(netID, prefab, pos, rot, parent.transform);
                NetManager.Instance.Send(packet, PacketProtocol.TCP);
            }
            else
            {
                if (packet.UnreadLength < 2)
                {
                    Debug.LogError("<color=red><b>CNet</b></color>: SpawnService was not passed a NetID event though this client is a guest.");
                }

                netID = (int)packet.ReadShort();
                spawnedPrefabActionCache.Dequeue()?.Invoke(SpawnPrefab(netID, prefab, pos, rot, parent.transform));
            }
        }
        catch (Exception e)
        {
            Debug.LogError("<color=red><b>CNet</b></color>: SpawnService error - " + e.Message);
        }
    }

    internal GameObject SpawnPrefab(int netID, GameObject original, Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject obj = parent == null ? Instantiate(original, position, rotation) : Instantiate(original, position, rotation, parent);
        SyncedObject syncedObj = obj.GetComponent<SyncedObject>();
        syncedObj.NetID = netID;
        NetManager.Instance.NetObjects.Add(netID, syncedObj);
        return obj;
    }
}
