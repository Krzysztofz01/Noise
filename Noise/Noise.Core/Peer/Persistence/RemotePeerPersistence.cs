namespace Noise.Core.Peer.Persistence
{
    public class RemotePeerPersistence
    {
        public int Identifier { get; set; }
        public string PublicKey { get; set; }
        public string Alias { get; set; }
        public string ReceivingSignature { get; set; }
        public string SendingSignature { get; set; }
    }
}
