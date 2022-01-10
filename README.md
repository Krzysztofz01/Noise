
![noise-logo-long](https://user-images.githubusercontent.com/46250989/148530923-17e5ac63-b8bf-496b-8665-b74eae53334a.png)

#  Noise
Noise is a decentralized connection (TCP) communication protocol based on a peer-to-peer network. It guarantees anonymity and security of transmitted data, due to several layers of encryption. The name itself to some extent explains how this protocol works, because communication looks in such a way that every peer sends the same package to any peer, which makes it difficult to determine the identity of a particular peer in the network, this huge amount of network traffic can be called a like noise.

##  Security and privacy
Each peer has a pair of RSA keys. Thanks to this, each message is end-to-end encrypted. The key itself, as in the case of many cryptocurrencies, is treated like a identifier, but that's not all when it comes to cryptographic functionalities. When sending messages or discovery packets (MSG/DSC and KEY packet's), the content of the message is encrypted using AES-GCM. It is worth noting that for each packet the key AES-GCM will be different, in the context of one peer. The key itself is generated using so-called "cryptographically secure random numbers". Later the key to this cipher is encrypted by the public RSA key of the peer, to which we want to send a message. Such a pair of packets is then sent to all known peers.

![noise-graph2](https://user-images.githubusercontent.com/46250989/148533067-fcc44cc6-59fb-4dfc-a21c-ab3ca2944a26.jpg)
##  Automated host discovery
In order to discover new peers, we can add them by hand (public key and IP address) but the protocol itself works so that after connecting to the network, so-called "Discovery" packets are sent. These types of packets work the same way as discussed above message packets, but the content of the payload contains a list of public keys and IP addresses of known hosts. Such packets are then sent to all known peers.

![noise-graph1](https://user-images.githubusercontent.com/46250989/148530904-e084e8e7-39d7-4933-b49b-6f0e445405b6.jpg)

## Installation
```
# Windows
cmd build.cmd

# Linux
chmod +x ./build.sh
sudo sh ./build.sh

```

## Usage
After running the program, we can use the ```help``` command, which will list all available commands. Command list:

- EXIT - Close connections, save local data and exit.
- CLEAR - Clear the screen.
- LIST - List available peer keys, aliases or endpoints.
- SELECT - Select a peer to perform interactions.
- RESET - Reset selected peer.
- SEND - Send message to selected peer.
- ALIAS - Set alias to certain peer.
- INSERT - Insert new peer key and optional alias or a endpoint.
- HELP - Show available commands.

## Technological stack
The application was developed using the open-source .NET Core technology. It has no third party dependencies. Only .NET standard libraries are used. Unit testing uses xUnit.

## Roadmap
Functionalities that need to be implemented or repaired:

🔲 The multithreaded server high CPU and memory usage.

🔲 Packet pairs are sometimes corrupted.

🔲 Correct way of handling 2 way disconnection.

🔲 Scope of the peer configuration.

🔲 Local encryption of the peer.noise file.

🔲 Migration from .NET5 to .NET6 LTS


The terminal promot is inspired by the computer found in the game Submchanie 4: The Lab by Mateusz Skutnik. In my opinion, one of the best series of Flash games.