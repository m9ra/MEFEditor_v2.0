using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommendedExtensions.Core.Languages.CSharp
{
    class Strip
    {
        internal Strip Next { get; set; }
        internal string Data { get; private set; }
        internal readonly int StartingOffset;
        internal readonly bool IsOriginal;

        internal int EndingOffset { get { return StartingOffset + Data.Length; } }

        /// <summary>
        /// Create strip from original data, originaly placed at given startingOffset
        /// </summary>
        /// <param name="data"></param>
        /// <param name="startingOffset"></param>
        internal Strip(string data, int startingOffset)
        {
            Data = data;
            IsOriginal = true;
            StartingOffset = startingOffset;
        }

        /// <summary>
        /// Create non-original data strip
        /// </summary>
        /// <param name="data"></param>
        internal Strip(string data)
        {
            Data = data;
            IsOriginal = false;
            StartingOffset = int.MinValue;
        }

        internal Strip(Strip toClone)
        {
            Data = toClone.Data;
            StartingOffset = toClone.StartingOffset;
            IsOriginal = toClone.IsOriginal;

            if (toClone.Next != null)
                Next = new Strip(toClone.Next);
        }

        internal void Split(int offset)
        {
            if (!Contains(offset))
                throw new NotSupportedException("Cannot split according to not contained offset");

            if (offset == EndingOffset)
                //there is no need to split strip
                return;

            var splitLine = offset - StartingOffset;
            var part1 = Data.Substring(0, splitLine);
            var part2 = Data.Substring(splitLine);

            Data = part1;

            var strip = new Strip(part2, offset);
            strip.Next = this.Next;
            this.Next = strip;
        }

        internal bool Contains(int offset)
        {
            if (!IsOriginal)
                return false;

            return StartingOffset <= offset && offset <= EndingOffset;
        }

        public override string ToString()
        {
            return string.Format("'{0}' off: {1}", Data, StartingOffset);
        }
    }

    public class StripManager
    {
        readonly Strip _root;

        public StripManager(string data)
        {
            _root = new Strip(data, 0);
        }

        public StripManager(StripManager toClone)
        {
            _root = new Strip(toClone._root);
        }

        public void Write(int offset, string data)
        {
            var strip = getStrip(offset);
            var nStrip = new Strip(data);

            insert(nStrip, strip, offset);
        }

        public void Remove(int offset, int length)
        {
            if (length <= 0)
            {
                //nothing to remove
                return;
            }

            var strip = getStrip(offset);

            var endOffset = offset + length;
            if (strip == null)
            {
                var removedStrip = getStrip(endOffset);
                if (removedStrip != null)
                {
                    throw new NotSupportedException("Cannot remove partial strip");
                }

                //has already been removed - double removing is not a problem
                return;
            }

            strip.Split(offset);

            var endStrip = getStrip(endOffset, strip);
            endStrip.Split(endOffset);

            var current = strip;
            while (current != endStrip)
            {
                if (!current.IsOriginal)
                {
                    //we dont want to remove inserted strips
                    strip.Next = current;
                    strip = current;
                }
                current = current.Next;
            }
            strip.Next = endStrip.Next;
        }

        public void Move(int start, int len, int target)
        {
            var strip = getStrip(start);
            strip.Split(start);

            var endOffset = start + len;
            var movedStripEnd = getStrip(endOffset, strip);

            movedStripEnd.Split(endOffset);

            var targetStrip = getStrip(target);
            targetStrip.Split(target);

            //--reconnect strips--
            var movedStripStart = strip.Next;

            //Remove moved strip from original position
            strip.Next = movedStripEnd.Next;

            //Connect moved strip at target position
            movedStripEnd.Next = targetStrip.Next;
            targetStrip.Next = movedStripEnd;
        }

        /// <summary>
        /// Get string representation of stripped data
        /// </summary>
        public string Data
        {
            get
            {
                var builder = new StringBuilder();
                var strip = _root;

                while (strip != null)
                {
                    builder.Append(strip.Data);
                    strip = strip.Next;
                }

                return builder.ToString();
            }
        }

        private void insert(Strip inserted, Strip target, int offset)
        {
            target.Split(offset);

            inserted.Next = target.Next;
            target.Next = inserted;
        }

        /// <summary>
        /// Get strip containing given offset
        /// </summary>
        /// <param name="offset">Searched offset</param>
        /// <returns>Strip containing given offset</returns>
        private Strip getStrip(int offset, Strip startStrip = null)
        {
            if (startStrip == null)
            {
                startStrip = _root;
            }

            while (startStrip != null)
            {
                if (startStrip.Contains(offset))
                    return startStrip;

                startStrip = startStrip.Next;
            }

            return null;
        }
    }
}
