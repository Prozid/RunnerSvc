using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

namespace runnerSvc
{
    public partial class runnerService : ServiceBase
    {
        private static int puerto = 1989;
        private Thread server;
        private runnerDBDataClassesDataContext db;
        public runnerService()
        {
            InitializeComponent();
            if (!System.Diagnostics.EventLog.SourceExists("runnerSource"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "runnerSource", "runnerLog");
            }
            
            runner_eventLog.Source = "runnerSource";
            runner_eventLog.Log = "runnerLog";
            
        }

        protected override void OnStart(string[] args)
        {
            db = new runnerDBDataClassesDataContext();   
            runner_eventLog.WriteEntry("Entrando en el servicio");
            server = new Thread(new ThreadStart(runnerServerSocket));
            server.Start();
            runner_eventLog.WriteEntry("Creado hilo servidor socket con id:" + server.ManagedThreadId);
        }

        protected override void OnStop()
        {
            runner_eventLog.WriteEntry("Parando servidor socket");
            server.Interrupt();
            if (!server.IsAlive) runner_eventLog.WriteEntry("Servidor socket interrumpido");
            runner_eventLog.WriteEntry("Parando el servicio");
        }

        protected void runnerServerSocket()
        {
            // CREACIÓN SERVER SOCKET
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, puerto);
            Socket server = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            server.Bind(ipep);
            server.Listen(10);

            // BUCLE CONEXIONEs
            while (true)
            {

                runner_eventLog.WriteEntry("Esperando conexiones en " + puerto);
                
                
                // NUEVA CONEXION
                Socket cliente = server.Accept();
                runner_eventLog.WriteEntry("Registrada conexión.");

                
                // Recibir datos de simulación
                byte[] rBytes = new byte[1024];
                byte[] sBytes;
                int raw;
                string mensaje;

                raw = cliente.Receive(rBytes);
                mensaje = System.Text.Encoding.ASCII.GetString(rBytes, 0, raw);
                runner_eventLog.WriteEntry("[DATOS]" + mensaje);

                // Confirmar a cliente la recepción de los datos
                sBytes = Encoding.ASCII.GetBytes(mensaje);
                cliente.Send(sBytes);

                // Creación de hilo simulación
                hiloSimulacion hilo = new hiloSimulacion();
                Thread thread = new Thread(new ThreadStart(hilo.run));
                runner_eventLog.WriteEntry("[" + thread.ManagedThreadId + "] RunnerThread creado");


                // Registro de nuevo hilo de simulación
                logThread infoHilo = new logThread
                {
                    idMensaje = Guid.NewGuid(),
                    idThread = thread.ManagedThreadId.ToString(),
                    fecha = DateTime.Now,
                    texto = "Creación"
                };                

                // Datos de la simulación
                logThread row = new logThread
                {
                    idMensaje = Guid.NewGuid(),
                    idThread = thread.ManagedThreadId.ToString(),
                    fecha = DateTime.Now,
                    texto = mensaje                    
                };


                // Inserción en BD
                
                db.logThread.InsertOnSubmit(infoHilo);
                db.logThread.InsertOnSubmit(row);
                runner_eventLog.WriteEntry("Preparadas ROW para log en BD");               

                try
                {
                    db.Connection.Open();
                    db.SubmitChanges();
                    runner_eventLog.WriteEntry("[SEARCH]"+db.logThread.Where(r => r.idMensaje == row.idMensaje).Single().texto);
                    db.Connection.Close();
                }
                catch (Exception e)
                {
                    runner_eventLog.WriteEntry("ERROR en BD: " + e);
                }


                // Ejecución del hilo de simulación
                try
                {
                    runner_eventLog.WriteEntry("[" + thread.ManagedThreadId + "] RunnerThread iniciado.");
                    thread.Start();
                }
                catch (ThreadStateException e)
                {
                    runner_eventLog.WriteEntry("ERROR Thread: " + e);
                }


                // Esperando finalización del hilo
                runner_eventLog.WriteEntry("[" + thread.ManagedThreadId + "] Esperando a que finalice.");
                thread.Join();
                runner_eventLog.WriteEntry("[" + thread.ManagedThreadId + "] Finalizado.");
            }
        }
    }
}
