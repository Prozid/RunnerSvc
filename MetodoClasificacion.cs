//------------------------------------------------------------------------------
// <auto-generated>
//    Este código se generó a partir de una plantilla.
//
//    Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//    Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace runnerSvc
{
    using System;
    using System.Collections.Generic;
    
    public partial class MetodoClasificacion
    {
        public MetodoClasificacion()
        {
            this.Simulacion = new HashSet<Simulacion>();
        }
    
        public System.Guid IdMetodoClasificacion { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string ParametrosXDefecto { get; set; }
    
        public virtual ICollection<Simulacion> Simulacion { get; set; }
    }
}