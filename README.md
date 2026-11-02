<div align="center">

# 📦 Bapak Kos-ku Ternyata Seorang Boti!
**A 3D Physics-Rush Inspection Micro-Game**

![Unity 6](https://img.shields.io/badge/Unity-6000.0-2D2D2D?style=flat-square&logo=unity&logoColor=white)
![URP](https://img.shields.io/badge/Pipeline-URP-2D2D2D?style=flat-square&logo=unity&logoColor=white)
![Platform](https://img.shields.io/badge/Platform-Windows%20x64-2D2D2D?style=flat-square&logo=windows&logoColor=white)
![Sprint](https://img.shields.io/badge/Dev-48--Hour%20Sprint-2D2D2D?style=flat-square)

---

<!-- Ganti URL di bawah dengan GIF / Screenshot gameplay kamu -->
<img alt="previewGif" src="https://github.com/user-attachments/assets/9a3c2c2c-c401-4a68-9046-102635e8b60b" width="100%"/>


</div>

> **Premis Permainan:**  
> Waktu kamu hanya **90 detik** sebelum Bapak Kos datang memeriksa kebersihan kamar. Amankan semua barang berantakan ke dalam lemari atau buang keluar jendela sebelum inspeksi dimulai!

---

## 🎮 Kontrol Permainan

| Input | Akses / Aksi |
| :--- | :--- |
| <kbd>W</kbd> <kbd>A</kbd> <kbd>S</kbd> <kbd>D</kbd> | Pergerakan Karakter |
| <kbd>Mouse Move</kbd> | Rotasi Kamera (*Smooth Lerp Interpolation*) |
| <kbd>Tahan Klik Kiri</kbd> / <kbd>E</kbd> | Mengambil Barang / Tarik Pintu Lemari |
| <kbd>Klik Kanan</kbd> | Melempar Barang (*Impulse Throw*) |

---

## 🛠️ Sistem & Mekanik Utama

<details>
<summary><b>1. Physics-Based Item Interaction</b></summary>
<br>

* **Hold-to-Grab Mechanics:** Menggunakan perhitungan `Rigidbody.linearVelocity` & `linearDamping` untuk mengambil dan menahan barang secara stabil tanpa benturan *jitter*.
* **Impulse Throw:** Melempar barang secara instan menggunakan dorongan gaya fisika (*ForceMode.Impulse*).
</details>

<details>
<summary><b>2. Normalized Drag & Magnetic Cabinet Doors</b></summary>
<br>

* **Progress Ratio System:** Pergerakan pintu menggunakan nilai rasio terukur (`0.0` sampai `1.0`), menghindari masalah rotasi sudut minus (*negative Euler angle bugs*).
* **Magnetic Snap Zone:** Pintu secara otomatis akan menyedot/merapatkan engsel ke posisi tertutup rapat ($0\%$) atau terbuka penuh ($100\%$) saat berada di dalam rentang *threshold*.
* **Multi-Door Calibration:** Fitur *Invert Rotation* dan *Invert Drag* terpisah untuk memfasilitasi engsel pintu kiri dan kanan.
</details>

<details>
<summary><b>3. Dynamic Safe-Zone Evaluator</b></summary>
<br>

* Overlap deteksi area menggunakan `RoomZone` dan `CabinetZone`.
* **Kondisi Kelayakan:** Barang di dalam area lemari hanya dihitung **AMAN** apabila seluruh pintu lemari berada dalam status **TERTUTUP RAPAT**. Jika pintu terbuka saat inspeksi, barang di dalamnya tetap dihitung sebagai pelanggaran.
</details>

<details>
<summary><b>4. Timeline Inspection Sequence</b></summary>

* Integrasi Unity **Timeline & Playable Director** yang terpicu otomatis saat timer menyentuh `00:00`.
* Menggunakan **Signal Emitter** di akhir *track* sinematik untuk memicu evaluasi poin dan memunculkan UI Ending tanpa perlu *hardcoded coroutine movement*.
</details>

---

## 🏆 Cabang Ending

| Ending | Syarat Kelulusan | Deskripsi |
| :--- | :--- | :--- |
| **Good Ending** | Sisa barang berantakan $\le 1$ item | Kamar dinilai rapi. Uang sewa bulan depan dipotong Rp 50.000. |
| **Bad Ending** | Sisa barang berantakan $> 1$ item | Kamar dinilai seperti kapal pecah. Surat pengusiran diterbitkan. |
| **Secret Ending** | $100\%$ barang dibuang ke luar jendela | Kamar dalam kondisi kosong melompong total. |

---

## 💻 Arsitektur & Spesifikasi Teknis

* **Engine Version:** Unity 6 (6000.0)
* **Rendering Pipeline:** Universal Render Pipeline (URP)
* **Post-Processing Profile:** ACES Tonemapping, Screen Space Ambient Occlusion (SSAO), Color Adjustments, & Subtle Film Grain (`0.3`)
* **Input Handler:** Unity New Input System (`Mouse.current.delta` & `Keyboard.current`)

---

<div align="center">

### 🚀 Unduh & Mainkan

[![Download Standalone Build](https://img.shields.io/badge/Release-Windows%20Build%20(.zip)-2D2D2D?style=for-the-badge&logo=windows&logoColor=white)](https://github.com/kholikdev/Bapak-Kos-ku-ternyata-seorang-Boti-/releases)

Dibuat oleh **[Kholik](https://github.com/kholikdev)** — 48-Hour Weekend Game Sprint Portfolio.

</div>
