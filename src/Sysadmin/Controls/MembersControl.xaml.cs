﻿using SysAdmin.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using SysAdmin.ActiveDirectory.Repositories;
using SysAdmin.ActiveDirectory.Services.Ldap;
using System;
using System.ComponentModel;

namespace Sysadmin.Controls
{
    /// <summary>
    /// Interaction logic for MembersControl.xaml
    /// </summary>
    public partial class MembersControl : UserControl
    {

        public delegate void MembersHandler();
        public event MembersHandler Changed;

        public ObservableCollection<MemberItem> Items { get; private set; } = new ObservableCollection<MemberItem>();

        private MemberItem selected;
        public MemberItem Selected
        {
            get { return selected; }
            set
            {
                selected = value;
                if (value != null)
                    deleteButton.IsEnabled = true;
                else
                    deleteButton.IsEnabled = false;
            }
        }

        public static readonly DependencyProperty MembersProperty =
        DependencyProperty.Register(
            name: "Members",
            propertyType: typeof(List<string>),
            ownerType: typeof(MembersControl));

        public List<string> Members
        {
            get => (List<string>)GetValue(MembersProperty);
            set
            {
                SetValue(MembersProperty, value);
                Update(value);
            }
        }

        public static readonly DependencyProperty CNProperty =
        DependencyProperty.Register(
            name: "CN",
            propertyType: typeof(string),
            ownerType: typeof(MembersControl),
            typeMetadata: new FrameworkPropertyMetadata(defaultValue: string.Empty));

        public string CN
        {
            get => (string)GetValue(CNProperty);
            set => SetValue(CNProperty, value);
        }

        public MembersControl()
        {
            InitializeComponent();

            DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(MembersProperty, typeof(MembersControl));

            if (dpd != null)
            {
                dpd.AddValueChanged(this, OnChanged);
            }
        }

        private void OnChanged(object? sender, EventArgs e)
        {
            Update(Members);
        }

        private void Update(List<string> value)
        {
            Items.Clear();
            if (value != null)
                foreach (string item in value)
                    Items.Add(new MemberItem() { Name = SysAdmin.ActiveDirectory.ADHelper.ExtractCN(item), DistinguishedName = item });
        }

        private void addButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void deleteButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private async Task DeleteMember(string groupCN, string distinguishedName)
        {
            await Task.Run(async () =>
            {
                using (var ldap = new LdapService(App.SERVER, App.CREDENTIAL))
                {
                    using (var groupsRepository = new GroupsRepository(ldap))
                    {
                        var group = await groupsRepository.GetByCNAsync(groupCN);

                        if (group != null)
                        {
                            await groupsRepository.DeleteMemberAsync(group, distinguishedName);
                        }

                    }
                }
            });
        }

    }
}
