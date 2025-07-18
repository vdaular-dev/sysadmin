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
    public partial class ContactsViewModel : ViewModel
    {
        private bool isInitialized = false;

        private INavigationService navigationService;
        private IExchangeService exchangeService;

        [ObservableProperty]
        private IEnumerable<ContactEntry> _contacts = new List<ContactEntry>();

        private List<ContactEntry> cache = new List<ContactEntry>();

        [ObservableProperty]
        private bool _isBusy;

        private string searchText = string.Empty;
        private bool isAsc = true;

        public ContactsViewModel(INavigationService navigationService, IExchangeService exchangeService)
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
            navigationService.Navigate(typeof(Views.Pages.AddContactPage));
        }

        [RelayCommand]
        private void OnSelectedItemsChanged(IEnumerable<object> items)
        {
            if (items.Any())
            {
                exchangeService.SetParameter((ContactEntry)items.First());
                navigationService.Navigate(typeof(Views.Pages.ContactPage));
            }
        }

        public async Task ListAsync()
        {

            IsBusy = true;

            await Task.Run(async () =>
            {
                using (var ldap = new LdapService(App.SERVER, App.CREDENTIAL))
                {
                    using (var contactsRepository = new ContactsRepository(ldap))
                    {
                        cache = await contactsRepository.ListAsync();
                        if (cache == null)
                            cache = new List<ContactEntry>();
                        Contacts = cache.OrderBy(c => c.CN);
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

        private void SortingAndFiltering()
        {
            if (cache != null)
            {
                if (string.IsNullOrEmpty(searchText))
                {
                    Contacts = cache;
                }
                else
                {
                    Contacts = cache.Where(c => c.CN.ToUpper().StartsWith(searchText.ToUpper()));
                }

                if (isAsc)
                    Contacts = Contacts.OrderBy(c => c.CN);
                else
                    Contacts = Contacts.OrderByDescending(c => c.CN);
            }
        }

    }
}