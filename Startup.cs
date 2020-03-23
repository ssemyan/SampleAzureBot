using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Bot.Builder.AI.QnA;

namespace SampleBot
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            // Create the Bot Framework Adapter with error handling enabled.
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();

            // Create QnAMaker endpoint as a singleton
            services.AddSingleton(new QnAMakerEndpoint
            {
                KnowledgeBaseId = Configuration.GetValue<string>($"QnAKnowledgebaseId"),
                EndpointKey = Configuration.GetValue<string>($"QnAAuthKey"),
                Host = Configuration.GetValue<string>($"QnAEndpointHostName")
            });

            // Create the User state using memory for storage
            services.AddSingleton(new UserState(new MemoryStorage()));

            // Create the Bot Config to hold extra data (like the base URL)
            services.AddSingleton(new BotConfig { BaseUrl = Configuration.GetValue<string>($"BaseUrl") });

            // Create the bot as a transient. In this case the ASP Controller is expecting an IBot.
            services.AddTransient<IBot, Bots.SampleBotBot>();

            // Add CORS
            services.AddCors(
                options => options.AddPolicy("AllowCors",
                    builder =>
                    {
                        builder
                            .WithOrigins("https://botservice.hosting.portal.azure.net", "https://hosting.onecloud.azure-test.net/") // allow access from azure bot service
                            .AllowAnyMethod()
                            .AllowAnyHeader();
                    })
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseWebSockets();

            //Enable CORS policy "AllowCors"  
            app.UseCors("AllowCors");

            app.UseMvc();
        }
    }
}
