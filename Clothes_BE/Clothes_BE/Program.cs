using AutoMapper;
using Clothes_BE.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.Internal;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using static System.Net.Mime.MediaTypeNames;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddResponseCaching();
builder.Services.AddMemoryCache(opttion =>
{
    opttion.SizeLimit = 1024;
});

//add custom catching
//builder.Services.AddControllers(option =>
//{
//    option.CacheProfiles.Add("API_CACHING",
//        new Microsoft.AspNetCore.Mvc.CacheProfile()
//        {
//            Duration = 30
//        });
//});
builder.Services.AddControllers();
//add automapper
builder.Services.AddAutoMapper(typeof(Program).Assembly);
builder.Services.AddMvc();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
//#1 add database
builder.Services.AddDbContext<DatabaseContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DBConnection")));
//# add cors
//builder.Services.AddCors(p => p.AddPolicy("Mycors", build =>
//{
//    build.WithOrigins().AllowAnyMethod().AllowAnyHeader();

//}));

//
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(option =>
    {
        option.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            //validissuer = builder.configuration["jwt:issuer"],
            //validaudience = builder.configuration["jwt:audience"],
            ValidateIssuerSigningKey = true,            
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["jwt:key"]))
        };
    });
//#6 add authorization with swagger
builder.Services.AddSwaggerGen(op =>
{
    op.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Clothes_BE", Version = "v1" });
    op.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please enter token",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "bearer"
    });
    op.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type=ReferenceType.SecurityScheme,
                    Id="Bearer"
                }
            },
            new string[]{}
        }
    });

});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//static files
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "images")),
    RequestPath = "/images"
});
//app.MapGet("/products/get-test", async (DatabaseContext db) => await db.products.ToListAsync());

app.UseHttpsRedirection();
app.UseCors();
app.MapControllers();
app.UseAuthentication();
app.UseAuthorization();
//use middleware
//app.Use(async (context, next) =>
//{
//    string session_cookie_name = "guest_id";
//    //var identity = context.User.Identity as ClaimsIdentity;
//    //var user = identity.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name)?.Value;    
//    //if(user == null)
//    //{
//        //create guest_id = session_id
//        if (!context.Request.Cookies.ContainsKey(session_cookie_name))
//        {
//            var sessionId = Guid.NewGuid().ToString();
//            //add cookie
//            context.Response.Cookies.Append(session_cookie_name, sessionId, new CookieOptions
//            {
//                HttpOnly = false,// enable access of javacript
//                Secure = true,
//                IsEssential = true,
//                Expires = DateTime.UtcNow.AddDays(30)
//            });
//            //set items value for other middleware
//            context.Items[session_cookie_name] = sessionId;
//        }      
//    //}
//    await next.Invoke();
//});
//app.Run(async (context,next) =>
//{
//    string sessionId = context.Request.Cookies["user_unknown_cookie"] ?? "Not found cookie";
//     //context.Response.WriteAsync($"Session ID: {sessionId}");
//    await next.Invoke();
//});
app.Use(async (context, next) =>
{
    //var get = context.Request.Path.ToString();
    var rd = new Random();
    int? user = context.User.Identity.IsAuthenticated
                    ? int.Parse(context.User.FindFirst(ClaimTypes.Name)?.Value)
                    : null;
    string session_id = context.Request.Cookies[builder.Configuration["Settings:Cookie_key"]];
    if (context.Request.Path.ToString() == "/api/cart-items/add-to-cart")
    {
        context.Items["auth"] = new List<string>{ user.ToString(),session_id };
    }
    else
    {
        context.Items["random"] = "not";
    }   
    await next.Invoke();
});
app.Run();
