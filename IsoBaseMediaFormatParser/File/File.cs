using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace IsoBaseMediaFileFormat.File
{
    public class File : IEnumerator<Box>
    {
        private Box current;
        private long nextBoxPosition = 0;

        Stream input;

        public File(Stream input)
        {
            this.input = input;
        }

        public Box Current
        {
            get { return current; }
        }

        public void Dispose()
        {
        }

        object System.Collections.IEnumerator.Current
        {
            get { return current; }
        }

        public bool MoveNext()
        {
            Box box = null;
            while (MoveNext(out box))
                if (box != null)
                {
                    current = box;
                    return true;
                }

            return false;
        }

        protected bool MoveNext(out Box box)
        {
            if (nextBoxPosition == input.Length)
            {
                box = null;
                return false;
            }

            if (input.Position < nextBoxPosition)
                input.Position = nextBoxPosition;

            long startPosition = nextBoxPosition;

            box = null; //Box.Create(input);
            //nextBoxPosition = box.Size.HasValue ? startPosition + (long)box.Size.Value : input.Length;

            return true;
        }

        public void Reset()
        {
            nextBoxPosition = input.Position = 0;
        }
    }
}
