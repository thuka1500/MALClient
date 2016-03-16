﻿using System;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Navigation;
using MALClient.UserControls;
using MALClient.ViewModels;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace MALClient.Pages
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private bool _initialized;

        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            ToggleCache.IsOn = Settings.IsCachingEnabled();
            ComboCachePersistency.SelectedIndex = SecondsToIndexHelper(Settings.GetCachePersitence());
            SetSortOrder();
            BtnDescending.IsChecked = Settings.IsSortDescending();
            PopulateCachedEntries();
            SetDesiredStatus();
            SliderSetup();
            Utils.GetMainPageInstance().CurrentStatus = "Settings - v1.4";
            _initialized = true;

            NavMgr.RegisterBackNav(PageIndex.PageAnimeList, null);

            base.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            NavMgr.DeregisterBackNav();
        }


        private void SetSortOrder()
        {
            switch (Settings.GetSortOrder())
            {
                case SortOptions.SortNothing:
                    Sort4.IsChecked = true;
                    break;
                case SortOptions.SortTitle:
                    Sort1.IsChecked = true;
                    break;
                case SortOptions.SortScore:
                    Sort2.IsChecked = true;
                    break;
                case SortOptions.SortWatched:
                    Sort3.IsChecked = true;
                    break;
                case SortOptions.SortAirDay:
                    Sort5.IsChecked = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }


        private async void PopulateCachedEntries()
        {
            var files = await ApplicationData.Current.LocalFolder.GetFilesAsync();
            foreach (var file in files)
            {
                if (file.FileType == ".json")
                {
                    ListCurrentlyCached.Items.Add(new CachedEntryItem(file, file.DisplayName.Contains("anime")));
                }
            }
            if (files.Count == 0)
                ListEmptyNotice.Visibility = Visibility.Visible;
            else
                ListEmptyNotice.Visibility = Visibility.Collapsed;
            try
            {
                var folder = await ApplicationData.Current.LocalFolder.GetFolderAsync("AnimeDetails");
                var data = await folder.GetFilesAsync();
                TxtRemoveAllDetails.Text += " (" + data.Count + " files)";
            }
            catch (Exception)
            {
                //No folder
                BtnRemoveAllDetails.Visibility = Visibility.Collapsed;                
            }
            
        }


        /// <summary>
        ///     Converts seconds to combo box item index.
        /// </summary>
        /// <returns></returns>
        private int SecondsToIndexHelper(int secs)
        {
            switch (secs)
            {
                case 600: //10m
                    return 0;
                case 3600: //1h
                    return 1;
                case 7200: //2h
                    return 2;
                case 10800: //3h
                    return 3;
                case 18000: //5h
                    return 4;
                case 36000: //10h
                    return 5;
                case 86400: //1d
                    return 6;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private int IndexToSecondsHelper(int index)
        {
            switch (index)
            {
                case 0: //10m
                    return 600;
                case 1: //1h
                    return 3600;
                case 2: //2h
                    return 7200;
                case 3: //3h
                    return 10800;
                case 4: //5h
                    return 18000;
                case 5: //10h
                    return 36000;
                case 6: //1d
                    return 86400;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private void ChangeCachePersistency(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized)
                return;
            ;
            var cmb = sender as ComboBox;
            ApplicationData.Current.LocalSettings.Values["CachePersistency"] = IndexToSecondsHelper(cmb.SelectedIndex);
        }

        private void ToggleDataCaching(object sender, RoutedEventArgs e)
        {
            if (!_initialized)
                return;
            ApplicationData.Current.LocalSettings.Values["EnableCache"] = ToggleCache.IsOn;
        }

        private void SelectSortOrder(object sender, RoutedEventArgs e)
        {
            var btn = sender as ToggleMenuFlyoutItem;
            Sort1.IsChecked = false;
            Sort2.IsChecked = false;
            Sort3.IsChecked = false;
            Sort4.IsChecked = false;
            Sort5.IsChecked = false;
            btn.IsChecked = true;
            SortOptions sortOptions;
            switch (btn.Text)
            {
                case "Title":
                    sortOptions = SortOptions.SortTitle;
                    break;
                case "MyScore":
                    sortOptions = SortOptions.SortScore;
                    break;
                case "Watched":
                    sortOptions = SortOptions.SortWatched;
                    break;
                case "Soonest airing":
                    sortOptions = SortOptions.SortAirDay;
                    break;
                default:
                    sortOptions = SortOptions.SortNothing;
                    break;
            }
            ApplicationData.Current.LocalSettings.Values["SortOrder"] = (int) sortOptions;
        }

        private void ChangeSortOrder(object sender, RoutedEventArgs e)
        {
            var btn = sender as ToggleMenuFlyoutItem;
            ApplicationData.Current.LocalSettings.Values["SortDescending"] = btn.IsChecked;
        }

        private void ChangeDefaultFilter(object sender, SelectionChangedEventArgs e)
        {
            if (!_initialized)
                return;
            ApplicationData.Current.LocalSettings.Values["DefaultFilter"] =
                Utils.StatusToInt((string) ((sender as ComboBox).SelectedItem as ComboBoxItem).Content);
        }

        private void SetDesiredStatus()
        {
            var value = Settings.GetDefaultAnimeFilter();
            value = value == 6 || value == 7 ? value - 1 : value;
            value--;
            CmbDefaultFilter.SelectedIndex = value;
        }

        private void ChangeItemsPerPage(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_initialized || Math.Abs(e.NewValue - e.OldValue) < 1)
                return;
            ApplicationData.Current.LocalSettings.Values["ItemsPerPage"] = (int) e.NewValue;
            ViewModelLocator.AnimeList.UpdatePageSetup(true);
        }

        private void ChangedReviewsToPull(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_initialized || Math.Abs(e.NewValue - e.OldValue) < 1)
                return;
            ApplicationData.Current.LocalSettings.Values["ReviewsToPull"] = (int) e.NewValue;
        }

        private void ChangedRecommsToPull(object sender, RangeBaseValueChangedEventArgs e)
        {
            if (!_initialized || Math.Abs(e.NewValue - e.OldValue) < 1)
                return;
            ApplicationData.Current.LocalSettings.Values["RecommsToPull"] = (int) e.NewValue;
        }

        private void SliderSetup()
        {
            SliderItemsPerPage.Value = Settings.GetItemsPerPage();
            SliderReccommsToPull.Value = Settings.GetRecommsToPull();
            SliderReviewsToPull.Value = Settings.GetReviewsToPull();
        }

        private async void RemoveAllAnimeDetails(object sender, RoutedEventArgs e)
        {
            try
            {
                await (await ApplicationData.Current.LocalFolder.GetFolderAsync("AnimeDetails")).DeleteAsync(
                    StorageDeleteOption.PermanentDelete);

                (sender as Button).IsEnabled = false;
            }
            catch (Exception)
            {

            }
        }
    }
}