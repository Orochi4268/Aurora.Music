﻿// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using Aurora.Music.Controls;
using Aurora.Music.Core;
using Aurora.Music.Core.Models;
using Aurora.Music.Core.Storage;
using Aurora.Music.Pages;
using Aurora.Music.PlaybackEngine;
using Aurora.Shared.Extensions;
using Aurora.Shared.Helpers;
using Aurora.Shared.MVVM;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppExtensions;
using Windows.ApplicationModel.Core;
using Windows.Devices.Enumeration;
using Windows.Foundation.Collections;
using Windows.Media.Devices;
using Windows.Services.Store;
using Windows.Storage;
using Windows.System;
using Windows.System.Threading;

namespace Aurora.Music.ViewModels
{
    class SettingsPageViewModel : ViewModelBase
    {
        private int audioSelectedIndex = -1;
        public int AudioSelectedIndex
        {
            get { return audioSelectedIndex; }
            set
            {
                if (value == -1)
                    return;
                SetProperty(ref audioSelectedIndex, value);

                Settings.Current.OutputDeviceID = DevicList[value].ID;
                Settings.Current.Save();
                Player.Current.ChangeAudioEndPoint(Settings.Current.OutputDeviceID);
            }
        }

        public DelegateCommand NavigateToPrivacy
        {
            get => new DelegateCommand(async () =>
            {
                await Launcher.LaunchUriAsync(new Uri("http://198.181.41.120/privacypolicy.htm"));
            });
        }

        public DelegateCommand ShowUpdateInfo
        {
            get => new DelegateCommand(async () =>
            {
                UpdateInfo u = new UpdateInfo();
                await u.ShowAsync();
            });
        }

        public DelegateCommand ShowEaseAccess
        {
            get => new DelegateCommand(async () =>
            {
                EaseAccess u = new EaseAccess();
                await u.ShowAsync();
            });
        }

        public DelegateCommand CommentInStore
        {
            get => new DelegateCommand(async () =>
            {
                await Launcher.LaunchUriAsync(new Uri($"ms-windows-store://review/?ProductId={Consts.ProductID}"));
            });
        }

        public DelegateCommand Github
        {
            get => new DelegateCommand(async () =>
            {
                await Launcher.LaunchUriAsync(new Uri(Consts.Github));
            });
        }

        public DelegateCommand ReportABug
        {
            get => new DelegateCommand(async () =>
            {
                var launcher = Microsoft.Services.Store.Engagement.StoreServicesFeedbackLauncher.GetDefault();
                await launcher.LaunchAsync();
            });
        }

        public DelegateCommand GetExtensions
        {
            get => new DelegateCommand(async () =>
            {
                await Launcher.LaunchUriAsync(new Uri($"ms-windows-store://search/?query={Consts.ExtensionContract}"));
            });
        }

        public DelegateCommand About
        {
            get => new DelegateCommand(() =>
            {
                MainPage.Current.Navigate(typeof(AboutPage));
            });
        }

        public DelegateCommand OpenExtensionManager
        {
            get => new DelegateCommand(async () =>
            {
                var mgr = new ExtensionsManager();
                await mgr.ShowAsync();
            });
        }

        public DelegateCommand DownloadPath
        {
            get => new DelegateCommand(async () =>
            {
                var folderPicker = new Windows.Storage.Pickers.FolderPicker
                {
                    SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.ComputerFolder
                };
                folderPicker.FileTypeFilter.Add(".mp3");
                folderPicker.FileTypeFilter.Add(".m4a");
                folderPicker.FileTypeFilter.Add(".wav");
                folderPicker.FileTypeFilter.Add(".flac");

                StorageFolder folder = await folderPicker.PickSingleFolderAsync();
                if (folder != null)
                {
                    Settings.Current.DownloadPathToken = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList.Add(folder);
                    Settings.Current.Save();
                    DownloadPathText = folder.Path;
                }
            });
        }

        private bool metaDataEnabled = Settings.Current.MetaDataEnabled;
        public bool MetaDataEnabled
        {
            get { return metaDataEnabled; }
            set
            {
                Settings.Current.MetaDataEnabled = value;
                Settings.Current.Save();
                SetProperty(ref metaDataEnabled, value);
            }
        }
        private bool dataPlayEnabled = Settings.Current.DataPlayEnabled;
        public bool DataPlayEnabled
        {
            get { return dataPlayEnabled; }
            set
            {
                Settings.Current.DataPlayEnabled = value;
                Settings.Current.Save();
                SetProperty(ref dataPlayEnabled, value);
            }
        }
        private bool dataDownloadEnabled = Settings.Current.DataDownloadEnabled;
        public bool DataDownloadEnabled
        {
            get { return dataDownloadEnabled; }
            set
            {
                Settings.Current.DataDownloadEnabled = value;
                Settings.Current.Save();
                SetProperty(ref dataDownloadEnabled, value);
            }
        }

        private string downloadPathText;
        public string DownloadPathText
        {
            get { return downloadPathText; }
            set { SetProperty(ref downloadPathText, value); }
        }

        private bool canClearCache = true;
        public bool CanClearCache
        {
            get { return canClearCache; }
            set { SetProperty(ref canClearCache, value); }
        }

        public DelegateCommand ClearCache
        {
            get => new DelegateCommand(async () =>
            {
                CanClearCache = false;
                var folder = ApplicationData.Current.TemporaryFolder;
                var f = await folder.GetItemsAsync();
                foreach (var item in f)
                {
                    await item.DeleteAsync(StorageDeleteOption.PermanentDelete);
                }
                CanClearCache = true;
                MainPage.Current.PopMessage(Consts.Localizer.GetString("ClearCacheText"));
            });
        }

        public DelegateCommand DeleteAll
        {
            get => new DelegateCommand(async () =>
            {
                var opr = SQLOperator.Current();
                opr.Dispose();
                opr = null;
                GC.Collect();
                GC.WaitForPendingFinalizers();

                await Task.Delay(300);

                await ApplicationData.Current.ClearAsync();
                Settings.Current.DANGER_DELETE();
                await Task.Delay(300);
                App.Current.Exit();
            });
        }

        private bool onlinePurchase = Settings.Current.OnlinePurchase;
        public bool OnlinePurchase
        {
            get { return onlinePurchase; }
            set { SetProperty(ref onlinePurchase, value); }
        }

        private double playerVolume=Settings.Current.PlayerVolume;
        public double PlayerVolume
        {
            get { return playerVolume; }
            set
            {
                if (!value.AlmostEqualTo(playerVolume))
                {
                    Player.Current.ChangeVolume(value);
                    Settings.Current.PlayerVolume = value;
                    Settings.Current.Save();
                }

                SetProperty(ref playerVolume, value);
            }
        }

        private int crrentLyricIndex = -1;
        public int CurrentLyricIndex
        {
            get { return crrentLyricIndex; }
            set
            {
                SetProperty(ref crrentLyricIndex, value);
            }
        }

        private int currentOnlineIndex = -1;
        public int CurrentOnlineIndex
        {
            get { return currentOnlineIndex; }
            set
            {
                SetProperty(ref currentOnlineIndex, value);
            }
        }

        private int currentMetaIndex = -1;
        public int CurrentMetaIndex
        {
            get { return currentMetaIndex; }
            set
            {
                SetProperty(ref currentMetaIndex, value);
            }
        }

        private bool debugModeEnabled = Settings.Current.DebugModeEnabled;
        public bool DebugModeEnabled
        {
            get { return debugModeEnabled; }
            set
            {
                Settings.Current.DebugModeEnabled = value;
                Settings.Current.Save();
                SetProperty(ref debugModeEnabled, value);
            }
        }

        internal void ToggleEffectState(string tag)
        {
            switch (tag)
            {
                case "Threshold":
                    Settings.Current.AudioGraphEffects ^= Effects.Limiter;
                    break;
                case "Equalizer":
                    Settings.Current.AudioGraphEffects ^= Effects.Equalizer;
                    break;
                case "Reverb":
                    Settings.Current.AudioGraphEffects ^= Effects.Reverb;
                    break;
                default:
                    break;
            }

            Settings.Current.Save();

            EqualizerEnabled = Settings.Current.AudioGraphEffects.HasFlag(Effects.Equalizer);
            ThresholdEnabled = Settings.Current.AudioGraphEffects.HasFlag(Effects.Limiter);
            ReverbEnabled = Settings.Current.AudioGraphEffects.HasFlag(Effects.Reverb);
        }

        internal void ChangeLyricExt(object selectedItem)
        {
            if (selectedItem is ExtensionViewModel v)
            {
                Settings.Current.LyricExtensionID = v.UniqueId;
                Settings.Current.Save();
            }
            else
            {
                Settings.Current.LyricExtensionID = string.Empty;
                Settings.Current.Save();
            }
        }

        internal void ChangeOnlineExt(object selectedItem)
        {
            if (selectedItem is ExtensionViewModel v)
            {
                Settings.Current.OnlineMusicExtensionID = v.UniqueId;
                Settings.Current.Save();
            }
            else
            {
                Settings.Current.LyricExtensionID = string.Empty;
                Settings.Current.Save();
            }
        }

        internal void ChangeMetaExt(object selectedItem)
        {
            if (selectedItem is ExtensionViewModel v)
            {
                Settings.Current.MetaExtensionID = v.UniqueId;
                Settings.Current.Save();
            }
            else
            {
                Settings.Current.LyricExtensionID = string.Empty;
                Settings.Current.Save();
            }
        }

        internal async Task PurchaseOnlineExtension()
        {
            if (context == null)
            {
                context = StoreContext.GetDefault();
            }

            MainPage.Current.ShowModalUI(true, Consts.Localizer.GetString("WaitingResultText"));
            StorePurchaseResult result = await context.RequestPurchaseAsync(Consts.OnlineAddOnStoreID);


            // Capture the error message for the operation, if any.
            string extendedError = string.Empty;
            if (result.ExtendedError != null)
            {
                extendedError = result.ExtendedError.Message;
            }

            switch (result.Status)
            {
                case StorePurchaseStatus.AlreadyPurchased:
                case StorePurchaseStatus.Succeeded:
                    OnlinePurchase = true;
                    Settings.Current.OnlinePurchase = true;
                    Settings.Current.Save();
                    break;

                case StorePurchaseStatus.NotPurchased:
                    OnlinePurchase = false;
                    Settings.Current.OnlinePurchase = false;
                    Settings.Current.Save();
                    break;

                case StorePurchaseStatus.NetworkError:
                case StorePurchaseStatus.ServerError:
                default:
                    OnlinePurchase = false;
                    Settings.Current.OnlinePurchase = false;
                    Settings.Current.Save();
                    MainPage.Current.PopMessage("Purchase Error:\r\n" + extendedError);
                    break;
            }
            MainPage.Current.ShowModalUI(false);
            await MainPageViewModel.Current.ReloadExtensions();
        }

        public ObservableCollection<ExtensionViewModel> LyricExts { get; set; } = new ObservableCollection<ExtensionViewModel>();
        public ObservableCollection<ExtensionViewModel> OnlineExts { get; set; } = new ObservableCollection<ExtensionViewModel>();
        public ObservableCollection<ExtensionViewModel> MetaExts { get; set; } = new ObservableCollection<ExtensionViewModel>();

        private bool equalizerEnabled = Settings.Current.AudioGraphEffects.HasFlag(Effects.Equalizer);
        public bool EqualizerEnabled
        {
            get { return equalizerEnabled; }
            set { SetProperty(ref equalizerEnabled, value); }
        }

        private bool threshold = Settings.Current.AudioGraphEffects.HasFlag(Effects.Limiter);
        public bool ThresholdEnabled
        {
            get { return threshold; }
            set { SetProperty(ref threshold, value); }
        }

        private bool reverb = Settings.Current.AudioGraphEffects.HasFlag(Effects.Reverb);
        public bool ReverbEnabled
        {
            get { return reverb; }
            set { SetProperty(ref reverb, value); }
        }

        public SettingsPageViewModel()
        {
        }

        public ObservableCollection<DeviceInformationViewModel> DevicList = new ObservableCollection<DeviceInformationViewModel>();
        private StoreContext context;
        private AppExtensionCatalog _catalog;
        private StorageFolder downloadFolder;

        public async Task FindAllExtensions()
        {
            // load all the extensions currently installed
            IReadOnlyList<AppExtension> extensions = await _catalog.FindAllAsync();
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                foreach (AppExtension ext in extensions)
                {
                    // load this extension
                    await LoadExtension(ext);
                }
                LyricExts.ToList().ForEach(async (x) => { await x.Load(); });
                OnlineExts.ToList().ForEach(async (x) => { await x.Load(); });
                MetaExts.ToList().ForEach(async (x) => { await x.Load(); });
                try
                {
                    var f = LyricExts.First(x => x.UniqueId == (Settings.Current.LyricExtensionID.IsNullorEmpty() ? Consts.AppUserModelId + "$|$BuiltIn" : Settings.Current.LyricExtensionID));
                    if (f != null)
                    {
                        CurrentLyricIndex = LyricExts.IndexOf(f);
                    }
                    else
                    {
                        CurrentLyricIndex = -1;
                    }
                }
                catch (Exception)
                {
                    CurrentLyricIndex = -1;
                }
                try
                {
                    var f = OnlineExts.First(x => x.UniqueId == (Settings.Current.OnlineMusicExtensionID.IsNullorEmpty() ? Consts.AppUserModelId + "$|$BuiltIn" : Settings.Current.OnlineMusicExtensionID));
                    if (f != null)
                    {
                        CurrentOnlineIndex = OnlineExts.IndexOf(f);
                    }
                    else
                    {
                        CurrentOnlineIndex = -1;
                    }
                }
                catch (Exception)
                {
                    CurrentOnlineIndex = -1;
                }
                try
                {
                    var f = MetaExts.First(x => x.UniqueId == (Settings.Current.MetaExtensionID.IsNullorEmpty() ? Consts.AppUserModelId + "$|$BuiltIn" : Settings.Current.MetaExtensionID));
                    if (f != null)
                    {
                        CurrentMetaIndex = MetaExts.IndexOf(f);
                    }
                    else
                    {
                        CurrentOnlineIndex = -1;
                    }
                }
                catch (Exception)
                {
                    CurrentOnlineIndex = -1;
                }
            });
        }

        // loads an extension
        public async Task LoadExtension(AppExtension ext)
        {
            // get unique identifier for this extension
            string identifier = ext.AppInfo.AppUserModelId + Consts.ArraySeparator + ext.Id;

            // load the extension if the package is OK
            if (!(ext.Package.Status.VerifyIsOK()
#if !DEBUG
                    && Settings.Current.DebugModeEnabled ? true : ext.Package.SignatureKind == PackageSignatureKind.Store
#endif
                    ))
            {
                // if this package doesn't meet our requirements
                // ignore it and return
                return;
            }
            var properties = await ext.GetExtensionPropertiesAsync() as PropertySet;
            var cates = ((properties["Category"] as PropertySet)["#text"] as string).Split(';');
            foreach (var category in cates)
            {
                if (category == "Lyric")
                {
                    // if its already existing then this is an update
                    var existingLyricExt = LyricExts.Where(e => e.UniqueId == identifier).FirstOrDefault();
                    // new extension
                    if (existingLyricExt == null)
                    {
                        // get extension properties
                        LyricExts.Add(new ExtensionViewModel(ext, properties));
                    }
                    // update
                    else
                    {
                        // update the extension
                        await existingLyricExt.Update(ext);
                    }
                }

                if (category == "OnlineMusic")
                {
                    // if its already existing then this is an update
                    var existingOnlineExt = OnlineExts.Where(e => e.UniqueId == identifier).FirstOrDefault();
                    // new extension
                    if (existingOnlineExt == null)
                    {
                        // get extension properties
                        OnlineExts.Add(new ExtensionViewModel(ext, properties));
                    }
                    // update
                    else
                    {
                        // update the extension
                        await existingOnlineExt.Update(ext);
                    }
                }

                if (category == "OnlineMeta")
                {
                    // if its already existing then this is an update
                    var extingMeta = MetaExts.Where(e => e.UniqueId == identifier).FirstOrDefault();
                    // new extension
                    if (extingMeta == null)
                    {
                        // get extension properties
                        MetaExts.Add(new ExtensionViewModel(ext, properties));
                    }
                    // update
                    else
                    {
                        // update the extension
                        await extingMeta.Update(ext);
                    }
                }
            }

        }

        public async Task Init()
        {
            var t = ThreadPool.RunAsync(async x =>
            {
                if (!OnlinePurchase)
                {
                    if (context == null)
                    {
                        context = StoreContext.GetDefault();
                    }

                    // Specify the kinds of add-ons to retrieve.
                    string[] productKinds = { "Durable" };
                    List<String> filterList = new List<string>(productKinds);

                    // Specify the Store IDs of the products to retrieve.
                    string[] storeIds = new string[] { Consts.OnlineAddOnStoreID };

                    StoreProductQueryResult queryResult =
                        await context.GetStoreProductsAsync(filterList, storeIds);

                    if (queryResult.ExtendedError != null)
                    {
                        // The user may be offline or there might be some other server failure.
                        MainPage.Current.PopMessage($"ExtendedError: {queryResult.ExtendedError.Message}");
                        return;
                    }

                    foreach (KeyValuePair<string, StoreProduct> item in queryResult.Products)
                    {
                        // Access the Store info for the product.
                        StoreProduct product = item.Value;
                        await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                        {
                            OnlinePurchase = product.IsInUserCollection;
                        });
                        Settings.Current.OnlinePurchase = product.IsInUserCollection;
                        Settings.Current.Save();
                        await MainPageViewModel.Current.ReloadExtensions();
                    }
                }
            });

            _catalog = AppExtensionCatalog.Open(Consts.ExtensionContract);
            // set up extension management events
            _catalog.PackageInstalled += _catalog_PackageInstalled;
            _catalog.PackageUpdated += _catalog_PackageUpdated;
            _catalog.PackageUninstalling += _catalog_PackageUninstalling;
            _catalog.PackageUpdating += _catalog_PackageUpdating;
            _catalog.PackageStatusChanged += _catalog_PackageStatusChanged;
            // Scan all extensions
            await FindAllExtensions();


            try
            {
                if (Settings.Current.DownloadPathToken.IsNullorEmpty())
                {
                    var lib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
                    downloadFolder = await lib.SaveFolder.CreateFolderAsync("Download", CreationCollisionOption.OpenIfExists);
                }
                else
                {
                    downloadFolder = await Windows.Storage.AccessCache.StorageApplicationPermissions.
                FutureAccessList.GetFolderAsync(Settings.Current.DownloadPathToken);
                }
            }
            catch (Exception)
            {
                var lib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Music);
                downloadFolder = await lib.SaveFolder.CreateFolderAsync("Download", CreationCollisionOption.OpenIfExists);
            }

            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                DownloadPathText = downloadFolder.Path;

                while (DevicList.Count < 1)
                {
                    string audioSelector = MediaDevice.GetAudioRenderSelector();
                    var outputDevices = await DeviceInformation.FindAllAsync(audioSelector);
                    foreach (var device in outputDevices)
                    {
                        //var deviceItem = new ComboBoxItem();
                        //deviceItem.Content = device.Name;
                        //deviceItem.Tag = device;
                        //_audioDeviceComboBox.Items.Add(deviceItem);
                        DevicList.Add(new DeviceInformationViewModel()
                        {
                            Name = device.Name,
                            ID = device.Id,
                            Tag = device
                        });
                    }
                }

                DevicList.Insert(0, new DeviceInformationViewModel()
                {
                    Name = Consts.Localizer.GetString("SystemDefault"),
                    ID = null,
                    Tag = null
                });

                if (Settings.Current.OutputDeviceID.IsNullorEmpty())
                {
                    await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
                    {
                        // wating for listview updated, weird
                        await Task.Delay(500);
                        AudioSelectedIndex = 0;
                    });
                }
                else
                {
                    int index = -1;
                    for (int i = 0; i < DevicList.Count; i++)
                    {
                        if (DevicList[i].ID == Settings.Current.OutputDeviceID)
                        {
                            index = i;
                            break;
                        }
                    }
                    await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
                    {
                        // wating for listview updated, weird
                        await Task.Delay(500);
                        AudioSelectedIndex = index;
                    });
                }

            });


        }

        private async void _catalog_PackageStatusChanged(AppExtensionCatalog sender, AppExtensionPackageStatusChangedEventArgs args)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                LyricExts.Clear();
                MetaExts.Clear();
                OnlineExts.Clear();
            });
            await FindAllExtensions();
        }

        private async void _catalog_PackageUpdating(AppExtensionCatalog sender, AppExtensionPackageUpdatingEventArgs args)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                LyricExts.Clear();
                MetaExts.Clear();
                OnlineExts.Clear();
            });
            await FindAllExtensions();
        }

        private async void _catalog_PackageUninstalling(AppExtensionCatalog sender, AppExtensionPackageUninstallingEventArgs args)
        {
            await Task.Delay(1000);
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                LyricExts.Clear();
                MetaExts.Clear();
                OnlineExts.Clear();
            });
            await FindAllExtensions();
        }

        private async void _catalog_PackageUpdated(AppExtensionCatalog sender, AppExtensionPackageUpdatedEventArgs args)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                LyricExts.Clear();
                MetaExts.Clear();
                OnlineExts.Clear();
            });
            await FindAllExtensions();
        }

        private async void _catalog_PackageInstalled(AppExtensionCatalog sender, AppExtensionPackageInstalledEventArgs args)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                LyricExts.Clear();
                MetaExts.Clear();
                OnlineExts.Clear();
            });
            await FindAllExtensions();
        }
    }

    class DeviceInformationViewModel : ViewModelBase
    {
        public string Name { get; set; }
        public string ID { get; set; }
        public DeviceInformation Tag { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}
