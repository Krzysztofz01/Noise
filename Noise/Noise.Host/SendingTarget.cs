using Noise.Core.Peer;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Noise.Host
{
    internal class SendingTarget
    {
        private const int _publicKeyStripLength = 10;
        private List<RemotePeer> _selectedPeers;

        public void AddPeer(RemotePeer peer)
        {
            if (_selectedPeers.Any(p => p.PublicKey == peer.PublicKey)) return;

            _selectedPeers.Add(peer);
        }

        public RemotePeer GetTarget()
        {
            if (!IsSelected() && IsGroup())
                throw new InvalidOperationException("There must be a single peer selected to use this method.");

            return _selectedPeers.Single();
        }

        public IEnumerable<RemotePeer> GetTargets()
        {
            if (!IsSelected() && !IsGroup())
                throw new InvalidOperationException("There must be at least two peers selected to use this method.");

            return _selectedPeers;
        }

        public bool IsGroup()
        {
            return _selectedPeers.Count > 1;
        }

        public bool IsSelected()
        {
            return _selectedPeers.Count > 0;
        }

        public string GetTargetPrefix()
        {
            if (!IsSelected())
                throw new InvalidOperationException("There must a at least one peer selected to use this method.");
            
            if (_selectedPeers.Count == 1)
            {
                var selectedPeer = GetTarget();

                return selectedPeer.Alias != "Anonymous"
                    ? selectedPeer.Alias
                    : selectedPeer.PublicKey[.._publicKeyStripLength];
            }

            return $"Group[{_selectedPeers.Count}]";
        }

        public void Reset()
        {
            Initialize();
        }

        private void Initialize()
        {
            _selectedPeers = new List<RemotePeer>();
        }

        public SendingTarget() => Initialize();
    }
}
