namespace ApiBackend.DTOs.StoreDtos
{
    public class CreateStoreDto
    {
        public string Name { get; set; }
        public string ChainName { get; set; } // Migros, Şok vs.
        public string Region { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}