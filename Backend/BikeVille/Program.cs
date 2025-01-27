using BikeVille.BLogic;
using BikeVille.Models.MongoModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using System.Text.Json.Serialization;

namespace BikeVille
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers().AddJsonOptions(
               jopt => jopt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();

            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Your API Title", Version = "v1" });

                // Add JWT Bearer authentication
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                        { new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } }, new List<string>() }
                });
            });

            builder.Services.AddDbContext<AdventureWorksLt2019Context>(opts =>
            opts.UseSqlServer(builder.Configuration.GetConnectionString("MainSqlConnection")));

            StaticDbConnection.ConnectionString = builder.Configuration.GetConnectionString("MainSqlConnection") ??
                throw new InvalidOperationException("Connection string 'MainSqlConnection' not found.");

            builder.Services.Configure<MongoDbConfig>(builder.Configuration.GetSection("MongoDBSettings"));
            builder.Services.AddSingleton<MongoDbConfig>();

            JwtSettings jwtSettings = new();
            builder.Configuration.GetSection("JwtSettings").Bind(jwtSettings);
            _ = builder.Services.AddSingleton(jwtSettings);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(opts =>
                {
                    opts.TokenValidationParameters = new TokenValidationParameters
                    {
                        // tutto questo serve per inizializzare la validazione del token
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings.Issuer,
                        ValidAudience = jwtSettings.Audience,
                        RequireExpirationTime = true,
                        IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
                    };
                    Console.WriteLine($"JWT Settings (Programcs): Issuer={jwtSettings.Issuer}, Audience={jwtSettings.Audience}");
                });

            builder.Services.AddAuthorization();
            builder.Services.AddScoped<DbManager>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://127.0.0.1:5500", "http://localhost:5500")
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            app.UseCors("AllowFrontend");

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
