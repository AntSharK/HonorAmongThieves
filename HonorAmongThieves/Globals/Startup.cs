using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices.ReactDevelopmentServer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HonorAmongThieves
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            services.AddMvc(option => option.EnableEndpointRouting = false).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);

            services.AddSignalR().AddNewtonsoftJsonProtocol();
            services.AddSingleton<Heist.GameLogic.HeistGame>();
            services.AddSingleton<Cakery.GameLogic.CakeryGame>();

            // In prod, the React files are served from here
            services.AddSpaStaticFiles(config =>
            {
                config.RootPath = "ReactApp/build";
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();
            app.UseCookiePolicy();

            app.UseRouting();
            app.UseEndpoints(endPoints =>
            {
                endPoints.MapHub<Heist.HeistHub>("/heistHub");
                endPoints.MapHub<Cakery.CakeryHub>("/cakeryHub");
            });

            app.UseMvc();

            // Use React SPA for requests to the react app
            app.MapWhen(x => (x.Request.Path.Value.ToLower().StartsWith("/react")),
                builder =>
                {
                    app.UseSpa(spa =>
                    {
                        spa.Options.SourcePath = "ReactApp";
                        if (env.IsDevelopment())
                        {
                            spa.UseReactDevelopmentServer(npmScript: "start");
                        }
                    });
                });
        }
    }
}
