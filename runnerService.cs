﻿using System;
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
using System.Xml.Linq;
using System.Xml;

namespace runnerSvc
{
    public partial class runnerService : ServiceBase
    {
        private static int puertoIn = 1990; // Puerto en el que escucha el demonio Linux.
        private static int puertoOut = 5959;// Puerto en el que escucha el servicio Windows.
        private static int MAXUSER = 5;     // Número de simulaciones running que puede tener un usuario como máximo.
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


            runner_eventLog.WriteEntry("Initializing...");

            // Comprobamos simulaciones Run y las establecemos en ToRun
            try
            {
                EstadoSimulacion RunState = webDB.EstadoSimulacion.Where(s => s.nombre.Equals("Running")).Single();
                EstadoSimulacion ToRunState = webDB.EstadoSimulacion.Where(s => s.nombre.Equals("ToRun")).Single();
                List<Simulacion> simRunning = webDB.Simulacion.Where(s => s.EstadoSimulacion.Equals(RunState)).ToList<Simulacion>();

                foreach (Simulacion s in simRunning)
                {
                    s.idEstadoSimulacion = ToRunState.idEstadoSimulacion;
                }
                webDB.SubmitChanges();

                runner_eventLog.WriteEntry("[CHECK SIM] " + simRunning.Count);

                
                // Configuramos el timer

                timerUpdateSimulations = new System.Threading.Timer(
                    new System.Threading.TimerCallback(timerUpdateSimulations_Elapsed),
                    null,
                    1000,
                    1000
                    );

                //Iniciamos el socket del backgroundWorker
                initServerSocket();


                // Lanzamos el backgroundworker
                //backgroundWorkerListener.RunWorkerAsync();
                
                  
            }
            catch (Exception e)
            {
                runner_eventLog.WriteEntry("[CHECK SIM] Error: "+e);
                this.Stop();
            }
                     

        }

        protected override void OnStop()
        {
            timerUpdateSimulations.Change(System.Threading.Timeout.Infinite, System.Threading.Timeout.Infinite);
            timerUpdateSimulations.Dispose();
            timerUpdateSimulations = null;
            
            runner_eventLog.WriteEntry("Stopping...");
        }

        // Inicializacion del serverSocket
        protected void initServerSocket()
        {
            
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, puertoIn);
            server = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            server.Bind(ipep);
            server.Listen(10);
        }

        private void backgroundWorkerListener_DoWork(object sender, DoWorkEventArgs eArgs)
        {

            Guid idCarpeta = Guid.Parse("6066d0f2-ebd4-4f43-a557-45016c89ace0");
            String idUsuario = "dani";
            while (true)
            {
                runner_eventLog.WriteEntry("[BACKGROUNDWORKER] Waiting connections in " + puertoIn);


                // Esperamos que el demonio Linux contacte con nosotros
                cliente = server.Accept();
                runner_eventLog.WriteEntry("[BACKGROUNDWORKER] New connection");


                // Recibir datos de simulación
                byte[] rBytes = new byte[1024];
                byte[] sBytes;
                int raw;
                string datos;

                raw = cliente.Receive(rBytes);
                datos = System.Text.Encoding.ASCII.GetString(rBytes, 0, raw);
                runner_eventLog.WriteEntry("[BACKGROUNDWORKER][DATA]" + datos);

                // Confirmar al demonio la recepción de los datos
                sBytes = Encoding.ASCII.GetBytes(datos);
                cliente.Send(sBytes);


                // Procesamos los resultados
                Archivo newArchivo = new Archivo
                {
                    idArchivo = Guid.NewGuid(),
                    idCarpeta = idCarpeta,
                    usuario = idUsuario,
                    publico = true,
                    nombre = "archivo_" + new Random(10).ToString(),
                    content_type = "text/plain",
                    datos = Encoding.ASCII.GetBytes(datos),
                    fechaCreacion = DateTime.Now,
                    descripcion = null,
                    baseDatos = false
                };
                try
                {
                    webDB.Archivo.InsertOnSubmit(newArchivo);
                    webDB.SubmitChanges();
                    runner_eventLog.WriteEntry("[BACKGROUNDWORKER][DATA PROCESSED] Ok");
                }
                catch (Exception e)
                {
                    runner_eventLog.WriteEntry("[BACKGROUNDWORKER][DATA PROCESSED] Error: " + e);
                }
            }

            
                
        }

        private void backgroundWorkerListener_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            runner_eventLog.WriteEntry("[BACKGROUNDWORKER] Finished");
        }

        private void timerUpdateSimulations_Elapsed(object state)
        {
            //runner_eventLog.WriteEntry("[UPDATE SIMULATIONS] New tick at "+DateTime.Now);

            EstadoSimulacion runState = webDB.EstadoSimulacion.Where(s => s.nombre.Equals("Running")).Single();
            EstadoSimulacion toRunState = webDB.EstadoSimulacion.Where(s => s.nombre.Equals("ToRun")).Single();
            
            // Consultamos las simulaciones ejecutándose por usuario.
            Dictionary<String, int> simByUser = new Dictionary<String, int>();
            List<String> usersSimRunning = webDB.Simulacion.Where(sr => sr.EstadoSimulacion == runState).Select(sr => sr.usuario).ToList();
            foreach (String u in usersSimRunning)
            {
                if (!simByUser.ContainsKey(u))
                {
                    simByUser.Add(u, 1);
                }
                else
                {
                    simByUser[u] = simByUser[u] + 1;
                }
            }

            // Consultamos las simulaciones que se encuentran en el estado ToRun.            
            List<Simulacion> simToRun = webDB.Simulacion.Where(s => s.EstadoSimulacion.Equals(toRunState)).ToList<Simulacion>();
            //runner_eventLog.WriteEntry("[SIMULATIONS QUEUE] " + simToRun.Count());
            foreach (Simulacion s in simToRun)
            {
                // Descartamos aquellas que superen el máximo de simulaciones por usuario MAXUSER.
                if (simByUser.ContainsKey(s.usuario) && simByUser[s.usuario] > MAXUSER)
                {
                    runner_eventLog.WriteEntry("[UPDATE SIMULATIONS] " + s.usuario + ": Too simulations. Denegate");
                }
                else
                {
                    // Establecemos dichas simulaciones a Run, es decir, las lanzamos.           
                    s.EstadoSimulacion = runState;
                    try
                    {
                        webDB.SubmitChanges();
                        runSimulation(s.idSimulacion);
                    }
                    catch (Exception e)
                    {
                        runner_eventLog.WriteEntry("[UPDATE SIMULATIONS] Update error: " + e.ToString());
                    }
                    
                }
            }
            
            simByUser = null;
        }
        
        private void runSimulation(Guid idSimulacion)
        {
            runner_eventLog.WriteEntry("[RUN SIMULATION] "+idSimulacion);

            // Obtenemos los datos de la simulación así como el archivo de datos del proyecto
            Simulacion simulacion = webDB.Simulacion.Where(s => s.idSimulacion.Equals(idSimulacion)).Single();
            Archivo archivoDatos = webDB.Archivo
                .Where(a => 
                    a.idArchivo.Equals(
                        webDB.Proyecto
                            .Where(p => p.idProyecto.Equals(simulacion.idProyecto))
                            .Single()
                            .idArchivo
                    )
                )
                .Single();

            // Creamos un documento XML con los datos a enviar
            runner_eventLog.WriteEntry("[RUN SIMULATION] Building XML...");
            String valor_nulo = "";
            XDocument datosXML = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XComment("Simulación"),
                new XElement("Simulacion",
                    new XAttribute("idSimulacion", simulacion.idSimulacion.ToString()),
                    new XElement("idProyecto", simulacion.idProyecto.ToString()),
                    new XElement("nombre", simulacion.nombre),
                    new XElement("descripcion",simulacion.descripcion),
                    new XElement("fechaCreacion",simulacion.fechaCreacion.ToString()),
                    new XElement("idEstadoSimulacion", simulacion.idEstadoSimulacion.ToString()),
                    new XElement("idMetodoClasificacion", simulacion.idMetodoClasificacion.ToString()),
                    new XElement("idMetodoSeleccion",simulacion.idMetodoSeleccion.ToString()),
                    new XElement("parametrosClasificacion",simulacion.parametrosClasificacion),
                    new XElement("parametrosSeleccion",simulacion.parametrosSeleccion),
                    new XElement("usuario",simulacion.usuario)
                )
                //new XElement("Datos",archivoDatos.datos)
            );

            //Guardamos una copia para testeo
            runner_eventLog.WriteEntry("[RUN SIMULATION] Saving XML");
            datosXML.Save("C:\\svc_created_xml.xml");


            // Preparamos todo lo necesario para la conexión
            runner_eventLog.WriteEntry("[RUN SIMULATION] Preparing data to send");
            Byte[] sBytes = Encoding.ASCII.GetBytes(datosXML.ToString());
            Byte[] rBytes = new Byte[1024];
            int raw;

            IPEndPoint ipep = new IPEndPoint(
               IPAddress.Parse("127.0.0.1"),
               puertoOut
            );
            
            Socket conexion = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
            );

            try
            {
                // Conectamos con el servicio
                runner_eventLog.WriteEntry("[SEND SIMULATION] Try to connect...");
                conexion.Connect(ipep);
                runner_eventLog.WriteEntry("[SEND SIMULATION] Connected");


                // Enviamos datos            
                runner_eventLog.WriteEntry("[SEND SIMULATION] Starting to send...");
                conexion.Send(sBytes);

                // Esperamos confirmación
                runner_eventLog.WriteEntry("[SEND SIMULATION] Waiting answer...");
                raw = conexion.Receive(rBytes);

                // Cerrando conexión.
                runner_eventLog.WriteEntry("[SEND SIMULATION] Complete");
                conexion.Close();

                // Procesamos resultados
                String respuestaString = System.Text.Encoding.ASCII.GetString(rBytes, 0, raw);
                runner_eventLog.WriteEntry("[SEND SIMULATION] Parsing answer to XML...");
                XDocument rXML = XDocument.Parse(respuestaString);
                rXML.Save("C:\\rXML.xml");


                // Finalizamos simulación
                runner_eventLog.WriteEntry("[END SIMULATION] Establecemos finalizada la simulación");
                simulacion.EstadoSimulacion = webDB.EstadoSimulacion.Where(es => es.nombre.Equals("Finished")).Single();
                webDB.SubmitChanges();
                
            }
            catch (Exception error)
            {
                runner_eventLog.WriteEntry("[SEND SIMULATION] No se ha podido conectar con el cluster: " + error);
            }
                    

        }
    }
}
