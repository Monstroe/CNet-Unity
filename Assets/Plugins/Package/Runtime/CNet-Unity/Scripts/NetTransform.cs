using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SyncedTransform))]
public class NetTransform : MonoBehaviour
{
    public Vector3 Position { get => syncedTransform.Position; set => syncedTransform.Position = value; }
    public Quaternion Rotation { get => syncedTransform.Rotation; set => syncedTransform.Rotation = value; }
    public Vector3 Scale { get => syncedTransform.Scale; set => syncedTransform.Scale = value; }

    public Vector3 LocalPosition
    {
        get
        {
            return parentTransform != null ? parentTransform.InverseTransformPoint(Position) : Position;
        }
        set
        {
            if (parentTransform != null)
                Position = parentTransform.TransformPoint(value);
            else
                Position = value;
        }
    }

    public Quaternion LocalRotation
    {
        get
        {
            return parentTransform != null ? Quaternion.Inverse(parentTransform.rotation) * Rotation : Rotation;
        }
        set
        {
            if (parentTransform != null)
                Rotation = parentTransform.rotation * value;
            else
                Rotation = value;
        }
    }

    public Vector3 LocalScale
    {
        get
        {
            return parentTransform != null ? new Vector3(Scale.x / parentTransform.localScale.x, Scale.y / parentTransform.localScale.y, Scale.z / parentTransform.localScale.z) : Scale;
        }
        set
        {
            if (parentTransform != null)
                Scale = new Vector3(value.x * parentTransform.localScale.x, value.y * parentTransform.localScale.y, value.z * parentTransform.localScale.z);
            else
                Scale = value;
        }
    }

    private SyncedTransform syncedTransform;
    private Transform parentTransform;

    void Awake()
    {
        syncedTransform = GetComponent<SyncedTransform>();
        parentTransform = transform.parent;
    }

    public void Rotate(Vector3 eulers)
    {
        Rotation = Quaternion.Euler(eulers) * Rotation;
    }

    public void Rotate(float xAngle, float yAngle, float zAngle)
    {
        Rotation = Quaternion.Euler(xAngle, yAngle, zAngle) * Rotation;
    }
}
