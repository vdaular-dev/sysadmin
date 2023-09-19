﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sysadmin.Services;
using Sysadmin.WMI;
using Sysadmin.WMI.Models;
using Sysadmin.WMI.Services;
using SysAdmin.ActiveDirectory.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Wpf.Ui.Common.Interfaces;
using Wpf.Ui.Mvvm.Contracts;

namespace Sysadmin.ViewModels
{
    public partial class PerformanceViewModel : ObservableObject, INavigationAware
    {
        private bool _isInitialized = false;

        private INavigationService _navigationService;
        private IExchangeService _exchangeService;

        [ObservableProperty]
        private ComputerEntry _computer = new ComputerEntry();

        [ObservableProperty]
        private IEnumerable<EventEntity> _items = new List<EventEntity>();

        [ObservableProperty]
        private string _errorMessage = string.Empty;

        [ObservableProperty]
        private bool _isBusy = false;

        public PerformanceViewModel(INavigationService navigationService, IExchangeService exchangeService)
        {
            _navigationService = navigationService;
            _exchangeService = exchangeService;
        }

        public async void OnNavigatedTo()
        {
            if (!_isInitialized)
                InitializeViewModel();

            if (_exchangeService.GetParameter() is ComputerEntry entry)
            {
                Computer = entry;
                await Get(Computer.DnsHostName);
            }
        }

        public void OnNavigatedFrom()
        {

        }

        private void InitializeViewModel()
        {
            _isInitialized = true;
        }

        [RelayCommand]
        private void OnClose()
        {
            _navigationService.Navigate(typeof(Views.Pages.ComputerPage));
        }

        public async Task Get(string computerAddress)
        {
            IsBusy = true;

            //ICredential? credential = null;

            //if (App.CREDENTIAL != null)
            //    credential = new Credential() { UserName = App.CREDENTIAL.UserName, Password = App.CREDENTIAL.Password };

            //List<EventEntity> entities = new List<EventEntity>();

            //try
            //{
            //    await Task.Run(() =>
            //    {
            //        using (var wmi = new WMIService(computerAddress, credential))
            //        {
            //            string today = string.Format("{0:yyyyMMddHHmmss}.000000000", DateTime.Today);

            //            string queryString = "Select RecordNumber, EventType, EventCode, Type, TimeGenerated, SourceName, Category, Logfile, Message From Win32_NTLogEvent Where TimeGenerated > '" + today + "'"; ;

            //            switch (filter)
            //            {
            //                case EventsFilter.TodayErrors:
            //                    queryString += " And EventType = 1";
            //                    break;

            //                case EventsFilter.TodayWarnings:
            //                    queryString += " And EventType = 2";
            //                    break;

            //                case EventsFilter.TodayInformations:
            //                    queryString += " And EventType = 3";
            //                    break;

            //                case EventsFilter.TodaySecurityAuditSuccess:
            //                    queryString += " And EventType = 4";
            //                    break;

            //                case EventsFilter.TodaySecurityAuditFailure:
            //                    queryString += " And EventType = 5";
            //                    break;
            //            }

            //            List<Dictionary<string, object>> queryResult = wmi.Query(queryString);

            //            foreach (Dictionary<string, object> properties in queryResult)
            //            {
            //                EventEntity entity = WmiResolver<EventEntity>.GetValues(properties);
            //                entities.Add(entity);
            //            }
            //        }
            //    });
            //}
            //catch (Exception ex)
            //{
            //    ErrorMessage = ex.Message;
            //}

            IsBusy = false;
        }

    }
}