using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crestron.RAD.Common.Transports;
using System.Net.Sockets;
using PrimS.Telnet;
using Crestron.SimplSharp;

namespace MyPKG
{
    public class myPDU_Transport : ATransportDriver
    {
        #region Declarations
        public Client telnetClient { get; set; }
        public bool connectionStatus;
        public bool authStatus;

        private string deviceIP;
        private int devicePort;
        
        #endregion


       public myPDU_Transport(string devIP, int devPort)
        {
            this.deviceIP = devIP;
            this.devicePort = devPort;
        }

        public string GetResponse()
        {

           return telnetClient.TerminatedRead("\n>");
        }

        public override void SendMethod(string message, object[] paramaters)
        {

            ErrorLog.Notice("@@Transport.SendMethod()@@  Message:{0} State:{1}", message, telnetClient.IsConnected);

            telnetClient.WriteLine(message);


        }

        public override void Start()
        {

           

            telnetClient = new Client(deviceIP, devicePort, System.Threading.CancellationToken.None);

            RefreshConnection();

            authStatus = telnetClient.TryLogin("pakedge", "pakedgep", 1000, ":", "\n");

            ErrorLog.Notice("@@Transport.Start()@@ Connected: {0}, Authenticated: {1}", connectionStatus, authStatus);

        }

        public override void Stop()
        {
            SendMethod("exit", null);
            telnetClient.Dispose();

            RefreshConnection();
            ErrorLog.Notice("@@Transport.Stop()@@ Connected: {0}, Authenticated: {1}", connectionStatus, authStatus);
        }


        public void RefreshConnection()
        {
            connectionStatus = telnetClient.IsConnected;
            base.IsConnected = connectionStatus;
            ErrorLog.Notice("@@Transport.RefreshConnection()@@ Connected: {0}, Authenticated: {1}", connectionStatus, authStatus);
        }
    }
}
