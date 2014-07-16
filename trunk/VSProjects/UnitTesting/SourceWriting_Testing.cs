using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using UnitTesting.AssemblyProviders_TestUtils;

namespace UnitTesting
{
    /// <summary>
    /// Testing of <see cref="Source"/> correctnes of handling code writing.
    /// </summary>
    [TestClass]
    public class SourceWriting_Testing
    {
        [TestMethod]
        public void WriteMapping()
        {
            "MMM"
            .Write(1, "Y",
                "MYMM"
            )

            .Write(1, "Z",
                "MZYMM"
            )

            .Write(2, "X",
                "MZYMXM"
            )

            .Write(0, "A",
                "AMZYMXM"
            );
        }

        [TestMethod]
        public void WriteToMoveMapping1()
        {
            "MAAMMZ"
            .Move(1, 2, 5,
                "MMMAAZ"
            )

            .Write(1, "B",
                "MBMMAAZ"
            )

            .Write(2, "C",
                "MBMMACAZ"
            )
            ;
        }

        [TestMethod]
        public void WriteToMoveMapping2()
        {
            "MAAMZ"
            .Move(1, 2, 4,
                "MMAAZ"
            )

            .Write(2, "E",
                "MMAEAZ"
            )
            ;
        }

        [TestMethod]
        public void WriteToMoveMapping3()
        {
            "MAAMMMZ"
            .Move(1, 2, 5,
                "MMMAAMZ"
            )

            .Write(4, "B",
                "MMBMAAMZ"
            )


            ;
        }

    }
}
