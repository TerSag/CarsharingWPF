using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace CarsharingWPF.Models
{
    public class Rental
    {
        public int Id { get; set; }

        public int ClientId { get; set; }
        public int CarId { get; set; }

        public DateTime StartTime { get; set; } = DateTime.Now;

        public DateTime? EndTime { get; set; }

        public int? TotalPrice { get; set; }

        public Client? Client { get; set; }

        public Car? Car { get; set; }

        [NotMapped]
        public bool IsActive => EndTime == null;
    }
} 