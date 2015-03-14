using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace PBioSvc
{
    public class PBioEventLogger
    {
        public static EventLog initLogger()
        {
            EventLog logger = new System.Diagnostics.EventLog();
            ((System.ComponentModel.ISupportInitialize)(logger)).BeginInit();
            if (!EventLog.SourceExists("PBio"))
            {
                EventLog.CreateEventSource(
                    "PBio", "PBioLog");
            }

            // Configuramos el registro de eventos del servicio
            logger.Source = "PBio";
            logger.Log = "PBioLog";

            return logger;
        }
    }
}
