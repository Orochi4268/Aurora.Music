﻿// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Music.Core.Models
{
    public class OnlineMusicItem : GenericMusicItem
    {
        public string[] OnlineID { get; private set; }
        public string OnlineAlbumId { get; }

        public OnlineMusicItem(string title, string description, string addtional, string[] id, string albumId)
        {
            Title = title;
            Description = description;
            Addtional = addtional;
            OnlineID = id;
            IsOnline = true;
            OnlineAlbumId = albumId;
        }
    }
}
