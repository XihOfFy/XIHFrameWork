using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace XIHEmptyWeb
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDirectoryBrowser();  //开启目录浏览
        }

        private void UseStaticFiles(IApplicationBuilder app, string filePath)
        {
            var staticfile = new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                FileProvider = new PhysicalFileProvider(filePath),
                DefaultContentType = "application/x-msdownload"
            };
            // 设置MIME类型类型
            staticfile.ContentTypeProvider = new FileExtensionContentTypeProvider
            {
                Mappings =
                {
                    ["*"] = "application/x-msdownload"
                }
            };
            app.UseDirectoryBrowser(new DirectoryBrowserOptions() { FileProvider = staticfile.FileProvider });
            app.UseStaticFiles(staticfile);
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment()) app.UseDeveloperExceptionPage();

            string curDir = Directory.GetCurrentDirectory();
            var config = new ConfigurationBuilder()
                .SetBasePath(curDir)
                .AddJsonFile("appsettings.json", true)
                .Build();

            string configDir = config["ResRoot"];

            configDir = new DirectoryInfo(configDir).FullName;

            System.Console.WriteLine($"ResPath:{configDir}   \r\nCurDir:{curDir}");
            if (!string.IsNullOrEmpty(configDir) && !Directory.Exists(configDir))
            {
                Directory.CreateDirectory(configDir);
            }

            UseStaticFiles(app, configDir);
            app.Run(async (context) => { await context.Response.WriteAsync("Hello World"); });
        }

    }
}
