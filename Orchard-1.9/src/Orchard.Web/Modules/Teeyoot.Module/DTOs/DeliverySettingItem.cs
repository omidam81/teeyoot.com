﻿namespace Teeyoot.Module.DTOs
{
    public class DeliverySettingItem
    {
        public double DeliveryCost { get; set; }
        public string State { get; set; }
        public bool Enabled { get; set; }

        public int DeliverSetting { get; set; }

        public int DeliveryTime { get; set; }
    }
}