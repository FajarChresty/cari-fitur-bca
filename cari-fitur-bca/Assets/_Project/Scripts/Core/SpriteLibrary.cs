using System.Collections.Generic;
using UnityEngine;

namespace CariFiturBCA.Core
{
    // Mencari sprite di Resources/Art (subfolder mana pun) berdasarkan nama file.
    // Cocok kalau nama file sama persis ATAU diawali nama yang dicari. Contoh: mencari
    // "fitur_01" menemukan fitur_01_mybca. Jadi perbedaan kecil nama di akhir tidak jadi masalah.
    public static class SpriteLibrary
    {
        static Dictionary<string, Sprite> semua;
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();
        static readonly HashSet<string> sudahDilaporkan = new HashSet<string>();

        static void Muat()
        {
            if (semua != null) return;
            semua = new Dictionary<string, Sprite>();
            foreach (var s in Resources.LoadAll<Sprite>("Art"))
                semua[s.name] = s;
        }

        static Sprite Temukan(string namaAtauAwalan)
        {
            Muat();
            if (cache.TryGetValue(namaAtauAwalan, out var hit)) return hit;

            Sprite hasil = null;
            if (semua.TryGetValue(namaAtauAwalan, out var persis)) hasil = persis;
            else
            {
                string terpilih = null;
                foreach (var kv in semua)
                {
                    if (!kv.Key.StartsWith(namaAtauAwalan)) continue;
                    // kalau ada beberapa yang cocok, ambil nama terpendek biar hasilnya stabil
                    if (terpilih == null || kv.Key.Length < terpilih.Length) terpilih = kv.Key;
                }
                if (terpilih != null) hasil = semua[terpilih];
            }
            cache[namaAtauAwalan] = hasil;
            return hasil;
        }

        // Mengembalikan null kalau aset belum ada. Peringatan hanya sekali per nama.
        public static Sprite Get(string namaAtauAwalan)
        {
            var hasil = Temukan(namaAtauAwalan);
            if (hasil == null && sudahDilaporkan.Add(namaAtauAwalan))
                Debug.LogWarning($"[SpriteLibrary] Sprite belum ada: {namaAtauAwalan}");
            return hasil;
        }

        // Mencoba beberapa kemungkinan nama (kalau penamaan artist belum pasti). Memakai yang pertama ketemu.
        public static Sprite GetAny(params string[] kandidat)
        {
            foreach (var k in kandidat)
            {
                var s = Temukan(k);
                if (s != null) return s;
            }
            if (sudahDilaporkan.Add(kandidat[0]))
                Debug.LogWarning($"[SpriteLibrary] Sprite belum ada (dicoba: {string.Join(", ", kandidat)})");
            return null;
        }

        public static void Bersihkan()
        {
            semua = null;
            cache.Clear();
            sudahDilaporkan.Clear();
        }
    }
}
