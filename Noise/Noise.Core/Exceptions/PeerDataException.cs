using System;

namespace Noise.Core.Exceptions
{
    public class PeerDataException : Exception
    {
        public const string _publicKeyNotFound = "Peer with given public key not found or is ambiguous.";
        public const string _signatureNotFound = "Peer with given signature not found or is ambiguous.";
        public const string _endpointNotFound = "Given endpoint is unknown or is ambiguous.";
        public const string _aliasNotFound = "Peer with given alias not found or is ambiguous.";
        public const string _ordinalNumberNotFound = "Peer with given ordinal number not found or is ambiguous.";
        public const string _wrongPeerSecret = "Invalid password for given peer.";
        public const string _undefinedMessage = "Unexpected peer data behavior.";
        public const string _versionMismatch = "A version unhandled version mismatch detected between peer and host.";

        private PeerDataException() { }
        public PeerDataException(PeerDataProblemType peerDataProblemType) : base(MapReasonToMessage(peerDataProblemType)) { }

        private static string MapReasonToMessage(PeerDataProblemType peerDataProblemType)
        {
            return peerDataProblemType switch
            {
                PeerDataProblemType.PUBLIC_KEY_NOT_FOUND => _publicKeyNotFound,
                PeerDataProblemType.SIGNATURE_NOT_FOUND => _signatureNotFound,
                PeerDataProblemType.ENDPOINT_NOT_FOUND => _endpointNotFound,
                PeerDataProblemType.ALIAS_NOT_FOUND => _aliasNotFound,
                PeerDataProblemType.ORDINAL_NUMER_NOT_FOUND => _ordinalNumberNotFound,
                PeerDataProblemType.WRONG_PEER_SECRET => _wrongPeerSecret,
                PeerDataProblemType.VERSION_MISMATCH => _versionMismatch,
                _ => _undefinedMessage,
            };
        }
    }

    public enum PeerDataProblemType
    {
        PUBLIC_KEY_NOT_FOUND,
        SIGNATURE_NOT_FOUND,
        ENDPOINT_NOT_FOUND,
        ALIAS_NOT_FOUND,
        ORDINAL_NUMER_NOT_FOUND,
        WRONG_PEER_SECRET,
        VERSION_MISMATCH
    }
}
