using System;
using NUnit.Framework;
using ProjetoFinal.Localization;

namespace ProjetoFinal.Tests
{
    public class LocalizerTests
    {
        private const string Csv =
            "key,en,pt-BR\n" +
            "# comentário\n" +
            "menu.play,PLAY,JOGAR\n" +
            "only.en,ONLY EN,\n";

        private sealed class FakeStorage : ILanguageStorage
        {
            public string Saved;
            public string Load() => Saved;
            public void Save(string languageCode) => Saved = languageCode;
        }

        private static Localizer Create(FakeStorage storage = null, string preferred = "pt-BR") =>
            new(LocalizationTable.FromCsv(Csv), storage ?? new FakeStorage(), preferred, "en");

        [Test]
        public void Table_ParsesLanguagesAndSkipsComments()
        {
            var table = LocalizationTable.FromCsv(Csv);

            CollectionAssert.AreEqual(new[] { "en", "pt-BR" }, table.Languages);
            CollectionAssert.AreEqual(new[] { "menu.play", "only.en" }, table.Keys);
        }

        [Test]
        public void Table_DuplicateKey_Throws()
        {
            Assert.Throws<FormatException>(() => LocalizationTable.FromCsv("key,en\na,1\na,2"));
        }

        [Test]
        public void Get_ReturnsTranslationForCurrentLanguage()
        {
            var localizer = Create();

            Assert.AreEqual("JOGAR", localizer.Get("menu.play"));
            localizer.SetLanguage("en");
            Assert.AreEqual("PLAY", localizer.Get("menu.play"));
        }

        [Test]
        public void Get_MissingTranslation_FallsBackThenShowsKey()
        {
            var localizer = Create();

            Assert.AreEqual("ONLY EN", localizer.Get("only.en"));
            Assert.AreEqual("#does.not.exist", localizer.Get("does.not.exist"));
        }

        [Test]
        public void Constructor_PrefersSavedLanguage_ThenPreferred_ThenFallback()
        {
            Assert.AreEqual("en", Create(new FakeStorage { Saved = "en" }).CurrentLanguage);
            Assert.AreEqual("pt-BR", Create(new FakeStorage { Saved = "xx" }).CurrentLanguage);
            Assert.AreEqual("en", Create(preferred: "fr").CurrentLanguage);
        }

        [Test]
        public void SetLanguage_PersistsAndRaisesEventOnlyOnChange()
        {
            var storage = new FakeStorage();
            var localizer = Create(storage);
            var raised = 0;
            localizer.LanguageChanged += _ => raised++;

            localizer.SetLanguage("en");
            localizer.SetLanguage("en");

            Assert.AreEqual(1, raised);
            Assert.AreEqual("en", storage.Saved);
        }

        [Test]
        public void CycleLanguage_FollowsCsvOrderAndWrapsAround()
        {
            var localizer = Create(preferred: "en");

            Assert.AreEqual("pt-BR", localizer.GetNextLanguage());
            Assert.AreEqual("pt-BR", localizer.CycleLanguage());
            Assert.AreEqual("en", localizer.CycleLanguage());
        }

        [Test]
        public void SetLanguage_Unsupported_Throws()
        {
            Assert.Throws<ArgumentException>(() => Create().SetLanguage("xx"));
        }
    }
}
