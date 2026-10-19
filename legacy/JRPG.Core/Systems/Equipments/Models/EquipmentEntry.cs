namespace JRPG.Core.Systems.Equipments.Models
{
    public class EquipmentEntry
    {
        public Equipment Equipment { get; }
        public int Quantity { get; set; }

        public EquipmentEntry(Equipment equipment, int quantity)
        {
            Equipment = equipment;
            Quantity = quantity;
        }
    }
}
