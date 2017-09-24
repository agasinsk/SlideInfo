using System.IO;
using Microsoft.AspNetCore.Hosting;

namespace SlideInfo.App
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseSetting("detailedErrors", "true")
                .UseIISIntegration()
                .UseStartup<Startup>()
                .CaptureStartupErrors(true)
                .UseApplicationInsights()
                .Build();

            host.Run();
        }
    }
}
