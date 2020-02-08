using System;

namespace ScraperLinkedInService
{
    public static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main(string[] args)
        {
            try
            {
                var service = new Service();
#if (DEBUG)
                service.RunAsConsole(args);
#else
                ServiceBase.Run(new ServiceBase[] { service });
#endif
            }
            catch (Exception ex)
            {
#if (DEBUG)
                Console.WriteLine(ex);
#endif
            }
        }
    }
}
