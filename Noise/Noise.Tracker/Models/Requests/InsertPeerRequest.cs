using System.ComponentModel.DataAnnotations;

namespace Noise.Tracker.Models.Requests
{
    public class InsertPeerRequest
    {
        [Required]
        public string PublicKey { get; set; }
        
        [Required]
        public string Endpoint { get; set; }
    }
}
