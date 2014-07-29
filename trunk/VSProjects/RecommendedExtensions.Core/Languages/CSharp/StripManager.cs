using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecommendedExtensions.Core.Languages.CSharp
{
    /// <summary>
    /// Strip of source code used by <see cref="StripManager"/>
    /// for providing source edits.
    /// </summary>
    class Strip
    {
        /// <summary>
        /// Gets or sets the next strip.
        /// </summary>
        /// <value>The next.</value>
        internal Strip Next;

        /// <summary>
        /// Gets the code represented by current strip.
        /// </summary>
        /// <value>The data.</value>
        internal string Data { get; private set; }

        /// <summary>
        /// The starting offset of strip.
        /// </summary>
        internal readonly int StartingOffset;

        /// <summary>
        /// Indicator that strip is on original position.
        /// </summary>
        internal readonly bool IsOriginal;

        /// <summary>
        /// Gets the ending offset of current strip.
        /// </summary>
        /// <value>The ending offset.</value>
        internal int EndingOffset { get { return StartingOffset + Data.Length; } }

        /// <summary>
        /// Create strip from original data, that was originally placed at given startingOffset.
        /// </summary>
        /// <param name="data">The data.</param>
        /// <param name="startingOffset">The starting offset.</param>
        internal Strip(string data, int startingOffset)
        {
            Data = data;
            IsOriginal = true;
            StartingOffset = startingOffset;
        }

        /// <summary>
        /// Create non-original data strip.
        /// </summary>
        /// <param name="data">The data.</param>
        internal Strip(string data)
        {
            Data = data;
            IsOriginal = false;
            StartingOffset = int.MinValue;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Strip"/> class.
        /// </summary>
        /// <param name="toClone">To clone.</param>
        internal Strip(Strip toClone)
        {
            Data = toClone.Data;
            StartingOffset = toClone.StartingOffset;
            IsOriginal = toClone.IsOriginal;

            if (toClone.Next != null)
                Next = new Strip(toClone.Next);
        }

        /// <summary>
        /// Splits the specified offset.
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <exception cref="System.NotSupportedException">Cannot split according to not contained offset</exception>
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

        /// <summary>
        /// Determines whether [contains] [the specified offset].
        /// </summary>
        /// <param name="offset">The offset.</param>
        /// <returns><c>true</c> if [contains] [the specified offset]; otherwise, <c>false</c>.</returns>
        internal bool Contains(int offset)
        {
            if (!IsOriginal)
                return false;

            return StartingOffset <= offset && offset <= EndingOffset;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Format("'{0}' off: {1}", Data, StartingOffset);
        }
    }

    /// <summary>
    /// Manager of source strips. It can handle source writing and removing based
    /// on source transformations.
    /// 
    /// It uses <see cref="Strip"/> sequential structure that is repaired by
    /// proceeded writings and removings.
    /// </summary>
    public class StripManager
    {
        /// <summary>
        /// The root strip which with its children covers whole source code.
        /// </summary>
        readonly Strip _root;

        /// <summary>
        /// Initializes a new instance of the <see cref="StripManager" /> class
        /// from given source code data.
        /// </summary>
        /// <param name="data">The source code.</param>
        public StripManager(string data)
        {
            _root = new Strip(data, 0);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StripManager" /> class
        /// from given strip manager, which data will be cloned.
        /// </summary>
        /// <param name="toClone">The manager that will be cloned.</param>
        public StripManager(StripManager toClone)
        {
            _root = new Strip(toClone._root);
        }

        /// <summary>
        /// Writes data at the specified offset.
        /// </summary>
        /// <param name="offset">The offset where data will be written.</param>
        /// <param name="data">The data that will be written.</param>
        public void Write(int offset, string data)
        {
            var strip = getStrip(offset);
            var nStrip = new Strip(data);

            insert(nStrip, strip, offset);
        }

        /// <summary>
        /// Removes part of source code at specified offset.
        /// </summary>
        /// <param name="offset">The offset where removing start.</param>
        /// <param name="length">The length of removed data.</param>
        /// <exception cref="System.NotSupportedException">Cannot remove partial strip</exception>
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

        /// <summary>
        /// Moves source code from start and given length to given target position.
        /// </summary>
        /// <param name="start">The start offset of moved data.</param>
        /// <param name="len">The length of moved data.</param>
        /// <param name="target">The target where moved data will be placed.</param>
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
        /// Get string representation of stripped data.
        /// </summary>
        /// <value>The data that are represented by current state of strips.</value>
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

        /// <summary>
        /// Inserts the specified <see cref="Strip"/> behind target strip
        /// at given offset.
        /// </summary>
        /// <param name="inserted">The inserted strip.</param>
        /// <param name="target">The target strip.</param>
        /// <param name="offset">The offset in target strip where data will be placed.</param>
        private void insert(Strip inserted, Strip target, int offset)
        {
            target.Split(offset);

            inserted.Next = target.Next;
            target.Next = inserted;
        }

        /// <summary>
        /// Get strip containing given offset.
        /// </summary>
        /// <param name="offset">Searched offset.</param>
        /// <param name="startStrip">The start strip.</param>
        /// <returns>Strip containing given offset.</returns>
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
