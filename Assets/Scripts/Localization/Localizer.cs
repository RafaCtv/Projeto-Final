using System;
using System.Collections.Generic;

namespace ProjetoFinal.Localization
{
    public sealed class Localizer
    {
        private readonly LocalizationTable table;
        private readonly ILanguageStorage storage;
        private readonly string fallbackLanguage;

        public string CurrentLanguage { get; private set; }
        public IReadOnlyList<string> Languages => table.Languages;

        public event Action<string> LanguageChanged;

        /// <param name="preferredLanguage">Idioma inicial quando não há um salvo.</param>
        /// <param name="fallbackLanguage">Idioma usado quando falta tradução no atual.</param>
        public Localizer(LocalizationTable table, ILanguageStorage storage, string preferredLanguage, string fallbackLanguage)
        {
            this.table = table ?? throw new ArgumentNullException(nameof(table));
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));

            if (table.Languages.Count == 0)
                throw new ArgumentException("A tabela não tem nenhum idioma.", nameof(table));

            this.fallbackLanguage = table.HasLanguage(fallbackLanguage) ? fallbackLanguage : table.Languages[0];

            var saved = storage.Load();
            if (table.HasLanguage(saved))
                CurrentLanguage = saved;
            else if (table.HasLanguage(preferredLanguage))
                CurrentLanguage = preferredLanguage;
            else
                CurrentLanguage = this.fallbackLanguage;
        }

        public bool HasLanguage(string code) => table.HasLanguage(code);

        public void SetLanguage(string code)
        {
            if (!table.HasLanguage(code))
                throw new ArgumentException($"Idioma não suportado: '{code}'.", nameof(code));

            if (code == CurrentLanguage)
                return;

            CurrentLanguage = code;
            storage.Save(code);
            LanguageChanged?.Invoke(code);
        }

        /// <summary>Segue a ordem das colunas do CSV, voltando ao primeiro após o último.</summary>
        public string GetNextLanguage()
        {
            var languages = table.Languages;
            var index = -1;
            for (var i = 0; i < languages.Count; i++)
                if (languages[i] == CurrentLanguage)
                    index = i;
            return languages[(index + 1) % languages.Count];
        }

        public string CycleLanguage()
        {
            SetLanguage(GetNextLanguage());
            return CurrentLanguage;
        }

        /// <summary>Retorna "#chave" quando não há tradução nem no idioma de fallback.</summary>
        public string Get(string key) => TryGet(key, out var value) ? value : $"#{key}";

        public bool TryGet(string key, out string value) =>
            table.TryGet(CurrentLanguage, key, out value) || table.TryGet(fallbackLanguage, key, out value);
    }
}
