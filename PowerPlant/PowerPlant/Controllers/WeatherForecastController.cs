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
            double load = Convert.ToDouble(payload.load);
            Fuels fuels = payload.fuels;
            List<PowerPlantItem> powerplants = payload.powerplants;

            // Step 2: Calculate the merit-order based on fuel costs taking into account efficiency
            //checking each power plant type and based on it we choos ethe correct fuel cost and multiply it with
            //the 1 over the efficincy to know the cost per hour
            var sortedPowerPlants = powerplants.OrderBy(p =>
            {
                double cost = 0;
                if (p.type == "gasfired")
                    cost = fuels.gas * (1 / p.efficiency);
                else if (p.type == "turbojet")
                    cost = fuels.kerosine * (1 / p.efficiency);
                return cost * p.pmin; // cost is also related to the pmin and the order will depend on both of that
            });

            // Step 3:Allocate power production for each power plant
            var powerProductions = new List<double>();
            int lastIndexHasP = 0;
            foreach (var powerPlant in sortedPowerPlants)
            {
                if (load != 0)
                {
                    if (powerPlant.type == "windturbine")
                    {
                        double windPower = (powerPlant.pmax * fuels.wind) / 100.0;
                        powerProductions.Add(windPower);
                        load -= (int)windPower;
                    }
                    else if (powerPlant.type == "gasfired" || powerPlant.type == "turbojet")
                    {
                        double powerOutput = Math.Min(load, powerPlant.pmax);
                        powerOutput = Math.Max(powerOutput, powerPlant.pmin);
                        powerProductions.Add(powerOutput);
                        load -= powerOutput;
                    }
                    lastIndexHasP = lastIndexHasP + 1;
                }
                else
                    powerProductions.Add(0);// in case the load is already distrbuted 

            }
            //Check if last powerplant didn't reach its max
            //Assign him the first expensive one load which will be always tj1 turbojet using kerosine
            if (powerProductions[lastIndexHasP - 1] < sortedPowerPlants.ElementAt(lastIndexHasP - 1).pmax)
            {
                powerProductions[lastIndexHasP - 1] += powerProductions[0];
                powerProductions[0] = 0;
            }
            // Round the power production values to the nearest multiple of 0.1 MW
            powerProductions = powerProductions.Select(p => Math.Round(p, 1)).ToList();

            // Step 4:Create the response JSON object
            var response = new
            {
                powerplants = sortedPowerPlants.Select((p, index) => new
                {
                    name = p.name,
                    p = powerProductions[index]
                })
            };

            return Ok(response);
        }

    }
}
