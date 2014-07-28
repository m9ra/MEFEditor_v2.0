using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using MEFEditor.UnitTesting.AssemblyProviders_TestUtils;

namespace MEFEditor.UnitTesting
{
    /// <summary>
    /// Testing of <see cref="Source"/> correctnes of handling code writing.
    /// </summary>
    [TestClass]
    public class SourceWriting_Testing
    {

        /// <summary>
        /// Test case.
        /// </summary>
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

        /// <summary>
        /// Test case.
        /// </summary>
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

        /// <summary>
        /// Test case.
        /// </summary>
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

        /// <summary>
        /// Test case.
        /// </summary>
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
