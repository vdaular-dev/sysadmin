﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sysadmin.Services;
using SysAdmin.ActiveDirectory;
using SysAdmin.ActiveDirectory.Models;
using SysAdmin.ActiveDirectory.Repositories;
using SysAdmin.ActiveDirectory.Services.Ldap;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Wpf.Ui;
using Wpf.Ui.Controls;

namespace Sysadmin.ViewModels
{
    public partial class ComputersViewModel : ViewModel
    {
        private bool isInitialized = false;

        private INavigationService navigationService;
        private IExchangeService exchangeService;

        [ObservableProperty]
        private IEnumerable<ComputerEntry> _computers = new List<ComputerEntry>();

        private List<ComputerEntry> cache = new List<ComputerEntry>();

        [ObservableProperty]
        private bool _isBusy;

        public enum Filters
        {
            All,
            AccountEnabled,
            AccountDisabled
        }

        private Filters filters = Filters.All;
        private string searchText = string.Empty;
        private bool isAsc = true;

        public ComputersViewModel(INavigationService navigationService, IExchangeService exchangeService)
        {
            this.navigationService = navigationService;
            this.exchangeService = exchangeService;
        }

        public override async void OnNavigatedTo()
        {
            if (!isInitialized)
                InitializeViewModel();

            await ListAsync();

            SortingAndFiltering();
        }

        private void InitializeViewModel()
        {
            isInitialized = true;
        }

        [RelayCommand]
        private void OnAdd()
        {
            navigationService.Navigate(typeof(Views.Pages.AddComputerPage));
        }

        [RelayCommand]
        private void OnSelectedItemsChanged(IEnumerable<object> items)
        {
            if (items != null && items.Any())
            {
                exchangeService.SetParameter((ComputerEntry)items.First());
                navigationService.Navigate(typeof(Views.Pages.ComputerPage));
            }
        }

        public async Task ListAsync()
        {

            IsBusy = true;

            await Task.Run(async () =>
            {
                using (var ldap = new LdapService(App.SERVER, App.CREDENTIAL))
                {
                    using (var computersRepository = new ComputersRepository(ldap))
                    {
                        cache = await computersRepository.ListAsync();
                        if (cache == null)
                            cache = new List<ComputerEntry>();
                        Computers = cache.OrderBy(c => c.CN);
                    }
                }
            });

            IsBusy = false;
        }

        [RelayCommand]
        private void OnSearch(string text)
        {
            searchText = text;

            SortingAndFiltering();
        }

        [RelayCommand]
        private void OnSort(MenuItem menu)
        {
            switch (menu.Tag)
            {
                case "asc":
                    isAsc = true;
                    break;
                case "desc":
                    isAsc = false;
                    break;
            }

            SortingAndFiltering();
        }

        [RelayCommand]
        private void OnFilter(MenuItem menu)
        {
            switch (menu.Tag)
            {
                case "all":
                    filters = Filters.All;
                    break;
                case "enabled":
                    filters = Filters.AccountEnabled;
                    break;
                case "disabled":
                    filters = Filters.AccountDisabled;
                    break;
            }

            SortingAndFiltering();
        }

        private void SortingAndFiltering()
        {
            if (cache != null)
            {
                if (string.IsNullOrEmpty(searchText))
                {
                    Computers = cache;
                }
                else
                {
                    Computers = cache.Where(c => c.CN.ToUpper().StartsWith(searchText.ToUpper()));
                }

                switch (filters)
                {
                    case Filters.AccountEnabled:
                        Computers = Computers.Where(c => (c.UserControl & UserAccountControls.ACCOUNTDISABLE) != UserAccountControls.ACCOUNTDISABLE);
                        break;

                    case Filters.AccountDisabled:
                        Computers = Computers.Where(c => (c.UserControl & UserAccountControls.ACCOUNTDISABLE) == UserAccountControls.ACCOUNTDISABLE);
                        break;
                }

                if (isAsc)
                    Computers = Computers.OrderBy(c => c.CN);
                else
                    Computers = Computers.OrderByDescending(c => c.CN);
            }
        }

    }
}