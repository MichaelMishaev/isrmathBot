
using BL.Serives;
using BLL.Functions;
using BLL.Services;
using Serilog;

namespace mathProj
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var logFilePath = Path.Combine(AppContext.BaseDirectory, "Logs", "log-.txt");
            var logsDirectory = Path.Combine(AppContext.BaseDirectory, "Logs");
            Console.WriteLine($"Log file path: {logFilePath}");
            if (!Directory.Exists(logsDirectory))
            {
                Directory.CreateDirectory(logsDirectory);
            }
            Log.Logger = new LoggerConfiguration()
           //.MinimumLevel.Debug()
           .WriteTo.Console() // Optional: Logs to the console
           .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
           .CreateLogger();

            builder.Host.UseSerilog();
            //Log.Information("This is an informational message.");
            //Log.Warning("This is a warning.");
            //Log.Error("This is an error message.");
            //Log.Fatal("This is a critical error.");


            // Add services to the container.

            builder.Services.AddSingleton(Log.Logger);
            builder.Services.AddControllers();
            builder.Services.AddScoped<IndexService>();
            builder.Services.AddScoped<HelperService>();
            builder.Services.AddHttpClient<ChatGPTService>();
            builder.Services.AddScoped<WhatsAppService>();
            builder.Services.AddScoped<ImgFunctions>();
            //builder.Services.AddScoped<TestWhatsAppService>();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddTransient<ExerciseRepository>();
            builder.Services.AddTransient<CommonFunctions>();
            builder.Services.AddTransient<LeaderBoardFuncs>();
            builder.Services.AddTransient<QuizService>();
            builder.Services.AddTransient<RiddleService>();
            
            builder.Services.AddSingleton<IConfiguration>(builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
