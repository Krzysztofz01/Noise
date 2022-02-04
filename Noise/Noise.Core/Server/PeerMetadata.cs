using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Noise.Core.Server
{
    public class PeerMetadata
    {
        internal CancellationTokenSource TokenSource { get; set; }
        internal CancellationToken Token { get; set; }

        public void Dispose()
        {

        }

        public PeerMetadata(TcpClient tcpClient)
        {

        }
    }
}
