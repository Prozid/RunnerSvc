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
    
    public partial class MetodoSeleccion
    {
        public MetodoSeleccion()
        {
            this.Simulacion = new HashSet<Simulacion>();
        }
    
        public System.Guid IdMetodoSeleccion { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }
        public string ParametrosXDefecto { get; set; }
    
        public virtual ICollection<Simulacion> Simulacion { get; set; }
    }
}