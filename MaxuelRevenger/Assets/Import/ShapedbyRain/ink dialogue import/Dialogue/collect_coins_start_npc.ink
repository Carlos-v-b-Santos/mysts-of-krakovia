INCLUDE globals.ink

->collectCoinsStart

=== collectCoinsStart ===
{ VisitPillarsQuestState :
    - "REQUIREMENTS_NOT_MET": -> requirementsNotMet
    - "CAN_START": -> canStart
    - "IN_PROGRESS": -> inProgress
    - "CAN_FINISH": -> canFinish
    - "FINISHED": -> finished
    - else: -> default
}

= requirementsNotMet
// not possible for this quest, but putting something here anyways
Volte mais tarde.
-> END

= canStart
Você consegue pegar 3 cubos?
* [Yes]
    ~ StartQuest(VisitPillarsQuestId)
    Ótimo!
* [No]
    Oh, tudo bem. Volte se você mudar de ideia.
- -> END

= inProgress
Não esqueça de pegar aqueles cubos, certo.
-> END

= canFinish
Oh! Você pegou todos os cubos?
~ FinishQuest(VisitPillarsQuestId)
Obrigado por pegar todos os cubos!
-> END

= finished
Obrigado por pegar todos os cubos!
-> END

= default
eae, tudo bem?
    -> END