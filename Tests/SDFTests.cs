using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Numerics;
using Walgelijk;

namespace Tests;

[TestClass]
public class SDFTests
{
    [TestMethod]
    public void Rectangle()
    {
        {
            var point = new Vector2(2, 1);
            var offset = new Vector2(0, 0);
            float w = 2;
            float h = 2;
            float expected = 0;
            float actual = SDF.Rectangle(point, offset, new Vector2(w, h));
            Assert.AreEqual(expected, actual, 0.001f);
        }

        {
            var point = new Vector2(1, 0);
            var offset = new Vector2(0, 0);
            float w = 2;
            float h = 2;
            float expected = -1;
            float actual = SDF.Rectangle(point, offset, new Vector2(w, h));
            Assert.AreEqual(expected, actual, 0.001f);
        }

        {
            var point = new Vector2(15, 0);
            var offset = new Vector2(0, 0);
            float w = 3;
            float h = 3;
            float expected = 15 - 3;
            float actual = SDF.Rectangle(point, offset, new Vector2(w, h));
            Assert.AreEqual(expected, actual, 0.001f);
        }

        {
            var point = new Vector2(15, 0);
            var offset = new Vector2(5, 0);
            float w = 3;
            float h = 3;
            float expected = 15 - 3 - 5;
            float actual = SDF.Rectangle(point, offset, new Vector2(w, h));
            Assert.AreEqual(expected, actual, 0.001f);
        }
    }

    [TestMethod]
    public void RoundedRectangle()
    {
        {
            var point = new Vector2(2, 1);
            var offset = new Vector2(0, 0);
            float w = 2;
            float h = 2;
            float expected = 0;
            float actual = SDF.RoundedRectangle(point, offset, new Vector2(w, h), 1);
            Assert.AreEqual(expected, actual, 0.001f);
        }

        {
            var point = new Vector2(1, 0);
            var offset = new Vector2(0, 0);
            float w = 2;
            float h = 2;
            float expected = -1;
            float actual = SDF.RoundedRectangle(point, offset, new Vector2(w, h), 1);
            Assert.AreEqual(expected, actual, 0.001f);
        }

        {
            var point = new Vector2(15, 0);
            var offset = new Vector2(0, 0);
            float w = 3;
            float h = 3;
            float expected = 15 - 3;
            float actual = SDF.RoundedRectangle(point, offset, new Vector2(w, h), 1);
            Assert.AreEqual(expected, actual, 0.001f);
        }

        {
            var point = new Vector2(15, 0);
            var offset = new Vector2(5, 0);
            float w = 3;
            float h = 3;
            float expected = 15 - 3 - 5;
            float actual = SDF.RoundedRectangle(point, offset, new Vector2(w, h), 1);
            Assert.AreEqual(expected, actual, 0.001f);
        }
    }

    [TestMethod]
    public void Triangle()
    {
        var p0 = new Vector2(-1, -1);
        var p1 = new Vector2(1, -1);
        var p2 = new Vector2(0, 1);
        var p = new Vector2(0, 0);
        float expectedDistance = 1 / -MathF.Sqrt(5);
        float actualDistance = SDF.Triangle(p, Vector2.Zero, p0, p1, p2);
        Assert.AreEqual(expectedDistance, actualDistance);

        // Test a point outside the triangle
        p = new Vector2(0, 2);
        expectedDistance = 1;
        actualDistance = SDF.Triangle(p, Vector2.Zero, p0, p1, p2);
        Assert.AreEqual(expectedDistance, actualDistance);

        // Test a point on the edge of the triangle
        p = new Vector2(0.5f, 0);
        expectedDistance = 0;
        actualDistance = SDF.Triangle(p, Vector2.Zero, p0, p1, p2);
        Assert.AreEqual(expectedDistance, actualDistance);
    }
}
