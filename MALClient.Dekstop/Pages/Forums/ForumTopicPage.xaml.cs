﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.System;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;
using MALClient.Shared.Managers;
using MALClient.Models.Enums;
using MALClient.XShared.Comm;
using MALClient.XShared.Comm.Anime;
using MALClient.XShared.Comm.MagicalRawQueries;
using MALClient.XShared.NavArgs;
using MALClient.XShared.Utils;
using MALClient.XShared.Utils.Enums;
using MALClient.XShared.ViewModels;
using MALClient.XShared.ViewModels.Forums;
using MALClient.XShared.ViewModels.Forums.Items;
using WinRTXamlToolkit.Controls.Extensions;
using WebViewExtensions = MALClient.Shared.UserControls.AttachedProperties.WebViewExtensions;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MALClient.Pages.Forums
{

    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ForumTopicPage : Page , ForumTopicViewModel.IScrollInfoProvider
    {
        public ForumTopicViewModel ViewModel => ViewModelLocator.ForumsTopic;


        public ForumTopicPage()
        {
            this.InitializeComponent();
            ViewModel.ScrollInfoProvider = this;
            ViewModel.RequestScroll += ViewModelOnRequestScroll;
        }

        public EventHandler<ForumTopicMessageEntryViewModel> HeightRegistar { get; set; }

        private void ViewModelOnRequestScroll(object sender, int i)
        {
            ListView.ScrollIntoView(ViewModel.Messages[i], ScrollIntoViewAlignment.Leading);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ViewModel.Init(e.Parameter as ForumsTopicNavigationArgs);
            base.OnNavigatedTo(e);
            
        }


        //private async void ClipboardOnContentChanged(object sender, object o)
        //{
        //    DataPackageView dataPackageView = Clipboard.GetContent();
        //    if (dataPackageView.Contains(StandardDataFormats.Text))
        //    {
        //        string text = await dataPackageView.GetTextAsync();
        //        ViewModelLocator.GeneralMain.Navigate(PageIndex.PageSearch, new SearchPageNavigationArgs
        //        {
        //            Anime = !ViewModel.IsMangaBoard,
        //            Query = text.Trim(),
        //            ForceQuery = true,
        //        });
        //    }
        //}       
          
        private void TopicWebView_OnNavigationStarting(WebView sender, WebViewNavigationStartingEventArgs args)
        {
            try
            {
                if (args.Uri != null)
                {
                    var uri = args.Uri.Host == string.Empty ? $"https://myanimelist.net" + args.Uri.AbsolutePath : args.ToString();
                    args.Cancel = true;
                    var navArgs = MalLinkParser.GetNavigationParametersForUrl(uri);
                    if (navArgs != null)
                    {
                        if(!navArgs.Item1.GetAttribute<EnumUtilities.PageIndexEnumMember>().OffPage)
                            ViewModel.RegisterSelfBackNav();

                        ViewModelLocator.GeneralMain.Navigate(navArgs.Item1, navArgs.Item2);
                    }
                    else if (Settings.ArticlesLaunchExternalLinks)
                    {
                        ResourceLocator.SystemControlsLauncherService.LaunchUri(args.Uri);
                    }
                }
            }
            catch (Exception)
            {
                args.Cancel = true;
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            ViewModel.NavigatedFrom();
            base.OnNavigatedFrom(e);
        }

        private void GotoInputOnKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter)
            {
                if (string.IsNullOrEmpty(GotoPageTextBox.Text))
                    return;
                int val;
                if (!int.TryParse(GotoPageTextBox.Text, out val))
                {
                    GotoPageFlyout.Hide();
                    return;
                }

                GotoPageFlyout.Hide();
                ViewModel.LoadPageCommand.Execute(val);
                e.Handled = true;
            }
        }

        private void GotoPageTextBox_OnQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            ViewModel.LoadGotoPageCommand.Execute(null);
        }


        public int GetFirstVisibleItemIndex()
        {
            return ListView.GetFirstVisibleIndex();
        }

        private void NewReplyButtonOnClick(object sender, RoutedEventArgs e)
        {
            ListView.ScrollToBottom();
        }

        private void ScrollToIndex()
        {
           // ListView.ScrollIntoView(ViewModel.Messages[_requestedIndex], ScrollIntoViewAlignment.Leading);
           // _requestedIndex = 0;
        }

        private void VirtualizingStackPanel_OnCleanUpVirtualizedItemEvent(object sender, CleanUpVirtualizedItemEventArgs e)
        {

        }
    }
}
