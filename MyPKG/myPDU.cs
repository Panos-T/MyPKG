using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Crestron.RAD.Common.Interfaces;
using Crestron.RAD.Common.Interfaces.ExtensionDevice;
using Crestron.RAD.DeviceTypes.ExtensionDevice;
using Crestron.RAD.Common.Enums;
using Crestron.SimplSharp;
using Crestron.RAD.Common.Transports;
using Crestron.RAD.Common.Attributes.Programming;

namespace MyPKG
{
    public class myPDU : AExtensionDevice, ITcp, IAuthentication2
    {
        #region Field declaration
        public myPDU_Transport Transport;
        public myPDU_Protocol Protocol;
        
        

        private string[] Button_Key;
        private PropertyValue<bool>[] Button_Property;
        
        
        private PropertyValue<string> SecondaryIcon, devTemp;


        private const string SecondaryIconString = "SecondaryIconValue";
        private const string tempString = "tempString";
        internal int ButtonNum;
        
        #endregion

        #region Inititalize
        public void Initialize(IPAddress devIP, int devPort)
        {
            var locGR = GetLocalizedStrings("el-GR");
            var locEN = GetLocalizedStrings("en-US");
            ButtonNum = 8;
            Create_Device_Definition();
            UpdateUI("30.2°");

            
            
            CrestronConsole.AddNewConsoleCommand(connectPrint, "connectP", "Connect And Print Logs", ConsoleAccessLevelEnum.AccessAdministrator);
            CrestronConsole.AddNewConsoleCommand(disconnectPrint, "disconnectP", "Disconnect And Print Logs", ConsoleAccessLevelEnum.AccessAdministrator);
            CrestronConsole.AddNewConsoleCommand(togglePDU, "pduToggle", "pduToggle socketNum state", ConsoleAccessLevelEnum.AccessAdministrator);
            CrestronConsole.AddNewConsoleCommand(togglePDUS, "pduToggleS", "pduToggle 1", ConsoleAccessLevelEnum.AccessAdministrator);
            CrestronConsole.AddNewConsoleCommand(togglePDUD, "pduToggleD", "pduToggle Direct", ConsoleAccessLevelEnum.AccessAdministrator);
            CrestronConsole.AddNewConsoleCommand(refreshTemp, "refreshT", "Set Temp to 27.5", ConsoleAccessLevelEnum.AccessAdministrator);
            CrestronConsole.AddNewConsoleCommand(PrintMSG, "printmsg", "Print current MSG Variable", ConsoleAccessLevelEnum.AccessAdministrator);

            ErrorLog.Notice("@@Execute myPDU.Initialize@@");
            ErrorLog.Notice("@@myPDU.Initialize@@ DefUsr: {0} DefPsw: {1} || user:{2} pass:{3} ", DefaultUsername, DefaultPassword, SupportsUsername,SupportsPassword);


           
            /*
            foreach (var i in locGR)
            {

                ErrorLog.Notice("@@myPDU.Initialize@@ Localizaton: {0}#####{1}", i.Key, i.Value);
            }
            */
            // Implement TCP Transport
            try
            {

                ErrorLog.Notice("@@myPDU.Initialize attempting Transport@@");
                Transport = new myPDU_Transport(devIP.ToString(), devPort);
       
                ConnectionTransport = Transport;

                ErrorLog.Notice("@@myPDU.Initialize Exited Transport result: {0} | {1}@@", Transport.connectionStatus, Transport.authStatus);
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("@@Connection Error: {0}@@", e);
            }

            //Initialize Protocol

            try
            {
                Protocol = new myPDU_Protocol(Transport, Id, this)
                {
                    EnableLogging = InternalEnableLogging,
                    CustomLogger = InternalCustomLogger
                };

                DeviceProtocol = Protocol;
                DeviceProtocol.Initialize(DriverData);
            }
            catch (Exception e)
            {
                CrestronConsole.PrintLine("driver Error: {0}", e);
            }

           
        }


        private void Create_Device_Definition()
        {
            Button_Property = new PropertyValue<bool>[ButtonNum];
            Button_Key = new string[ButtonNum];
           
            
            for(int i = 0; i < ButtonNum; i++)
            {
                Button_Key[i] = "Socket" + Convert.ToString(i+1) + "value";
                Button_Property[i] = CreateProperty<bool>(new PropertyDefinition(Button_Key[i], null, DevicePropertyType.Boolean));

            }

            SecondaryIcon = CreateProperty<string>(new PropertyDefinition(SecondaryIconString, null, DevicePropertyType.String));
            SecondaryIcon.Value = "icGenericScene1";

            devTemp = CreateProperty<string>(new PropertyDefinition(tempString, null, DevicePropertyType.String));
            SecondaryIcon.Value = "icGenericScene1";
        }
        #endregion



        #region Debugging Functions

        private void PrintMSG(string cmdParameters)
        {

            ErrorLog.Notice("MSG-> {0}",Transport.GetResponse());
        }

        private void refreshTemp(string cmdParameters)
        {
            if (cmdParameters[0].Equals(""))
            {
                devTemp.Value = "27.5°";
            }
            else
            {
                devTemp.Value = ""+cmdParameters[0] + cmdParameters[1] + cmdParameters[2] + cmdParameters[3] + "°";
            }
            Commit();
        }

        private void connectPrint(string cmdParameters)
        {
            ErrorLog.Notice("@@Connect and Print@@ Connection Status: {0}, Authentication Status: {1}.", Transport.connectionStatus, Transport.authStatus);
            ErrorLog.Notice("@@Connect and Print@@ Calling Connect");

            Transport.Start();

            Connected = Transport.connectionStatus;
            ErrorLog.Notice("@@Connect and Print@@ Transport Start Ended");
            ErrorLog.Notice("@@Connect and Print@@ AFTER@@Connection Status: {0}, Authentication Status: {1}.", Transport.connectionStatus, Transport.authStatus);


        }


        private void disconnectPrint(string cmdParameters)
        {
            ErrorLog.Notice("@@DisConnect and Print@@ Connection Status: {0}, Authentication Status: {1}.", Transport.connectionStatus, Transport.authStatus);
            ErrorLog.Notice("@@DisConnect and Print@@ Calling Connect");

            Transport.Stop();

            Connected = Transport.connectionStatus;
            ErrorLog.Notice("@@DisConnect and Print@@ Transport Start Ended");
            ErrorLog.Notice("@@DisConnect and Print@@ AFTER@@Connection Status: {0}, Authentication Status: {1}.", Transport.connectionStatus, Transport.authStatus);


        }

        private void togglePDU(string cmdParameters)
        {
            Protocol.toggleSocket(((int)cmdParameters[0]), (int)cmdParameters[1]);
        }

        private void togglePDUS(string cmdParameters)
        {
            Protocol.toggleSocket(1,1);
        }

        private void togglePDUD(string cmdParameters)
        {
            Transport.telnetClient.WriteLine("pset 1 1");
        }






        #endregion

        #region Connection Functions


        public override void Connect()
        {


            ErrorLog.Notice("@@myPDU.Connect@@ Exexute");

            Transport.Start();

            Connected = Transport.connectionStatus;

            ErrorLog.Notice("@@myPDU.Connect@@ Connection Status: {0}", Connected);
            
        }



        public override void Disconnect()
        {
            if (Protocol == null)
            {
                return;
            }
            else
            {
                //Disconnect from the Server
                //base.Disconnect();

                Transport.Stop();
                Connected = Transport.connectionStatus;
                ErrorLog.Notice("@@myPDU.Disconnect@@ Connection Status: {0}", Connected);
            }
        }

        public void Connection_Changed(bool ConnectionStatus)
        {
            Connected = ConnectionStatus;
        }

        #endregion


        #region Protocol<->UI

        public void UpdateUI(Dictionary<string,bool> pduState)
        {
            bool allOn =true, allOff = true;

            ErrorLog.Notice("@@myPDU.UpdateUI()@@ Initialize");
            for (int i = 0; i < ButtonNum; i++)
            {


                Button_Property[i].Value = Convert.ToBoolean(pduState["0" + (i+1)]);

                allOn = Button_Property[i].Value && allOn;
                allOff = !(Button_Property[i].Value) && allOff;

                ErrorLog.Notice("@@myPDU.UpdateUI()@@ ButtonID {0}->Value {1}", Button_Property[i].Id, Button_Property[i].Value);
            }



            
            
            ErrorLog.Notice("@@myPDU.UpdateUI()@@ allOnBOOL {0}-AllOffBOOL {1}", allOn, allOff);
           if (allOn)
            {
                SecondaryIcon.Value = "icRemoteButtonYellow";
            }
           else if (allOff)
            {
                SecondaryIcon.Value = "icGenericSecurity";
            }
            else
            {
                SecondaryIcon.Value = "icGenericScene1";
            }

            Commit();
        }

        public void UpdateUI(string temperature)
        {
            devTemp.Value = temperature;
            Commit();
        }


        protected override IOperationResult DoCommand(string command, string[] parameters)
        {

            switch (command)
            {
                case "SocketAllOn":
                    ErrorLog.Notice("@@myPDU.DoCommand@@ Command <{0}> found", command);
                    Protocol.AllSocketsOn();
                    break;
                case "SocketAllOff":
                    ErrorLog.Notice("@@myPDU.DoCommand@@ Command <{0}> found", command);
                    Protocol.AllSocketsOff();
                    break;
                case "SocketRefresh":
                    ErrorLog.Notice("@@myPDU.DoCommand@@ Command <{0}> found", command);
                    Protocol.SocketRefresh();
                    break;
                case "TempRefresh":
                    ErrorLog.Notice("@@myPDU.DoCommand@@ Command <{0}> found", command);
                    Protocol.TempRefresh();
                    break;
                default:
                    ErrorLog.Notice("@@myPDU.DoCommand@@ Command <{0}> not found", command);
                    break;
            }
            Commit();
            return new OperationResult(OperationResultCode.Success);
        }

        protected override IOperationResult SetDriverPropertyValue<T>(string propertyKey, T value)
        {
            ErrorLog.Notice("@@myPDU.SetDriverPropertyValue 1@@ Entering");

            for (int i = 0; i < ButtonNum; i++)
            {


                ErrorLog.Notice("@@myPDU.SetDriverPropertyValue 1@@ Before If propertyKey: {0}", propertyKey);
                if (propertyKey == Button_Key[i])
                {
                    var state = value as bool?; //check for null with ? operator
                    
                    ErrorLog.Notice("@@myPDU.SetDriverPropertyValue 1@@ propertyKey: {0}", propertyKey);

                    if (state != null) Protocol.toggleSocket(i + 1, (bool) state ? 1:0) ;
                    
                    Button_Property[i].Value = (bool)state;

                    ErrorLog.Notice("@@myPDU.SetDriverPropertyValue 1@@ propertyKey: {0}", Button_Property[i].Value);
                    Commit();


                    return new OperationResult(OperationResultCode.Success);
                }   
            }
            return new OperationResult(OperationResultCode.Error);
        }

        protected override IOperationResult SetDriverPropertyValue<T>(string objectId, string propertyKey, T value)
        {

            ErrorLog.Notice("@@myPDU.SetDriverPropertyValue 2@@ Entering");
            for (int i = 0; i <= ButtonNum; i++)
            {
                ErrorLog.Notice("@@myPDU.SetDriverPropertyValue 2@@ Before If propertyKey: {0}", propertyKey);
                if (propertyKey == Button_Key[i])
                {
                    var state = value as bool?; //check for null with ? operator

                    if (state != null) Protocol.toggleSocket(i + 1, (bool)state ? 1 : 0);
                    Button_Property[i].Value = (bool)state;
                    Commit();


                    return new OperationResult(OperationResultCode.Success);
                }
            }
            return new OperationResult(OperationResultCode.Error);
        }

        #endregion



        [ProgrammableOperation("Socket 1 On")]
        public void Socket1On()
        {
            Protocol.toggleSocket(1, 1);
        }


        [ProgrammableOperation("Socket 1 Off")]
        public void Socket1Off()
        {
            Protocol.toggleSocket(1, 0);
        }
    }
}
