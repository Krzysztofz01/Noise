using Noise.Core.Models;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using IO = System.IO;

namespace Noise.Core.File
{
    public class FileHandler
    {
        private const string _privateKeyFileName = "private.noise";
        private const string _peerDataFileName = "peer.noise";

        public bool PrivateKeyFileExists()
        {
            var filePath = IO.Path.Combine(IO.Directory.GetCurrentDirectory(), _privateKeyFileName);

            return IO.File.Exists(filePath);
        }

        public async Task<bool> SavePrivateKeyToFileAsync(string privateKeyFile)
        {
            try
            {
                var filePath = IO.Path.Combine(IO.Directory.GetCurrentDirectory(), _privateKeyFileName);

                await IO.File.WriteAllTextAsync(filePath, privateKeyFile);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> SavePeerDataToFileAsync(PeerData peerData)
        {
            try
            {
                var filePath = IO.Path.Combine(IO.Directory.GetCurrentDirectory(), _peerDataFileName);

                var serializedPeerData = JsonSerializer.Serialize(peerData);

                await IO.File.WriteAllTextAsync(filePath, serializedPeerData);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string GetPrivateKeyFromFile()
        {
            var filePath = IO.Path.Combine(IO.Directory.GetCurrentDirectory(), _privateKeyFileName);

            return IO.File.ReadAllText(filePath);
        }

        public PeerData GetPeerDataFromFile()
        {
            var filePath = IO.Path.Combine(IO.Directory.GetCurrentDirectory(), _peerDataFileName);

            var serializedPeerData = IO.File.ReadAllText(filePath);

            return JsonSerializer.Deserialize<PeerData>(serializedPeerData);
        }
    }
}
