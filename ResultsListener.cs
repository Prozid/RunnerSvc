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
        // State object for reading client data asynchronously
        private class StateObject
        {
            // Client socket.
            public Socket workSocket = null;
            // Siz e of receive buffer
            public const int BufferSize = 1024;
            // Receive buffer.
            public byte[] buffer = new byte[BufferSize];
            // Received data string
            public StringBuilder sb = new StringBuilder();
        }

        private const String EndOfFileTag = "<EOF>";
        private const String ResultsFileTag = "<Results>";
        private const String ErrorFileTag = "<Error>";
        public static ManualResetEvent allDone = new ManualResetEvent(false);
        private PBioServiceConfiguration sConfig;
        private webappDBEntities webDB;
        private EventLog PBioEventLog; // TODO Igual es interesante no tener aqui el EventLog, me parece una guarrada
        

        public ResultsListener(webappDBEntities webDB, PBioServiceConfiguration configuration, EventLog PBioLog) {
            this.sConfig = configuration;
            this.webDB = webDB;
            this.PBioEventLog = PBioLog;
        }

        public void StartListening() {
            // Data buffer for incoming data.
            byte[] bytes = new Byte[1024];

            // Establish the local endpoint for the socket.
            IPEndPoint ipep = new IPEndPoint(IPAddress.Any, sConfig.PortSvc);

            // Create a TCP/IP socket.
            Socket listener = new Socket(AddressFamily.InterNetwork,
                SocketType.Stream, ProtocolType.Tcp );

            // Bind the socket to the local endpoint and listen for incoming connections.
            try {
                listener.Bind(ipep);
                listener.Listen(100);

                while (true) {
                    // Set the event to nonsignaled state.
                    allDone.Reset();

                    // Start an asynchronous socket to listen for connections.
                    PBioEventLog.WriteEntry("[Results Listener] Waiting for a connection in port "+sConfig.PortSvc);
                    listener.BeginAccept( 
                        new AsyncCallback(AcceptCallback),
                        listener );

                    // Wait until a connection is made before continuing.
                    allDone.WaitOne();
                }

            } catch (Exception e) {
                PBioEventLog.WriteEntry(e.ToString());
            }
        
        }

        public void AcceptCallback(IAsyncResult ar) {
            // Signal the main thread to continue.
            allDone.Set();

            // Get the socket that handles the client request.
            Socket listener = (Socket) ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            // Create the state object.
            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive( state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
        }

        public void ReadCallback(IAsyncResult ar) {
            String content = String.Empty;
        
            // Retrieve the state object and the handler socket
            // from the asynchronous state object.
            StateObject state = (StateObject) ar.AsyncState;
            Socket handler = state.workSocket;

            // Read data from the client socket. 
            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                // There  might be more data, so store the data received so far.
                state.sb.Append(Encoding.ASCII.GetString(
                    state.buffer, 0, bytesRead));

                // Not all data received. Get more.
                handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0,
                new AsyncCallback(ReadCallback), state);
            }
            else
            {
                // All the data has been read from the 
                // client. Display it on the console.
                PBioEventLog.WriteEntry("[RESULTS LISTENER] Read " + content.Length + " bytes from socket. \n");

                // Chequeamos si es Resultados o Error
                if (content.StartsWith(ErrorFileTag))
                {
                    // Eliminamos ErrorTag
                    content.Replace(ErrorFileTag, "");

                    // Parseamos el XML
                    XDocument errorXML = XDocument.Parse(content);

                    // Enviamos longitud XML recibido como respuesta
                    Send(state.workSocket, errorXML.ToString().Length.ToString());

                    // Guardamos error log en BD
                    Log l = Log.LoadFromXML(errorXML);
                    Guid idSimulacion = Log.GetIdSimulationOfLogFromXML(errorXML);

                    webDB.Log.Add(l);
                    webDB.Simulacion.Single(s => s.IdSimulacion.Equals(idSimulacion)).Log = l;
                    webDB.Simulacion.Single(s => s.IdSimulacion.Equals(idSimulacion)).EstadoSimulacion = webDB.EstadoSimulacion.Where(es => es.Nombre.Equals("Error")).Single();
                    webDB.SaveChanges();

                    PBioEventLog.WriteEntry("[RESULTS LISTENER] Error file received: " + l.Texto);
                }
                else if (content.StartsWith(ResultsFileTag))
                {
                    try
                    {
                        // Eliminamos el ResultsTag
                        content.Replace(ResultsFileTag, "");

                        // Parseamos el XML
                        XDocument resultadosXML = XDocument.Parse(content);

                        // Enviamos longitud XML recibido como respuesta
                        Send(state.workSocket, resultadosXML.ToString().Length.ToString());

                        // Convertimos a clase Resultado
                        Resultado resultado = Resultado.LoadFromXML(resultadosXML);

                        // Guardamos en base de datos los resultados
                        webDB.Resultado.Add(resultado);

                        // Establecemos como finalizada la simulación
                        PBioEventLog.WriteEntry("[RESULTS LISTENER] Establecemos finalizada la simulación");
                        webDB.Simulacion.Single(s => s.IdSimulacion.Equals(resultado.IdSimulacion)).EstadoSimulacion = webDB.EstadoSimulacion.Where(es => es.Nombre.Equals("Terminate")).Single();
                        webDB.SaveChanges();
                    }
                    catch (Exception e)
                    {
                        PBioEventLog.WriteEntry("[RESULTS LISTENER] ERROR: " + e.ToString());
                    }
                }
                else
                {
                    PBioEventLog.WriteEntry("[RESULTS LISTENER] Unknow tag.");
                }
            }
        }
    
        private void Send(Socket handler, String data) {
            // Convert the string data to byte data using ASCII encoding.
            byte[] byteData = Encoding.ASCII.GetBytes(data);

            // Begin sending the data to the remote device.
            handler.BeginSend(byteData, 0, byteData.Length, 0,
                new AsyncCallback(SendCallback), handler);
        }

        private void SendCallback(IAsyncResult ar) {
            try {
                // Retrieve the socket from the state object.
                Socket handler = (Socket) ar.AsyncState;

                // Complete sending the data to the remote device.
                int bytesSent = handler.EndSend(ar);
                PBioEventLog.WriteEntry("Sent "+ bytesSent +" bytes to daemon.");

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            } catch (Exception e) {
                PBioEventLog.WriteEntry(e.ToString());
            }
        }


    }
}
