namespace Hoyo_Inventory_Scanner
{
    public class Substat {
        private string stat { get; set; }
        private float value { get; set; }

        public Substat(string stat, float value) {
            this.stat = stat;
            this.value = value;
        }
    }
}
