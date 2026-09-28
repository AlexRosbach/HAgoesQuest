using System;
using UnityEngine;

namespace HAgoesQuest
{
    public sealed class WorldKeyboard
    {
        string value;
        bool upper;
        readonly bool secret;
        readonly Transform parent;
        readonly WorldCard display;
        readonly System.Collections.Generic.List<Tuple<WorldCard, string>> letters = new System.Collections.Generic.List<Tuple<WorldCard, string>>();

        public WorldKeyboard(Transform parent, string initial, bool secret, Action<string> accept, Action cancel)
        {
            this.parent = parent;
            this.secret = secret;
            value = initial ?? "";
            display = WorldCard.Create(parent, new Vector3(0, 0.27f, 0), new Vector2(740, 65), "");
            display.Interactable = false;
            display.Label.fontSize = 21;
            string[] rows = { "1234567890", "qwertzuiop", "asdfghjkl", "yxcvbnm.-_/:" };
            for (int row = 0; row < rows.Length; row++)
            for (int col = 0; col < rows[row].Length; col++)
            {
                string key = rows[row][col].ToString();
                var card = WorldCard.Create(parent, new Vector3((col - (rows[row].Length - 1) * 0.5f) * 0.064f, 0.16f - row * 0.077f, 0),
                    new Vector2(58, 64), key, () => { if (value.Length < 4096) value += upper ? key.ToUpperInvariant() : key; Refresh(); });
                letters.Add(Tuple.Create(card, key));
            }
            Small(-0.27f, "ABC / abc", () => { upper = !upper; foreach (var letter in letters) letter.Item1.Label.text = upper ? letter.Item2.ToUpperInvariant() : letter.Item2; });
            Small(-0.09f, "Löschen", () => { if (value.Length > 0) value = value.Substring(0, value.Length - 1); Refresh(); });
            Small(0.09f, "Leeren", () => { value = ""; Refresh(); });
            Small(0.27f, "Einfügen", () => { string pasted = GUIUtility.systemCopyBuffer; if (!string.IsNullOrEmpty(pasted) && pasted.Length <= 4096) value = pasted.Trim(); Refresh(); });
            WorldCard.Create(parent, new Vector3(-0.19f, -0.29f, 0), new Vector2(350, 65), "Zurück", cancel);
            WorldCard.Create(parent, new Vector3(0.19f, -0.29f, 0), new Vector2(350, 65), "Übernehmen", () => accept(value));
            Refresh();
        }

        void Small(float x, string label, Action action) => WorldCard.Create(parent, new Vector3(x, -0.18f, 0), new Vector2(168, 65), label, action);
        void Refresh()
        {
            display.Label.text = secret ? new string('•', Math.Min(32, value.Length)) + "  (" + value.Length + " Zeichen)" :
                value.Length > 60 ? "…" + value.Substring(value.Length - 60) : value;
            if (value.Length == 0) display.Label.text = secret ? "Token eingeben oder einfügen" : "Adresse eingeben";
        }
    }
}
