using API.Mastery.Udemy.Configuration;
using FurnitureStoreData;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "FurnitureStore API",
        Version = "v1"
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Autorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. " +
        "\r\n\r\n Enter Prefix  (Bearer), space, then token." +
        " Example Bearer 123154125151251"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string [] {}
        }
    });
});

//Se agrega la db al program
builder.Services.AddDbContext<APIcontext>(options =>
options.UseSqlite(builder.Configuration.GetConnectionString("APIFurnitureStoreContext")));
// Agrego la dependencia del JWT Token  
builder.Services.Configure<JwtConfig>(builder.Configuration.GetSection("JwtConfig"));

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;//Esquema del jwt token
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;

})
.AddJwtBearer(jwt =>
{
    // Estos son los parametros para el token de autenticacion y autorizacion.
    var key = Encoding.ASCII.GetBytes(builder.Configuration.GetSection("JwtConfig:Secret").Value);
    jwt.SaveToken = true;// almacena el token cuando es verdadera la autenticacion
    jwt.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuerSigningKey = true,//La validacion tiene que suceder
        IssuerSigningKey = new SymmetricSecurityKey(key),//Le decimos cual es la validacion
        ValidateIssuer = false, //Una vez que esta en prod, esto tiene que estar en true
        ValidateAudience = false,
        RequireExpirationTime = false,
        ValidateLifetime = true
    };

});

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
    //En prod esto esta en verdadero para confirmar el mail
    options.SignIn.RequireConfirmedAccount = false)
    .AddEntityFrameworkStores<APIcontext>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
