using System.Collections;
using System.Collections.Generic;
using CariFiturBCA.Core;
using CariFiturBCA.Data;
using CariFiturBCA.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CariFiturBCA.Gameplay
{
    // Alur layar GDD bagian 4: Layar awal -> Gameplay 30 detik -> End screen 5 detik -> Layar awal.
    // Semua dibangun lewat kode saat game mulai, jadi cukup buka scene kosong lalu Play.
    public class GameController : MonoBehaviour
    {
        enum Fase { Awal, Main, Menunggu, Akhir }

        const string PesanMenang = "Kamu berhasil! Ingat, BCA tidak pernah minta OTP, PIN, atau password lewat chat.";
        const string PesanKalah = "Hampir! Ingat, BCA tidak pernah minta OTP, PIN, atau password lewat chat.";

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Boot()
        {
            if (FindFirstObjectByType<GameController>() != null) return;
            new GameObject("GameController").AddComponent<GameController>();
        }

        Fase fase = Fase.Awal;
        RectTransform canvasRoot, dunia, tumpukan, layarAwal, layarAkhir, flash;
        Image flashImg;
        Hud hud;
        Text akhirJudul, akhirDetail, akhirPesan;

        int hp, terkumpul;
        float sisaWaktu, waktuAkhirMulai;
        bool menang;
        Coroutine flashCo, shakeCo;

        void Awake()
        {
            Input.multiTouchEnabled = false;                       // satu jari saja (GDD bagian 4)
            Screen.orientation = ScreenOrientation.LandscapeLeft;  // landscape 1920x1080
            Application.targetFrameRate = 60;

            BuatCanvas();
            BuatDunia();
            BuatLayarAwal();
            BuatLayarAkhir();
            BuatFlash();
            KeLayarAwal();
        }

        // ---- Susunan canvas ----

        void BuatCanvas()
        {
            var go = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            go.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = GameConfig.Layar;
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            canvasRoot = (RectTransform)go.transform;

            if (FindFirstObjectByType<EventSystem>() == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        }

        static Image Latar(Transform induk, Color cadangan, params string[] kandidat)
        {
            var rt = UIFactory.Kotak(induk, "Latar", Vector2.zero, Vector2.zero);
            UIFactory.Penuh(rt);
            var img = rt.gameObject.AddComponent<Image>();
            img.raycastTarget = false;
            var s = SpriteLibrary.GetAny(kandidat);
            if (s != null) { img.sprite = s; img.color = Color.white; } else img.color = cadangan;
            return img;
        }

        void BuatDunia()
        {
            dunia = UIFactory.Kotak(canvasRoot, "Dunia", Vector2.zero, Vector2.zero);
            UIFactory.Penuh(dunia);
            Latar(dunia, new Color(0.82f, 0.9f, 0.97f), "bg_main");
            hud = new Hud(dunia);
            // Tumpukan dibuat terakhir supaya item yang di-drag tampil di atas deck dan HUD.
            tumpukan = UIFactory.Kotak(dunia, "Tumpukan", Vector2.zero, Vector2.zero);
            UIFactory.Penuh(tumpukan);
        }

        // Layar awal: gambar bg_awal dari artist SUDAH berisi logo BCA mobile, ponsel, dan tulisan
        // "Sentuh untuk memulai". Jadi kode tidak menambah judul atau tombol lagi (dulu bikin dobel).
        // Gambar ditampilkan utuh tanpa melar (preserveAspect), sisa layar diisi warna latar.
        void BuatLayarAwal()
        {
            layarAwal = UIFactory.Kotak(canvasRoot, "LayarAwal", Vector2.zero, Vector2.zero);
            UIFactory.Penuh(layarAwal);

            // Warna isi samping gambar. Samakan dengan warna dasar bg_awal (sekarang putih).
            var isi = UIFactory.Kotak(layarAwal, "IsiLatar", Vector2.zero, Vector2.zero);
            UIFactory.Penuh(isi);
            var isiImg = isi.gameObject.AddComponent<Image>();
            isiImg.color = Color.white;
            isiImg.raycastTarget = false;

            var art = Latar(layarAwal, UIFactory.BiruBca, "bg_awal", "bg_title", "layar_awal", "title");
            art.preserveAspect = true;

            // Cadangan: kalau gambar belum ada, tampilkan teks supaya tetap bisa dites.
            if (art.sprite == null)
            {
                UIFactory.Teks(layarAwal, "Cari Fitur BCA", new Vector2(0, 150), new Vector2(1400, 180), 110,
                               TextAnchor.MiddleCenter, Color.white);
                UIFactory.Teks(layarAwal, "Sentuh untuk mulai", new Vector2(0, -150), new Vector2(1000, 100), 56,
                               TextAnchor.MiddleCenter, Color.white);
            }

            // Seluruh layar adalah satu tombol besar untuk memulai game
            var tap = layarAwal.gameObject.AddComponent<Image>();
            tap.color = new Color(0, 0, 0, 0);
            var btn = layarAwal.gameObject.AddComponent<Button>();
            btn.transition = Selectable.Transition.None;
            btn.onClick.AddListener(MulaiMain);
        }

        // End screen: bg_menang / bg_kalah dari artist sudah memuat judul dan pesan edukasi (sudah tertulis di gambar).
        // Kode hanya menambah satu baris hasil (sisa waktu / fitur terkumpul) di panel putih.
        // Kalau gambar belum ada, tampil judul + pesan cadangan supaya tetap bisa dites.
        void BuatLayarAkhir()
        {
            layarAkhir = UIFactory.Kotak(canvasRoot, "LayarAkhir", Vector2.zero, Vector2.zero);
            UIFactory.Penuh(layarAkhir);

            // Warna isi samping gambar (gambar artist berbentuk persegi, layar 16:9). Putih sesuai dasar gambar.
            var isi = UIFactory.Kotak(layarAkhir, "IsiLatar", Vector2.zero, Vector2.zero);
            UIFactory.Penuh(isi);
            var isiImg = isi.gameObject.AddComponent<Image>();
            isiImg.color = Color.white;
            isiImg.raycastTarget = false;

            akhirJudul = UIFactory.Teks(layarAkhir, "", new Vector2(0, 220), new Vector2(1500, 180), 120,
                                        TextAnchor.MiddleCenter, Color.white);
            akhirDetail = UIFactory.Teks(layarAkhir, "", new Vector2(0, 60), new Vector2(1500, 120), 64,
                                         TextAnchor.MiddleCenter, Color.white);
            akhirPesan = UIFactory.Teks(layarAkhir, "", new Vector2(0, -180), new Vector2(1400, 260), 52,
                                        TextAnchor.MiddleCenter, Color.white);
        }

        Image latarAkhir;

        void BuatFlash()
        {
            flash = UIFactory.Kotak(canvasRoot, "Flash", Vector2.zero, Vector2.zero);
            UIFactory.Penuh(flash);
            flashImg = flash.gameObject.AddComponent<Image>();
            flashImg.color = new Color(1, 1, 1, 0);
            flashImg.raycastTarget = false;
        }

        // ---- Alur layar ----

        void KeLayarAwal()
        {
            fase = Fase.Awal;
            BersihkanTumpukan();
            dunia.gameObject.SetActive(false);
            layarAkhir.gameObject.SetActive(false);
            layarAwal.gameObject.SetActive(true);
            dunia.anchoredPosition = Vector2.zero;
        }

        void MulaiMain()
        {
            if (fase != Fase.Awal) return;
            hp = GameConfig.HpAwal;
            terkumpul = 0;
            sisaWaktu = GameConfig.DurasiMain;
            menang = false;
            hud.SetHp(hp);
            hud.SetCounter(terkumpul);
            hud.SetWaktu(sisaWaktu);

            layarAwal.gameObject.SetActive(false);
            layarAkhir.gameObject.SetActive(false);
            dunia.gameObject.SetActive(true);
            tumpukan.gameObject.SetActive(true);
            BuatTumpukan();
            fase = Fase.Main;
        }

        void Update()
        {
            if (fase == Fase.Awal)
            {
                // Layar awal diam. Tombol "Sentuh untuk memulai" sudah menyatu di gambar artist.
            }
            else if (fase == Fase.Main)
            {
                sisaWaktu -= Time.deltaTime;
                hud.SetWaktu(sisaWaktu);
                if (sisaWaktu <= 0)
                {
                    sisaWaktu = 0;
                    hud.SetWaktu(0);
                    LepasSemuaDrag(); // item yang sedang di-drag dilepas, langsung ke end screen
                    TampilkanAkhir(false);
                }
            }
            else if (fase == Fase.Akhir)
            {
                if (Time.time - waktuAkhirMulai >= GameConfig.DurasiEndScreen) KeLayarAwal();
            }
        }

        // ---- Tumpukan (GDD bagian 9) ----

        // Area tempat item muncul: kiri deck dan di bawah bar atas, tidak menutupi UI dan deck.
        static Rect AreaTumpukan()
        {
            // Batas kiri, bawah, atas mengikuti area biru gelap bg_main (GameConfig.AreaMain).
            float xMin = GameConfig.AreaMain.xMin;
            float xMax = GameConfig.DeckPos.x - GameConfig.DeckUkuran.x / 2f - GameConfig.DropDeckTambahan - 20f;
            float yMin = GameConfig.AreaMain.yMin;
            float yMax = GameConfig.AreaMain.yMax;
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        void BersihkanTumpukan()
        {
            for (int i = tumpukan.childCount - 1; i >= 0; i--) Destroy(tumpukan.GetChild(i).gameObject);
        }

        void BuatTumpukan()
        {
            BersihkanTumpukan();

            // 9 desain threat x 5 salinan = 45 threat, diacak urutannya.
            var urutan = new List<ItemDef>();
            foreach (var t in ItemCatalog.Threat)
                for (int i = 0; i < ItemCatalog.SalinanPerThreat; i++) urutan.Add(t);
            Kocok(urutan);

            // 5 fitur asli disisipkan di lapisan bawah atau tengah, tidak pernah paling atas.
            foreach (var asli in ItemCatalog.FiturAsli)
            {
                int indeks = Random.Range(0, Mathf.Min(GameConfig.IndeksMaksFiturAsli, urutan.Count) + 1);
                urutan.Insert(indeks, asli);
            }

            // Indeks 0 = lapisan paling bawah. Posisi dan rotasi diacak tiap ronde.
            var area = AreaTumpukan();
            foreach (var def in urutan)
            {
                float ukuran = Random.Range(GameConfig.UkuranItemMin, GameConfig.UkuranItemMax);
                float rotasi = Random.Range(-GameConfig.RotasiMaks, GameConfig.RotasiMaks);
                if (!def.asli)
                {
                    // Threat boleh bervariasi rotasi dan ukuran sedikit (warna tidak diubah).
                    rotasi += Random.Range(-GameConfig.RotasiThreatVariasi, GameConfig.RotasiThreatVariasi);
                    ukuran *= 1f + Random.Range(-GameConfig.UkuranThreatVariasi, GameConfig.UkuranThreatVariasi);
                }
                var pos = TitikAcak(area, ukuran, rotasi);
                var item = DraggableItem.Buat(tumpukan, def, pos, ukuran, rotasi);
                item.SaatDilepas = SaatItemDilepas;
            }
        }

        // Titik acak di dalam area, dengan jarak setengah ukuran item dari tepi supaya item tidak keluar area.
        // Rotasi ikut dihitung: item yang miring butuh jarak lebih besar dari tepi.
        static Vector2 TitikAcak(Rect area, float ukuran, float rotasi)
        {
            float s = DraggableItem.SetengahLebar(ukuran, rotasi);
            return new Vector2(Random.Range(area.xMin + s, area.xMax - s), Random.Range(area.yMin + s, area.yMax - s));
        }

        static void Kocok<T>(List<T> daftar)
        {
            for (int i = daftar.Count - 1; i > 0; i--)
            {
                int j = Random.Range(0, i + 1);
                (daftar[i], daftar[j]) = (daftar[j], daftar[i]);
            }
        }

        void LepasSemuaDrag()
        {
            foreach (var item in tumpukan.GetComponentsInChildren<DraggableItem>())
                if (item.SedangDrag) item.PaksaLepas();
        }

        // ---- Aturan drop (GDD bagian 2) ----

        void SaatItemDilepas(DraggableItem item)
        {
            if (fase != Fase.Main) return;

            bool didalamDeck = Hud.AreaDropDeck().Contains(item.Posisi);
            if (!didalamDeck) return; // di luar deck: tetap di posisi terakhir, tanpa penalti

            if (item.Def.asli) ItemBenar(item);
            else ItemSalah(item);
        }

        void ItemBenar(DraggableItem item)
        {
            terkumpul++;
            hud.SetCounter(terkumpul);
            StartCoroutine(item.MasukDeck(Hud.TengahDeck));
            Kilat(new Color(0.2f, 1f, 0.4f, 0.4f));

            if (terkumpul >= GameConfig.TargetFitur)
            {
                fase = Fase.Menunggu; // beri waktu animasi item terakhir selesai
                StartCoroutine(AkhiriSetelah(0.45f, true));
            }
        }

        void ItemSalah(DraggableItem item)
        {
            hp -= GameConfig.DamageThreat;
            hud.SetHp(hp);
            Kilat(new Color(1f, 0.15f, 0.15f, 0.5f));
            Goyang();
            // Threat yang salah masuk mental balik ke tumpukan.
            StartCoroutine(item.MentalKe(TitikAcak(AreaTumpukan(), item.GetComponent<RectTransform>().sizeDelta.x, item.transform.localEulerAngles.z)));

            if (hp <= 0)
            {
                fase = Fase.Menunggu;
                StartCoroutine(AkhiriSetelah(0.5f, false));
            }
        }

        IEnumerator AkhiriSetelah(float detik, bool berhasil)
        {
            yield return new WaitForSeconds(detik);
            LepasSemuaDrag();
            TampilkanAkhir(berhasil);
        }

        // ---- End screen (GDD bagian 3 dan 5) ----

        void TampilkanAkhir(bool berhasil)
        {
            menang = berhasil;
            fase = Fase.Akhir;
            waktuAkhirMulai = Time.time;
            StopAllCoroutines();
            shakeCo = null;
            flashCo = null;
            flashImg.color = new Color(1, 1, 1, 0);
            dunia.anchoredPosition = Vector2.zero;

            dunia.gameObject.SetActive(false);
            if (latarAkhir != null) Destroy(latarAkhir.gameObject);
            latarAkhir = Latar(layarAkhir, menang ? new Color(0.1f, 0.6f, 0.35f) : new Color(0.75f, 0.25f, 0.25f),
                               menang ? new[] { "bg_menang", "end_menang", "end_win", "win" }
                                      : new[] { "bg_kalah", "end_kalah", "end_lose", "lose" });
            latarAkhir.transform.SetSiblingIndex(1); // tepat di atas warna isi samping
            bool adaGambar = latarAkhir.sprite != null;
            latarAkhir.preserveAspect = true;

            string detail = menang
                ? $"Sisa waktu: {sisaWaktu:0.0} detik"
                : $"Fitur terkumpul: {terkumpul}/{GameConfig.TargetFitur}";
            var rtDetail = akhirDetail.rectTransform;
            if (adaGambar)
            {
                // Judul dan pesan sudah ada di gambar: hanya tampilkan baris hasil, di dalam panel putih.
                akhirJudul.text = "";
                akhirPesan.text = "";
                rtDetail.anchoredPosition = GameConfig.PosDetailAkhir;
                // Panel putih di gambar sempit (sekitar 380 px), jadi teks dibuat kecil dan otomatis mengecil kalau kepanjangan.
                rtDetail.sizeDelta = new Vector2(340, 70);
                akhirDetail.fontSize = 34;
                akhirDetail.resizeTextForBestFit = true;
                akhirDetail.resizeTextMinSize = 18;
                akhirDetail.resizeTextMaxSize = 34;
                akhirDetail.color = UIFactory.BiruBca;
            }
            else
            {
                rtDetail.anchoredPosition = new Vector2(0, 60);
                rtDetail.sizeDelta = new Vector2(1500, 120);
                akhirDetail.resizeTextForBestFit = false;
                akhirDetail.fontSize = 64;
                akhirDetail.color = Color.white;
                akhirJudul.text = menang ? "Menang!" : "Kalah";
                akhirPesan.text = menang ? PesanMenang : PesanKalah;
            }
            akhirDetail.text = detail;
            akhirDetail.transform.SetAsLastSibling();
            layarAkhir.gameObject.SetActive(true);
        }

        // ---- Feedback (GDD bagian 10) ----

        void Kilat(Color warna)
        {
            if (flashCo != null) StopCoroutine(flashCo);
            flashCo = StartCoroutine(JalankanKilat(warna));
        }

        IEnumerator JalankanKilat(Color warna)
        {
            const float durasi = 0.35f;
            for (float t = 0; t < durasi; t += Time.deltaTime)
            {
                flashImg.color = new Color(warna.r, warna.g, warna.b, warna.a * (1 - t / durasi));
                yield return null;
            }
            flashImg.color = new Color(1, 1, 1, 0);
        }

        void Goyang()
        {
            if (shakeCo != null) StopCoroutine(shakeCo);
            shakeCo = StartCoroutine(JalankanGoyang());
        }

        IEnumerator JalankanGoyang()
        {
            const float durasi = 0.3f, kekuatan = 22f; // goyang sedikit
            for (float t = 0; t < durasi; t += Time.deltaTime)
            {
                float sisa = 1 - t / durasi;
                dunia.anchoredPosition = Random.insideUnitCircle * kekuatan * sisa;
                yield return null;
            }
            dunia.anchoredPosition = Vector2.zero;
        }
    }
}