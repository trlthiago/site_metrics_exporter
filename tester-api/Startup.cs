using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using tester_core;

namespace tester_api
{
    public class Startup
    {
        public static bool TakeSnapshootOnError;

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();

            //services.AddTransient<Services>();

            services.AddSingleton(x => new ScreenshotService("screens"));
            services.AddSingleton<Services>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                Services.ChromePath = @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe";
            }

            app.UseRouting();

            TakeSnapshootOnError = bool.Parse(System.Environment.GetEnvironmentVariable("SSERROR") ?? "false");
            System.Console.WriteLine("SSERROR " + TakeSnapshootOnError);

            if (!System.IO.Directory.Exists("screens"))
                System.IO.Directory.CreateDirectory("screens");

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(System.IO.Path.Combine(System.IO.Directory.GetCurrentDirectory(), "screens")),
                RequestPath = "/screens"
            });

            //app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
