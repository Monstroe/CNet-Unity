using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestGameManager : NetBehavior
{
    [SerializeField] private GameObject testHostPrefab;
    [SerializeField] private GameObject testGuestPrefab;

    // Start is called before the first frame update
    void Start()
    {
        if (!NetManager.Instance.IsHost)
        {
            Debug.Log("I am the guest");
            NetInstantiate(testHostPrefab, Vector3.zero, Quaternion.identity, (hostObject) =>
            {
                Debug.Log("Instantiated guest prefab: " + hostObject.name);
                if (!NetManager.Instance.IsHost)
                {
                    NetInstantiate(testGuestPrefab, Vector3.zero, Quaternion.identity, (guestObject) =>
                    {
                        Debug.Log("Instantiated another guest prefab: " + guestObject.name);
                    });
                }
            });
        }
        else
        {
            Debug.Log("I am the host");
            NetFXManager.Instance.PlaySFX("test.wav", 1.0f);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
