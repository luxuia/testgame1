-------------------------------------------------------
-- Need.lua (converted from Need.json)
-------------------------------------------------------

return {
  Need = {
    oxygen = {
      LocalizationName = "need_oxygen",
      GrowthRate = 0.3,
      Damage = 20,
      HighToLow = true,
      RestoreNeedAmount = 100,
      RestoreNeedFurn = "oxygen_generator",
      RestoreNeedTime = 10,
      EventActions = {
        OnUpdate = {
          "OnUpdate_Oxygen",
        },
      },
    },
    sleep = {
      LocalizationName = "need_sleep",
      GrowthRate = 0.15,
      HighToLow = true,
      RestoreNeedAmount = 100,
      RestoreNeedFurn = "simple_bed",
      RestoreNeedTime = 30,
    },
  },
}
