﻿using System; 


namespace EliteAPI
{
    public class PayFineInfo
    {
        public DateTime timestamp { get; set; }
        public int Amount { get; set; }
        public bool AllFines { get; set; }
        public int ShipID { get; set; }
    }
}
