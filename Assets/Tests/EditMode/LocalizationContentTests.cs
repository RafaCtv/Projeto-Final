using NUnit.Framework;
using ProjetoFinal.Localization;
using UnityEngine;

namespace ProjetoFinal.Tests
{
    public class LocalizationContentTests
    {
        private LocalizationSettings settings;
        private LocalizationTable table;

        [SetUp]
        public void SetUp()
        {
            settings = Resources.Load<LocalizationSettings>(LocalizationSettings.ResourcesPath);
            Assert.IsNotNull(settings, "LocalizationSettings não encontrado em Resources.");
            Assert.IsNotNull(settings.StringsCsv, "LocalizationSettings sem CSV.");
            table = LocalizationTable.FromCsv(settings.StringsCsv.text);
        }

        [Test]
        public void EveryKey_HasTranslationInEveryLanguage()
        {
            foreach (var language in table.Languages)
            foreach (var key in table.Keys)
                Assert.IsTrue(table.TryGet(language, key, out _), $"Falta tradução de '{key}' em '{language}'.");
        }

        [Test]
        public void EveryLanguageDefinition_MatchesCsvColumnAndHasFlag()
        {
            Assert.IsNotEmpty(settings.Languages);
            Assert.IsNotNull(settings.FallbackLanguage, "Fallback não definido.");

            foreach (var language in settings.Languages)
            {
                Assert.IsNotNull(language, "Entrada vazia na lista de idiomas.");
                Assert.IsTrue(table.HasLanguage(language.Code), $"CSV não tem coluna '{language.Code}'.");
                Assert.IsNotNull(language.Flag, $"Idioma '{language.Code}' sem bandeira.");
            }
        }

        [Test]
        public void EveryCsvLanguage_HasLanguageDefinition()
        {
            foreach (var code in table.Languages)
                Assert.IsNotNull(settings.FindByCode(code),
                    $"Coluna '{code}' do CSV sem LanguageDefinition em LocalizationSettings (ficaria sem bandeira).");
        }
    }
}
