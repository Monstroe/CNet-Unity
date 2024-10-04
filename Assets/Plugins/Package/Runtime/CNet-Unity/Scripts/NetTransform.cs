using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SyncedTransform))]
public class NetTransform : MonoBehaviour
{
    public Vector3 Position { get => syncedTransform.Position; set => syncedTransform.Position = value; }
    public Quaternion Rotation { get => syncedTransform.Rotation; set => syncedTransform.Rotation = value; }
    public Vector3 Scale { get => syncedTransform.Scale; set => syncedTransform.Scale = value; }

    private SyncedTransform syncedTransform;

    void Awake()
    {
        syncedTransform = GetComponent<SyncedTransform>();
    }
}
