using CariFiturBCA.Data;
using UnityEngine;
using UnityEngine.UI;

namespace CariFiturBCA.UI
{
    // Tata letak GDD bagian 6: bar HP kiri atas, timer tengah atas, deck hp BCA + counter kanan atas.
    public class Hud
    {
        static readonly Vector2 HpPos = new Vector2(-600, 450);
        static readonly Vector2 HpUkuran = new Vector2(560, 56);
        static readonly Vector2 TimerPos = new Vector2(0, 450);

        readonly RectTransform hpIsi;
        readonly Text hpTeks, timerTeks, counterTeks;
        readonly Color timerNormal = Color.white;
        readonly Color timerMerah = new Color(1f, 0.25f, 0.25f);

        public Hud(Transform induk)
        {
            // HP
            UIFactory.Gambar(induk, "ui_hp", HpPos, new Vector2(HpUkuran.x + 40, HpUkuran.y + 40), new Color(0, 0, 0, 0.35f));
            var latar = UIFactory.Gambar(induk, null, HpPos, HpUkuran, new Color(0.15f, 0.15f, 0.2f));
            hpIsi = UIFactory.Kotak(induk, "HpIsi", HpPos, HpUkuran);
            hpIsi.pivot = new Vector2(0, 0.5f);
            hpIsi.anchoredPosition = HpPos - new Vector2(HpUkuran.x / 2f, 0);
            var isi = hpIsi.gameObject.AddComponent<Image>();
            isi.color = new Color(0.25f, 0.85f, 0.4f);
            isi.raycastTarget = false;
            hpTeks = UIFactory.Teks(induk, "", HpPos, HpUkuran, 32, TextAnchor.MiddleCenter, Color.white);
            latar.raycastTarget = false;

            // Timer
            UIFactory.Gambar(induk, "ui_timer", TimerPos, new Vector2(300, 120), new Color(0, 0, 0, 0.35f));
            timerTeks = UIFactory.Teks(induk, "", TimerPos, new Vector2(300, 120), 84, TextAnchor.MiddleCenter, timerNormal);

            // Deck hp BCA (asli 400 x 600, ditampilkan lebih kecil) dan counter di bawahnya
            var deck = UIFactory.Gambar(induk, "deck", GameConfig.DeckPos, GameConfig.DeckUkuran, UIFactory.BiruBca);
            if (!deck.sprite)
                UIFactory.Teks(induk, "HP BCA", GameConfig.DeckPos, GameConfig.DeckUkuran, 48, TextAnchor.MiddleCenter, Color.white);
            var posCounter = GameConfig.DeckPos + new Vector2(0, -GameConfig.DeckUkuran.y / 2f - 50);
            UIFactory.Gambar(induk, "ui_counter", posCounter, new Vector2(220, 80), new Color(0, 0, 0, 0.35f));
            counterTeks = UIFactory.Teks(induk, "", posCounter, new Vector2(220, 80), 56, TextAnchor.MiddleCenter, Color.white);
        }

        // Area drop deck: sedikit lebih besar dari gambarnya (GDD bagian 9). Koordinat canvas.
        public static Rect AreaDropDeck()
        {
            Vector2 ukuran = GameConfig.DeckUkuran + Vector2.one * GameConfig.DropDeckTambahan * 2f;
            return new Rect(GameConfig.DeckPos - ukuran / 2f, ukuran);
        }

        // Tengah deck, tujuan animasi item yang masuk.
        public static Vector2 TengahDeck => GameConfig.DeckPos;

        public void SetHp(int hp)
        {
            float p = Mathf.Clamp01(hp / (float)GameConfig.HpAwal);
            hpIsi.sizeDelta = new Vector2(HpUkuran.x * p, HpUkuran.y);
            hpTeks.text = $"HP {Mathf.Max(hp, 0)}";
        }

        public void SetWaktu(float sisa)
        {
            timerTeks.text = Mathf.CeilToInt(Mathf.Max(sisa, 0)).ToString();
            timerTeks.color = sisa <= GameConfig.BatasTimerMerah ? timerMerah : timerNormal;
        }

        public void SetCounter(int n) => counterTeks.text = $"{n}/{GameConfig.TargetFitur}";
    }
}
