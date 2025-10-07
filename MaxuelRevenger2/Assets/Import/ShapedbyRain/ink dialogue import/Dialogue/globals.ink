VAR pokemon_name = ""
VAR VisitPillarsQuestId = "VisitPillarsQuest"
VAR DestruaCaixasId = "DestruaCaixasQuest"
VAR VisitPillarsQuestState = "REQUIREMENTS_NOT_MET"
VAR DestruaCaixasQuestState = "REQUIREMENTS_NOT_MET"

//~ playEmote("exclamation")



//EXTERNAL playEmote(emoteName)
EXTERNAL StartQuest(questId)
EXTERNAL AdvanceQuest(questId)
EXTERNAL FinishQuest(questId)