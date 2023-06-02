using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using PowerPlant.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PowerPlant.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PowerPlantController : ControllerBase
    {
        private readonly ILogger<PowerPlantController> _logger;

        public PowerPlantController(ILogger<PowerPlantController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult PostProductionPlan([FromBody] PowerPlantData payload)
        {
            // Step 1: Extract data from the payload
            int load = payload.load;
            Fuels fuels = payload.fuels;
            List<PowerPlantItem> powerplants = payload.powerplants;

            // Calculate the merit-order based on fuel costs
            var sortedPowerPlants = powerplants.OrderBy(p =>
            {
                double cost = 0;
                if (p.type == "gasfired")
                    cost = fuels.gas * (1 / p.efficiency);
                else if (p.type == "turbojet")
                    cost = fuels.kerosine * (1 / p.efficiency);
                return cost;
            });

            // Allocate power production for each power plant
            var powerProductions = new List<double>();
            foreach (var powerPlant in sortedPowerPlants)
            {
                if (powerPlant.type == "windturbine")
                {
                    double windPower = (powerPlant.pmax * fuels.wind) / 100.0;
                    powerProductions.Add(windPower);
                    load -= (int)windPower;
                }
                else if (powerPlant.type == "gasfired" || powerPlant.type == "turbojet")
                {
                    int powerOutput = Math.Min(load, powerPlant.pmax);
                    powerOutput = Math.Max(powerOutput, powerPlant.pmin);
                    powerProductions.Add(powerOutput);
                    load -= powerOutput;
                }
            }

            // Adjust the last power plant's production to match the load
            if (load != 0)
            {
                int lastIndex = powerProductions.Count - 1;
                powerProductions[lastIndex] += load;
            }

            // Round the power production values to the nearest multiple of 0.1 MW
            powerProductions = powerProductions.Select(p => Math.Round(p, 1)).ToList();

            // Create the response JSON object
            var response = new
            {
                powerplants = powerplants.Select((p, index) => new
                {
                    name = p.name,
                    p = powerProductions[index]
                })
            };

            return Ok(response);
        }
    }
}
