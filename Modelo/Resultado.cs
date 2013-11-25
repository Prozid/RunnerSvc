using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.IO;

namespace runnerSvc
{
    public partial class Resultado
    {
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
    }
}
