using System;
using System.Collections.Generic;

namespace ProjetoFinal.Localization
{
    /// <summary>
    /// Traduções carregadas de um CSV com cabeçalho "key,&lt;idioma&gt;,...".
    /// Linhas com chave iniciada por '#' são ignoradas.
    /// </summary>
    public sealed class LocalizationTable
    {
        private readonly List<string> languages = new();
        private readonly List<string> keys = new();
        private readonly Dictionary<string, Dictionary<string, string>> valuesByLanguage = new();

        public IReadOnlyList<string> Languages => languages;
        public IReadOnlyList<string> Keys => keys;

        public static LocalizationTable FromCsv(string csv)
        {
            var rows = CsvParser.Parse(csv);
            if (rows.Count == 0)
                throw new FormatException("CSV de localização vazio.");

            var header = rows[0];
            if (header.Length < 2 || header[0].Trim() != "key")
                throw new FormatException("Cabeçalho deve ser: key,<idioma1>,<idioma2>,...");

            var table = new LocalizationTable();
            for (var col = 1; col < header.Length; col++)
            {
                var code = header[col].Trim();
                if (code.Length == 0 || table.valuesByLanguage.ContainsKey(code))
                    throw new FormatException($"Coluna de idioma inválida ou duplicada: '{code}'.");

                table.languages.Add(code);
                table.valuesByLanguage[code] = new Dictionary<string, string>();
            }

            for (var r = 1; r < rows.Count; r++)
            {
                var row = rows[r];
                var key = row[0].Trim();
                if (key.Length == 0 || key.StartsWith("#"))
                    continue;

                if (table.valuesByLanguage[table.languages[0]].ContainsKey(key))
                    throw new FormatException($"Chave duplicada: '{key}'.");

                table.keys.Add(key);
                for (var l = 0; l < table.languages.Count; l++)
                {
                    var value = l + 1 < row.Length ? row[l + 1] : string.Empty;
                    table.valuesByLanguage[table.languages[l]][key] = value;
                }
            }

            return table;
        }

        public bool HasLanguage(string code) => code != null && valuesByLanguage.ContainsKey(code);

        /// <summary>Valores vazios são tratados como ausentes.</summary>
        public bool TryGet(string language, string key, out string value)
        {
            value = null;
            return key != null
                && valuesByLanguage.TryGetValue(language ?? string.Empty, out var values)
                && values.TryGetValue(key, out value)
                && !string.IsNullOrEmpty(value);
        }
    }
}
