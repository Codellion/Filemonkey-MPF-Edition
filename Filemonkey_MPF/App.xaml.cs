using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows;
using System.ComponentModel;
using System.Xml.Serialization;
using FileMonkey.Pandora.dal;
using FileMonkey.Pandora.dal.entities;
using System.IO;
using Filemonkey.Pandora.dbl;
using Memento.Persistence;
using Memento.Persistence.Commons.Keygen;
using Memento.Persistence.Interfaces;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using log4net.Config;
using log4net;

namespace FileMonkey.Picasso
{
    /// <summary>
    /// Lógica de interacción para App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static FmSettings Opciones { get; set; }

        public static Boolean SonarActivo { get; set; }

        public static MainWindow Home { get; set; }        
        public static InspectorDetail ActualInspectorDetail { get; set; }
        public static Settings ConfigurarOpciones { get; set; }

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
            var work = new BackgroundWorker();

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

        private void LoadOpciones()
        {
            if(File.Exists("Settings.xml"))
            {
                Stream fVault = new FileStream("Settings.xml", FileMode.Open);

                var unMarshall = new XmlSerializer(typeof(FmSettings));
                Opciones = (FmSettings)unMarshall.Deserialize(fVault);

                fVault.Close();
            }
            else
            {
                Opciones = new FmSettings();
            }
        }

        
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            SystemEvents.SessionSwitch += SystemEventsOnSessionSwitch;
            SystemEvents.SessionEnded += SystemEventsOnSessionEnded;
            


            swriter = new StringWriter();
            Console.SetOut(swriter);

            XmlConfigurator.Configure();
            
            RegistrarLog("<b>Inicializando la aplicación...</b>");

            RegistrarLog("<b>Cargando opciones de la aplicación...</b>");
            LoadOpciones();
            
            SonarActivo = true;

            Sonar = new Dictionary<int, BackgroundWorker>();

            try
            {
                IPersistence<FileInspector> servInspF = new Persistence<FileInspector>();
                var inspFilterF = new FileInspector();

                var rastreadoresF = servInspF.GetEntities(inspFilterF);

                foreach (var insp in rastreadoresF)
                {
                    AddWork(insp);
                }

                IPersistence<ServiceInspector> servInspV = new Persistence<ServiceInspector>();
                var inspFilterV = new ServiceInspector();

                var rastreadoresV = servInspV.GetEntities(inspFilterV);

                foreach (var insp in rastreadoresV)
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

        private void SystemEventsOnSessionEnded(object sender, SessionEndedEventArgs sessionEndedEventArgs)
        {
            switch (sessionEndedEventArgs.Reason)
            {
                case SessionEndReasons.Logoff:
                    SendPushNotif("Notificacion de seguridad", "Su sesion de usuario ha sido cerrada.");
                    break;
                case SessionEndReasons.SystemShutdown:
                    SendPushNotif("Notificacion de seguridad", "Su equipo ha sido apagado.");
                    break;
            }
        }

        private void SystemEventsOnSessionSwitch(object sender, SessionSwitchEventArgs sessionSwitchEventArgs)
        {
            switch (sessionSwitchEventArgs.Reason)
            {
                case SessionSwitchReason.SessionLogon:
                    //Logon
                    SendPushNotif("Notificacion de seguridad", "Su sesion de usuario ha sido iniciada.");

                    break;
                case SessionSwitchReason.SessionLogoff:
                    //Logoff
                    SendPushNotif("Notificacion de seguridad", "Su sesion de usuario ha sido cerrada.");

                    break;
                case SessionSwitchReason.RemoteConnect:
                    //Remote Connect
                    SendPushNotif("Notificacion de seguridad", "Se ha iniciado una conexion remota a su equipo.");

                    break;
                case SessionSwitchReason.RemoteDisconnect:
                    //Remote Disconnect
                    SendPushNotif("Notificacion de seguridad", "Se ha cerrado una conexion remota a su equipo.");

                    break;
                case SessionSwitchReason.SessionLock:
                    //lock
                    SendPushNotif("Notificacion de seguridad", "Su equipo ha sido bloqueado.");

                    break;
                case SessionSwitchReason.SessionUnlock:
                    //Unlock
                    SendPushNotif("Notificacion de seguridad", "Su equipo ha sido desbloqueado.");

                    break;
            }
        }

        private void bw_DoWork(object sender, DoWorkEventArgs e)
        {
            var worker = sender as BackgroundWorker;
            var inspector = e.Argument as Inspector;
            
            String prefix = "<b>[" + inspector.Name + "] - </b>";

            RegistrarLog(prefix + "Activando rastreador " + " ...", worker);

            while (inspector.Enable.Value)
            {
                if (worker.CancellationPending)
                {
                    RegistrarLog(prefix + "Cancelando rastreador " + " ...", worker);

                    e.Cancel = true;
                    return;
                }
                
                if (SonarActivo)
                {
                    RegistrarLog(prefix + "Ejecutando rastreador...", worker);

                    switch (inspector.InspectorType)
                    {
                        case (int) Inspector.EnumInspectorType.File:
                            ExecuteSonarFile(inspector as FileInspector, prefix, worker);

                            break;
                        case (int)Inspector.EnumInspectorType.Service:
                            //ExecuteSonarFile(inspector, prefix, worker);

                            break;
                    }
                    
                    RegistrarLog(prefix + "Fin de la ejecución del rastreador", worker);
                }

                WaitTime(inspector.CheckPeriod.Value);
            }
        }

        private void ExecuteSonarFile(FileInspector inspector, String prefix, BackgroundWorker worker)
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

            var listMsg = new List<string>();

            string msgFile = string.Empty;

            foreach (var file in files)
            {
                if (inspector.Action == (int)FileInspector.TypeActions.MoveSubDir)
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

                    FileSystem.DeleteDirectory(file.FullName, UIOption.OnlyErrorDialogs, 
                        RecycleOption.SendToRecycleBin);
                    
                    RegistrarLog(prefix + "Fichero eliminado correctamente", worker);
                }

                if (string.Concat(msgFile, file.Name).Length < 485)
                {
                    if (string.IsNullOrWhiteSpace(msgFile))
                    {
                        msgFile += file.Name;
                    }
                    else
                    {
                        msgFile += ", " + file.Name;
                    }
                }
                else
                {
                    listMsg.Add(msgFile);

                    msgFile = file.Name;
                }
            }

            if (!string.IsNullOrWhiteSpace(Opciones.PushoverUserKey)
                && inspector.EnablePushNotification.HasValue 
                && inspector.EnablePushNotification.Value
                && !string.IsNullOrWhiteSpace(msgFile))
            {
                foreach (var msgQueue in listMsg)
                {
                    RegistrarLog(prefix + "Enviando notificación Push...", worker);

                    if (SendPushNotif(inspector.Name, "Ficheros Procesados: " + msgQueue))
                    {
                        RegistrarLog(prefix + "Notificación enviada correctamente", worker);
                    }
                    else
                    {
                        RegistrarLog(prefix + "Error al enviar la notificación", worker);
                    }
                }

                RegistrarLog(prefix + "Enviando notificación Push...", worker);

                if(SendPushNotif(inspector.Name, "Ficheros Procesados: " + msgFile))
                {
                    RegistrarLog(prefix + "Notificación enviada correctamente", worker);
                }
                else
                {
                    RegistrarLog(prefix + "Error al enviar la notificación", worker);
                }
            }
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

            if (e.Cancelled)
            {
                
            }
            else if (e.Error != null)
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

        bool SendPushNotif(string title, string message)
        {
            string paramPushover = string.Format("user={0}&message={1}&title={2}", 
                Opciones.PushoverUserKey, message,title);

            if(!string.IsNullOrWhiteSpace(Opciones.PushoverDeviceName))
            {
                paramPushover += "&device=" + Opciones.PushoverDeviceName;
            }

            string res = HttpPost("https://api.pushover.net/1/messages.json", 
                "token=zjriLuuThOsusykQ8rEpIiJrBVUC82&" 
                + paramPushover);

            if (res != null && res.EndsWith(":1}"))
            {
                return true;
            }

            return false;
        }

        string HttpPost(string uri, string parameters)
        {
            // parameters: name1=value1&name2=value2	
            WebRequest webRequest = WebRequest.Create(uri);
            //string ProxyString = 
            //   System.Configuration.ConfigurationManager.AppSettings
            //   [GetConfigKey("proxy")];
            //webRequest.Proxy = new WebProxy (ProxyString, true);
            //Commenting out above required change to App.Config
            webRequest.ContentType = "application/x-www-form-urlencoded";
            webRequest.Method = "POST";
            byte[] bytes = Encoding.ASCII.GetBytes(parameters);
            Stream os = null;
            try
            { // send the Post
                webRequest.ContentLength = bytes.Length;   //Count bytes to send
                os = webRequest.GetRequestStream();
                os.Write(bytes, 0, bytes.Length);         //Send it
            }
            catch (WebException ex)
            {
            }
            finally
            {
                if (os != null)
                {
                    os.Close();
                }
            }

            try
            { // get the response
                WebResponse webResponse = webRequest.GetResponse();
                if (webResponse == null)
                { return null; }
                StreamReader sr = new StreamReader(webResponse.GetResponseStream());
                return sr.ReadToEnd().Trim();
            }
            catch (WebException ex)
            {
            }

            return null;
        } // end HttpPost 

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            KeyGeneration.Synchronize();
        }
    }
}
