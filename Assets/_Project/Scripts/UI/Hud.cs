using CariFiturBCA.Data;
using UnityEngine;
using UnityEngine.UI;

namespace CariFiturBCA.UI
{
    // Tata letak GDD bagian 6: bar HP kiri atas, timer tengah atas, deck hp BCA + counter kanan atas.
    public class Hud
    {
        // Pita atas (area biru muda di bg_main) berada di y sekitar 318 sampai 474.
        // Semua elemen atas ditaruh di tengah pita itu supaya tidak menabrak garis border.
        const float YBarAtas = 396f;
        // Geser X: makin besar (mendekati 0) = makin ke kanan, menjauh dari border kiri.
        static readonly Vector2 HpPos = new Vector2(-550, YBarAtas);
        static readonly Vector2 HpUkuran = new Vector2(560, 56);
        static readonly Vector2 TimerPos = new Vector2(0, YBarAtas);
        // Gambar ui_timer punya ikon jam di kiri, jadi angka digeser ke kanan supaya
        // tepat di tengah ruang kosong. Ubah angka ini kalau masih kurang pas.
        static readonly Vector2 TimerAngkaGeser = new Vector2(36, 0);

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
            timerTeks = UIFactory.Teks(induk, "", TimerPos + TimerAngkaGeser, new Vector2(150, 120), 76, TextAnchor.MiddleCenter, timerNormal);
            timerTeks.font = UIFactory.FontTimer; // font digital khusus timer

            // Deck hp BCA (asli 400 x 600, ditampilkan lebih kecil).
            // Gambar deck.png dari artist sudah punya kotak "0/5" di bagian bawah. Angka di gambar tidak
            // bisa berubah, jadi kotak itu ditutup plat biru lalu diganti counter dari kode.
            // Kalau artist mengekspor ulang deck.png tanpa angka, plat biru ini boleh dihapus.
            var deck = UIFactory.Gambar(induk, "deck", GameConfig.DeckPos, GameConfig.DeckUkuran, UIFactory.BiruBca);
            if (!deck.sprite)
                UIFactory.Teks(induk, "HP BCA", GameConfig.DeckPos, GameConfig.DeckUkuran, 48, TextAnchor.MiddleCenter, Color.white);

            // Posisi kotak "0/5" pada deck.png (dihitung dari gambar 400 x 600, tampil 300 x 450).
            var posCounter = GameConfig.DeckPos + new Vector2(0, -147);
            UIFactory.Gambar(induk, null, posCounter, new Vector2(170, 52), new Color(0.02f, 0.22f, 0.62f)); // plat penutup
            UIFactory.Gambar(induk, null, posCounter, new Vector2(112, 42), Color.white);                    // kotak putih counter
            counterTeks = UIFactory.Teks(induk, "", posCounter, new Vector2(112, 42), 34, TextAnchor.MiddleCenter, UIFactory.TeksGelap);
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