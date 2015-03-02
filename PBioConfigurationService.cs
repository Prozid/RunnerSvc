using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Configuration;

namespace PBioSvc
{
    [Serializable]
    public class PBioServiceConfiguration
    {
        private String _ConnectionString;
		private int _PortSvc;   // Puerto en el que escucha el servicio Windows
		private int _PortDaemon; // Puerto en el que escucha el demonio Linux
		private String _IpDaemon; // IP del servicio en Windows
        private int _MaxUsers; // Máximo de simulaciones por usuario ejecutándose simultaneamente

		private const String DEFAULT_PATH = "config.xml";


		public PBioServiceConfiguration()
		{
            this._PortSvc           = int.Parse(ConfigurationManager.AppSettings["service_port"].ToString());
            this._PortDaemon        = int.Parse(ConfigurationManager.AppSettings["daemon_port"].ToString());
            this._IpDaemon          = ConfigurationManager.AppSettings["daemon_ip"].ToString();
            this._MaxUsers          = int.Parse(ConfigurationManager.AppSettings["max_by_user"].ToString());
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
