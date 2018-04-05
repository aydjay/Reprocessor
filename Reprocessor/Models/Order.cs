using System;

namespace Reprocessor.Models
{
    internal class Order
    {
        public string OrderId { get; set; }

        public int TypeId { get; set; }

        public string LocationId { get; set; }
        public string VolumeTotal { get; set; }
        public string VolumeRemain { get; set; }
        public string MinVolume { get; set; }
        public double Price { get; set; }
        public bool IsBuyOrder { get; set; }
        public int Duration { get; set; }
        public DateTime Issued { get; set; }
        public string Range { get; set; }
    }
}