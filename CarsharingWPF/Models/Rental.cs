using System;

namespace CarsharingWPF.Models
{
    public class Rental
    {
        public int Id { get; set; }
        public int ClientId { get; set; }
        public int CarId { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; } // Значення може бути порожнім (null), поки авто не повернули
        public int? TotalPrice { get; set; }
    }
}
