using System.Collections.Generic;

namespace PowerPlant.Models
{
    public class PowerPlantData
    {
        public int load { get; set; }
        public Fuels fuels { get; set; }
        public List<PowerPlantItem> powerplants { get; set; }
    }
}
