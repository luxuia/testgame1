-------------------------------------------------------
-- GameEvents.lua (converted from GameEvents.json)
-------------------------------------------------------

return {
  GameEvent = {
    NewCrewMember = {
      MaxRepeats = 3,
      Preconditions = {
        "Precondition_Event_NewCrewMember",
      },
      ExecutionActions = {
        "Execute_Event_NewCrewMember",
      },
    },
  },
}
