using CariFiturBCA.Core;
using CariFiturBCA.Data;
using UnityEngine;
using UnityEngine.UI;

namespace CariFiturBCA.UI
{
    // Pembuat elemen UI lewat kode. Memakai sprite artist berdasarkan nama file;
    // kalau belum ada, tampil sebagai kotak berwarna supaya game tetap bisa dites.
    // Koordinat: patokan 1920 x 1080, titik (0,0) = tengah layar, Y ke atas.
    public static class UIFactory
    {
        public static readonly Color BiruBca = new Color(0.0f, 0.38f, 0.74f);
        public static readonly Color TeksGelap = new Color(0.08f, 0.1f, 0.2f);

        static Font font;
        // GDD 11: satu font untuk semua UI. Taruh file font bernama "ui_font" di Resources/Fonts
        // untuk menggantinya. Kalau tidak ada, dipakai font bawaan Unity.
        public static Font Font
        {
            get
            {
                if (font != null) return font;
                font = Resources.Load<Font>("Fonts/ui_font");
                if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); // Unity 2022.2+
                if (font == null) font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                return font;
            }
        }

        // Font khusus angka timer (gaya jam digital 7-segmen). File: Resources/Fonts/timer_font.ttf.
        static Font fontTimer;
        public static Font FontTimer
        {
            get
            {
                if (fontTimer != null) return fontTimer;
                fontTimer = Resources.Load<Font>("Fonts/timer_font");
                if (fontTimer == null) fontTimer = Font;
                return fontTimer;
            }
        }

        // Latar gameplay dan tutorial. Memakai bg_main_potret (atau bg_main) kalau orientasi gambarnya cocok
        // dengan mode layar. Kalau tidak cocok (mis. bg_main landscape dipakai di mode potret), digambar
        // latar sementara lewat kode: biru gelap + pita biru muda di atas + garis border cyan.
        public static void LatarGameplay(Transform induk)
        {
            var s = SpriteLibrary.GetAny("bg_main_potret", "bg_main_portrait", "bg_main");
            bool cocok = s != null && ((s.rect.height > s.rect.width) == GameConfig.Potret);

            var dasar = Kotak(induk, "Latar", Vector2.zero, Vector2.zero);
            Penuh(dasar);
            var di = dasar.gameObject.AddComponent<Image>();
            di.raycastTarget = false;
            if (cocok) { di.sprite = s; di.color = Color.white; return; }

            if (!GameConfig.Potret) { di.color = new Color(0.82f, 0.9f, 0.97f); return; }

            di.color = new Color(0.0f, 0.27f, 0.65f);
            Gambar(induk, null, new Vector2(0, 775), new Vector2(1000, 230), new Color(0.73f, 0.93f, 1f)); // pita atas
            var garis = new Color(0.1f, 0.7f, 1f);
            Gambar(induk, null, new Vector2(0, 928), new Vector2(1042, 10), garis);
            Gambar(induk, null, new Vector2(0, -928), new Vector2(1042, 10), garis);
            Gambar(induk, null, new Vector2(-516, 0), new Vector2(10, 1866), garis);
            Gambar(induk, null, new Vector2(516, 0), new Vector2(10, 1866), garis);
        }

        public static RectTransform Kotak(Transform induk, string nama, Vector2 pos, Vector2 ukuran)
        {
            var go = new GameObject(nama, typeof(RectTransform));
            var rt = (RectTransform)go.transform;
            rt.SetParent(induk, false);
            rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.anchoredPosition = pos;
            rt.sizeDelta = ukuran;
            return rt;
        }

        public static void Penuh(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = rt.offsetMax = Vector2.zero;
        }

        public static Image Gambar(Transform induk, string spriteNama, Vector2 pos, Vector2 ukuran, Color? cadangan = null)
        {
            var rt = Kotak(induk, spriteNama ?? "Gambar", pos, ukuran);
            var img = rt.gameObject.AddComponent<Image>();
            img.raycastTarget = false;
            var sprite = spriteNama != null ? SpriteLibrary.Get(spriteNama) : null;
            if (sprite != null) { img.sprite = sprite; img.color = Color.white; img.preserveAspect = true; }
            else if (cadangan.HasValue) img.color = cadangan.Value;
            else img.enabled = false;
            return img;
        }

        public static Text Teks(Transform induk, string isi, Vector2 pos, Vector2 ukuran, int besar = 36,
                                TextAnchor posisi = TextAnchor.MiddleCenter, Color? warna = null)
        {
            var rt = Kotak(induk, "Teks", pos, ukuran);
            var t = rt.gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = isi;
            t.fontSize = besar;
            t.alignment = posisi;
            t.color = warna ?? TeksGelap;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }
    }
}