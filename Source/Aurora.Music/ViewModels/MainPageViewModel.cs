﻿// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Aurora.Music.Core.Storage;
using Aurora.Shared.MVVM;
using Windows.Storage;
using Aurora.Music.Pages;
using Windows.UI.Text;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Media;
using System.Collections.ObjectModel;
using Windows.System.Threading;
using Aurora.Shared.Extensions;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Controls;
using Aurora.Music.Core.Models;
using Aurora.Music.PlaybackEngine;
using Aurora.Music.Core;
using Windows.Foundation.Collections;
using Aurora.Music.Controls;
using Aurora.Shared.Helpers;
using Newtonsoft.Json;
using System.Diagnostics;

namespace Aurora.Music.ViewModels
{
    class MainPageViewModel : ViewModelBase, IDisposable
    {
        public static MainPageViewModel Current;

        public List<HamPanelItem> HamList { get; set; } = new List<HamPanelItem>()
        {
            new HamPanelItem
            {
                Title = Consts.Localizer.GetString("HomeText"),
                TargetType = typeof(HomePage),
                Icon="\uE80F",
                IsCurrent = true,
                BG = new Uri("ms-appx:///Assets/Images/albums.png")
            },
            new HamPanelItem
            {
                Title = Consts.Localizer.GetString("LibraryText"),
                Icon="\uE2AC",
                TargetType = typeof(LibraryPage),
                BG = new Uri("ms-appx:///Assets/Images/songs.png")
            },
            new HamPanelItem
            {
                Title = Consts.Localizer.GetString("DouText"),
                Icon = "\uE2AC",
                TargetType = typeof(DoubanPage),
                BG = new Uri("ms-appx:///Assets/Images/radio.png")
            }
            //new HamPanelItem
            //{
            //    Title = "Playlist",
            //    Icon="\uE955",
            //    TargetType = typeof(HomePage),
            //    BG = new Uri("ms-appx:///Assets/Images/songs.png")
            //},
        };

        public ObservableCollection<SongViewModel> NowPlayingList { get; set; } = new ObservableCollection<SongViewModel>();
        private IPlayer player;

        private SolidColorBrush _lastLeftTop;
        private SolidColorBrush leftTopColor;
        public SolidColorBrush LeftTopColor
        {
            get { return leftTopColor; }
            set
            {
                _lastLeftTop = leftTopColor;
                SetProperty(ref leftTopColor, value);
                ApplicationViewTitleBar titleBar = ApplicationView.GetForCurrentView().TitleBar;
                titleBar.ButtonForegroundColor = leftTopColor.Color;
                titleBar.ForegroundColor = leftTopColor.Color;
            }
        }

        public Extension LyricExtension { get; private set; }
        public Extension OnlineMusicExtension { get; private set; }
        public Extension OnlineMetaExtension { get; private set; }

        private bool needShowPanel = false;
        public bool NeedShowPanel
        {
            get { return needShowPanel; }
            set { SetProperty(ref needShowPanel, value); }
        }

        private Uri currentArtwork;
        public Uri CurrentArtwork
        {
            get { return currentArtwork; }
            set { SetProperty(ref currentArtwork, value); }
        }

        private double nowPlayingPosition;
        public double NowPlayingPosition
        {
            get { return nowPlayingPosition; }
            set { SetProperty(ref nowPlayingPosition, value); }
        }

        private bool? isPlaying;
        public bool? IsPlaying
        {
            get { return isPlaying; }
            set { SetProperty(ref isPlaying, value); }
        }

        internal void RestoreLastTitle()
        {
            NeedShowTitle = _lastneedshow;
            Title = _lasttitle;
            LeftTopColor = _lastLeftTop;
        }

        private string placeholderText = Consts.Localizer.GetString("SearchInLibraryText");
        public string PlaceholderText
        {
            get { return placeholderText; }
            set { SetProperty(ref placeholderText, value); }
        }

        private TimeSpan currentPosition;
        public TimeSpan CurrentPosition
        {
            get { return currentPosition; }
            set { SetProperty(ref currentPosition, value); }
        }

        private TimeSpan totalDuration;
        public TimeSpan TotalDuration
        {
            get { return totalDuration; }
            set { SetProperty(ref totalDuration, value); }
        }

        private string currentTitle;
        public string CurrentTitle
        {
            get { return currentTitle.IsNullorEmpty() ? Consts.Localizer.GetString("AppNameText") : currentTitle; }
            set { SetProperty(ref currentTitle, value); }
        }

        private string currentAlbum;
        private string lastUriPath;

        public string CurrentAlbum
        {
            get { return currentAlbum.IsNullorEmpty() ? Consts.Localizer.GetString("NotPlayingText") : currentAlbum; }
            set { SetProperty(ref currentAlbum, value); }
        }

        private string nowListPreview = "-/-";

        public string NowListPreview
        {
            get { return nowListPreview; }
            set { SetProperty(ref nowListPreview, value); }
        }

        private int currentIndex = -1;
        public int CurrentIndex
        {
            get { return currentIndex; }
            set { SetProperty(ref currentIndex, value); }
        }

        private bool _lastneedshow;
        private bool needHeader;
        public bool NeedShowTitle
        {
            get { return needHeader; }
            set
            {
                _lastneedshow = needHeader;
                SetProperty(ref needHeader, value);
            }
        }

        private string _lasttitle;
        private string title;
        public string Title
        {
            get { return title; }
            set
            {
                _lasttitle = title;
                SetProperty(ref title, value);
            }
        }

        internal async Task<Song> GetOnlineSongAsync(string id)
        {
            var querys = new ValueSet()
            {
                new KeyValuePair<string,object>("q", "online_music"),
                new KeyValuePair<string, object>("action", "song"),
                new KeyValuePair<string, object>("id", id),
                new KeyValuePair<string, object>("bit_rate", Settings.Current.GetPreferredBitRate())
            };
            var songResult = await OnlineMusicExtension.ExecuteAsync(querys);
            if (songResult is Song s)
            {
                return s;
            }
            return null;
        }

        internal async Task<Album> GetOnlineAlbumAsync(string id)
        {
            var querys = new ValueSet()
            {
                new KeyValuePair<string,object>("q", "online_music"),
                new KeyValuePair<string, object>("action", "album"),
                new KeyValuePair<string, object>("id", id),
                new KeyValuePair<string, object>("bit_rate", Settings.Current.GetPreferredBitRate())
            };
            var songResult = await OnlineMusicExtension.ExecuteAsync(querys);
            if (songResult is Album s)
            {
                return s;
            }
            return null;
        }

        internal async Task<AlbumInfo> GetAlbumInfoAsync(string album, string artist)
        {
            var querys = new ValueSet()
            {
                new KeyValuePair<string,object>("album", album),
                new KeyValuePair<string, object>("artist", artist),
                new KeyValuePair<string, object>("action", "album"),
                new KeyValuePair<string, object>("q", "online_meta"),
            };
            var result = await OnlineMetaExtension.ExecuteAsync(querys);
            if (result is AlbumInfo s)
            {
                return s;
            }
            return null;
        }

        internal async Task<Core.Models.Artist> GetArtistInfoAsync(string artist)
        {
            var querys = new ValueSet()
            {
                new KeyValuePair<string, object>("action", "artist"),
                new KeyValuePair<string, object>("artist", artist),
                new KeyValuePair<string, object>("q", "online_meta"),
            };
            var result = await OnlineMetaExtension.ExecuteAsync(querys);
            if (result is Core.Models.Artist s)
            {
                return s;
            }
            return null;
        }

        public DelegateCommand GoPrevious
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    player?.Previous();
                });
            }
        }

        public DelegateCommand GoNext
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    player?.Next();
                });
            }
        }

        public DelegateCommand Stop
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    player?.Stop();
                });
            }
        }

        public DelegateCommand PlayPause
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    if (IsPlaying is bool b)
                    {
                        if (b)
                        {
                            player?.Pause();
                        }
                        else
                        {
                            player?.Play();
                        }
                    }
                    else
                    {
                        player?.Play();
                    }
                });
            }
        }

        public DelegateCommand GotoSettings
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    MainPage.Current.GoBackFromNowPlaying();
                    MainPage.Current.Navigate(typeof(SettingsPage));
                });
            }
        }

        public DelegateCommand GotoAbout
        {
            get
            {
                return new DelegateCommand(() =>
                {
                    MainPage.Current.GoBackFromNowPlaying();
                    MainPage.Current.Navigate(typeof(AboutPage));
                });
            }
        }

        private bool? isShuffle = false;
        public bool? IsShuffle
        {
            get { return isShuffle; }
            set
            {
                SetProperty(ref isShuffle, value);

                player?.Shuffle(value);
            }
        }

        private bool? isLoop = false;
        public bool? IsLoop
        {
            get { return isLoop; }
            set
            {
                SetProperty(ref isLoop, value);

                player?.Loop(value);
            }
        }

        private bool darkAccent;
        public bool IsDarkAccent
        {
            get { return darkAccent; }
            set { SetProperty(ref darkAccent, value); }
        }

        public MainPageViewModel()
        {
            player = Player.Current;
            Current = this;
            player.DownloadProgressChanged += Player_DownloadProgressChanged;
            player.StatusChanged += Player_StatusChanged;
            player.PositionUpdated += Player_PositionUpdated;
            var t = ThreadPool.RunAsync(async x =>
            {
                if (Settings.Current.LastUpdateBuild < SystemInfoHelper.GetPackageVersionNum())
                {
                    var k = CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
                    {
                        UpdateInfo i = new UpdateInfo();
                        await i.ShowAsync();
                        Settings.Current.LastUpdateBuild = SystemInfoHelper.GetPackageVersionNum();
                        Settings.Current.Save();
                    });
                }
                await ReloadExtensions();
                await FindFileChanges();
            });
        }

        public async Task ReloadExtensions()
        {
            LyricExtension = await Extension.Load<LyricExtension>(Settings.Current.LyricExtensionID);
            if (Settings.Current.OnlinePurchase)
            {
                OnlineMusicExtension = await Extension.Load<OnlineMusicExtension>(Settings.Current.OnlineMusicExtensionID);
            }
            else
            {
                OnlineMusicExtension = null;
            }

            OnlineMetaExtension = await Extension.Load<OnlineMetaExtension>(Settings.Current.MetaExtensionID);

            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                if (OnlineMusicExtension != null)
                {
                    PlaceholderText = Consts.Localizer.GetString("SearchInWebText");
                }
            });
        }

        private double downloadProgress;
        private string _lastQuery;

        public double BufferProgress
        {
            get { return downloadProgress; }
            set { SetProperty(ref downloadProgress, value); }
        }

        private async void Player_DownloadProgressChanged(object sender, DownloadProgressChangedArgs e)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                BufferProgress = 100 * e.Progress;
            });
        }

        public ObservableCollection<GenericMusicItemViewModel> SearchItems { get; set; } = new ObservableCollection<GenericMusicItemViewModel>();

        internal async Task Search(string text)
        {
            if (OnlineMusicExtension != null)
            {
                _lastQuery = string.Copy(text);
                var s = string.Copy(_lastQuery);

                var job = ThreadPool.RunAsync(async work =>
                {
                    var querys = new ValueSet()
                    {
                        new KeyValuePair<string,object>("q", "online_music"),
                        new KeyValuePair<string, object>("action", "search"),
                        new KeyValuePair<string, object>("keyword", text)
                    };
                    var dd = CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
                    {
                        await Task.Delay(200);
                        MainPage.Current.ShowAutoSuggestPopup();
                    });
                    var webResult = await OnlineMusicExtension.ExecuteAsync(querys);
                    if (webResult is IEnumerable<OnlineMusicItem> items)
                    {
                        if (MainPage.Current.CanAdd && s.Equals(_lastQuery, StringComparison.Ordinal))
                            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                            {
                                lock (MainPage.Current.Lockable)
                                {
                                    if (SearchItems.Count > 0 && SearchItems[0].InnerType == MediaType.Placeholder)
                                    {
                                        SearchItems.Clear();
                                    }
                                    foreach (var item in items.Reverse())
                                    {
                                        SearchItems.Insert(0, new GenericMusicItemViewModel(item));
                                    }
                                    MainPage.Current.HideAutoSuggestPopup();
                                }
                            });
                    }
                });
            }

            var result = await FileReader.Search(text);

            if (MainPage.Current.CanAdd && !result.IsNullorEmpty())
                await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
                {
                    lock (MainPage.Current.Lockable)
                    {
                        if (SearchItems.Count > 0 && SearchItems[0].InnerType == MediaType.Placeholder)
                        {
                            SearchItems.Clear();
                        }
                        foreach (var item in result)
                        {
                            SearchItems.Add(new GenericMusicItemViewModel(item));
                        }
                    }
                    MainPage.Current.HideAutoSuggestPopup();
                });
        }

        public async Task FindFileChanges()
        {
            var addedFiles = await FileTracker.FindChanges();
            if (!(addedFiles.Count == 0))
            {
                var reader = new FileReader();
                reader.ProgressUpdated += Reader_ProgressUpdated;
                reader.Completed += Reader_Completed;
                await reader.ReadFileandSave(addedFiles);
            }
        }

        private void Reader_Completed(object sender, EventArgs e)
        {
            var t = CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, async () =>
            {
                MainPage.Current.ProgressUpdate(100);
                MainPage.Current.ProgressUpdate(Consts.Localizer.GetString("FileUpdatingText"), Consts.Localizer.GetString("CompletedText"));
                await Task.Delay(1500);
                MainPage.Current.ProgressUpdate(false);
            });
        }

        private void Reader_ProgressUpdated(object sender, ProgressReport e)
        {
            var t = CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                MainPage.Current.ProgressUpdate();
                MainPage.Current.ProgressUpdate(e.Current * 100.0 / e.Total);
                MainPage.Current.ProgressUpdate(Consts.Localizer.GetString("FileUpdatingText"), e.Description);
            });
        }

        private async void Reader_NewSongsAdded(object sender, SongsAddedEventArgs e)
        {
            await new FileReader().AddToAlbums(e.NewSongs);
        }

        private async void Player_PositionUpdated(object sender, PositionUpdatedArgs e)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                CurrentPosition = e.Current;
                TotalDuration = e.Total;
            });
        }

        private async void Player_StatusChanged(object sender, PlayingItemsChangedArgs e)
        {
            await CoreApplication.MainView.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High, () =>
            {
                IsPlaying = player.IsPlaying;

                if (e.CurrentIndex == -1)
                {
                    NowPlayingList.Clear();
                    NowListPreview = "-/-";
                    CurrentTitle = null;
                    CurrentAlbum = null;
                    CurrentArtwork = null;
                    lastUriPath = null;
                    CurrentIndex = -1;
                    NeedShowPanel = false;
                    return;
                }

                if (e.CurrentSong != null)
                {
                    var p = e.CurrentSong;
                    CurrentTitle = p.Title.IsNullorEmpty() ? p.FilePath.Split('\\').LastOrDefault() : p.Title;
                    CurrentAlbum = p.Album.IsNullorEmpty() ? (p.Performers.IsNullorEmpty() ? Consts.UnknownAlbum : string.Join(Consts.CommaSeparator, p.Performers)) : p.Album;
                    if (!p.PicturePath.IsNullorEmpty())
                    {
                        if (lastUriPath == p.PicturePath)
                        {

                        }
                        else
                        {
                            CurrentArtwork = new Uri(p.PicturePath);
                            lastUriPath = p.PicturePath;
                        }
                    }
                    else
                    {
                        CurrentArtwork = null;
                    }
                    Task.Run(() =>
                    {
                        Tile.SendNormal(CurrentTitle, CurrentAlbum, string.Join(Consts.CommaSeparator, p.Performers ?? new string[] { }), p.PicturePath);
                    });
                }
                if (e.Items is IList<Song> l)
                {
                    NowListPreview = $"{e.CurrentIndex + 1}/{l.Count}";
                    NowPlayingList.Clear();
                    for (int i = 0; i < l.Count; i++)
                    {
                        NowPlayingList.Add(new SongViewModel(l[i])
                        {
                            Index = (uint)i,
                        });
                    }
                }
                if (e.CurrentIndex < NowPlayingList.Count)
                {
                    CurrentIndex = e.CurrentIndex;
                }
                if (MainPage.Current.IsCurrentDouban)
                {
                    return;
                }
                else
                {
                    NeedShowPanel = true;
                }
            });
        }

        internal async Task SavePointAsync()
        {
            if (!NowPlayingList.IsNullorEmpty())
            {
                var status = new PlayerStatus(NowPlayingList.Select(s => s.Song), CurrentIndex, CurrentPosition);
                await status.SaveAsync();
            }
        }

        public void Dispose()
        {
            //player.StatusChanged -= Player_StatusChanged;
            //player.PositionUpdated -= Player_PositionUpdated;
        }

        internal async Task InstantPlay(IList<Song> songs, int startIndex = 0)
        {
            await player.NewPlayList(songs, startIndex);
            player.Play();
        }

        internal async Task InstantPlay(List<StorageFile> list)
        {
            await player.NewPlayList(list);
            player.Play();
        }

        internal async Task PlayNext(IList<Song> songs)
        {
            await player.AddtoNextPlay(songs);
            player.Play();
        }

        public Symbol NullableBoolToSymbol(bool? b)
        {
            if (b is bool bb)
            {
                return bb ? Symbol.Pause : Symbol.Play;
            }
            return Symbol.Play;
        }

        public string NullableBoolToString(bool? b)
        {
            if (b is bool bb)
            {
                return bb ? Consts.Localizer.GetString("PauseText") : Consts.Localizer.GetString("PlayText");
            }
            return Consts.Localizer.GetString("PlayText");
        }

        internal void SkiptoItem(SongViewModel songViewModel)
        {
            player.SkiptoIndex(songViewModel.Index);
        }

        public double PositionToValue(TimeSpan t1, TimeSpan total)
        {
            if (total == null || total == default(TimeSpan))
            {
                return 0;
            }
            return 100 * (t1.TotalMilliseconds / total.TotalMilliseconds);
        }

        internal async Task<IList<Song>> ComingNewSongsAsync(List<StorageFile> list)
        {
            return await new FileReader().ReadFileandSendBack(list);
        }
    }


    class HamPanelItem : ViewModelBase
    {
        public string Title { get; set; }

        public Type TargetType { get; set; }

        public string Icon { get; set; }

        public Uri BG { get; set; }

        private bool isCurrent;
        public bool IsCurrent
        {
            get { return isCurrent; }
            set { SetProperty(ref isCurrent, value); }
        }

        public FontWeight ChangeWeight(bool b)
        {
            return b ? FontWeights.Bold : FontWeights.Normal;
        }

        public SolidColorBrush ChangeForeground(bool b)
        {
            return (SolidColorBrush)(b ? MainPage.Current.Resources["SystemControlBackgroundAccentBrush"] : MainPage.Current.Resources["SystemControlDisabledBaseLowBrush"]);
        }

        public SolidColorBrush ChangeTextForeground(bool b)
        {
            return (SolidColorBrush)(b ? MainPage.Current.Resources["SystemControlForegroundBaseHighBrush"] : MainPage.Current.Resources["SystemControlDisabledBaseLowBrush"]);
        }

        public double BoolToOpacity(bool b)
        {
            return b ? 1.0 : 0.1;
        }
    }
}
