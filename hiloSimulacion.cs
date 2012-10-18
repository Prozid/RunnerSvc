using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net.Sockets;

namespace runnerSvc
{
    class hiloSimulacion
    {
        private runnerDBDataContext db;
        private int idHilo;

        public hiloSimulacion()
        {
            db = new runnerDBDataContext();
            idHilo = Thread.CurrentThread.ManagedThreadId;
        }

        public void run()
        {
            
        }
    }
}
