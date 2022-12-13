﻿
using NASA_APIs.WPF.Infrastructure;
using NASA_APIs.WPF.Services;
using NASA_APIs.WPF.Stores;
using NASA_APIs.WPF.ViewModels.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace NASA_APIs.WPF.ViewModels
{
    public class MenuViewModel : BaseVM
    {
        public ICommand MarsControlCommand { get; }
        public ICommand ApodControlCommand { get; }
        public MenuViewModel(NavigationService navigationService)
        {
            MarsControlCommand = new NavigateCommand(navigationService);
            ApodControlCommand = new NavigateCommand(navigationService);
        }
    }
}