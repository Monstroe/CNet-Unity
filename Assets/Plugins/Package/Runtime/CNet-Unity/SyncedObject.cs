using UnityEngine;

public abstract class SyncedObject : MonoBehaviour
{
    public int NetID { get; internal set; }
}
