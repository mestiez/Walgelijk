using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Globalization;
using Walgelijk.Localisation;

namespace Tests;

[TestClass]
public class LocalisationTests
{
    private readonly Language english = new(
        "English",
        new CultureInfo("en-GB"), Flags.UnitedKingdom, new Dictionary<string, string>
        {
            {"driehoek-p1", "triangles are shapes with tree sides"},
            {"spoorweg-p1", "the railway is a vast network of steel tracks on which trains move around"},
            {"only-english-p1", "this sentence is only available in english :("},
        });

    private readonly Language dutch = new(
        "Nederlands",
        new CultureInfo("nl-NL"), Flags.Netherlands, new Dictionary<string, string>
        {
            {"driehoek-p1", "driehoeken zijn vormen met drie kanten"},
            {"spoorweg-p1", "de spoorweg is een uitgestrekt netwerk van stalen sporen waarop treinen rondrijden"},
        });

    [TestMethod]
    public void StringCulture()
    {
        Assert.AreEqual("52.38", 52.38f.ToString(english.Culture));
        Assert.AreEqual("52,38", 52.38f.ToString(dutch.Culture));
    }

    [TestMethod]
    public void SimpleTranslation()
    {
        Localisation.FallbackLanguage = english;
        Localisation.CurrentLanguage = english;
        foreach (var item in english.Table)
            Assert.AreEqual(item.Value, Localisation.Get(item.Key));

        Localisation.FallbackLanguage = dutch;
        Localisation.CurrentLanguage = dutch;
        foreach (var item in dutch.Table)
            Assert.AreEqual(item.Value, Localisation.Get(item.Key));
    }

    [TestMethod]
    public void FallbackTranslation()
    {
        Localisation.FallbackLanguage = english;
        Localisation.CurrentLanguage = dutch;

        Assert.AreEqual("driehoeken zijn vormen met drie kanten", Localisation.Get("driehoek-p1"));
        Assert.AreEqual("this sentence is only available in english :(", Localisation.Get("only-english-p1"));
    }
}
