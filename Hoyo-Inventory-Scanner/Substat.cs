namespace Hoyo_Inventory_Scanner
{
    public class Substat {
        public string stat { get; set; }
        public string value { get; set; }

        public Substat(string stat, string value) {
            this.stat = stat;
            this.value = value;
        }

        public string generateExport() {
            return "{\"key\":\"" + stat + "\",\"value\":" + value + "}";
        }
    }
}
