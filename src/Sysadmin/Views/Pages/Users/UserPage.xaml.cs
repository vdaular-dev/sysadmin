﻿using System;
using System.IO;
using System.Windows;
using Wpf.Ui;

namespace Sysadmin.Views.Pages
{
    /// <summary>
    /// Interaction logic for DataView.xaml
    /// </summary>
    public partial class UserPage : Wpf.Ui.Controls.INavigableView<ViewModels.UserViewModel>
    {

        private ISnackbarService snackbarService;

        public ViewModels.UserViewModel ViewModel
        {
            get;
        }

        public UserPage(ViewModels.UserViewModel viewModel, ISnackbarService snackbarService)
        {
            ViewModel = viewModel;
            DataContext = this;

            this.snackbarService = snackbarService;

            InitializeComponent();

            ViewModel.PropertyChanged += ViewModel_PropertyChanged;
        }

        private void ViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "User")
            {
                memberOf.MemberOf = ViewModel.User.MemberOf;
                memberOf.PrimaryGroupId = ViewModel.User.PrimaryGroupId;

                if(string.IsNullOrEmpty(ViewModel.User.DisplayName))
                    personPicture.DisplayName = ViewModel.User.CN;
                else
                    personPicture.DisplayName = ViewModel.User.DisplayName;

                personPicture.JpegPhoto = ViewModel.User.JpegPhoto;
            }
        }

        private void DeleteMenuItem_Click(object sender, RoutedEventArgs e)
        {
            var result = System.Windows.MessageBox.Show("Are you sure you want to delete this user?", "Delete", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.Yes)
                ViewModel.DeleteCommand.Execute(ViewModel);
        }

        private async void PhotoMenuItem_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog();
                openFileDialog.Multiselect = false;
                openFileDialog.Filter = "Image files (*.jpg;*.jpeg)|*.jpg;*.jpeg|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog() == true)
                {
                    await ViewModel.UpdatePhoto(ViewModel.User.DistinguishedName, File.ReadAllBytes(openFileDialog.FileName));
                    await ViewModel.Get();
                    personPicture.JpegPhoto = ViewModel.User.JpegPhoto;
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Error");
            }
        }

        private async void MemberOfControl_Changed() //NOSONAR
        {
            await ViewModel.Get();
        }

        private void MemberOfControl_Error(string ErrorMessage)
        {
            snackbarService.Show("Error",
                ErrorMessage,
                Wpf.Ui.Controls.ControlAppearance.Danger,
                new Wpf.Ui.Controls.SymbolIcon(Wpf.Ui.Controls.SymbolRegular.ErrorCircle12),
                TimeSpan.FromSeconds(5)
            );
        }

        private async void btnDeletePhoto_Click(object sender, RoutedEventArgs e)
        {
            await ViewModel.DeletePhoto(ViewModel.User.DistinguishedName);
            await ViewModel.Get();
            personPicture.JpegPhoto = null;
        }
    }
}