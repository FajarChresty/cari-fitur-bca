using System;
using System.Collections.Generic;
using CariFiturBCA.Core;
using CariFiturBCA.Data;
using UnityEngine;
using UnityEngine.UI;

namespace CariFiturBCA.UI
{
    // Layar tutorial yang muncul setelah "Sentuh untuk memulai" dan sebelum gameplay.
    // Kiri: 5 fitur asli BCA + tanda centang hijau (aman, masukkan ke deck).
    // Kanan: threat berbentuk hp + tanda silang merah (bahaya, jangan dimasukkan).
    // Semua tanda (centang, silang, lingkaran) digambar lewat kode, tidak butuh aset baru.
    // Ikon diambil dari sprite artist (fitur_01..05, threat_xx). Kalau belum ada, tampil kotak biru + nama.
    public class TutorialScreen
    {
        static readonly Color Hijau = new Color(0.12f, 0.70f, 0.30f);
        static readonly Color Merah = new Color(0.86f, 0.13f, 0.13f);
        static readonly Color Kuning = new Color(1f, 0.78f, 0.15f);
        // Threat yang ditampilkan sebagai contoh (urutan di ItemCatalog.Threat, mulai dari 0).
        static readonly int[] ThreatContoh = { 0, 2, 5, 6, 8 };

        static Sprite lingkaran;

        public RectTransform Root { get; private set; }
        public Action SaatMulai; // dipanggil saat tombol "MULAI MAIN" disentuh

        public TutorialScreen(Transform induk)
        {
            Root = UIFactory.Kotak(induk, "LayarTutorial", Vector2.zero, Vector2.zero);
            UIFactory.Penuh(Root);

            // Latar: bg_main, sama seperti gameplay, supaya terasa satu rangkaian.
            var bg = UIFactory.Kotak(Root, "Latar", Vector2.zero, Vector2.zero);
            UIFactory.Penuh(bg);
            var bgImg = bg.gameObject.AddComponent<Image>();
            bgImg.raycastTarget = false;
            var bgSprite = SpriteLibrary.Get("bg_main");
            if (bgSprite != null) { bgImg.sprite = bgSprite; bgImg.color = Color.white; }
            else bgImg.color = new Color(0.0f, 0.25f, 0.6f);

            // Judul di pita biru muda bagian atas
            Tebal(UIFactory.Teks(Root, "Cara Main: Mana yang Aman?", new Vector2(0, 425), new Vector2(1600, 90), 70,
                                 TextAnchor.MiddleCenter, UIFactory.TeksGelap));
            UIFactory.Teks(Root, "Kumpulkan 5 fitur asli BCA sebelum waktu habis", new Vector2(0, 350), new Vector2(1600, 60), 34,
                           TextAnchor.MiddleCenter, UIFactory.TeksGelap);

            // Dua kartu
            var fitur = new List<ItemDef>(ItemCatalog.FiturAsli);
            var threat = new List<ItemDef>();
            foreach (int i in ThreatContoh) if (i < ItemCatalog.Threat.Count) threat.Add(ItemCatalog.Threat[i]);

            Kartu(-460f, Hijau, "FITUR ASLI BCA", fitur, true, "AMAN",
                  "AMAN untuk di seret ke Deck BCA.");
            Kartu(460f, Merah, "THREAT / PENIPUAN", threat, false, "BAHAYA",
                  "Jangan dimasukkan ke Deck! Salah masuk, HP berkurang 20.");

            // Tombol mulai
            Persegi(Root, new Vector2(0, -355), new Vector2(556, 116), UIFactory.TeksGelap);
            var isi = Persegi(Root, new Vector2(0, -355), new Vector2(530, 90), Kuning);
            isi.raycastTarget = true;
            var btn = isi.gameObject.AddComponent<Button>();
            btn.targetGraphic = isi;
            btn.onClick.AddListener(() => SaatMulai?.Invoke());
            Tebal(UIFactory.Teks(isi.transform, "MULAI MAIN", Vector2.zero, new Vector2(500, 90), 56,
                                 TextAnchor.MiddleCenter, UIFactory.TeksGelap));
        }

        void Kartu(float cx, Color warna, string judul, List<ItemDef> daftar, bool aman, string badge, string deskripsi)
        {
            // Outline tebal gelap + isi putih (selaras gaya outline tebal di GDD bagian 11)
            Persegi(Root, new Vector2(cx, 0), new Vector2(816, 576), UIFactory.TeksGelap);
            Persegi(Root, new Vector2(cx, 0), new Vector2(800, 560), Color.white);
            Persegi(Root, new Vector2(cx, 232), new Vector2(800, 96), warna);
            Tebal(UIFactory.Teks(Root, judul, new Vector2(cx, 232), new Vector2(780, 96), 46, TextAnchor.MiddleCenter, Color.white));

            // Baris ikon
            const float ukuran = 140f, jarak = 152f;
            float mulai = -(daftar.Count - 1) / 2f * jarak;
            for (int i = 0; i < daftar.Count; i++)
            {
                var pos = new Vector2(cx + mulai + i * jarak, 100);
                var img = UIFactory.Gambar(Root, daftar[i].id, pos, new Vector2(ukuran, ukuran), new Color(0.1f, 0.45f, 0.8f));
                if (img.sprite == null)
                    UIFactory.Teks(img.transform, daftar[i].nama, Vector2.zero, new Vector2(ukuran - 12, ukuran - 12), 22,
                                   TextAnchor.MiddleCenter, Color.white);
            }

            // Tanda besar + tulisan AMAN / BAHAYA
            var posBadge = new Vector2(cx - 190, -95);
            Lingkaran(posBadge, 132, UIFactory.TeksGelap);
            Lingkaran(posBadge, 116, warna);
            if (aman) Centang(posBadge, 116); else Silang(posBadge, 116);
            Tebal(UIFactory.Teks(Root, badge, new Vector2(cx + 60, -95), new Vector2(360, 110), 72,
                                 TextAnchor.MiddleLeft, warna));

            UIFactory.Teks(Root, deskripsi, new Vector2(cx, -208), new Vector2(720, 130), 32,
                           TextAnchor.MiddleCenter, UIFactory.TeksGelap);
        }

        // ---- Gambar sederhana lewat kode ----

        static void Tebal(Text t) { t.fontStyle = FontStyle.Bold; }

        static Image Persegi(Transform induk, Vector2 pos, Vector2 ukuran, Color warna)
        {
            var img = UIFactory.Gambar(induk, null, pos, ukuran, warna);
            img.raycastTarget = false;
            return img;
        }

        void Lingkaran(Vector2 pos, float ukuran, Color warna)
        {
            var rt = UIFactory.Kotak(Root, "Lingkaran", pos, new Vector2(ukuran, ukuran));
            var img = rt.gameObject.AddComponent<Image>();
            img.sprite = SpriteLingkaran();
            img.color = warna;
            img.raycastTarget = false;
        }

        // Batang putih (dipakai menyusun centang dan silang)
        void Batang(Vector2 pos, Vector2 ukuran, float rotasi)
        {
            var img = Persegi(Root, pos, ukuran, Color.white);
            img.rectTransform.localRotation = Quaternion.Euler(0, 0, rotasi);
        }

        void Centang(Vector2 pusat, float s)
        {
            float t = 0.13f * s;
            var geser = new Vector2(-0.05f, -0.05f) * s;
            Batang(pusat + geser + new Vector2(-0.14f, -0.05f) * s, new Vector2(t, 0.28f * s), 45f);
            Batang(pusat + geser + new Vector2(0.15f, 0.06f) * s, new Vector2(t, 0.55f * s), -45f);
        }

        void Silang(Vector2 pusat, float s)
        {
            float t = 0.13f * s;
            Batang(pusat, new Vector2(t, 0.58f * s), 45f);
            Batang(pusat, new Vector2(t, 0.58f * s), -45f);
        }

        // Sprite lingkaran dibuat sekali lewat kode (tidak perlu aset)
        static Sprite SpriteLingkaran()
        {
            if (lingkaran != null) return lingkaran;
            const int n = 128;
            var tex = new Texture2D(n, n, TextureFormat.RGBA32, false)
            { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[n * n];
            float c = (n - 1) / 2f, r = n / 2f - 1.5f;
            for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Mathf.Sqrt((x - c) * (x - c) + (y - c) * (y - c));
                    byte a = (byte)(Mathf.Clamp01(r - d) * 255f);
                    px[y * n + x] = new Color32(255, 255, 255, a);
                }
            tex.SetPixels32(px);
            tex.Apply();
            lingkaran = Sprite.Create(tex, new Rect(0, 0, n, n), new Vector2(0.5f, 0.5f), 100f);
            return lingkaran;
        }
    }
}