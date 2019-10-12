using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ApiHost
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
                                                                             WebHost.CreateDefaultBuilder(args)
                                                                            .UseKestrel(SetHost)
                                                                            .UseStartup<Startup>();

        private static void SetHost(Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerOptions options)
        {
            //options.Listen(IPAddress.Any, 443, option =>
            //{
            //    //填入之前iis中生成的pfx文件路径和指定的密码　
            //    FileInfo fileInfo = new FileInfo("D:/iisSSL/ssl.pfx");
            //    if (fileInfo.Exists)
            //        option.UseHttps(fileInfo.FullName, "123456");
            //});
            options.Listen(IPAddress.Any, 8001);
        }
    }
}
