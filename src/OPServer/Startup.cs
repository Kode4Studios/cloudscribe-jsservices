﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.DataProtection;
using System.Globalization;
using Microsoft.AspNetCore.SpaServices.Webpack;

namespace OPServer
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                .AddJsonFile("appsettings.dev.json", optional: true,  reloadOnChange: true)
                .AddEnvironmentVariables();

            Configuration = builder.Build();

            environment = env;
        }

        public IHostingEnvironment environment { get; set; }

        public IConfigurationRoot Configuration { get; }
        public bool SslIsAvailable { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // https://docs.asp.net/en/latest/security/data-protection/configuration/overview.html
            // the data protection keys are used for encrypting the auth cookie
            // they are normally stored on a keyring for the OS but you can control where they are stored
            // we use dataprotection to encrypt some content in the database specifically
            // the client secrets for social auth, and smtp password
            // since you can't decrypt that stuff without the keys you typically need to control
            // the keys if the site may need to be moved from one server to another or used in 
            // a web farm, so this example code stores them in the file system with the app
            // it is of paramount importance to keep the keys secure, so apply your own security policy and practices 
            // in considering how best to manage these keys and where to store them
            // anyone with access to the keys could forge a cookie with admin credentials and gain control of your app/site
            string pathToCryptoKeys = Path.Combine(environment.ContentRootPath, "dp_keys");
            services.AddDataProtection()
                .PersistKeysToFileSystem(new System.IO.DirectoryInfo(pathToCryptoKeys));

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto;
            });

            services.AddMemoryCache();
            
            //services.AddSession();

            ConfigureAuthPolicy(services);

            services.AddOptions();

            services.AddCloudscribeCoreNoDbStorage();
            services.AddCloudscribeLoggingNoDbStorage(Configuration);
            services.AddCloudscribeLogging();
            services.AddCloudscribeCore(Configuration);


            // If you need to call nodejs for processing from your server, uncomment this.
            // services.AddNodeServices();

            //var cert = new X509Certificate2(Path.Combine(environment.ContentRootPath, "yourcustomcert.pfx"), "");
            services.AddIdentityServer()
                        .AddCloudscribeCoreNoDbIdentityServerStorage()
                        .AddCloudscribeIdentityServerIntegration()
                        // https://identityserver4.readthedocs.io/en/dev/topics/crypto.html
                        //.AddSigningCredential(cert) // create a certificate for use in production
                        .AddTemporarySigningCredential() // don't use this for production
                        ;

            services.AddLocalization(options => options.ResourcesPath = "GlobalResources");

            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                    new CultureInfo("en-US"),
                    new CultureInfo("en-GB"),
                    new CultureInfo("fr-FR"),
                    new CultureInfo("fr"),
                };

                // State what the default culture for your application is. This will be used if no specific culture
                // can be determined for a given request.
                options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");

                // You must explicitly state which cultures your application supports.
                // These are the cultures the app supports for formatting numbers, dates, etc.
                options.SupportedCultures = supportedCultures;

                // These are the cultures the app supports for UI strings, i.e. we have localized resources for.
                options.SupportedUICultures = supportedCultures;

                // You can change which providers are configured to determine the culture for requests, or even add a custom
                // provider with your own logic. The providers will be asked in order to provide a culture for each request,
                // and the first to provide a non-null result that is in the configured supported cultures list will be used.
                // By default, the following built-in providers are configured:
                // - QueryStringRequestCultureProvider, sets culture via "culture" and "ui-culture" query string values, useful for testing
                // - CookieRequestCultureProvider, sets culture via "ASPNET_CULTURE" cookie
                // - AcceptLanguageHeaderRequestCultureProvider, sets culture via the "Accept-Language" request header
                //options.RequestCultureProviders.Insert(0, new CustomRequestCultureProvider(async context =>
                //{
                //  // My custom request culture logic
                //  return new ProviderCultureResult("en");
                //}));
            });

            // for production be sure to use ssl
            SslIsAvailable = Configuration.GetValue<bool>("AppSettings:UseSsl");
            services.Configure<MvcOptions>(options =>
            {
                if (SslIsAvailable)
                {
                    options.Filters.Add(new RequireHttpsAttribute());
                }

            });

            // it is recommended to use lower case urls
            services.AddRouting(options =>
            {
                options.LowercaseUrls = true;
            });

            services.AddMvc()
                .AddRazorOptions(options =>
                {
                    options.AddCloudscribeViewLocationFormats();

                    options.AddCloudscribeCommonEmbeddedViews();
                    options.AddCloudscribeNavigationBootstrap3Views();
                    options.AddCloudscribeCoreBootstrap3Views();
                    options.AddCloudscribeFileManagerBootstrap3Views();
                    options.AddCloudscribeLoggingBootstrap3Views();
                    options.AddCloudscribeCoreIdentityServerIntegrationBootstrap3Views();

                    options.ViewLocationExpanders.Add(new cloudscribe.Core.Web.Components.SiteViewLocationExpander());
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        // you can add things to this method signature and they will be injected as long as they were registered during 
        // ConfigureServices
        public void Configure(
            IApplicationBuilder app,
            IHostingEnvironment env,
            ILoggerFactory loggerFactory,
            IOptions<cloudscribe.Core.Models.MultiTenantOptions> multiTenantOptionsAccessor,
            IServiceProvider serviceProvider,
            IOptions<RequestLocalizationOptions> localizationOptionsAccessor,
            cloudscribe.Logging.Web.ILogRepository logRepo
            )
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();
            ConfigureLogging(loggerFactory, serviceProvider, logRepo);

            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("Is Development " + env.IsDevelopment().ToString());
            Console.ResetColor();

            if (env.IsDevelopment())
            {
                // app.UseStatusCodePages();
                app.UseWebpackDevMiddleware(
                    new WebpackDevMiddlewareOptions {
                        HotModuleReplacement = true
                    }
                );
            }
                
            app.UseExceptionHandler("/Error/Default");
            

            app.UseForwardedHeaders();
            app.UseStaticFiles();
            
            //app.UseSession();

            app.UseRequestLocalization(localizationOptionsAccessor.Value);
            
            var multiTenantOptions = multiTenantOptionsAccessor.Value;

            app.UseCloudscribeCore(
                    loggerFactory,
                    multiTenantOptions,
                    SslIsAvailable,
                    IdentityServerIntegratorFunc);

            UseMvc(app, multiTenantOptions.Mode == cloudscribe.Core.Models.MultiTenantMode.FolderName);

            CoreNoDbStartup.InitializeDataAsync(app.ApplicationServices).Wait();
            CloudscribeIdentityServerIntegrationNoDbStorage.InitializeDatabaseAsync(app.ApplicationServices).Wait();

        }

        private void UseMvc(IApplicationBuilder app, bool useFolders)
        {
            app.UseMvc(routes =>
            {
                routes.AddCloudscribeFileManagerRoutes();

                if (useFolders)
                {

					routes.MapRoute(
                       name: "foldererrorhandler",
                       template: "{sitefolder}/oops/error/{statusCode?}",
                       defaults: new { controller = "Oops", action = "Error" },
                       constraints: new { name = new cloudscribe.Core.Web.Components.SiteFolderRouteConstraint() }
                    );
					
                    routes.MapRoute(
                        name: "folderdefault",
                        template: "{sitefolder}/{controller}/{action}/{id?}",
                        defaults: new { controller = "Home", action = "Index" },
                        constraints: new { name = new cloudscribe.Core.Web.Components.SiteFolderRouteConstraint() }
                        );                    

                }

                routes.MapRoute(
                    name: "errorhandler",
                    template: "oops/error/{statusCode?}",
                    defaults: new { controller = "Error", action = "Oops" }
                    );

                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}"
                    );



            });
        }

        // this Func is passed optionally in to app.UseCloudscribeCore
        // to wire up identity server integration at the right point in the middleware pipeline
        private bool IdentityServerIntegratorFunc(IApplicationBuilder builder, cloudscribe.Core.Models.ISiteContext tenant)
        {
            builder.UseIdentityServer();

            //// this sets up the authentication for apis within this application endpoint
            //// ie apis that are hosted in the same web app endpoint with the authority server
            //// this is not needed here if you are only using separate api endpoints
            //// it is needed in the startup of those separate endpoints
            //// note that with both cookie auth and jwt auth middleware the principal is merged from both the cookie and the jwt token if it is passed
            var addressFeature = (Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature) 
                                    builder.ServerFeatures[typeof(Microsoft.AspNetCore.Hosting.Server.Features.IServerAddressesFeature)];
            var authority = addressFeature.Addresses?.FirstOrDefault();
            builder.UseIdentityServerAuthentication(new IdentityServerAuthenticationOptions
            {
               Authority = authority??"https://localhost:5000",
               // using the site aliasid as the scope so each tenant has a different scope
               // you can view the aliasid from site settings
               // clients must be configured with the scope to have access to the apis for the tenant
               ApiName = tenant.AliasId,
               //RoleClaimType = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role",
               //AuthenticationScheme = AuthenticationScheme.Application,

               RequireHttpsMetadata = SslIsAvailable
            });

            return true;
        }

        private void ConfigureAuthPolicy(IServiceCollection services)
        {
            //https://docs.asp.net/en/latest/security/authorization/policies.html

            services.AddAuthorization(options =>
            {
                options.AddCloudscribeCoreDefaultPolicies();
                options.AddCloudscribeLoggingDefaultPolicy();

                options.AddPolicy(
                    "FileManagerPolicy",
                    authBuilder =>
                    {
                        authBuilder.RequireRole("Administrators", "Content Administrators");
                    });

                options.AddPolicy(
                    "FileManagerDeletePolicy",
                    authBuilder =>
                    {
                        authBuilder.RequireRole("Administrators", "Content Administrators");
                    });

                // add other policies here 
                options.AddPolicy(
                    "IdentityServerAdminPolicy",
                    authBuilder =>
                    {
                        authBuilder.RequireRole("Administrators");
                    });

            });

        }

        private void ConfigureLogging(
            ILoggerFactory loggerFactory,
            IServiceProvider serviceProvider
            , cloudscribe.Logging.Web.ILogRepository logRepo
            )
        {
            // a customizable filter for logging
            LogLevel minimumLevel;
            if (environment.IsProduction())
            {
                minimumLevel = LogLevel.Warning;
            }
            else
            {
                minimumLevel = LogLevel.Information;
            }


            // add exclusions to remove noise in the logs
            var excludedLoggers = new List<string>
            {
                "Microsoft.AspNetCore.StaticFiles.StaticFileMiddleware",
                "Microsoft.AspNetCore.Hosting.Internal.WebHost",
            };

            Func<string, LogLevel, bool> logFilter = (string loggerName, LogLevel logLevel) =>
            {
                if (logLevel < minimumLevel)
                {
                    return false;
                }

                if (excludedLoggers.Contains(loggerName))
                {
                    return false;
                }

                return true;
            };

            loggerFactory.AddDbLogger(serviceProvider, logFilter, logRepo);
        }

    }
}
