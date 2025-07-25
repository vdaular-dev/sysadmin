﻿using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Sysadmin.Services;
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
    public partial class GroupsViewModel : ViewModel
    {
        private bool isInitialized = false;

        private INavigationService navigationService;
        private IExchangeService exchangeService;

        [ObservableProperty]
        private IEnumerable<GroupEntry> _groups = new List<GroupEntry>();

        private List<GroupEntry> cache = new List<GroupEntry>();

        [ObservableProperty]
        private bool _isBusy;

        public enum Filters
        {
            All,
            GlobalDistribution,
            DomainLocalDistribution,
            UniversalDistribution,
            GlobalSecurity,
            DomainLocalSecurity,
            UniversalSecurity,
            BuiltIn
        }

        private Filters filters = Filters.All;
        private string searchText = string.Empty;
        private bool isAsc = true;

        public GroupsViewModel(INavigationService navigationService, IExchangeService exchangeService)
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
            navigationService.Navigate(typeof(Views.Pages.AddGroupPage));
        }

        [RelayCommand]
        private void OnSelectedItemsChanged(IEnumerable<object> items)
        {
            if (items.Any())
            {
                exchangeService.SetParameter((GroupEntry)items.First());
                navigationService.Navigate(typeof(Views.Pages.GroupPage));
            }
        }

        public async Task ListAsync()
        {

            IsBusy = true;

            await Task.Run(async () =>
            {
                using (var ldap = new LdapService(App.SERVER, App.CREDENTIAL))
                {
                    using (var groupsRepository = new GroupsRepository(ldap))
                    {
                        cache = await groupsRepository.ListAsync();
                        if (cache == null)
                            cache = new List<GroupEntry>();
                        Groups = cache.OrderBy(c => c.CN);
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
                case "global_security":
                    filters = Filters.GlobalSecurity;
                    break;
                case "global_distribution":
                    filters = Filters.GlobalDistribution;
                    break;
                case "domain_security":
                    filters = Filters.DomainLocalSecurity;
                    break;
                case "domain_distribution":
                    filters = Filters.DomainLocalDistribution;
                    break;
                case "universal_security":
                    filters = Filters.UniversalSecurity;
                    break;
                case "universal_distribution":
                    filters = Filters.UniversalDistribution;
                    break;
                case "builtin":
                    filters = Filters.BuiltIn;
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
                    Groups = cache;
                }
                else
                {
                    Groups = cache.Where(c => c.CN.ToUpper().StartsWith(searchText.ToUpper()));
                }

                switch (filters)
                {
                    case Filters.BuiltIn:
                        Groups = Groups.Where(c => c.GroupType == -2147483643);
                        break;

                    case Filters.DomainLocalDistribution:
                        Groups = Groups.Where(c => c.GroupType == 4);
                        break;

                    case Filters.DomainLocalSecurity:
                        Groups = Groups.Where(c => c.GroupType == -2147483644);
                        break;

                    case Filters.GlobalDistribution:
                        Groups = Groups.Where(c => c.GroupType == 2);
                        break;

                    case Filters.GlobalSecurity:
                        Groups = Groups.Where(c => c.GroupType == -2147483646);
                        break;

                    case Filters.UniversalDistribution:
                        Groups = Groups.Where(c => c.GroupType == 8);
                        break;

                    case Filters.UniversalSecurity:
                        Groups = Groups.Where(c => c.GroupType == -2147483640);
                        break;
                }

                if (isAsc)
                    Groups = Groups.OrderBy(c => c.CN);
                else
                    Groups = Groups.OrderByDescending(c => c.CN);
            }
        }

    }
}