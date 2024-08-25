using System;
using System.Collections;
using System.Collections.Generic;
using CNet;
using UnityEngine;

public class NetBehavior : MonoBehaviour
{
    // TODO: COME BACK TO THESE FUNCTIONS AND MAKE SURE THEY ARE IDENTICAL TO THE ORIGINALS
    public void NetInstantiate(GameObject original, Action<GameObject> onSpawn = null)
    {
        NetInstantiate(original, original.transform.position, original.transform.rotation, null, onSpawn);
    }

    public void NetInstantiate(GameObject original, Transform parent, Action<GameObject> onSpawn = null)
    {
        NetInstantiate(original, parent.position, parent.rotation, parent, onSpawn);
    }

    public void NetInstantiate(GameObject original, Transform parent, bool instantiateInWorldSpace, Action<GameObject> onSpawn = null)
    {
        NetInstantiate(original, parent.position, parent.rotation, instantiateInWorldSpace ? null : parent, onSpawn);
    }

    public void NetInstantiate(GameObject original, Vector3 position, Quaternion rotation, Action<GameObject> onSpawn = null)
    {
        NetInstantiate(original, position, rotation, null, onSpawn);
    }

    public void NetInstantiate(GameObject original, Vector3 position, Quaternion rotation, Transform parent, System.Action<GameObject> onSpawn = null)
    {
        if (!original.TryGetComponent<SyncedObject>(out SyncedObject syncedPrefab))
        {
            Debug.LogError("<color=red><b>CNet</b></color>: NetInstantiate cannot instantiate a prefab that doesn't have a SyncedObject component");
            return;
        }

        if (!NetManager.Instance.NetPrefabs.Contains(syncedPrefab))
        {
            Debug.LogError("<color=red><b>CNet</b></color>: NetInstantiate cannot instantiate a prefab that isn't added to 'NetPrefabs' in NetManager");
            return;
        }

        SyncedObject synedParent = null;
        if (parent != null && !parent.TryGetComponent<SyncedObject>(out synedParent))
        {
            if (!parent.TryGetComponent<SyncedObject>(out synedParent))
            {
                Debug.LogError("<color=red><b>CNet</b></color>: NetInstantiate cannot instantiate a prefab under a parent that doesn't have a SyncedObject component");
                return;
            }

            if (!NetManager.Instance.NetObjects.TryGetValue(synedParent.NetID, out synedParent))
            {
                Debug.LogError("<color=red><b>CNet</b></color>: NetInstantiate cannot instantiate a prefab under a parent that isn't added to 'NetObjects' in NetManager");
                return;
            }
        }


        using (NetPacket packet = new NetPacket(NetManager.Instance.System, PacketProtocol.TCP))
        {
            packet.Write((short)ServiceType.Spawn);
            packet.Write((short)NetManager.Instance.NetPrefabs.IndexOf(syncedPrefab));
            packet.SerializeStruct<NetVector3>((NetVector3)position);
            packet.SerializeStruct<NetQuaternion>((NetQuaternion)rotation);

            if (parent == null)
            {
                packet.Write((short)-1);
            }
            else
            {
                packet.Write((short)synedParent.NetID);
            }

            if (NetManager.Instance.IsHost)
            {
                int netID = NetManager.Instance.GenerateNetID();
                packet.Write((short)netID);
                onSpawn.Invoke(((SpawnService)NetManager.Instance.NetServices[(int)ServiceType.Spawn]).SpawnPrefab(netID, original, position, rotation, parent));
            }
            else
            {
                ((SpawnService)NetManager.Instance.NetServices[(int)ServiceType.Spawn]).spawnedPrefabActionCache.Enqueue(onSpawn);
            }

            NetManager.Instance.Send(packet, PacketProtocol.TCP);
        }
    }

    public void NetDestroy(GameObject obj, float t = 0.0f)
    {
        if (!obj.TryGetComponent<SyncedObject>(out SyncedObject syncedObj))
        {
            Debug.LogError("<color=red><b>CNet</b></color>: NetDestroy cannot destroy an object that doesn't have a SyncedObject component");
            return;
        }

        if (!NetManager.Instance.NetObjects.TryGetValue(syncedObj.NetID, out syncedObj))
        {
            Debug.LogError("<color=red><b>CNet</b></color>: NetDestroy cannot destroy an object that isn't added to 'NetObjects' in NetManager");
            return;
        }

        using (NetPacket packet = new NetPacket(NetManager.Instance.System, PacketProtocol.TCP))
        {
            packet.Write((short)ServiceType.Destroy);
            packet.Write((short)syncedObj.NetID);

            if (NetManager.Instance.IsHost)
            {
                Destroy(obj, t);
            }

            NetManager.Instance.Send(packet, PacketProtocol.TCP, t);
        }
    }
}