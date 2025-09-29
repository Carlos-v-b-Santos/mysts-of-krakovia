INCLUDE globals.ink

->destruaCaixasStart

=== destruaCaixasStart ===
{ DestruaCaixasQuestState :
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
    Você poderia destruir as caixas? Por favorzinho!
* [Yes]
    ~ StartQuest(DestruaCaixasId)
    Isso, destrua TODAS!
* [No]
    Vai, destrua as caixas...
- -> END

= inProgress
As caixas, destrua todas!.
-> END

= canFinish
ISSO, CAIXAS DESTRUIDAS AHAHAHAHA!
~ FinishQuest(DestruaCaixasId)
DESTRUA MAIS E MAIS!
-> END

= finished
DESTRUIÇÃO ÀS CAIXAS HAHAHAHAHAHAHAHAHA!
-> END

= default
caixas?
    -> END