using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace runnerSvc
{
    public partial class runnerService : ServiceBase
    {
        private static int puerto = 1989;
        private static int MAXUSER = 5;
        //private Thread hiloServer;
        private runnerDBDataContext db;     // Conexión con DB del servicio.
        private webappDBDataContext webDB;  // Conexión con DB de la webApp.
        private Socket cliente;             // Socket generado cuando un cliente se conecta para enviarnos resultados.
        private Socket server;              // Socket que trabajará con el backgroundWorker para recibir los resultados de las simulaciones.

        public runnerService()
        {
            InitializeComponent();
            if (!System.Diagnostics.EventLog.SourceExists("runnerSource"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "runnerSource", "runnerLog");
            }
            
            // Configuramos el registro de eventos del servicio
            runner_eventLog.Source = "runnerSource";
            runner_eventLog.Log = "runnerLog";
            
        }

        protected override void OnStart(string[] args)
        {
            // Inicializamos las DB
            db = new runnerDBDataContext();
            webDB = new webappDBDataContext();


            runner_eventLog.WriteEntry("Servicio iniciado.");
                //hiloServer = new Thread(new ThreadStart(runnerServerSocket));
                //hiloServer.Start();
                //runner_eventLog.WriteEntry("Creado hilo servidor socket con id:" + hiloServer.ManagedThreadId);

            // Comprobamos simulaciones Run y las establecemos en ToRun
            Guid RunState = webDB.EstadoSimulacion.Where(s => s.nombre.Equals("Run")).Single().idEstadoSimulacion;
            Guid ToRunState = webDB.EstadoSimulacion.Where(s => s.nombre.Equals("ToRun")).Single().idEstadoSimulacion;


            foreach (Simulacion s in webDB.Simulacion.Where(s => s.EstadoSimulacion.Equals(RunState)).ToList<Simulacion>())
            {
                s.idEstadoSimulacion = ToRunState;
            }
            webDB.SubmitChanges();


            // Iniciamos el socket del backgroundWorker
            initServerSocket();


            // Lanzamos el backgroundworker y el timer
            backgroundWorkerListener.RunWorkerAsync();
            timerUpdateSimulations.Start();

            
        }

        protected override void OnStop()
        {
            runner_eventLog.WriteEntry("Parando servidor socket");
            //hiloServer.Interrupt();
            //if (!hiloServer.IsAlive) runner_eventLog.WriteEntry("Servidor socket interrumpido");
            runner_eventLog.WriteEntry("Parando el servicio");
        }

        // Inicializacion del serverSocket
        protected void initServerSocket()
        {
            
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, puerto);
            server = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            server.Bind(ipep);
            server.Listen(10);
        }

        private void backgroundWorkerListener_DoWork(object sender, DoWorkEventArgs eArgs)
        {
                runner_eventLog.WriteEntry("Esperando conexiones en " + puerto);
                
                
                // Esperamos que el demonio Linux contacte con nosotros
                cliente = server.Accept();
                runner_eventLog.WriteEntry("Registrada conexión.");

                
                // Recibir datos de simulación
                byte[] rBytes = new byte[1024];
                byte[] sBytes;
                int raw;
                string mensaje;

                raw = cliente.Receive(rBytes);
                mensaje = System.Text.Encoding.ASCII.GetString(rBytes, 0, raw);
                runner_eventLog.WriteEntry("[DATOS]" + mensaje);

                // Confirmar al demonio la recepción de los datos
                sBytes = Encoding.ASCII.GetBytes(mensaje);
                cliente.Send(sBytes);

                
                // Procesamos los resultados
            
            
                // Creación de hilo simulación
                hiloSimulacion hilo = new hiloSimulacion();
                Thread thread = new Thread(new ThreadStart(hilo.run));
                runner_eventLog.WriteEntry("[" + thread.ManagedThreadId + "] RunnerThread creado");


                try
                {
                    // Registro de nuevo hilo de simulación
                    EstadoHilo infoHilo = new EstadoHilo
                    {
                        idSimulacion = Guid.Parse(mensaje),
                        pid = thread.ManagedThreadId,
                        idThread = Guid.NewGuid()
                    };
                    // Inserción en BD

                    db.EstadoHilo.InsertOnSubmit(infoHilo);
                    runner_eventLog.WriteEntry("Preparadas ROW para log en BD");
                
                    try
                    { 
                        db.SubmitChanges();
                        runner_eventLog.WriteEntry("[SEARCH]"+db.EstadoHilo.Where(r => r.idSimulacion == infoHilo.idSimulacion).Single().pid);
                    }
                    catch (Exception e)
                    {
                        runner_eventLog.WriteEntry("ERROR en BD: " + e);
                    }
                }
                catch (Exception e)
                {
                    runner_eventLog.WriteEntry("[ERROR AL INSERTAR] "+e);
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

        private void timerUpdateSimulations_Tick(object sender, EventArgs e)
        {
            // Consultamos las simulaciones que se encuentran en el estado ToRun.

            // Descartamos aquellas que superen el máximo de simulaciones por usuario MAXUSER.

            // Establecemos dichas simulaciones a Run, es decir, las lanzamos.           


        }
    }
}
