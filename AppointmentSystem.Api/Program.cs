
namespace AppointmentSystem.Api
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using AppointmentSystem.Business.Services;
    using AppointmentSystem.Data.Interfaces;
    using AppointmentSystem.Data.Repositories;
    using AppointmentSystem.Data.DbContext;
    using Serilog;

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

            builder.Logging.ClearProviders();
            builder.Logging.AddSerilog();

            var configuration = builder.Configuration;


            builder.Services.AddControllers()
                .AddNewtonsoftJson(options =>
                    {
                        options.SerializerSettings.DateTimeZoneHandling = Newtonsoft.Json.DateTimeZoneHandling.Utc;
                        options.SerializerSettings.DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffZ";
                    });

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IAppointmentService, AppointmentService>();
            builder.Services.AddScoped<IAppointmentRepository, AppointmentRepository>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Enable Swagger (even in production for testing)
            if (app.Environment.IsDevelopment() || Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();
            app.Urls.Add("http://0.0.0.0:3000");

            app.Run();
        }
    }
}
