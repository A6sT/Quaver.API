/*
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * Copyright (c) 2017-2019 Swan & The Quaver Team <support@quavergame.com>.
*/

using System;
using System.Collections.Generic;
using System.Drawing;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Interop;
using Quaver.API.Helpers;

namespace Quaver.API.Maps.Structures
{
    [MoonSharpUserData]
    [Serializable]
    public class BookmarkInfo : IStartTime
    {
        public const string DefaultColorRgb = "255,255,0";

        public int StartTime
        {
            get;
            [MoonSharpVisible(false)]
            set;
        }

        public string Note
        {
            get;
            [MoonSharpVisible(false)]
            set;
        }

        public string ColorRgb
        {
            get;
            [MoonSharpVisible(false)]
            set;
        } = DefaultColorRgb;

        [MoonSharpVisible(false)]
        public Color GetColor() =>
            new Drain<char>(ColorRgb, ',') is var (tr, (tg, (tb, _))) &&
            byte.TryParse(tr, out var r) &&
            byte.TryParse(tg, out var g) &&
            byte.TryParse(tb, out var b)
                ? Color.FromArgb(r, g, b)
                : Color.Yellow;

        float IStartTime.StartTime
        {
            get => StartTime;
            set => StartTime = (int)value;
        }

        private sealed class TimeNoteEqualityComparer : IEqualityComparer<BookmarkInfo>
        {
            public bool Equals(BookmarkInfo x, BookmarkInfo y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;

                return x.StartTime == y.StartTime && x.Note == y.Note && x.ColorRgb == y.ColorRgb;
            }

            public int GetHashCode(BookmarkInfo obj) => HashCode.Combine(obj.StartTime, obj.Note, obj.ColorRgb);
        }

        public static IEqualityComparer<BookmarkInfo> ByValueComparer { get; } = new TimeNoteEqualityComparer();
    }
}
