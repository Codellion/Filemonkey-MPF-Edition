using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.ComponentModel;
using FileMonkey.Pandora.dal;
using FileMonkey.Pandora.dal.entities;
using System.IO;
using Memento.Persistence;
using Memento.Persistence.Commons.Keygen;
using Memento.Persistence.Interfaces;
using log4net.Config;
using log4net;

namespace FileMonkey.Picasso
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Boolean SonarActivo { get; set; }

        public static MainWindow Home { get; set; }        
        public static InspectorDetail ActualInspectorDetail { get; set; }

        private static Inspectors _Inspectors;
        private static Log _RegistryWindow;
        public static Inspectors Inspectors
        {
            get
            {
                if (_Inspectors == null)
                {
                    _Inspectors = new Inspectors();
                    _Inspectors.Closed += new EventHandler(_Inspectors_Closed);
                }
                
                return _Inspectors;
            }
            set { _Inspectors = value; }
        }
             
        public static Log RegistryWindow
        {
            get
            {
                if (_RegistryWindow == null)
                {
                    _RegistryWindow = new Log();
                    _RegistryWindow.Closed += new EventHandler(_RegistryWindow_Closed);
                }

                return _RegistryWindow;
            }
            set { _RegistryWindow = value; }
        }

        public static IDictionary<int, BackgroundWorker> Sonar { get; set; }
        public static App Single { get { return App.Current as App; } }

        public static readonly ILog log = LogManager.GetLogger(typeof(App));
        private static StringWriter swriter;
        public static String Registry { get { return swriter.ToString(); } }

        public void AddWork(Inspector insp)
        {
            BackgroundWorker work = new BackgroundWorker();

            work.WorkerReportsProgress = true;
            work.WorkerSupportsCancellation = true;

            work.DoWork += new DoWorkEventHandler(bw_DoWork);
            work.ProgressChanged += new ProgressChangedEventHandler(bw_ProgressChanged);
            work.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bw_RunWorkerCompleted);

            Sonar.Add(insp.InspectorId.Value, work);

            if (SonarActivo)
            {
                work.RunWorkerAsync(insp);
            }
        }

        public void RemoveWork(Inspector insp)
        {
            if (insp.InspectorId.HasValue && Sonar != null && Sonar.ContainsKey(insp.InspectorId.Value))
            {
                BackgroundWorker work = Sonar[insp.InspectorId.Value];
                work.CancelAsync();

                Sonar.Remove(insp.InspectorId.Value);    
            }
        }

        public void UpdateWork(Inspector insp)
        {
            RemoveWork(insp);
            AddWork(insp);
        }
        
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            swriter = new StringWriter();
            Console.SetOut(swriter);

            XmlConfigurator.Configure();
            
            RegistrarLog("<b>Inicializando la aplicación...</b>");
            
            SonarActivo = true;

            Sonar = new Dictionary<int, BackgroundWorker>();

            try
            {
                IPersistence<Inspector> servInsp = new Persistence<Inspector>();
                var inspFilter = new Inspector
                                           {
                                               Enable = true
                                           };


                var rastreadores = servInsp.GetEntities(inspFilter);

                foreach (var insp in rastreadores)
                {
                    AddWork(insp);
                }
                
            }
            catch (Exception ex)
            {
                RegistrarLog("<b>Error al inicializar la aplicación: </b>" + ex.Message);
            }

            RegistrarLog("<b>Inicialización de la aplicación Finalizada</b>");
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var inspector = e.Argument as Inspector;
            
            String prefix = "<b>[" + inspector.Name + "] - </b>";

            RegistrarLog(prefix + "Activando rastreador " + " ...", worker);

            while (inspector.Enable.Value)
            {
                if (worker.CancellationPending == true)
                {
                    RegistrarLog(prefix + "Cancelando rastreador " + " ...", worker);

                    e.Cancel = true;
                    return;
                }
                else
                {
                    if (SonarActivo)
                    {

                        ExecuteSonar(inspector, prefix, worker);
                    }

                    WaitTime(inspector.CheckPeriod.Value);
                }
            }
        }

        private void ExecuteSonar(Inspector inspector, String prefix, BackgroundWorker worker)
        {
            var dir = new DirectoryInfo(inspector.Path);

            if(!dir.Exists)
            {
                return;
            }

            var files = dir.GetFiles();

            foreach (var rule in inspector.Rules.Value)
            {
                if (rule.RuleType != null)
                    switch ((RuleFile.TypeFileRule)rule.RuleType)
                    {
                        case RuleFile.TypeFileRule.Date:
                            var qfilesD = from file in files
                                          where rule.DateFirst < file.LastWriteTime
                                                && rule.DateLast > file.LastWriteTime
                                          select file;
                            files = qfilesD.ToArray();

                            break;

                        case RuleFile.TypeFileRule.Extension:
                            var qfilesE = from file in files
                                          where file.Extension.Contains(rule.ExtensionPattern)
                                          select file;
                            files = qfilesE.ToArray();

                            break;

                        case RuleFile.TypeFileRule.FileName:
                            var qfiles = from file in files
                                         where file.Name.Contains(rule.NamePattern)
                                         select file;
                            files = qfiles.ToArray();

                            break;
                    }
            }

            RegistrarLog(prefix + "Ejecutando rastreador...", worker);                                           

            foreach (var file in files)
            {
                if (inspector.Action == (int)Inspector.TypeActions.MoveSubDir)
                {
                    String destName = inspector.SubDirAction + Path.DirectorySeparatorChar + file.Name;

                    destName = destName.Substring(0, destName.Length - file.Extension.Length);

                    String destNameAux = destName;

                    int j = 1;

                    while (File.Exists(destNameAux + file.Extension))
                    {
                        destNameAux = destName + "-" + j.ToString(CultureInfo.InvariantCulture);
                        j++;
                    }

                    RegistrarLog(prefix + "Moviendo fichero " + file.FullName + "...", worker);
                                
                    file.MoveTo(destNameAux + file.Extension);

                    RegistrarLog(prefix + "Fichero movido correctamente a " + destNameAux, worker);
                }
                else
                {
                    RegistrarLog(prefix + "Eliminando fichero " + file.FullName + "...", worker);

                    file.Delete();

                    RegistrarLog(prefix + "Fichero eliminado correctamente", worker);
                }                            
            }

            RegistrarLog(prefix + "Fin de la ejecución del rastreador", worker);
        }
        
        private void WaitTime(int pValue)
        {
            int timeout;

            if (pValue == 0)
            {
                timeout = 5000;
            }
            else if (pValue < 4)
            {                
                timeout = pValue * 15000;
            }
            else if (pValue < 11)
            {
                int value = 1;

                switch (pValue)
                {
                    case 5: value = 2;
                        break;
                    case 6: value = 5;
                        break;
                    case 7: value = 10;
                        break;
                    case 8: value = 15;
                        break;
                    case 9: value = 30;
                        break;
                    case 10: value = 45;
                        break;
                }

                timeout = value * 60000;
            }
            else
            {
                timeout = (pValue - 10) * 60 * 60000;
            }

            System.Threading.Thread.Sleep(timeout);
        }

        private void bw_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //Registrar en el log el resultado del rastreador

            if (e.Cancelled == true)
            {
                
            }

            else if (!(e.Error == null))
            {
                
            }

            else
            {
                
            }
        }

        private void bw_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            ActualizarVisorLog();
        }

        private void ActualizarVisorLog()
        {
            if (RegistryWindow != null)
            {
                RegistryWindow.RefreshLog();
            } 
        }

        public void RegistrarLog(String msg , BackgroundWorker bw = null)
        {
            log.Info(msg);

            if (bw != null)
            {
                bw.ReportProgress(1);
            }
            else
            {
                ActualizarVisorLog();
            }
        }

        static void _Inspectors_Closed(object sender, EventArgs e)
        {
            Inspectors = null;
        }

        static void _RegistryWindow_Closed(object sender, EventArgs e)
        {
            RegistryWindow = null;
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            KeyGeneration.Synchronize();
        }
    }
}
