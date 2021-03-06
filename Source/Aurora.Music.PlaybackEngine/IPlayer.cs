﻿using Aurora.Music.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Storage;

namespace Aurora.Music.PlaybackEngine
{
    public interface IPlayer
    {
        void Play();
        void Pause();
        void Stop();
        void Seek(TimeSpan position);
        void Next();
        void Previous();
        Task NewPlayList(IList<Song> songs, int startIndex = 0);
        Task NewPlayList(IList<StorageFile> list);
        Task AddtoNextPlay(IList<Song> song);
        void ChangeVolume(double vol);
        void ChangeAudioEndPoint(string outputDeviceID);
        void Loop(bool? isOn);
        void Shuffle(bool? isOn);

        bool? IsPlaying { get; }

        event EventHandler<PositionUpdatedArgs> PositionUpdated;
        event EventHandler<PlayingItemsChangedArgs> StatusChanged;
        event EventHandler<DownloadProgressChangedArgs> DownloadProgressChanged;

        void SkiptoIndex(uint index);
        Task ReloadCurrent();
        void RemoveCurrentItem();
        Task DetachCurrentItem();
        Task ReAttachCurrentItem();
        Task UpdateComingItems(List<Song> list);
    }

    public class DownloadProgressChangedArgs
    {
        /// <summary>
        /// 0.0 to 1.0
        /// </summary>
        public double Progress { get; set; }
    }

    public class PlayingItemsChangedArgs
    {
        public Song CurrentSong { get; set; }
        public bool IsLoop { get; set; }
        public bool IsShuffle { get; set; }

        public int CurrentIndex { get; set; }
        public IReadOnlyList<Song> Items { get; internal set; }
    }

    public class PlaybackStatusChangedArgs
    {
        public MediaPlaybackState PlaybackStatus { get; set; }
    }

    public class PositionUpdatedArgs
    {
        public TimeSpan Current { get; set; }
        public TimeSpan Total { get; set; }
    }
}
