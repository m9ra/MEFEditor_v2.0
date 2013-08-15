using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AssemblyProviders.CSharp.Interfaces
{
    public interface IPosition
    {
        int Offset { get; }

        IPosition Shift(int p);

        string GetStrip(IPosition endPosition);
    }
}
