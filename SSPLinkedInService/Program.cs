namespace SSPLinkedInService
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main(string[] args)
        {
            var service = new SSPService();
#if (DEBUG)
            service.RunAsConsole(args);
#else
            ServiceBase.Run(new ServiceBase[] { service });
#endif
        }
    }
}
