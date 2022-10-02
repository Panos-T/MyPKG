using Crestron.RAD.Common.BasicDriver;
using Crestron.RAD.Common.Transports;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crestron.SimplSharp;
using System.IO;

namespace MyPKG
{
    public class myPDU_Protocol : ABaseDriverProtocol, IDisposable
    {
        private myPDU parentDev;
   


        public myPDU_Protocol(ISerialTransport transport, byte id, myPDU Parent) : base(transport, id)
        {

           // Transport.DataHandler = parseResponse;
            Transport = (ATransportDriver)transport;

            parentDev = Parent;
        }



        

        public void SocketRefresh()
        {
            Task.Run(() =>
            {
                Transport.SendMethod("pshow", null);
                Task.Delay(500);
                parseResponse(parentDev.Transport.GetResponse());
            });

            Task.Run(() =>
            {
                Transport.SendMethod("temp", null);
                Task.Delay(500);
                parseResponse(parentDev.Transport.GetResponse());
            });


        }


        public void parseResponse(string msg)
        {
            /*
            * Read Line by Line and parse into Dictionary all 8 values of Sockets
             */

            ErrorLog.Notice("@@myPDU_Protocol.parseResponse@@ Parsing {0}", msg);

            if (msg.StartsWith("pshow")) { 
            Dictionary<string, bool> pduState = new Dictionary<string, bool>();
            using (StringReader sr = new StringReader(msg))
            {
                string line;
               

                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Replace(" ", "");
                    if (line.StartsWith("0"))
                    {
                        string[] fields = line.Split('|');

                        bool fieldState;
                        if (fields[2] == "ON") fieldState = true;
                        else fieldState = false;
                        pduState.Add(fields[0], fieldState);

                        ErrorLog.Notice("@@myPDU_Protocol.parseResponse@@ pduState key {0}| pduState Value {1}", fields[0], pduState[fields[0]]);

                    }
                }
            }


            //Update UI Values
            parentDev.UpdateUI(pduState);
            }
            else if (msg.StartsWith("temp"))
            {
                string temp= "00.0°";
                using (StringReader sr = new StringReader(msg))
                {
                    string line;

                    while ((line = sr.ReadLine()) != null)
                    {
                        if (line.StartsWith("Temperature"))
                        {

                            string[] fields = line.Split(' ');
                            temp = fields[1].Substring(0, 4) + "°";
                            

                        }


                    }
                }
                //Update UI Values
                parentDev.UpdateUI(temp);
            }
        }

        


        public void AllSocketsOn()
        {
            Transport.SendMethod("ps 1", null);
        }
        
        public void AllSocketsOff()
        {
            Transport.SendMethod("ps 0", null);
        }


        public void toggleSocket(int socketNum, int action)
        {

            Transport.SendMethod("pset " + socketNum + " " + action, null);
            ErrorLog.Notice("@@Protocol.toggleSocket()@@ Command Sent: >" + "pset " + socketNum + " " + action);
            
        }

        protected override void ChooseDeconstructMethod(ValidatedRxData validatedData)
        {

        }

        protected override void ConnectionChangedEvent(bool connection)
        {
            parentDev.Connection_Changed(connection);
        }
    }
}
