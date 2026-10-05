# Cari Fitur BCA

Mock game internal untuk latihan tim. Layar penuh tumpukan 50 item bernuansa BCA: 5 fitur asli dan 45 threat. Pemain menggeser item dengan satu jari dan men-drag 5 fitur asli ke deck hp BCA dalam 30 detik. Threat yang masuk deck mengurangi HP (-20, 5 kali salah = HP habis). Spesifikasi lengkap ada di `docs/GDD_Cari_Fitur_BCA.pdf`.

## Status

| Bagian | Status |
|---|---|
| Kode gameplay (tumpukan, drag, deck, HP, timer, feedback, alur layar) | Sudah ditulis, **belum pernah dibuka atau dikompilasi di Unity**. Hari pertama programmer: buka, perbaiki error kompilasi kalau ada |
| Aset gambar | Diletakkan oleh tim art di `Assets/_Project/Resources/Art/` |
| Suara | Opsional di GDD, belum dibuat |

Selama gambar belum ada, item tampil sebagai kotak biru berisi nama, jadi game tetap bisa dites. Console Unity menulis peringatan `Sprite belum ada: <nama>` sekali per nama.

## Cara menjalankan

1. Buka folder ini lewat Unity Hub (disarankan **Unity 2022.3 LTS atau lebih baru**).
2. Buka scene kosong apa saja, lalu Play. `GameController` membuat semua layar lewat kode.
3. Atur Game view ke 1920 x 1080 (landscape).

Catatan:
- UI memakai `StandaloneInputModule` (Input Manager lama). Kalau project memakai Input System baru, ganti ke `InputSystemUIInputModule` atau set Active Input Handling ke "Both".
- Impor PNG sebagai **Sprite (2D and UI)**, kalau tidak, `Resources.LoadAll<Sprite>` tidak menemukannya.
- Satu font untuk semua UI (GDD bagian 11): taruh file font bernama `ui_font` di `Resources/Fonts`. Kalau tidak ada, dipakai font bawaan Unity.

## Struktur

```
Assets/_Project/
  Scripts/
    Data/       GameConfig (semua angka GDD), ItemCatalog (5 fitur asli + 9 threat)
    Core/       SpriteLibrary (cari gambar berdasarkan nama file)
    UI/         UIFactory, Hud (HP, timer, deck, counter)
    Gameplay/   GameController (alur layar, aturan), DraggableItem (drag satu jari)
  Resources/
    Art/        taruh PNG di sini (subfolder bebas)
    Fonts/      ui_font
docs/           GDD
```

## Aturan GDD yang diterapkan

- Timer 30 detik, HP 100, threat di deck -20 HP, target 5 fitur asli, counter 0/5.
- Geser bebas di tumpukan tanpa penalti. Penalti hanya saat threat dilepas di area deck. Threat yang salah masuk mental balik ke tumpukan. Fitur asli yang dilepas di luar deck tetap di posisi terakhir.
- Posisi dan rotasi diacak tiap ronde. 5 fitur asli disisipkan di lapisan bawah atau tengah (indeks tertinggi 37 dari 49, sudah disimulasikan 20.000 ronde: tidak pernah paling atas).
- Item yang disentuh naik ke lapisan paling atas. Hanya item paling atas di titik sentuh yang bisa di-drag. Threat bervariasi rotasi dan ukuran, warna tidak diubah. Area drop deck lebih besar dari gambarnya.
- Feedback: benar = item mengecil masuk deck, counter naik, kilat hijau. Salah = layar goyang, kilat merah, HP turun. Sisa 10 detik = timer merah.
- End screen 5 detik lalu kembali ke layar awal. Kalau waktu atau HP habis saat item di-drag, item dilepas dan langsung ke end screen.

## Nama file gambar

Gambar dicari dari **awalan** nama, jadi `fitur_01_mybca.png` dan `fitur_01.png` sama-sama terbaca.

| Awalan yang dicari | Dipakai untuk |
|---|---|
| `fitur_01` sampai `fitur_05` | 5 fitur asli (myBCA, BCA mobile, KlikBCA, Flazz, Sakuku) |
| `threat_01` sampai `threat_09` | 9 desain threat (masing-masing dipakai 5 kali) |
| `deck` | Deck hp BCA (400 x 600) |
| `ui_hp`, `ui_timer`, `ui_counter` | UI |
| `bg_main` | Background gameplay |
| `bg_awal` (atau `bg_title`, `layar_awal`, `title`) | Layar awal |
| `bg_menang` (atau `end_menang`, `end_win`, `win`) | End screen menang |
| `bg_kalah` (atau `end_kalah`, `end_lose`, `lose`) | End screen kalah |

**Nama deck, UI, layar awal, dan end screen adalah tebakan**: GDD hanya memberi contoh `ui_hp.png` dan `bg_main.png`. Cocokkan dengan nama file artist yang sebenarnya.

## Yang perlu disesuaikan di Unity

Semua angka ada di `Data/GameConfig.cs`: posisi dan ukuran deck, tinggi bar atas, area tumpukan, ukuran item, kekuatan variasi rotasi. Posisi HP, timer, dan counter ada di `UI/Hud.cs`.

## Di luar scope

Leaderboard, karakter, level kesulitan, input nama, print hasil.
