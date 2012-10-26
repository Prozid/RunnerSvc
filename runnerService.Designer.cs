namespace runnerSvc
{
    partial class runnerService
    {
        /// <summary> 
        /// Variable del diseñador requerida.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Limpiar los recursos que se estén utilizando.
        /// </summary>
        /// <param name="disposing">true si los recursos administrados se deben eliminar; false en caso contrario, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Código generado por el Diseñador de componentes

        /// <summary> 
        /// Método necesario para admitir el Diseñador. No se puede modificar 
        /// el contenido del método con el editor de código.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.runner_eventLog = new System.Diagnostics.EventLog();
            this.timerUpdateSimulations = new System.Windows.Forms.Timer(this.components);
            this.backgroundWorkerListener = new System.ComponentModel.BackgroundWorker();
            ((System.ComponentModel.ISupportInitialize)(this.runner_eventLog)).BeginInit();
            // 
            // timerUpdateSimulations
            // 
            this.timerUpdateSimulations.Interval = 2000;
            this.timerUpdateSimulations.Tick += new System.EventHandler(this.timerUpdateSimulations_Tick);
            // 
            // backgroundWorkerListener
            // 
            this.backgroundWorkerListener.DoWork += new System.ComponentModel.DoWorkEventHandler(this.backgroundWorkerListener_DoWork);
            // 
            // runnerService
            // 
            this.ServiceName = "Service1";
            ((System.ComponentModel.ISupportInitialize)(this.runner_eventLog)).EndInit();

        }

        #endregion

        private System.Diagnostics.EventLog runner_eventLog;
        private System.Windows.Forms.Timer timerUpdateSimulations;
        private System.ComponentModel.BackgroundWorker backgroundWorkerListener;
    }
}
