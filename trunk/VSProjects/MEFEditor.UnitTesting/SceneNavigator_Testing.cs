using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Windows;

using MEFEditor.Drawing;
using MEFEditor.Drawing.ArrangeEngine;

using MEFEditor.UnitTesting.Drawing_TestUtils;

namespace MEFEditor.UnitTesting
{
    /// <summary>
    /// Testing of <see cref="SceneNavigator"/> that is important for collision handling.
    /// </summary>
    [TestClass]
    public class SceneNavigator_Testing
    {

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void Scene_ItemIntersection()
        {

            DrawingTest.Create
                .Item("A", 0, 0)
                .Item("B", 500, 0)
                .Item("C", 1000, 0)

                .AssertIntersection(
                    new Point(50, 50), //Point inside A
                    new Point(1050, 50), //Point inside C
                    "B"
                )
                ;
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void Scene_TargetIntersection()
        {

            DrawingTest.Create
                .Item("A", 0, 0)
                .Item("B", 200, 500)
                .Item("C", 0, 1000)

                .AssertIntersection(
                    new Point(50, 50), //Point inside A
                    new Point(50, 1050), //Point inside C
                    "C" //no obstacle is hitted
                )
                ;
        }

        /// <summary>
        /// Test case.
        /// </summary>
        [TestMethod]
        public void Scene_DiagonalIntersection()
        {

            DrawingTest.Create
                .Item("A", 0, 0)
                .Item("B", 500, 0)
                .Item("C", 1000, 50)

                .AssertIntersection(
                    new Point(50, 0), //Point inside A
                    new Point(1050, 100), //Point inside C
                    "B" //B obstacle is hitted
                )
                ;
        }
    }
}
