namespace ApiBackend.DTOs.StoreDtos
{
    public class StoreDto
    {
        public int Id { get; set; }
        public string Name { get; set; }       // "Migros MM Kadıköy"
        public string ChainName { get; set; }  // Migros, CarrefourSA, Metro
        public string? Region { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Calculated data for the "Compliance" bar in the Web Dashboard.
        public decimal ComplianceScore { get; set; } // Ex: 92
        public string Status { get; set; }           // "Compliant"
    }
}
