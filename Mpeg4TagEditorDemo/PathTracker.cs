using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IsoBaseMediaFileFormatParser;

namespace Mpeg4TagEditorDemo
{
    public class PathTracker
    {
        public PathTracker(string[] path)
        {
            Path = path.Select(t => Conversions.GetType(t)).ToArray();
        }

        public PathTracker(uint[] path)
        {
        }

        public uint[] Path
        {
            get;
            private set;
        }

        private Stack<uint> currentPath;
        public Stack<uint> CurrentPath
        {
            get
            {
                return currentPath;
            }
            set
            {
                currentPath = value;

                EqualsPath = PathTracker.IsEqual(CurrentPath, Path);
                if (EqualsPath)
                    EqualsChanged(this, null);

                OnPath = StartsWithPath(CurrentPath, Path);

                bool nowWasOnPath = WasOnPath || OnPath;
                if (!WasOnPath & nowWasOnPath)
                {
                    OffPath = true;
                    OffPathChanged(this, null);
                }

                WasOnPath = nowWasOnPath;
            }
        }

        public bool OnPath
        {
            get;
            private set;
        }


        public bool WasOnPath
        {
            get;
            private set;

        }

        public bool EqualsPath
        {
            get;
            private set;
        }

        public bool OffPath
        {
            get;
            private set;
        }

        public delegate void OffPathEventHandler(object sender, EventArgs e);

        public event OffPathEventHandler OffPathChanged;
        public event OffPathEventHandler EqualsChanged;

        private static bool IsEqual(Stack<uint> boxes, uint[] pathToMatch)
        {
            return boxes.Reverse().SequenceEqual(pathToMatch);
        }

        private static bool StartsWithPath(Stack<uint> boxes, uint[] pathToMatch)
        {
            return boxes.Reverse().Take(pathToMatch.Length).SequenceEqual(pathToMatch);
        }
    }
}
