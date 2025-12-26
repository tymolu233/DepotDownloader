// This file is subject to the terms and conditions defined
// in file 'LICENSE', which is part of this source code package.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SteamKit2.CDN;

namespace DepotDownloader
{
    /// <summary>
    /// CDNClientPool provides a pool of connections to CDN endpoints, requesting CDN tokens as needed
    /// </summary>
    class CDNClientPool
    {
        private readonly Steam3Session steamSession;
        private readonly uint appId;
        public Client CDNClient { get; }
        public Server ProxyServer { get; private set; }

        private readonly List<Server> servers = [];
        private int nextServer;

        public CDNClientPool(Steam3Session steamSession, uint appId)
        {
            this.steamSession = steamSession;
            this.appId = appId;

            if (steamSession != null)
            {
                CDNClient = new Client(steamSession.steamClient);
            }
            else
            {
                var clientConfiguration = SteamConfiguration.Create(config =>
                    config.WithHttpClientFactory(() => HttpClientFactory.CreateHttpClient()));
                CDNClient = new Client(clientConfiguration);
            }
        }

        public async Task UpdateServerList()
        {
            List<Server> serverList;

            if (steamSession == null)
            {
                if (Client.UseLancacheServer)
                {
                    // If we are using Lancache, we don't need to query Steam for servers
                    // But we need at least one server in the list to satisfy the loop
                    servers.Add(new Server { Host = "lancache", Protocol = Protocol.HTTP, Type = "CDN" });
                    return;
                }

                Console.WriteLine("No active session, logging in anonymously to retrieve CDN servers...");
                var anonymousSession = new Steam3Session(new SteamUser.LogOnDetails
                {
                    Username = null,
                });

                if (!anonymousSession.WaitForCredentials())
                {
                    throw new Exception("Failed to login anonymously for CDN server list.");
                }

                serverList = await anonymousSession.steamContent.GetServersForSteamPipe();
                anonymousSession.Disconnect();
            }
            else
            {
                serverList = await this.steamSession.steamContent.GetServersForSteamPipe();
            }

            ProxyServer = serverList.Where(x => x.UseAsProxy).FirstOrDefault();

            var weightedCdnServers = serverList

            var weightedCdnServers = serverList
                .Where(server =>
                {
                    var isEligibleForApp = server.AllowedAppIds.Length == 0 || server.AllowedAppIds.Contains(appId);
                    return isEligibleForApp && (server.Type == "SteamCache" || server.Type == "CDN");
                })
                .Select(server =>
                {
                    AccountSettingsStore.Instance.ContentServerPenalty.TryGetValue(server.Host, out var penalty);

                    return (server, penalty);
                })
                .OrderBy(pair => pair.penalty).ThenBy(pair => pair.server.WeightedLoad);

            foreach (var (server, weight) in weightedCdnServers)
            {
                for (var i = 0; i < server.NumEntries; i++)
                {
                    this.servers.Add(server);
                }
            }

            if (this.servers.Count == 0)
            {
                throw new Exception("Failed to retrieve any download servers.");
            }
        }

        public Server GetConnection()
        {
            return servers[nextServer % servers.Count];
        }

        public void ReturnConnection(Server server)
        {
            if (server == null) return;

            // nothing to do, maybe remove from ContentServerPenalty?
        }

        public void ReturnBrokenConnection(Server server)
        {
            if (server == null) return;

            lock (servers)
            {
                if (servers[nextServer % servers.Count] == server)
                {
                    nextServer++;

                    // TODO: Add server to ContentServerPenalty
                }
            }
        }
    }
}
