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
        private runnerDBDataClassesDataContext db;
        private int idHilo;

        public hiloSimulacion()
        {
            db = new runnerDBDataClassesDataContext();
            idHilo = Thread.CurrentThread.ManagedThreadId;
        }

        public void run()
        {
            logThread row = new logThread
            {
                idMensaje = Guid.NewGuid(),
                idThread = Thread.CurrentThread.ManagedThreadId.ToString(),
                texto = "Cerrando hilo",
                fecha = DateTime.Now
            };
            db.Connection.Open();
            db.logThread.InsertOnSubmit(row);
            db.SubmitChanges();
            db.Connection.Close();
        }
    }
}
