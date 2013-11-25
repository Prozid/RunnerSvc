using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace runnerSvc
{
    [Serializable]
    public class RunnerServiceConfiguration
    {
        private String _ConnectionString;
		private int _PortSvc;   // Puerto en el que escucha el servicio Windows
		private int _PortDaemon; // Puerto en el que escucha el demonio Linux
		private String _IpDaemon; // IP del servicio en Windows
        private int _MaxUsers; // Máximo de simulaciones por usuario ejecutándose simultaneamente

		private const String DEFAULT_PATH = "config.xml";


		public RunnerServiceConfiguration()
		{
		}

		public RunnerServiceConfiguration (String connectionString, int portSvc, int portDaemon, String ipDaemon, int maxUsers)
		{
			_ConnectionString = connectionString;
			_PortDaemon = portDaemon;
			_PortSvc = portSvc;
			_IpDaemon = ipDaemon;
            _MaxUsers = maxUsers;
		}

		public static RunnerServiceConfiguration Defaults()
		{
			int pSvc = 1990;
			int pDmn = 5959;
			String ipDaemon = "192.168.1.4"; // IP máquina virtual: 192.168.1.4 Cluster0
			String cs = "Server=localhost;Database=runnerDaemon;User ID=root;Password=dani;Pooling=false"; //String de conexión a la BD
            int maxUsers = 5;
            RunnerServiceConfiguration configDefault = new RunnerServiceConfiguration(cs, pSvc, pDmn, ipDaemon,maxUsers);
            try
            {
                RunnerServiceConfiguration.Serialize(configDefault);
            }
            catch
            {
                // TODO Informar de que no se puede guardar el archivo de configuración
            }

			return configDefault;
		}

        public static void Serialize(RunnerServiceConfiguration svcConfig)
		{
			String file = DEFAULT_PATH;
			System.Xml.Serialization.XmlSerializer xs 
				= new System.Xml.Serialization.XmlSerializer(svcConfig.GetType());
			
			StreamWriter writer = File.CreateText(file);
			xs.Serialize(writer,svcConfig);
			writer.Flush();
			writer.Close();
		}

        public static void Serialize(string file, RunnerServiceConfiguration svcConfig)
		{
			System.Xml.Serialization.XmlSerializer xs 
				= new System.Xml.Serialization.XmlSerializer(svcConfig.GetType());
			
			StreamWriter writer = File.CreateText(file);
			xs.Serialize(writer,svcConfig);
			writer.Flush();
			writer.Close();
		}

        public static RunnerServiceConfiguration Deserialize()
		{
			String file = DEFAULT_PATH;
            RunnerServiceConfiguration sc;
            try
            {
                System.Xml.Serialization.XmlSerializer xs
                    = new System.Xml.Serialization.XmlSerializer(typeof(RunnerServiceConfiguration));
                StreamReader reader = File.OpenText(file);
                sc = (RunnerServiceConfiguration)xs.Deserialize(reader);
                reader.Close();
            }
            catch
            {
                sc = RunnerServiceConfiguration.Defaults();
            }
			return sc;

		}

        public static RunnerServiceConfiguration Deserialize(string file)
		{
            RunnerServiceConfiguration sc;
			try {
				System.Xml.Serialization.XmlSerializer xs
                = new System.Xml.Serialization.XmlSerializer(typeof(RunnerServiceConfiguration));
				StreamReader reader = File.OpenText(file);
                sc = (RunnerServiceConfiguration)xs.Deserialize(reader);
				reader.Close ();
			} catch {
                sc = RunnerServiceConfiguration.Defaults();
			}
			return sc;
		}
		
		public String ConnectionString 
		{ 
			get { return _ConnectionString; }
			set { _ConnectionString = value; }
		}
		
		public String IpDaemon {
			get { return _IpDaemon; }
			set { _IpDaemon = value; }
		}
		
		public int PortSvc {
			get { return _PortSvc;}
			set { _PortSvc = value; }
		}

		public int PortDaemon {
			get { return _PortDaemon;}
			set { _PortDaemon = value; }
		}

        public int MaxUsers
        {
            get { return _MaxUsers; }
            set { _MaxUsers = value; }
        }
    }
}
