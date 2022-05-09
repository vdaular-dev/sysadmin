﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Navigation;
using SysAdmin.ActiveDirectory.Models;
using SysAdmin.Services.Dialogs;
using SysAdmin.ViewModels;
using System;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace SysAdmin.Views.Computers
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ComputersPage : Page
    {

        public ComputersViewModel ViewModel { get; } = new ComputersViewModel();

        public ComputersPage()
        {
            this.InitializeComponent();
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            await ViewModel.ListAsync();
        }

        private void computers_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is ComputerEntry)
            {
                Frame.Navigate(typeof(ComputerDetailsPage), e.AddedItems[0]);
            }
        }

        private void mnuSort_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            ToggleMenuFlyoutItem menu = (ToggleMenuFlyoutItem)sender;
            foreach (ToggleMenuFlyoutItem item in mnuSort.Items)
            {
                if (item != menu)
                    item.IsChecked = false;
            }
        }

        private void AutoSuggestBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            ViewModel.SearchCommand.Execute(sender.Text);
        }

        private void mnuFilter_Click(object sender, RoutedEventArgs e)
        {
            ToggleMenuFlyoutItem menu = (ToggleMenuFlyoutItem)sender;
            foreach (ToggleMenuFlyoutItem item in mnuFilter.Items)
            {
                if (item != menu)
                    item.IsChecked = false;
            }

            if (mnuFilterAll.IsChecked)
                ViewModel.FilterCommand.Execute(UsersViewModel.Filters.All);

            if (mnuFilterEnabled.IsChecked)
                ViewModel.FilterCommand.Execute(UsersViewModel.Filters.AccountEnabled);

            if (mnuFilterDisabled.IsChecked)
                ViewModel.FilterCommand.Execute(UsersViewModel.Filters.AccountDisabled);

        }

    }
}
