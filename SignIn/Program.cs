using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace SignIn
{
    class Program
    {
        static void Main(string[] args)
        {
            //ServicePointManager.ServerCertificateValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;
            log4net.Config.XmlConfigurator.Configure();
            HostFactory.Run(x =>
            {
                x.UseLog4Net();
                x.Service<TimingService>();

                x.SetDescription("定时任务服务");
                x.SetDisplayName("Timing Service");
                x.SetServiceName("TimingSvr");

                x.EnablePauseAndContinue();
            });
        }
    }
}
