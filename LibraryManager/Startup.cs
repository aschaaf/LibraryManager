using Amazon;
using Amazon.DynamoDBv2;
using Amazon.Runtime.CredentialManagement;
using LibraryManager.Adapters;
using LibraryManager.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LibraryManager
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
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Latest);

            var dynamoDbConfig = new AmazonDynamoDBConfig();
            dynamoDbConfig.RegionEndpoint = RegionEndpoint.USWest2;
            var sharedFile = new SharedCredentialsFile();
            sharedFile.TryGetProfile("default", out var profile);
            AWSCredentialsFactory.TryGetAWSCredentials(profile, sharedFile, out var credentials);

            services.AddSingleton<IDynamoDBAdapter, DynamoDBAdapter>(s => {
                return new DynamoDBAdapter(new AmazonDynamoDBClient(credentials, dynamoDbConfig));
            });
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
