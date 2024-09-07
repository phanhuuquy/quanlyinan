namespace QuanLyInAn.Configurations
{
    public class JwtConfig
    {
        public string SecretKey { get; set; }
        public int ExpirationInHours { get; set; }
        public string Issuer { get; set; } 
        public string Audience { get; set; }
    }
}
