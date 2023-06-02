namespace PowerPlant.Models
{
    public class PowerPlantItem
    {
        public string name { get; set; }
        public string type { get; set; }
        public double efficiency { get; set; }
        public int pmin { get; set; }
        public int pmax { get; set; }
    }
}
