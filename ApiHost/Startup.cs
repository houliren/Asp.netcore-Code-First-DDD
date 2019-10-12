using Domain;
using Domain.EfCoreContent;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Panda.DynamicWebApi;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;

namespace ApiHost
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
            //跨域配置
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(
                    builder =>
                    {
                        builder//.WithOrigins("*")
                           .AllowAnyHeader()
                           .AllowAnyMethod()
                           .AllowAnyOrigin()
                           .AllowCredentials();
                    });
            });

            services.AddDbContextPool<EfContent>(options =>
                options.UseMySQL("Database = 'studyddd'; Data Source = 'localhost'; User Id = 'root'; Password = ''; charset = 'utf8'; pooling = true; Allow Zero Datetime = True;Allow User Variables=True;TreatTinyAsBoolean=false",b=>b.MigrationsAssembly("Api")), 200);

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            //动态生成Api接口
            services.AddDynamicWebApi();
            services.AddMemoryCache();
            //配置SwaggerApi自动生成器
            services.AddSwaggerGen(options =>
            {
                options.DocInclusionPredicate((docName, description) => true);//swagger支持动态生成的api接口
                options.CustomSchemaIds(type => type.FullName);               //swagger支持动态生成的api接口

                options.SwaggerDoc("v1", new Info { Title = "My API", Version = "v1" });
                var basePath = Path.GetDirectoryName(typeof(Program).Assembly.Location);//获取代码运行的相对路径
                options.IncludeXmlComments(Path.Combine(basePath, "Api.xml"), true);//插入代码上的注释放入Swagger
                options.IncludeXmlComments(Path.Combine(basePath, "ApiController.xml"), true);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IServiceProvider serviceProvider)
        {
            //使用跨越配置
            app.UseCors();
            //使用SwaggerApi自动生成器
            app.UseSwagger();
            //使用SwaggerApi自动生成器的Ui界面
            app.UseSwaggerUI(option =>
            {
                option.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });


            using (var scope = serviceProvider.CreateScope())
            {
                var efContent = scope.ServiceProvider
                    .GetRequiredService<EfContent>();
                efContent.RunUpdateDataBaseEntity();
            }


            //if (env.IsDevelopment())
            //{
            //    app.UseDeveloperExceptionPage();
            //}
            //else
            //{
            //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //    app.UseHsts();
            //}

            //app.UseHttpsRedirection();
            app.UseMvc();

        }
    }
}
