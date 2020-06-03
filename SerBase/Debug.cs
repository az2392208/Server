using log4net;
using System;
using System.Collections.Generic;
using System.Text;
using log4net.Config;
using System.IO;

namespace ServerBase
{
    public static class Debug
    {
        private static ILog m_Log;
        static Debug()
        {
            XmlConfigurator.ConfigureAndWatch(new FileInfo(AppDomain.CurrentDomain.SetupInformation.ApplicationBase));
            m_Log = LogManager.GetLogger(typeof(Debug));
        }
    }
}
