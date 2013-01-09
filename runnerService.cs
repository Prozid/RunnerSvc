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
using System.Xml.Linq;
using System.Xml;
using System.Security.Cryptography;

namespace runnerSvc
{
    public partial class runnerService : ServiceBase
    {
        private RunnerServiceConfiguration sConfig; // Configuración del servicio.
        private runnerDBDataContext db;     // Conexión con DB del servicio.
        private webappDBDataContext webDB;  // Conexión con DB de la webApp.
        private Thread tListener;           // Thread para el serverSocket.

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

            sConfig = RunnerServiceConfiguration.Deserialize();
        }

        protected override void OnStart(string[] args)
        {
            // Inicializamos las DB
            db = new runnerDBDataContext();
            webDB = new webappDBDataContext();


            runner_eventLog.WriteEntry("Initializing...");
            
            // Inicializamos el serverSocket donde recibir los resultados de las simulaciones
            tListener = new Thread(new ThreadStart(serverSocketListener));
            tListener.Start();

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

        private void serverSocketListener()
        {
            // ZONA DECLARACIÓN DE VARIABLES
            Socket conexion;
            Socket server;
            int conexionesServidos;

            // INICIALIZACIÓN

            conexionesServidos = 0;
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, sConfig.PortSvc);
            server = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            server.Bind(ipep);
            server.Listen(10);
            runner_eventLog.WriteEntry("[SERVER SOCKET] Initialized.");

            while (true)
            {
                try
                {
                    // Esperamos que el servicio Windows contacte con nosotros
                    conexion = server.Accept();
                    conexionesServidos += 1;
                    runner_eventLog.WriteEntry("[SERVER SOCKET] New connection.");

                    // Preparamos lo necesario para la recepción de datos
                    byte[] rBytes = new byte[1024];
                    byte[] csrBytes = new byte[1024];
                    byte[] sBytes = new byte[1024];
                    int raw;
                    string datos;

                    // Recibimos los datos
                    raw = conexion.Receive(rBytes);

                    // Recibimos checksum
                    //csRaw = conexion.Receive(csrBytes);

                    // Confirmar al servicio la recepción de los datos mediante checkSum md5
                    MD5 checksum = new MD5CryptoServiceProvider();
                    sBytes = checksum.ComputeHash(rBytes);
                    conexion.Send(sBytes);

                    //runner_eventLog.WriteEntry("[SERVER SOCKET] Checksum check: " + checksum.ComputeHash(rBytes).Equals(checksum.ComputeHash(csrBytes)).ToString());
                    //runner_eventLog.WriteEntry("[SERVER SOCKET] Checksum received: " + BitConverter.ToString(checksum.ComputeHash(rBytes)));
                    //runner_eventLog.WriteEntry("[SERVER SOCKET] Checksum computed: " + BitConverter.ToString(checksum.ComputeHash(csrBytes)));
                    //runner_eventLog.WriteEntry("[SERVER SOCKET] Answer sended.");

                    // Procesamos datos
                    datos = System.Text.Encoding.ASCII.GetString(rBytes, 0, raw);
                    runner_eventLog.WriteEntry("[SERVER SOCKET] XML received.");

                    // Parseamos el XML y lo guardamos
                    XDocument datosSimulacion = XDocument.Parse(datos);
                    datosSimulacion.Save(conexionesServidos.ToString() + "_demonio_received_xml.xml");
                    
                    // Procesamos datos
                    XElement simulacion = datosSimulacion.Element("Simulacion");
                    Guid idSimulacion = Guid.Parse(simulacion.Attribute("idSimulacion").Value);

                    // Finalizamos simulación
                    runner_eventLog.WriteEntry("[END SIMULATION] Establecemos finalizada la simulación");
                    webDB.Simulacion.Single(s => s.idSimulacion.Equals(idSimulacion)).EstadoSimulacion = webDB.EstadoSimulacion.Where(es => es.nombre.Equals("Finished")).Single();
                    webDB.SubmitChanges();
                    


                }
                catch (Exception e)
                {
                    runner_eventLog.WriteEntry(e.ToString());
                }
            }

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
                if (simByUser.ContainsKey(s.usuario) && simByUser[s.usuario] > sConfig.MaxUsers)
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
                new XComment("Simulacion"),
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
            Byte[] sBytes = new Byte[1024];
            Byte[] rBytes = new Byte[1024];
            Byte[] csBytes = new Byte[1024];
            int raw;

            sBytes = Encoding.ASCII.GetBytes(datosXML.ToString());

            // Checksum para confirmación de envío correcto
            MD5 checksum = new MD5CryptoServiceProvider();  
            csBytes = checksum.ComputeHash(sBytes);


            IPEndPoint ipep = new IPEndPoint(
               IPAddress.Parse(sConfig.IpDaemon),
               sConfig.PortDaemon
            );
            
            Socket conexion = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp
            );

            conexion.SendTimeout = 5000;
            conexion.ReceiveTimeout = 5000;

            try
            {
                // Conectamos con el servicio
                runner_eventLog.WriteEntry("[SEND SIMULATION] Try to connect...");
                conexion.Connect(ipep);
                runner_eventLog.WriteEntry("[SEND SIMULATION] Connected");


                // Enviamos datos            
                runner_eventLog.WriteEntry("[SEND SIMULATION] Starting to send...");
                conexion.Send(sBytes);

                // Enviamos checksum
                //runner_eventLog.WriteEntry("[SEND SIMULATION] Sended XML. Sending checksum...");
                //conexion.Send(csBytes);

                // Esperamos confirmación
                runner_eventLog.WriteEntry("[SEND SIMULATION] Waiting answer...");
                raw = conexion.Receive(rBytes);

                // Confirmamos que el envío es correcto
                MD5 rChecksum = new MD5CryptoServiceProvider();
                bool cBool = checksum.ComputeHash(sBytes).Equals(rChecksum.ComputeHash(rBytes));
                //runner_eventLog.WriteEntry("[SEND SIMULATION] Checksum XML: "+BitConverter.ToString(checksum.ComputeHash(sBytes)));
                //runner_eventLog.WriteEntry("[SEND SIMULATION] Checksum computed: " +BitConverter.ToString(csBytes));
                //runner_eventLog.WriteEntry("[SEND SIMULATION] Checksum received: " + BitConverter.ToString(checksum.ComputeHash(rBytes)));


                // Cerrando conexión.
                runner_eventLog.WriteEntry("[SEND SIMULATION] Complete");
                conexion.Close();
            }
            catch (System.TimeoutException error)
            {
                runner_eventLog.WriteEntry("[SEND SIMULATION] Timeout finished: " + error);
            }
            catch (Exception error)
            {
                runner_eventLog.WriteEntry("[SEND SIMULATION] Error desconocido: " + error);
            }                  

        }
    }
}
