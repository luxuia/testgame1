-------------------------------------------------------
-- Utility.lua (converted from Utility.json)
-------------------------------------------------------

return {
  Utility = {
    power_cable = {
      LocalizationName = "util_power_cable",
      LocalizationDescription = "util_power_cable_desc",
      TypeTags = {
        "Power",
      },
      OrderActions = {
        Build = {
          JobTime = 1,
          Inventory = {
            copper_plate = 1,
          },
        },
        Deconstruct = {
          JobTime = 1,
          Inventory = {
            copper_plate = 1,
          },
        },
      },
    },
    fluid_pipe = {
      LocalizationName = "util_fluid_pipe",
      LocalizationDescription = "util_fluid_pipe_desc",
      TypeTags = {
        "Fluid",
      },
      OrderActions = {
        Build = {
          JobTime = 1,
          Inventory = {
            steel_plate = 2,
          },
        },
        Deconstruct = {
          JobTime = 1,
          Inventory = {
            steel_plate = 1,
          },
        },
      },
    },
  },
}
