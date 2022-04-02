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

        public static async Task<bool> SavePeerConfigurationCipher(PeerConfiguration peerConfiguration)
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), _peerConfigurationFileName);

                var encryptedPeerConfiguration = PeerEncryption.EncryptPeerConfiguration(peerConfiguration);

                await System.IO.File.WriteAllTextAsync(filePath, encryptedPeerConfiguration);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string GetPeerConfigurationCipher()
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), _peerConfigurationFileName);

                return System.IO.File.ReadAllText(filePath);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public static async Task<bool> SavePeerCard(PeerConfiguration peerConfiguration)
        {
            try
            {
                var fileName = $"{ DateTime.Now.ToString("yyyyMMddHHmmss") }.noisepeer";
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), fileName);

                await System.IO.File.WriteAllTextAsync(filePath, peerConfiguration.PublicKey);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public static string GetPeerCardPublicKey(string filePath)
        {
            try
            {
                if (!System.IO.File.Exists(filePath))
                    throw new FileNotFoundException("Peer card file not found.");

                return System.IO.File.ReadAllText(filePath);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
