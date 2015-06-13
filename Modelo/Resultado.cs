using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.IO;
using System.Diagnostics;

namespace PBioSvc
{
    public partial class Resultado
    {
        private const String ResultsFileTag = "<Resultado>";
        private const String ErrorFileTag = "<LogSimulacion>";
        private const String EndFileTag = "<PBIOEOF>";

        public static void Serialize(string file, Resultado resultado)
        {
            System.Xml.Serialization.XmlSerializer xs
                = new System.Xml.Serialization.XmlSerializer(resultado.GetType());

            StreamWriter writer = File.CreateText(file);
            xs.Serialize(writer, resultado);
            writer.Flush();
            writer.Close();
        }

        public static Resultado Deserialize(string file)
        {
            Resultado resultado;
            try
            {
                System.Xml.Serialization.XmlSerializer xs
                = new System.Xml.Serialization.XmlSerializer(typeof(Resultado));
                StreamReader reader = File.OpenText(file);
                resultado = (Resultado)xs.Deserialize(reader);
                reader.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
            return resultado;
        }

        public static Resultado LoadFromXML(XDocument xml)
        {
            Resultado resultado = new Resultado();

            try
            {
                if (xml.Root.Element("IdResultado") != null && xml.Root.Element("IdResultado").Value != "")
                    resultado.IdResultado = Guid.Parse(xml.Root.Element("IdResultado").Value);
                else
                    resultado.IdResultado = Guid.NewGuid();

                resultado.NombreGenesSolucion = xml.Root.Element("NombreGenesSolucion").Value;
                resultado.IdGenesSolucion = xml.Root.Element("IdGenesSolucion").Value;
                resultado.NumGenes = int.Parse(xml.Root.Element("NumGenes").Value);
                resultado.Accuracy_Media = double.Parse(xml.Root.Element("Accuracy_Media").Value);
                resultado.Accuracy_Std = double.Parse(xml.Root.Element("Accuracy_Std").Value);
                resultado.Sensitivity_Media = double.Parse(xml.Root.Element("Sensitivity_Media").Value);
                resultado.Sensitivity_Std = double.Parse(xml.Root.Element("Sensitivity_Std").Value);
                resultado.Specificity_Media = double.Parse(xml.Root.Element("Specificity_Media").Value);
                resultado.Specificity_Std = double.Parse(xml.Root.Element("Specificity_Std").Value);
                resultado.NombreGenes = xml.Root.Element("NombreGenes").Value;
                resultado.IdGenes = xml.Root.Element("IdGenes").Value;
                resultado.AccuracyXGenes = xml.Root.Element("AccuracyXGenes").Value;
                resultado.IdSimulacion = Guid.Parse(xml.Root.Element("IdSimulacion").Value);
                resultado.FechaLanzamiento = DateTime.Parse(xml.Root.Element("FechaLanzamiento").Value);
                resultado.FechaFinalizacion = DateTime.Parse(xml.Root.Element("FechaFinalizacion").Value);

            }
            catch (Exception e)
            {
                throw e;
            }

            return resultado;
        }

        public XDocument ToXML()
        {
            System.Xml.Serialization.XmlSerializer xs
                = new System.Xml.Serialization.XmlSerializer(this.GetType());

            XDocument xml = new XDocument();

            XmlWriter writer = xml.CreateWriter();

            xs.Serialize(writer, this);
            writer.Flush();
            writer.Close();

            return xml;
        }

        public static void SaveFromListener(String content)
        {
            EventLog PBioEventLog = PBioEventLogger.initLogger();
            webappDBEntities webDB = new webappDBEntities();

            // SAve results
            // All the data has been read from the 
            // client. Display it on the console.
            PBioEventLog.WriteEntry("[RESULTS LISTENER] Read " + content.Length + " bytes from socket. \n");

            // Remove <PBIOEOF>
            content = content.Replace(EndFileTag, "");

            // Chequeamos si es Resultados o Error
            if (content.StartsWith(ErrorFileTag))
            {
                PBioEventLog.WriteEntry("[RESULTS LISTENER] Error received. Processing:\n"+content);
                try
                {
                    // Eliminamos ErrorTag
                    //content.Replace(ErrorFileTag, "");

                    // Parseamos el XML
                    XDocument errorXML = XDocument.Parse(content);

                    // Guardamos error log en BD
                    Log l = Log.LoadFromXML(errorXML); 
                    Guid idSimulacion = Log.GetIdSimulationOfLogFromXML(errorXML);

                    PBioEventLog.WriteEntry("[RESULTS LISTENER] Error file received: " + l.Texto);
                    webDB.Log.Add(l);
                    webDB.SaveChanges();
                    webDB.Simulacion.Single(s => s.IdSimulacion.Equals(idSimulacion)).Log = l;
                    webDB.Simulacion.Single(s => s.IdSimulacion.Equals(idSimulacion)).EstadoSimulacion = webDB.EstadoSimulacion.Where(es => es.Nombre.Equals("Error")).Single();
                    webDB.SaveChanges();
                }
                catch (Exception e)
                {
                    PBioEventLog.WriteEntry("[RESULTS LISTENER] ERROR processing error response: " + e.ToString());
                }
            }
            else if (content.StartsWith(ResultsFileTag))
            {
                PBioEventLog.WriteEntry("[RESULTS LISTENER] Results received. Processing...");
                try
                {
                    // Eliminamos el ResultsTag
                    content.Replace(ResultsFileTag, "");

                    // Parseamos el XML
                    XDocument resultadosXML = XDocument.Parse(content);

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
                PBioEventLog.WriteEntry("[RESULTS LISTENER] Unknow tag:" + content.Substring(0,15));
            }
        }
    }
}
