using UnityEngine;
using Unity.Netcode;
using System.Collections;

// O PlayerAnimator agora é um simples recetor de comandos.
public class PlayerAnimator : NetworkBehaviour
{
    [Header("Referências")]
    [SerializeField] private Animator anim;
    [SerializeField] private PlayerController player; // Mantemos para a animação de slash

    // Hashes dos parâmetros para os comandos de evento
    private readonly int attackBoolHash = Animator.StringToHash("attack");
    private readonly int dieBoolHash = Animator.StringToHash("die");
    private readonly int hurtBoolHash = Animator.StringToHash("hurt");

    private void Awake()
    {
        if (anim == null) anim = GetComponent<Animator>();
        if (player == null) player = GetComponentInParent<PlayerController>();
    }

    // A lógica de Update, OnNetworkSpawn, e HandleMoveAnimation foi REMOVIDA.
    // O servidor agora controla tudo.

    // Estes métodos públicos continuam aqui para serem chamados pelos ClientRPCs
    // que estão nos outros scripts (PlayerCombat, PlayerHealth).
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