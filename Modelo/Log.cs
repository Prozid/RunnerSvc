﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.IO;

namespace PBioSvc
{
    public partial class Log
    {
        public XDocument ToXML()
        {
            XDocument xml = new XDocument(
                new XDeclaration("1.0", "utf-8", "yes"),
                new XElement("LogSimulacion",
                         new XElement("FechaLog", this.FechaLog),
                         new XElement("Texto", this.Texto))
                );
            return xml;
        }

        public static void Serialize(string file, Log logSimulacion)
        {
            System.Xml.Serialization.XmlSerializer xs
                = new System.Xml.Serialization.XmlSerializer(logSimulacion.GetType());

            StreamWriter writer = File.CreateText(file);
            xs.Serialize(writer, logSimulacion);
            writer.Flush();
            writer.Close();
        }

        public static Log Deserialize(string file)
        {
            Log logSimulacion;
            try
            {
                System.Xml.Serialization.XmlSerializer xs
                = new System.Xml.Serialization.XmlSerializer(typeof(Log));
                StreamReader reader = File.OpenText(file);
                logSimulacion = (Log)xs.Deserialize(reader);
                reader.Close();
            }
            catch (Exception e)
            {
                throw e;
            }
            return logSimulacion;
        }

        public static Log LoadFromXML(XDocument xml)
        {
            Log log = new Log();
            log.IdLog = Guid.NewGuid();

            Simulacion sim;
            webappDBEntities webDB = new webappDBEntities();
            try
            {
                if (xml.Root.Element("IdSimulacion") != null && xml.Root.Element("IdSimulacion").Value != "")
                {
                    String id = xml.Root.Element("IdSimulacion").Value;
                    try
                    {
                        Guid idSimulacion = Guid.Parse(id);
                        sim = webDB.Simulacion.Single(s => s.IdSimulacion.Equals(idSimulacion));

                        log.FechaLog = DateTime.Parse(xml.Root.Element("FechaLog").Value);
                        log.Texto = xml.Root.Element("Texto").Value;
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error parsing log of " + id + ". Exception \n:" + e.Message);
                    }
                }
                else
                {
                    throw new Exception("Error: No se encuentra el Id de la simulación en el XML recibido.");
                }
            }
            catch (Exception e)
            {
                throw e;
            }

            return log;
        }

        public static Guid GetIdSimulationOfLogFromXML(XDocument xml)
        {
            Simulacion sim;
            webappDBEntities webDB = new webappDBEntities();
            try
            {
                if (xml.Root.Element("IdSimulacion") != null && xml.Root.Element("IdSimulacion").Value != "")
                {
                    String id = xml.Root.Element("IdSimulacion").Value;
                    try
                    {
                        Guid idSimulacion = Guid.Parse(id);
                        sim = webDB.Simulacion.Single(s => s.IdSimulacion.Equals(idSimulacion));
                    }
                    catch (Exception e)
                    {
                        throw new Exception("Error parsing log of " + id + ". Exception \n:" + e.Message);
                    }
                }
                else
                {
                    throw new Exception("Error: No se encuentra el Id de la simulación en el XML recibido.");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
            return sim.IdSimulacion;
        }
    }
}
