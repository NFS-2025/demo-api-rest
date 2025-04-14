namespace demo_api_rest.Models
{
    public class CsrfTrapRequest
    {
        public string? Email { get; set; }
        public string? Cookies { get; set; }
        public string? AdditionalData { get; set; }
    }
}