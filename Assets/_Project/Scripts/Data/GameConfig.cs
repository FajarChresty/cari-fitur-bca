namespace CariFiturBCA.Data
{
    // Semua angka gameplay dari GDD ada di sini, supaya mudah diubah tanpa mencari di kode lain.
    public static class GameConfig
    {
        // GDD bagian 2 dan 4
        public const float DurasiMain = 30f;        // detik, hitung mundur
        public const int HpAwal = 100;
        public const int DamageThreat = 20;         // 5 kali salah = HP habis
        public const int TargetFitur = 5;
        public const float DurasiEndScreen = 5f;

        // GDD bagian 10
        public const float BatasTimerMerah = 10f;   // sisa detik saat timer berubah merah

        // GDD bagian 9: ukuran item di layar sekitar 150 sampai 200 px (patokan 1920 x 1080)
        public const float UkuranItemMin = 150f;
        public const float UkuranItemMax = 200f;
        public const float UkuranThreatVariasi = 0.12f; // threat boleh bervariasi ukuran sedikit (+/- 12%)
        public const float RotasiMaks = 35f;            // derajat, posisi dan rotasi diacak tiap ronde
        public const float RotasiThreatVariasi = 12f;   // tambahan variasi rotasi khusus threat

        // GDD bagian 9: fitur asli di lapisan bawah atau tengah, tidak pernah paling atas.
        // Fitur asli disisipkan di indeks 0 sampai ini (dari total 50 lapisan).
        public const int IndeksMaksFiturAsli = 33;

        // Area drop deck sedikit lebih besar dari gambarnya (piksel tambahan tiap sisi).
        public const float DropDeckTambahan = 60f;

        // MODE LAYAR: true = potret 1080 x 1920 (HP / tablet berdiri), false = landscape 1920 x 1080 (layar booth sesuai GDD).
        // Cukup ubah satu baris ini untuk berpindah mode, lalu samakan Default Orientation di Player Settings.
        public const bool Potret = true;

        // Tata letak (titik 0,0 = tengah layar, Y ke atas)
        public static readonly UnityEngine.Vector2 Layar = Potret ? new UnityEngine.Vector2(1080, 1920) : new UnityEngine.Vector2(1920, 1080);
        public static readonly UnityEngine.Vector2 DeckUkuran = new UnityEngine.Vector2(300, 450); // asli 400 x 600
        // Landscape: kanan atas. Potret: tengah bawah (HP dan timer memenuhi baris atas).
        public static readonly UnityEngine.Vector2 DeckPos = Potret ? new UnityEngine.Vector2(0, -640) : new UnityEngine.Vector2(740, 215);
        public const float TinggiBarAtas = 150f;   // area HP dan timer di atas
        public const float MarginTumpukan = 100f;  // jarak area tumpukan dari tepi layar

        // Area main = bagian BIRU GELAP di bg_main (di dalam garis border, di bawah pita biru muda).
        // Item (threat dan fitur asli) tidak boleh keluar dari kotak ini, baik saat muncul maupun saat di-drag.
        // Koordinat canvas (0,0 = tengah). Ubah angkanya kalau background diganti.
        public static readonly UnityEngine.Rect AreaMain = Potret
            ? UnityEngine.Rect.MinMaxRect(-495f, -905f, 495f, 650f)   // potret
            : UnityEngine.Rect.MinMaxRect(-915f, -420f, 915f, 320f);  // landscape
        // Seberapa "penuh" kotak item dianggap saat dihitung menempel ke tepi (1 = kotak penuh, lebih kecil = boleh lebih mepet).
        public const float KoefTepiItem = 0.85f;
        // Baris hasil di end screen (di dalam panel putih bg_menang / bg_kalah).
        // Diukur dalam piksel GAMBAR (sprite 950 x 1848), lalu dikalikan skala tampilan, jadi benar di potret maupun landscape.
        public static readonly UnityEngine.Vector2 PosDetailPx = new UnityEngine.Vector2(0, -156);
        public static readonly UnityEngine.Vector2 UkuranDetailPx = new UnityEngine.Vector2(582, 120);
        public const float FontDetailPx = 58f;
    }
}