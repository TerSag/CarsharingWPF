namespace CarsharingWPF.Models
{
    public class Car
    {
        public int Id { get; set; }
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public int PricePerHour { get; set; }
        public bool IsAvailable { get; set; } = true;

        public string FullName => $"{Brand} {Model} ({Year})";
    }
}
