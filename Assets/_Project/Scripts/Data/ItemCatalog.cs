using System.Collections.Generic;

namespace CariFiturBCA.Data
{
    public class ItemDef
    {
        public string id;       // contoh: fitur_01 atau threat_03
        public string nama;     // dipakai sebagai tulisan cadangan kalau gambar belum ada
        public bool asli;       // true = fitur asli BCA, false = threat
    }

    // GDD bagian 7 (fitur asli) dan 8 (threat). Gambar dicari dari awalan id,
    // jadi fitur_01_mybca.png dan fitur_01_apa_saja.png sama-sama terbaca.
    public static class ItemCatalog
    {
        public const int SalinanPerThreat = 5; // 9 desain x 5 = 45 threat

        public static readonly List<ItemDef> FiturAsli = new List<ItemDef>
        {
            new ItemDef { id = "fitur_01", nama = "myBCA", asli = true },
            new ItemDef { id = "fitur_02", nama = "BCA mobile", asli = true },
            new ItemDef { id = "fitur_03", nama = "KlikBCA", asli = true },
            new ItemDef { id = "fitur_04", nama = "Flazz", asli = true },
            new ItemDef { id = "fitur_05", nama = "Sakuku", asli = true },
        };

        public static readonly List<ItemDef> Threat = new List<ItemDef>
        {
            new ItemDef { id = "threat_01", nama = "Kirim OTP" },
            new ItemDef { id = "threat_02", nama = "Tagihan.apk" },
            new ItemDef { id = "threat_03", nama = "Klik Aku!" },
            new ItemDef { id = "threat_04", nama = "Saldo Gratis" },
            new ItemDef { id = "threat_05", nama = "Pinjol Kilat" },
            new ItemDef { id = "threat_06", nama = "Hadiah!!!" },
            new ItemDef { id = "threat_07", nama = "Minta PIN" },
            new ItemDef { id = "threat_08", nama = "CS Palsu" },
            new ItemDef { id = "threat_09", nama = "Akun Diblokir" },
        };
    }
}
