using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using NLog;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Messages;
using Microsoft.Extensions.Configuration;

namespace semv
{
    class Program
    {
        private static bool isWindowsOS = isWindows();
        private static string uri = "test.mosquitto.org";

        private static string idReq = Guid.NewGuid().ToString();

        private static string idDestination;

        private static Properties appProperties;

        private static Logger log;

        static void Main(string[] args)
        {
            // Console.WriteLine("Server Mqtt connection");
            // LogManager.LoadConfiguration("nlog.config");
            log = LogManager.GetCurrentClassLogger();            
            log.Info("Server Mqtt connection");
            loadJson();

            idDestination = appProperties.idClient;

            /**
            * El objeto utilizado para conectar con el broker y escuchar
            * los mensajes se podría implementar desde una interface para poder
            * soportar otros clientes del tipo pub/sub de la misma forma que se hizo
            * en Java (instanciando clase con reflexión)
            **/
            MqttClient client = new MqttClient(uri);
            client.ProtocolVersion = MqttProtocolVersion.Version_3_1_1;
            client.MqttMsgPublishReceived += subscriberMqttMsg;
            client.Connect(idReq);
            client.Subscribe(new string[]{ idDestination }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE});

            Console.ReadLine();
        }

        /**
        * Este objeto debería pertenecer a la implementación de la interfaz antes indicada.
        * Permitiría manejar según la implementación los comandos conocidos.
        **/
        private static void subscriberMqttMsg(object objSender, MqttMsgPublishEventArgs e)
        {
            // MqttClient mqttSender = (MqttClient) objSender;
            string rawMessage = Encoding.UTF8.GetString(e.Message);
            // log.Info("[RawMessage] => " + rawMessage);
            string[] msg = rawMessage.Split(";");
            string sender = msg[0];
            string cmd = msg[1];
            log.Info("[command] => " + cmd + " [idReq] => " + idReq + " [sender] => " + sender);

            if ("test".Equals(cmd))
            {
                // Console.WriteLine("Test Command");
                log.Info("Test Command");
            }
            else if ("reboot".Equals(cmd))
            {
                if (isWindowsOS)
                {
                    System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
                }
                else
                {
                    log.Warn("[i cannot reboot the machine]");
                }
            }
            else if ("shutdown".Equals(cmd))
            {
                if (isWindowsOS)
                {
                    System.Diagnostics.Process.Start("shutdown.exe", "-s -t 0");
                }
                else
                {
                    log.Warn("[i cannot shutdown the machine]");
                }
            }
            else
            {
                // Console.WriteLine("[unknown command] => " + cmd);
                log.Error("[unknown command] => " + cmd);
            }

        }


        private static bool isWindows()
        {
            bool result = true;
            OperatingSystem os = Environment.OSVersion;
            PlatformID     pid = os.Platform;
            switch (pid) 
            {
            case PlatformID.Win32NT:
            case PlatformID.Win32S:
            case PlatformID.Win32Windows:
            case PlatformID.WinCE:
                result = true;
                break;
            case PlatformID.Unix:
                result = false;
                break;
            case PlatformID.MacOSX:
                result = false;
                break;
            default:
                result = false;
                break;
            }
            return result;
        }

        private static void loadJson()
        {
            using (StreamReader r = new StreamReader("properties.json"))
            {
                string json = r.ReadToEnd();
                appProperties = JsonConvert.DeserializeObject<Properties>(json);
            }

            // var config = new ConfigurationBuilder()
            //     .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            //     .Build();            
        }        
    }
}
