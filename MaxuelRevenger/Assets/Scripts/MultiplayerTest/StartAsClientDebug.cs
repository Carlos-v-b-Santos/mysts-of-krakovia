using Unity.Netcode;
using UnityEngine;

public class StartAsClientDebug : MonoBehaviour
{
    #if !UNITY_EDITOR
    void Awake()
    {
        void Start()
        {
            NetworkManager.Singleton.StartClient();
        }
    }
    #endif
}
