using CoinPoker.Controllers;
using CoinPokerCommonLib;
using Hik.Communication.Scs.Client;
using Hik.Communication.Scs.Communication.EndPoints.Tcp;
using Hik.Communication.ScsServices.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CoinPoker
{
    public static class Session
    {
        /// <summary>
        /// Sesja użytkownika
        /// </summary>
        public class UserSession
        {
            // User session varibles
            public IScsServiceClient<IPokerService> Client { get; set; }

            // User
            public long SessionID { get; set; }
            public UserModel User { get; set; }
            public bool IsLogged
            {
                get
                {
                    if (this.SessionID == 0)
                    {
                        return false;
                    }
                    else { return true; }
                }
            }
        }

        /// <summary>
        /// Sesja użytkownika
        /// </summary>
        public static UserSession Data;
        private static ClientReConnecter _reconnector;

        public static bool IsConnected()
        {
            return (Data.Client.CommunicationState == Hik.Communication.Scs.Communication.CommunicationStates.Connected);
        }

        public static IPokerService Proxy()
        {
            return Data.Client.ServiceProxy;
        }

        /// <summary>
        /// Konfiguracja sieci i połączenie
        /// </summary>
        /// 
        public static void NetworkConfiguration()
        {
            Data = new UserSession();
            Data.Client = ScsServiceClientBuilder.CreateClient<IPokerService>(new ScsTcpEndPoint("127.0.0.1", 10048), PokerClient.Instance);
            _reconnector = new ClientReConnecter(Data.Client) { ReConnectCheckPeriod = 1000 };

            try
            {
                Data.Client.ConnectTimeout = 3000;
                Data.Client.Connect();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Błąd podczas łączenia z siecią");
            }
        }
    }
}
