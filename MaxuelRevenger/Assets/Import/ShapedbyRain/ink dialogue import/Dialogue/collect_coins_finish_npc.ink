INCLUDE globals.ink

-> collectCoinsFinish

=== collectCoinsFinish ===
{ VisitPillarsQuestState:
    - "FINISHED": -> finished
    - else: -> default
}

= finished
Thank you!
-> END

= default
Hm? What do you want?
* [Nothing, I guess.]
    -> END
* { VisitPillarsQuestState == "CAN_FINISH" } [Here are some cubes.]
    ~ FinishQuest(VisitPillarsQuestId)
    Oh? These cubes are for me? Thank you!
-> END