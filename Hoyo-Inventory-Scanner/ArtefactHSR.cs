namespace Hoyo_Inventory_Scanner
{
    class ArtefactHSR : Artefact {
        public string set { get; set; }
        public string slot { get; set; }
        public string location { get; set; }
        public string mainstat { get; set; }
        public string level { get; set; }
        public Substat[] substats { get; set; } = new Substat[4];

        public ArtefactHSR(string set, string slot, string location, string mainstat, string level, Substat[] substats) {
            this.set = set;
            this.slot = slot;
            this.location = location;
            this.mainstat = mainstat;
            this.level = level;
            this.substats = substats;
        }

        public string generateExport() {
            string sub = "";
            foreach (Substat substat in substats) {
                sub += substat.generateExport() + ",";
            }
            sub = sub.Remove(sub.Length - 1);
            return
                "\n{\"set_id\":\"" + set + "\"," +
                "\"slot\":\"" + slot + "\"," +
                "\"rarity\":5," +
                "\"level\":" + level + "," +
                "\"mainstat\":\"" + mainstat + "\"," +
                "\"substats\":[" + sub + "]}";
        }
    }
}
