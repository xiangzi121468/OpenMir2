using System.Text;
using ActivityModule;
using AppraisalModule;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using ShopModule;
using ShopModule.Repository;
using VipModule;
using WebApi.Admin.Services;

namespace WebApi
{
    public class Startup
    {
        private const string JwtSecret = "OpenMir2_Admin_Secret_Key_2024_Very_Long_String";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // 数据库连接字符串
            var connectionString = Configuration.GetConnectionString("Default") 
                ?? "server=127.0.0.1;uid=root;pwd=;database=mir2_db;";

            // JWT认证
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(JwtSecret)),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ClockSkew = TimeSpan.Zero
                    };
                });

            // 注册服务
            services.AddSingleton(new AdminService(connectionString));
            services.AddSingleton<IShopRepository>(new MySqlShopRepository(connectionString));
            services.AddSingleton<IShopService, ShopService>();
            services.AddSingleton(connectionString); // 用于控制器直接注入

            // 活动系统服务
            var activityService = new ActivityService(connectionString);
            var activityScheduler = new ActivityScheduler(activityService);
            services.AddSingleton<IActivityService>(activityService);
            services.AddSingleton(activityScheduler);
            
            // VIP地图服务
            services.AddSingleton(new VipMapService(connectionString));
            
            // 装备鉴定服务
            services.AddSingleton(new AppraisalService(connectionString));
            
            // 启动活动调度器 (可选，根据需要启用)
            // activityScheduler.Start();

            // CORS
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo()
                {
                    Version = "v0.0.1",
                    Title = "OpenMir2 API",
                    Description = "接口文档说明",
                    Contact = new OpenApiContact()
                    {
                        Name = "",
                        Email = "",
                        Url = null
                    }
                });
                
                var xmlFile = Path.Combine(AppContext.BaseDirectory, "WebApi.xml");
                if (File.Exists(xmlFile))
                {
                    options.IncludeXmlComments(xmlFile, true);
                }

                //定义JwtBearer认证方式
                options.AddSecurityDefinition("JwtBearer", new OpenApiSecurityScheme()
                {
                    Description = "JWT认证(直接在输入框中输入认证信息，不需要在开头添加Bearer)",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer"
                });

                OpenApiSecurityScheme scheme = new OpenApiSecurityScheme()
                {
                    Reference = new OpenApiReference() { Type = ReferenceType.SecurityScheme, Id = "JwtBearer" }
                };
                options.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    [scheme] = new string[0]
                });
            });

            services.AddControllers();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // 静态文件支持（GM后台前端）
            app.UseStaticFiles();

            app.UseRouting();

            // CORS
            app.UseCors();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
            });

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}