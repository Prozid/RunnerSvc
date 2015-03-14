using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Net;
using System.Xml.Linq;
using System.Diagnostics;

namespace PBioSvc
{
    public class ResultsListener
    {
        private int port;

        public ResultsListener(int port)
        {
            this.port = port;
        }

        public void StartListening()
        {
            PBioSocketServer.StartListening(this.port);            
        }
    }  
}
