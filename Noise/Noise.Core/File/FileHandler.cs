using Noise.Core.Peer;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Noise.Core.File
{
    public class FileHandler
    {
        private const string _peerConfigurationFileName = "peer.noise";

        public static bool PeerConfigurationFileExists()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), _peerConfigurationFileName);

            return System.IO.File.Exists(filePath);
        }

        public static async Task<bool> SavePeerConfigurationFile(PeerConfiguration peerConfiguration)
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), _peerConfigurationFileName);

                var serializedPeerConfiguration = peerConfiguration.Serialize();

                await System.IO.File.WriteAllTextAsync(filePath, serializedPeerConfiguration);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static PeerConfiguration GetPeerConfiguration()
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), _peerConfigurationFileName);

                var serializedPeerConfiguration = System.IO.File.ReadAllText(filePath);

                return PeerConfiguration.Factory.Deserialize(serializedPeerConfiguration);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
