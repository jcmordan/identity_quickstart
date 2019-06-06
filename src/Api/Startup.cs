using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;

namespace Api
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddMvcCore()
                .AddAuthorization()
                .AddJsonFormatters();

            // services.AddAuthentication("Bearer")
            //     .AddJwtBearer("Bearer", options =>
            //     {
            //         options.Authority = "http://localhost:5000";
            //         options.RequireHttpsMetadata = false;

            //         options.Audience = "api1";
            //     });

            // services.AddAuthentication("Bearer")
            //     .AddIdentityServerAuthentication(options =>
            //     {
            //         options.Authority = "http://localhost:5000";
            //         options.RequireHttpsMetadata = false;
            //         options.ApiSecret = "secrect";
            //         options.ApiName = "api1";
            //     });

            services.AddAuthentication("token")
                .AddIdentityServerAuthentication("token", options =>
                {
                    options.Authority = "http://localhost:5000";
                    options.RequireHttpsMetadata = false;

                    options.ApiName = "api1";
                    options.ApiSecret = "secret";
                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            return NewMethod(context);
                        },
                        OnAuthenticationFailed = context =>
                        {
                            return NewMethod1(context);
                        }
                    };

                });

            services.AddCors(options =>
            {
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins("http://localhost:5003")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });
        }

        private static Task NewMethod1(AuthenticationFailedContext context)
        {
            var te = context.Exception;
            return Task.CompletedTask;
        }

        private static Task NewMethod(MessageReceivedContext context)
        {
            if ((context.Request.Path.Value.StartsWith("/signalrhome")
                                            || context.Request.Path.Value.StartsWith("/looney")
                                            || context.Request.Path.Value.StartsWith("/usersdm")
                                           )
                                            && context.Request.Query.TryGetValue("token", out StringValues token)
                                        )
            {
                context.Token = token;
            }

            return Task.CompletedTask;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("default");

            app.UseAuthentication();

            app.UseMvc();
        }
    }
}
