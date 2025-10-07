using UnityEngine;
using Unity.Netcode;

public class NetworkUI : MonoBehaviour
{
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
        gameObject.SetActive(false); // Esconde os botões depois de conectar
    }

    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
        gameObject.SetActive(false); // Esconde os botões depois de conectar
    }
}