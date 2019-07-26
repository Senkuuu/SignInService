using Quartz;
using Quartz.Impl;
using Topshelf;
using System.Threading.Tasks;
namespace SignIn
{
    public class TimingService : ServiceControl, ServiceSuspend
    {
		IScheduler scheduler;

		public TimingService()
		{
		    //scheduler = await StdSchedulerFactory.GetDefaultScheduler();
		}

        public bool Continue(HostControl hostControl)
        {
            scheduler.ResumeAll(); return true;
        }

        public bool Pause(HostControl hostControl)
        {
            scheduler.PauseAll(); return true;
        }

        public bool Start(HostControl hostControl)
        {
		Task.Factory.StartNew(async()  =>
		{
			scheduler = await StdSchedulerFactory.GetDefaultScheduler(); scheduler.Start(); 
		});
          return true;
        }

        public bool Stop(HostControl hostControl)
        {
            scheduler.Shutdown(false); return true;
        }
    }
}


