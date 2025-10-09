using UnityEngine;
using Unity.Netcode;
using System.Collections;

// O PlayerAnimator agora � um simples recetor de comandos.
public class PlayerAnimator : NetworkBehaviour
{
    [Header("Refer�ncias")]
    [SerializeField] private Animator anim;
    [SerializeField] private PlayerController player; // Mantemos para a anima��o de slash

    // Hashes dos par�metros para os comandos de evento
    private readonly int attackBoolHash = Animator.StringToHash("attack");
    private readonly int dieBoolHash = Animator.StringToHash("die");
    private readonly int hurtBoolHash = Animator.StringToHash("hurt");

    private void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (player == null) player = GetComponentInParent<PlayerController>();
    }

    // A l�gica de Update, OnNetworkSpawn, e HandleMoveAnimation foi REMOVIDA.
    // O servidor agora controla tudo.

    // Estes m�todos p�blicos continuam aqui para serem chamados pelos ClientRPCs
    // que est�o nos outros scripts (PlayerCombat, PlayerHealth).
    public void TriggerAttackAnimation()
    {
        StartCoroutine(SetBoolForOneFrameCoroutine(attackBoolHash));
        if (player.combat.slashAnimator != null)
        {
            player.combat.slashAnimator.SetTrigger("attack");
        }
    }

    public void TriggerHurtAnimation()
    {
        StartCoroutine(SetBoolForOneFrameCoroutine(hurtBoolHash));
    }

    public void TriggerDieAnimation()
    {
        anim.SetBool(dieBoolHash, true);
    }

    private IEnumerator SetBoolForOneFrameCoroutine(int boolHash)
    {
        anim.SetBool(boolHash, true);
        yield return null;
        anim.SetBool(boolHash, false);
    }
}