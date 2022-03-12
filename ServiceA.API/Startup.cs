using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceA.API
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

            services.AddControllers();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "ServiceA.API", Version = "v1" });
            });

            services.AddHttpClient<ProductService>(x =>
            {
                x.BaseAddress = new Uri("https://localhost:5003/api/products/");
            }).AddPolicyHandler(GetRetryPolicy()); //Retry Pattern ile iþlem yapýlmasý gerektiði belirtiyoruz.
        }

        private IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions.HandleTransientHttpError()
                    .OrResult(x => x.StatusCode == HttpStatusCode.NotFound)
                    .WaitAndRetryAsync( 15, //Ýsteðin toplamda kaç kere tekrarlanacaðý
                                        retryAttemp =>   
                                        {
                                            Debug.WriteLine($"Tekrar Sayýsý:{retryAttemp}");
                                            return TimeSpan.FromSeconds(3);  //Tekrarlanmalar arasýnda ne kadar süre bekleyeceði
                                        },
                                        onRetryAsync: onRetryAsync); // Tekrarlama iþlemini gerçekleþtirmeden önce çalýþtýrýlmak istenen iþ/metod
        }

        private Task onRetryAsync(DelegateResult<HttpResponseMessage> arg1, TimeSpan arg2) 
        {
            Debug.WriteLine($"Ýstek tekrarlandý{arg2.TotalMilliseconds}");
            return Task.CompletedTask; 
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ServiceA.API v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
