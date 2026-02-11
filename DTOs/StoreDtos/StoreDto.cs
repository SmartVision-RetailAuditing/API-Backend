namespace ApiBackend.DTOs.StoreDtos
{
    public class StoreDto
    {
        public int Id { get; set; }
        public string Name { get; set; }        // "Migros MM Kadıköy"
        public string ChainName { get; set; }   // "Migros"
        public string Region { get; set; }      // "Anadolu Yakası"
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Web Dashboard'daki  "Compliance" barı için hesaplanmış veri
        public decimal ComplianceScore { get; set; } // Örn: 92
        public string Status { get; set; }           // "Compliant"
    }
}
