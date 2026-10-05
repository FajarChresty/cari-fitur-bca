using System;
using System.Collections;
using CariFiturBCA.Core;
using CariFiturBCA.Data;
using CariFiturBCA.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CariFiturBCA.Gameplay
{
    // Satu item di tumpukan (fitur asli atau threat). Hanya bisa digeser satu per satu:
    // karena item dirender bertumpuk, raycast UI otomatis hanya mengenai item paling atas
    // di titik sentuh (GDD bagian 9). Item yang disentuh naik ke lapisan paling atas.
    [RequireComponent(typeof(Image))]
    public class DraggableItem : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        public ItemDef Def { get; private set; }
        public bool SedangDrag { get; private set; }

        // Dipanggil saat item dilepas oleh pemain. Controller yang menentukan benar atau salah.
        public Action<DraggableItem> SaatDilepas;

        RectTransform rt;
        RectTransform areaLokal; // induk, dipakai untuk mengubah posisi sentuh ke koordinat canvas
        Image img;
        Vector2 selisihGenggam;
        bool bisaDigeser = true;
        Vector2 batasLayar;

        public static DraggableItem Buat(Transform induk, ItemDef def, Vector2 pos, float ukuran, float rotasi)
        {
            var rt = UIFactory.Kotak(induk, def.id, pos, new Vector2(ukuran, ukuran));
            var img = rt.gameObject.AddComponent<Image>();
            var item = rt.gameObject.AddComponent<DraggableItem>();
            item.Def = def;
            item.rt = rt;
            item.img = img;
            item.areaLokal = (RectTransform)induk;
            item.batasLayar = GameConfig.Layar / 2f;

            var sprite = SpriteLibrary.Get(def.id);
            if (sprite != null) { img.sprite = sprite; img.color = Color.white; img.preserveAspect = true; }
            else
            {
                // Gambar belum ada: kotak biru + nama, supaya bisa dites. Warna sama untuk semua item.
                img.color = new Color(0.1f, 0.45f, 0.8f);
                UIFactory.Teks(rt, def.nama, Vector2.zero, new Vector2(ukuran - 16, ukuran - 16), 26,
                                  TextAnchor.MiddleCenter, Color.white);
            }
            img.raycastTarget = true;
            rt.localRotation = Quaternion.Euler(0, 0, rotasi);
            return item;
        }

        // ---- Input satu jari ----

        public void OnPointerDown(PointerEventData e)
        {
            if (!bisaDigeser) return;
            transform.SetAsLastSibling(); // item yang disentuh naik ke lapisan paling atas
            SedangDrag = true;
            selisihGenggam = rt.anchoredPosition - KePosisiLokal(e);
        }

        public void OnDrag(PointerEventData e)
        {
            if (!SedangDrag || !bisaDigeser) return;
            var target = KePosisiLokal(e) + selisihGenggam;
            float sisi = rt.sizeDelta.x / 2f;
            target.x = Mathf.Clamp(target.x, -batasLayar.x + sisi, batasLayar.x - sisi);
            target.y = Mathf.Clamp(target.y, -batasLayar.y + sisi, batasLayar.y - sisi);
            rt.anchoredPosition = target;
        }

        public void OnPointerUp(PointerEventData e)
        {
            if (!SedangDrag) return;
            SedangDrag = false;
            if (bisaDigeser) SaatDilepas?.Invoke(this);
        }

        // Dipakai saat waktu atau HP habis ketika item sedang di-drag: item dilepas tanpa penilaian.
        public void PaksaLepas()
        {
            SedangDrag = false;
        }

        public void Kunci() { bisaDigeser = false; img.raycastTarget = false; }

        Vector2 KePosisiLokal(PointerEventData e)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(areaLokal, e.position, null, out var lokal);
            return lokal;
        }

        public Vector2 Posisi => rt.anchoredPosition;

        // ---- Animasi ----

        // Benar: mengecil sambil masuk ke deck, lalu hilang.
        public IEnumerator MasukDeck(Vector2 tujuan, float durasi = 0.3f)
        {
            Kunci();
            Vector2 awal = rt.anchoredPosition;
            Vector3 skalaAwal = rt.localScale;
            for (float t = 0; t < durasi; t += Time.deltaTime)
            {
                float p = Mathf.SmoothStep(0, 1, t / durasi);
                rt.anchoredPosition = Vector2.Lerp(awal, tujuan, p);
                rt.localScale = Vector3.Lerp(skalaAwal, Vector3.zero, p);
                yield return null;
            }
            Destroy(gameObject);
        }

        // Salah: mental balik ke tumpukan, lalu bisa digeser lagi.
        public IEnumerator MentalKe(Vector2 tujuan, float durasi = 0.4f)
        {
            Kunci();
            Vector2 awal = rt.anchoredPosition;
            for (float t = 0; t < durasi; t += Time.deltaTime)
            {
                float p = t / durasi;
                float lompat = Mathf.Sin(p * Mathf.PI) * 80f; // lengkung kecil supaya terasa mental
                rt.anchoredPosition = Vector2.Lerp(awal, tujuan, 1 - (1 - p) * (1 - p)) + Vector2.up * lompat;
                yield return null;
            }
            rt.anchoredPosition = tujuan;
            bisaDigeser = true;
            img.raycastTarget = true;
        }
    }
}
