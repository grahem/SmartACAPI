using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using SmartACDeviceAPI.Options;
using SmartACDeviceAPI.Services;
using System.Diagnostics;
using System.Text;
using Microsoft.OpenApi.Models;

namespace SmartACDeviceAPI
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
            //app settings
            var appSettingsSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsSection);
            var appSettings = appSettingsSection.Get<AppSettings>();
            var key = Encoding.ASCII.GetBytes(appSettings.Secret);

            //jwt auth
            services.AddAuthentication(x =>
            {
                x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(x =>
            {
                x.RequireHttpsMetadata = false;
                x.SaveToken = true;
                x.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });
            services.Configure<AuthZOptions>(options => Configuration.GetSection("Jwt").Bind(options));

            //aws
            services.AddDefaultAWSOptions(Configuration.GetAWSOptions());
            services.AddAWSService<IAmazonDynamoDB>();
            services.AddScoped<IDynamoDBContext, DynamoDBContext>();
            services.AddScoped<IAmazonDynamoDB, AmazonDynamoDBClient>();


            //services and helpers
            services.AddTransient<DeviceService>();
            services.AddTransient<IDeviceAuthZService, DeviceAuthZService>();
            services.AddTransient<IUserAuthZService, UserAuthZService>();
            services.AddTransient<UserAuthZService>();
            services.AddTransient<MaintenanceService>();
            services.AddTransient<MeasurementService>();

            //swager
            services.AddSwaggerGen(c => c.SwaggerDoc("v1", new OpenApiInfo { Title = "SMart AC Device API", Version = "v1" }));

            //controllers
            services.AddControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();

            app.UseAuthorization();

            app.UseSwagger();

app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
    });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
