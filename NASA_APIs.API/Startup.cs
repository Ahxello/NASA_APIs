using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NASA_APIs.DAL.Context;
using Microsoft.EntityFrameworkCore;
using NASA_APIs.API.Data;
using NASA_APIs.Interfaces.Base.Repositories;
using NASA_APIs.DAL.Repositories;

namespace NASA_APIs.API
{
    public record Startup(IConfiguration Ņonfiguration)
    {

       
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<DataDB>(opt => opt
            .UseSqlServer(
                Ņonfiguration.GetConnectionString("Data"), 
            o => o.MigrationsAssembly("NASA_APIs.DAL.SqlServer")));
            services.AddTransient<DataDbInitializer>();

            services.AddScoped(typeof(IRepository<>), typeof(DbRepository<>));
            services.AddScoped(typeof(INamedRepository<>), typeof(DbNamedRepository<>));

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "NASA_APIs.API", Version = "v1" });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, DataDbInitializer db)
        {
            db.Initialize();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NASA_APIs.API v1"));
            }

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
