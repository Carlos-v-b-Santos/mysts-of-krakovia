using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Caixa : MonoBehaviour
{
    [Header("Id da quest aceita")]
    [SerializeField] private string caixa;

    private void OnEnable()
    {
        GameEventsManager.Instance.questEvents.OnStartQuest += StartQuest;
    }

    private void OnDisable()
    {
        GameEventsManager.Instance.questEvents.OnStartQuest -= StartQuest;
    }

    private void StartQuest(string questId)
    {
        if (questId == "destrua_a_caixa")
        {
            this.gameObject.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        print("Caixa destruida");
        GameEventsManager.Instance.miscEvents.CaixaDestruida();
    }
}
