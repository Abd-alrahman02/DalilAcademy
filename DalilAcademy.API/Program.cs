
using DalilAcademy.API.Middleware;
using DalilAcademy.Core.Models;
using DalilAcademy.Core.Repository;
using DalilAcademy.Core.Security;
using DalilAcademy.Core.Service;
using DalilAcademy.Core.Storage;
using DalilAcademy.Core.Validators.Students;
using DalilAcademy.Infra.Data;
using DalilAcademy.Infra.Repository;
using DalilAcademy.Infra.Security;
using DalilAcademy.Infra.Service;
using DalilAcademy.Infra.Storage;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped<ICourseRepository, CourseRepository>();
builder.Services.AddScoped<ICourseService, CourseService>();

builder.Services.AddScoped<IStudentRepository,  StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();

builder.Services.AddSingleton<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddSingleton<ITokenService, JwtTokenService>();

builder.Services.AddSingleton<IImageService, LocalImageService>();

// DB Connection and seed data
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
    .UseSeeding((context, _) =>
    {
        var dbContext = (AppDbContext)context;

        // Seed Admin User
        var adminEmail = "admin@dalil.jo";

        if (!dbContext.Users.Any(u => u.Email == adminEmail))
        {
            var adminPassword =
                builder.Configuration["SeedAdmin:Password"];

            if (string.IsNullOrWhiteSpace(adminPassword))
            {
                throw new InvalidOperationException(
                    "SeedAdmin:Password is missing from configuration.");
            }

            dbContext.Users.Add(new User
            {
                Email = adminEmail,

                Role = Roles.Admin,

                PasswordHash =
                    BCrypt.Net.BCrypt.HashPassword(adminPassword)
            });

            dbContext.SaveChanges();
        }
        // Seed Instructors
        if (!dbContext.Instructors.Any())
        {
            dbContext.Instructors.AddRange(
                new Instructor
                {
                    FullName = "Dana Kanan"
                },
                new Instructor
                {
                    FullName = "Raghad Quran"
                },
                new Instructor
                {
                    FullName = "Heba Kasasbeh"
                }
            );

            dbContext.SaveChanges();
        }

        // Seed Courses
        if (!dbContext.Courses.Any())
        {
            var instructors = dbContext.Instructors.ToList();

            dbContext.Courses.AddRange(
                new Course
                {
                    Title = "C# Fundamentals",
                    Price = 100,
                    InstructorId = instructors[0].Id
                },
                new Course
                {
                    Title = "ASP.NET Core Web API",
                    Price = 150,
                    InstructorId = instructors[1].Id
                },
                new Course
                {
                    Title = "Entity Framework Core",
                    Price = 120,
                    InstructorId = instructors[2].Id
                }
            );

            dbContext.SaveChanges();
        }
    })
);

// Validators
builder.Services.AddValidatorsFromAssemblyContaining<CreateStudentDtoValidator>();

//Global Exception Handler
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var jwtSection = builder.Configuration.GetSection("Jwt");

var jwtSettings = jwtSection.Get<JwtSettings>()
    ?? throw new InvalidOperationException(
        "JWT settings are missing.");

if (Encoding.UTF8.GetByteCount(jwtSettings.Key) < 32)
{
    throw new InvalidOperationException(
        "JWT Key must contain at least 32 bytes.");
}

builder.Services.Configure<JwtSettings>(jwtSection);

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,

                ValidateAudience = true,

                ValidateLifetime = true,

                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtSettings.Issuer,

                ValidAudience = jwtSettings.Audience,

                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSettings.Key)),

                ClockSkew = TimeSpan.FromMinutes(1)
            };
    });

builder.Services.AddAuthorization();


var app = builder.Build(); //------------------------------------------------------------


app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
