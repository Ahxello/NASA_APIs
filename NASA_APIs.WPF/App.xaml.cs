﻿
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NASA_APIs.DAL.Entities;
using NASA_APIs.Interfaces.Base.Repositories;
using NASA_APIs.WebApiClients.Repositories;
using NASA_APIs.WPF.Services;
using NASA_APIs.WPF.Stores;
using NASA_APIs.WPF.ViewModels;
using NASA_APIs.WPF.ViewModels.Apod;
using NASA_APIs.WPF.Views.Windows;
using System;
using System.Linq;
using System.Windows;

namespace NASA_APIs.WPF
{
    public partial class App : Application
    {
        private static IHost _Hosting;

        public static IHost Hosting => _Hosting ??= CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

        public static IServiceProvider Services => Hosting.Services;

        public static IHostBuilder CreateHostBuilder(string[] args) => Host
           .CreateDefaultBuilder(args)
           .ConfigureServices(ConfigureServices);

        private static void ConfigureServices(HostBuilderContext host, IServiceCollection services)
        {
            services.AddScoped<MainWindowViewModel>();
            services.AddTransient<ApodMenuUserControlViewModel>();
            services.AddTransient<ApodSearchViewUserControlViewModel>();
            services.AddTransient<ApodSearchByCountChoiceUserControlViewModel>();
            services.AddHttpClient<IRepository<ApodValue>, WebRepository<ApodValue>>(client =>
            {
                client.BaseAddress = new Uri($"{host.Configuration["WebAPI"]}/api/ApodValue/");
            });
        }
        protected override async void OnStartup(StartupEventArgs e)
        {
            NavigationStore navigationStore = new NavigationStore();
            navigationStore.CurrentViewModel = new MenuViewModel(navigationStore);
            MainWindow = new MainWindow()
            {
                DataContext = new MainWindowViewModel(navigationStore)
            };
            MainWindow.Show();
            var host = Hosting;
            base.OnStartup(e);
            await host.StartAsync().ConfigureAwait(true);
        }
        protected override async void OnExit(ExitEventArgs e)
        {
            using var host = Hosting;
            base.OnExit(e);
            await host.StopAsync().ConfigureAwait(false);
        }
    }
}
