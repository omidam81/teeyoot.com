namespace Teeyoot.Module.Models
{
    /// <summary>
    /// This class is used FOR DOMESTIC delivery settings only. 
    /// </summary>
    public class DeliverySettingRecord
    {
        public virtual int Id { get; protected set; }
        public virtual string State { get; set; }
        public virtual bool Enabled { get; set; }
        public virtual CountryRecord Country { get; set; }
        public virtual double PostageCost { get; set; }
        public virtual double CodCost { get; set; }

        public virtual int DeliveryTime { get; set; }
    }
}