namespace Hoyo_Inventory_Scanner
{
    class Artefact {
        private string set { get; set; }
        private string slot { get; set; }
        private string location { get; set; }
        private string mainstat { get; set; }
        private string rarity { get; set; }
        private Substat[] substats { get; set; } = new Substat[4];

        public Artefact(string set, string slot, string location, string mainstat, string rarity, Substat[] substats) {
            this.set = set;
            this.slot = slot;
            this.location = location;
            this.mainstat = mainstat;
            this.rarity = rarity;
            this.substats = substats;
        }

        public string toString() {

            return "";
        }
    }
}
