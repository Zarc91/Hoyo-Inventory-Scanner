namespace Hoyo_Inventory_Scanner
{
    interface Artefact {
        string set { get; set; }
        string slot { get; set; }
        string location { get; set; }
        string mainstat { get; set; }
        string level { get; set; }
        Substat[] substats { get; set; }

    string generateExport();
    }
}
