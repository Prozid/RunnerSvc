using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using System.Xml;
using System.IO;

namespace PBioSvc
{
    public partial class Simulacion
    {
        public static void Serialize(string file, Simulacion sim)
        {
            System.Xml.Serialization.XmlSerializer xs
                = new System.Xml.Serialization.XmlSerializer(sim.GetType());

            StreamWriter writer = File.CreateText(file);
            xs.Serialize(writer, sim);
            writer.Flush();
            writer.Close();
        }

        public static Simulacion Deserialize(string file)
        {
            Simulacion sim;
            try
            {
                System.Xml.Serialization.XmlSerializer xs
                = new System.Xml.Serialization.XmlSerializer(typeof(Simulacion));
                StreamReader reader = File.OpenText(file);
                sim = (Simulacion)xs.Deserialize(reader);
                reader.Close();
            }
            catch
            {
                sim = null;
            }
            return sim;
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

        public static String ParseSelectionParameters(String s)
        {
            /* Formato esperado:
             * SELECTION_METHOD=8979932d-e69f-4ac1-8fbe-0e15d8820668;NUM_EXECUTIONS=1;CHOOSE_CRITERIA=1
             * Hay que modificar el Guid por el nombre del método (Posiciones 16-52)
             */
            String old;
            Boolean exito = false;
            String parameters = s.Substring(52);
            Guid idMetodoSeleccion;
            String nombreMetodoSeleccion = "";
            exito = Guid.TryParse(s.Substring(16, 36), out idMetodoSeleccion);
            if (exito)
            {
                using (webappDBEntities db = new webappDBEntities())
                {
                    MetodoSeleccion ms = db.MetodoSeleccion.Find(idMetodoSeleccion);
                    nombreMetodoSeleccion = (ms == null) ? "" : ms.Nombre;
                }

                old = "SELECTION_METHOD=" + nombreMetodoSeleccion + parameters;
            }
            else
            {
                old = null;
            }

            if (nombreMetodoSeleccion == "")
                s = "";
            else
                s = old;
            s = old;
            return s;
        }

        public static String ParseClasificationParameters(String s)
        {
            /* Formato esperado:
             * CLASIFICATION_METHOD=4ad04057-4bd4-4996-aa9a-2b039cc61c2e;NUM_NEIGHBOURS=1;DISTANCE_TYPE=1
             * Hay que modificar el Guid por el nombre del método (Posiciones 16-52)
             */
            String old;
            Boolean exito = false;
            String parameters = s.Substring(57);
            Guid idMetodoClasificacion;
            String nombreMetodoClasificacion = "";
            exito = Guid.TryParse(s.Substring(21, 36), out idMetodoClasificacion);
            if (exito)
            {
                using (webappDBEntities db = new webappDBEntities())
                {
                    MetodoClasificacion mc = db.MetodoClasificacion.Find(idMetodoClasificacion);
                    nombreMetodoClasificacion = (mc == null) ? "" : mc.Nombre;
                }

                old = "CLASIFICATION_METHOD=" + nombreMetodoClasificacion + parameters;
            }
            else
            {
                old = null;
            }

            if (nombreMetodoClasificacion == "")
                s = "";
            else
                s = old;

            return s;
        }
    }
}
