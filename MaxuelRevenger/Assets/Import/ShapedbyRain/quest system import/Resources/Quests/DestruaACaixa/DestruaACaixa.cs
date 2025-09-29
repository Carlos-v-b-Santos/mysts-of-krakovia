using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestruaACaixa : QuestStep
{
    [Header("Parameters")]
    [SerializeField] private int caixasDestruidas = 0;
    [SerializeField] private int caixasParaDestruir = 3;

    private void OnEnable()
    {
        GameEventsManager.Instance.miscEvents.onCaixaDestruida += CaixaDestruida;
    }

    private void OnDisable()
    {
        GameEventsManager.Instance.miscEvents.onCaixaDestruida -= CaixaDestruida;
    }

    private void CaixaDestruida()
    {
        if (caixasDestruidas < caixasParaDestruir)
        {
            caixasDestruidas++;
            UpdateState();
        }

        if (caixasDestruidas >= caixasParaDestruir)
        {
            FinishQuestStep();
        }
    }
    private void UpdateState()
    {
        string state = caixasDestruidas.ToString();
        string status = "Destruidas " + caixasDestruidas + " / " + caixasParaDestruir + " caixas.";
        ChangeState(state, status);
    }

    protected override void SetQuestStepState(string state)
    {
        this.caixasDestruidas = System.Int32.Parse(state);
        UpdateState();
    }
}
