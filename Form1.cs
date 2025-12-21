using Npgsql;
using System;
using System.Data;
using System.Windows.Forms;

namespace VTYS_Kutuphane
{
    public partial class Form1 : Form
    {
        // Seçili kayıtların ID'lerini tutmak için
        private int seciliKitapID = -1;
        private int seciliUyeID = -1;
        private int seciliIslemID = -1;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                // Veritabanı bağlantısını test et
                if (!VeritabaniBaglantisi.TestConnection())
                {
                    MessageBox.Show("Veritabanına bağlanılamadı! Lütfen bağlantı ayarlarını kontrol edin.",
                        "Bağlantı Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Tüm verileri yükle
                KitaplariYukle();
                UyeleriYukle();
                OduncIslemleriniYukle();

                // ComboBox'ları doldur
                YazarlariYukle();
                KategorileriYukle();
                YayinevleriniYukle();
                RaflariYukle();
                DilleriYukle();
                IlceleriYukle();
                UyeleriComboBoxaYukle();
                KitaplariComboBoxaYukle();

                // Yönetim sekmesi DataGridView'larını yükle
                YazarlarListesiniYukle();
                KategorilerListesiniYukle();
                YayinevleriListesiniYukle();
                DillerListesiniYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Veriler yüklenirken hata oluştu: " + ex.Message,
                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #region Veri Yükleme Metodları

        private void KitaplariYukle()
        {
            string query = @"
                SELECT k.KitapID AS kitapid, k.KitapAdi AS kitapadi, k.SayfaSayisi AS sayfasayisi, k.ISBN AS isbn, 
                       y.YazarAdi || ' ' || y.YazarSoyadi AS yazar,
                       kat.KategoriAdi AS kategoriadi, yay.YayineviAdi AS yayineviadi, 
                       d.DolapKodu || ' - ' || r.RafNumarasi AS konum,
                       dil.DilAdi AS diladi
                FROM Kitaplar k
                JOIN Yazarlar y ON k.YazarID = y.YazarID
                JOIN Kategoriler kat ON k.KategoriID = kat.KategoriID
                JOIN Yayinevleri yay ON k.YayineviID = yay.YayineviID
                JOIN Raflar r ON k.RafID = r.RafID
                JOIN Dolaplar d ON r.DolapID = d.DolapID
                JOIN Diller dil ON k.DilID = dil.DilID
                ORDER BY k.KitapID";
            dgvKitaplar.DataSource = VeritabaniBaglantisi.ExecuteQuery(query);
        }

        private void UyeleriYukle()
        {
            string query = @"
                SELECT u.UyeID AS uyeid, k.Ad AS ad, k.Soyad AS soyad, k.TC AS tc, k.Tel AS tel, 
                       i.IlceAdi AS ilceadi, s.SehirAdi AS sehiradi, u.UyelikTarihi AS uyeliktarihi, u.Puan AS puan
                FROM Uyeler u
                JOIN Kisiler k ON u.UyeID = k.KisiID
                LEFT JOIN Ilceler i ON k.IlceID = i.IlceID
                LEFT JOIN Sehirler s ON i.SehirPlaka = s.SehirPlaka
                ORDER BY u.UyeID";
            dgvUyeler.DataSource = VeritabaniBaglantisi.ExecuteQuery(query);
        }

        private void OduncIslemleriniYukle()
        {
            string query = @"
                SELECT o.IslemID AS islemid, 
                       k.Ad || ' ' || k.Soyad AS uyeadi,
                       kit.KitapAdi AS kitapadi,
                       o.VerilisTarihi AS verilstarihi, 
                       o.TeslimTarihi AS teslimtarihi,
                       CASE WHEN o.TeslimTarihi IS NULL THEN 'Teslim Edilmedi' ELSE 'Teslim Edildi' END AS durum,
                       COALESCE(c.CezaTutari, 0) AS cezatutari,
                       COALESCE(c.OdendiMi, false) AS odendimi
                FROM OduncIslemleri o
                JOIN Uyeler u ON o.UyeID = u.UyeID
                JOIN Kisiler k ON u.UyeID = k.KisiID
                JOIN Kitaplar kit ON o.KitapID = kit.KitapID
                LEFT JOIN Cezalar c ON o.IslemID = c.IslemID
                ORDER BY o.IslemID DESC";
            dgvOduncListesi.DataSource = VeritabaniBaglantisi.ExecuteQuery(query);
        }

        private void YazarlariYukle()
        {
            string query = "SELECT YazarID AS yazarid, YazarAdi || ' ' || YazarSoyadi AS yazaradsoyad FROM Yazarlar ORDER BY YazarAdi";
            DataTable dt = VeritabaniBaglantisi.ExecuteQuery(query);
            comboBoxYazar.DisplayMember = "yazaradsoyad";
            comboBoxYazar.ValueMember = "yazarid";
            comboBoxYazar.DataSource = dt;
            if (dt.Rows.Count > 0) comboBoxYazar.SelectedIndex = -1;
        }

        private void KategorileriYukle()
        {
            string query = "SELECT KategoriID AS kategoriid, KategoriAdi AS kategoriadi FROM Kategoriler ORDER BY KategoriAdi";
            DataTable dt = VeritabaniBaglantisi.ExecuteQuery(query);
            comboBoxKategori.DisplayMember = "kategoriadi";
            comboBoxKategori.ValueMember = "kategoriid";
            comboBoxKategori.DataSource = dt;
            if (dt.Rows.Count > 0) comboBoxKategori.SelectedIndex = -1;
        }

        private void YayinevleriniYukle()
        {
            string query = "SELECT YayineviID AS yayineviid, YayineviAdi AS yayineviadi FROM Yayinevleri ORDER BY YayineviAdi";
            DataTable dt = VeritabaniBaglantisi.ExecuteQuery(query);
            comboBoxYayinevi.DisplayMember = "yayineviadi";
            comboBoxYayinevi.ValueMember = "yayineviid";
            comboBoxYayinevi.DataSource = dt;
            if (dt.Rows.Count > 0) comboBoxYayinevi.SelectedIndex = -1;
        }

        private void RaflariYukle()
        {
            string query = @"
                SELECT r.RafID AS rafid, d.DolapKodu || ' - ' || r.RafNumarasi AS rafkonum 
                FROM Raflar r 
                JOIN Dolaplar d ON r.DolapID = d.DolapID 
                ORDER BY d.DolapKodu, r.RafNumarasi";
            DataTable dt = VeritabaniBaglantisi.ExecuteQuery(query);
            comboBoxRaf.DisplayMember = "rafkonum";
            comboBoxRaf.ValueMember = "rafid";
            comboBoxRaf.DataSource = dt;
            if (dt.Rows.Count > 0) comboBoxRaf.SelectedIndex = -1;
        }

        private void DilleriYukle()
        {
            string query = "SELECT DilID AS dilid, DilAdi AS diladi FROM Diller ORDER BY DilAdi";
            DataTable dt = VeritabaniBaglantisi.ExecuteQuery(query);
            comboBoxDil.DisplayMember = "diladi";
            comboBoxDil.ValueMember = "dilid";
            comboBoxDil.DataSource = dt;
            if (dt.Rows.Count > 0) comboBoxDil.SelectedIndex = -1;
        }

        private void IlceleriYukle()
        {
            string query = @"
                SELECT i.IlceID AS ilceid, i.IlceAdi || ' / ' || s.SehirAdi AS ilcesehir 
                FROM Ilceler i 
                JOIN Sehirler s ON i.SehirPlaka = s.SehirPlaka 
                ORDER BY s.SehirAdi, i.IlceAdi";
            DataTable dt = VeritabaniBaglantisi.ExecuteQuery(query);
            comboBoxIlce.DisplayMember = "ilcesehir";
            comboBoxIlce.ValueMember = "ilceid";
            comboBoxIlce.DataSource = dt;
            if (dt.Rows.Count > 0) comboBoxIlce.SelectedIndex = -1;
        }

        private void UyeleriComboBoxaYukle()
        {
            string query = @"
                SELECT u.UyeID AS uyeid, k.Ad || ' ' || k.Soyad AS uyeadsoyad 
                FROM Uyeler u 
                JOIN Kisiler k ON u.UyeID = k.KisiID 
                ORDER BY k.Ad";
            DataTable dt = VeritabaniBaglantisi.ExecuteQuery(query);
            comboBoxUyeAdiOduncIade.DisplayMember = "uyeadsoyad";
            comboBoxUyeAdiOduncIade.ValueMember = "uyeid";
            comboBoxUyeAdiOduncIade.DataSource = dt;
            if (dt.Rows.Count > 0) comboBoxUyeAdiOduncIade.SelectedIndex = -1;
        }

        private void KitaplariComboBoxaYukle()
        {
            string query = "SELECT KitapID AS kitapid, KitapAdi AS kitapadi FROM Kitaplar ORDER BY KitapAdi";
            DataTable dt = VeritabaniBaglantisi.ExecuteQuery(query);
            comboBoxKitapAdiOduncIade.DisplayMember = "kitapadi";
            comboBoxKitapAdiOduncIade.ValueMember = "kitapid";
            comboBoxKitapAdiOduncIade.DataSource = dt;
            if (dt.Rows.Count > 0) comboBoxKitapAdiOduncIade.SelectedIndex = -1;
        }

        #endregion

        #region Kitap İşlemleri

        private void buttonKitapEkle_Click(object sender, EventArgs e)
        {
            try
            {
                // Validasyon
                if (string.IsNullOrWhiteSpace(txtKitapAdi.Text))
                {
                    MessageBox.Show("Kitap adı boş olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtISBN.Text) || txtISBN.Text.Length != 13)
                {
                    MessageBox.Show("ISBN 13 haneli olmalıdır!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (comboBoxYazar.SelectedIndex == -1 || comboBoxKategori.SelectedIndex == -1 ||
                    comboBoxYayinevi.SelectedIndex == -1 || comboBoxRaf.SelectedIndex == -1 ||
                    comboBoxDil.SelectedIndex == -1)
                {
                    MessageBox.Show("Lütfen tüm seçimleri yapınız!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int sayfaSayisi = 0;
                if (!string.IsNullOrWhiteSpace(txtSayfaSayisi.Text))
                {
                    if (!int.TryParse(txtSayfaSayisi.Text, out sayfaSayisi))
                    {
                        MessageBox.Show("Sayfa sayısı geçerli bir sayı olmalıdır!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }

                // sp_KitapEkle stored procedure'ünü çağır
                string procedureCall = "sp_KitapEkle(@p_KitapAdi, @p_SayfaSayisi, @p_ISBN, @p_YazarID, @p_KategoriID, @p_YayineviID, @p_RafID, @p_DilID)";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@p_KitapAdi", txtKitapAdi.Text.Trim()),
                    new NpgsqlParameter("@p_SayfaSayisi", sayfaSayisi),
                    new NpgsqlParameter("@p_ISBN", txtISBN.Text.Trim()),
                    new NpgsqlParameter("@p_YazarID", (int)comboBoxYazar.SelectedValue),
                    new NpgsqlParameter("@p_KategoriID", (int)comboBoxKategori.SelectedValue),
                    new NpgsqlParameter("@p_YayineviID", (int)comboBoxYayinevi.SelectedValue),
                    new NpgsqlParameter("@p_RafID", (int)comboBoxRaf.SelectedValue),
                    new NpgsqlParameter("@p_DilID", (int)comboBoxDil.SelectedValue)
                };

                VeritabaniBaglantisi.ExecuteProcedure(procedureCall, parameters);

                MessageBox.Show("Kitap başarıyla eklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Formu temizle ve verileri yenile
                KitapFormunuTemizle();
                KitaplariYukle();
                KitaplariComboBoxaYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kitap eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvKitaplar_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvKitaplar.Rows[e.RowIndex];

                    // Boş satır kontrolü (yeni kayıt satırı)
                    if (row.Cells["kitapid"].Value == null || row.Cells["kitapid"].Value == DBNull.Value)
                    {
                        return; // Boş satıra tıklandıysa işlem yapma
                    }

                    seciliKitapID = Convert.ToInt32(row.Cells["kitapid"].Value);

                    txtKitapAdi.Text = row.Cells["kitapadi"].Value?.ToString() ?? "";
                    txtSayfaSayisi.Text = row.Cells["sayfasayisi"].Value?.ToString() ?? "";
                    txtISBN.Text = row.Cells["isbn"].Value?.ToString() ?? "";

                    // ComboBox'ları seçmek için veritabanından kitap detaylarını al
                    string query = "SELECT YazarID AS yazarid, KategoriID AS kategoriid, YayineviID AS yayineviid, RafID AS rafid, DilID AS dilid FROM Kitaplar WHERE KitapID = @id";
                    DataTable dt = VeritabaniBaglantisi.ExecuteQuery(query, new NpgsqlParameter("@id", seciliKitapID));

                    if (dt.Rows.Count > 0)
                    {
                        comboBoxYazar.SelectedValue = dt.Rows[0]["yazarid"];
                        comboBoxKategori.SelectedValue = dt.Rows[0]["kategoriid"];
                        comboBoxYayinevi.SelectedValue = dt.Rows[0]["yayineviid"];
                        comboBoxRaf.SelectedValue = dt.Rows[0]["rafid"];
                        comboBoxDil.SelectedValue = dt.Rows[0]["dilid"];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Satır seçilirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonKitapGuncelle_Click(object sender, EventArgs e)
        {
            try
            {
                if (seciliKitapID == -1)
                {
                    MessageBox.Show("Lütfen güncellenecek kitabı seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validasyon
                if (string.IsNullOrWhiteSpace(txtKitapAdi.Text))
                {
                    MessageBox.Show("Kitap adı boş olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int sayfaSayisi = 0;
                int.TryParse(txtSayfaSayisi.Text, out sayfaSayisi);

                string query = @"
                    UPDATE Kitaplar SET 
                        KitapAdi = @kitapAdi, 
                        SayfaSayisi = @sayfaSayisi, 
                        ISBN = @isbn,
                        YazarID = @yazarID, 
                        KategoriID = @kategoriID, 
                        YayineviID = @yayineviID,
                        RafID = @rafID, 
                        DilID = @dilID
                    WHERE KitapID = @kitapID";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@kitapAdi", txtKitapAdi.Text.Trim()),
                    new NpgsqlParameter("@sayfaSayisi", sayfaSayisi),
                    new NpgsqlParameter("@isbn", txtISBN.Text.Trim()),
                    new NpgsqlParameter("@yazarID", (int)comboBoxYazar.SelectedValue),
                    new NpgsqlParameter("@kategoriID", (int)comboBoxKategori.SelectedValue),
                    new NpgsqlParameter("@yayineviID", (int)comboBoxYayinevi.SelectedValue),
                    new NpgsqlParameter("@rafID", (int)comboBoxRaf.SelectedValue),
                    new NpgsqlParameter("@dilID", (int)comboBoxDil.SelectedValue),
                    new NpgsqlParameter("@kitapID", seciliKitapID)
                };

                VeritabaniBaglantisi.ExecuteNonQuery(query, parameters);
                MessageBox.Show("Kitap başarıyla güncellendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                KitapFormunuTemizle();
                KitaplariYukle();
                KitaplariComboBoxaYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kitap güncellenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonKitapSil_Click(object sender, EventArgs e)
        {
            try
            {
                if (seciliKitapID == -1)
                {
                    MessageBox.Show("Lütfen silinecek kitabı seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // İade edilmemiş ödünç işlemi kontrolü
                string checkQuery = "SELECT COUNT(*) FROM OduncIslemleri WHERE KitapID = @id AND TeslimTarihi IS NULL";
                DataTable dt = VeritabaniBaglantisi.ExecuteQuery(checkQuery, new NpgsqlParameter("@id", seciliKitapID));

                if (Convert.ToInt32(dt.Rows[0][0]) > 0)
                {
                    MessageBox.Show("Bu kitap şu anda ödünç verilmiş durumda. Önce iade edilmesi gerekiyor!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Kitabın geçmiş ödünç kaydı var mı kontrol et
                string historyQuery = "SELECT COUNT(*) FROM OduncIslemleri WHERE KitapID = @id";
                DataTable dtHistory = VeritabaniBaglantisi.ExecuteQuery(historyQuery, new NpgsqlParameter("@id", seciliKitapID));
                int oduncSayisi = Convert.ToInt32(dtHistory.Rows[0][0]);

                string confirmMessage = "Bu kitabı silmek istediğinizden emin misiniz?";
                if (oduncSayisi > 0)
                {
                    confirmMessage += $"\n\nNot: Bu kitaba ait {oduncSayisi} adet geçmiş ödünç kaydı da silinecektir.";
                }

                DialogResult result = MessageBox.Show(confirmMessage,
                    "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Önce bu kitaba ait ödünç işlemlerinin cezalarını sil
                    string deleteCezaQuery = @"
                        DELETE FROM Cezalar 
                        WHERE IslemID IN (SELECT IslemID FROM OduncIslemleri WHERE KitapID = @id)";
                    VeritabaniBaglantisi.ExecuteNonQuery(deleteCezaQuery, new NpgsqlParameter("@id", seciliKitapID));

                    // Sonra ödünç işlemlerini sil
                    string deleteOduncQuery = "DELETE FROM OduncIslemleri WHERE KitapID = @id";
                    VeritabaniBaglantisi.ExecuteNonQuery(deleteOduncQuery, new NpgsqlParameter("@id", seciliKitapID));

                    // En son kitabı sil
                    string deleteKitapQuery = "DELETE FROM Kitaplar WHERE KitapID = @id";
                    VeritabaniBaglantisi.ExecuteNonQuery(deleteKitapQuery, new NpgsqlParameter("@id", seciliKitapID));

                    MessageBox.Show("Kitap başarıyla silindi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    KitapFormunuTemizle();
                    KitaplariYukle();
                    KitaplariComboBoxaYukle();
                    OduncIslemleriniYukle(); // Ödünç listesini de güncelle
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kitap silinirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonKitapAra_Click(object sender, EventArgs e)
        {
            try
            {
                string aramaMetni = txtKitapAra.Text.Trim();

                if (string.IsNullOrWhiteSpace(aramaMetni))
                {
                    KitaplariYukle();
                    return;
                }

                string query = @"
                    SELECT k.KitapID AS kitapid, k.KitapAdi AS kitapadi, k.SayfaSayisi AS sayfasayisi, k.ISBN AS isbn, 
                           y.YazarAdi || ' ' || y.YazarSoyadi AS yazar,
                           kat.KategoriAdi AS kategoriadi, yay.YayineviAdi AS yayineviadi, 
                           d.DolapKodu || ' - ' || r.RafNumarasi AS konum,
                           dil.DilAdi AS diladi
                    FROM Kitaplar k
                    JOIN Yazarlar y ON k.YazarID = y.YazarID
                    JOIN Kategoriler kat ON k.KategoriID = kat.KategoriID
                    JOIN Yayinevleri yay ON k.YayineviID = yay.YayineviID
                    JOIN Raflar r ON k.RafID = r.RafID
                    JOIN Dolaplar d ON r.DolapID = d.DolapID
                    JOIN Diller dil ON k.DilID = dil.DilID
                    WHERE LOWER(k.KitapAdi) LIKE LOWER(@arama) 
                       OR k.ISBN LIKE @arama
                       OR LOWER(y.YazarAdi || ' ' || y.YazarSoyadi) LIKE LOWER(@arama)
                    ORDER BY k.KitapID";

                dgvKitaplar.DataSource = VeritabaniBaglantisi.ExecuteQuery(query,
                    new NpgsqlParameter("@arama", "%" + aramaMetni + "%"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Arama sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void KitapFormunuTemizle()
        {
            txtKitapAdi.Clear();
            txtSayfaSayisi.Clear();
            txtISBN.Clear();
            comboBoxYazar.SelectedIndex = -1;
            comboBoxKategori.SelectedIndex = -1;
            comboBoxYayinevi.SelectedIndex = -1;
            comboBoxRaf.SelectedIndex = -1;
            comboBoxDil.SelectedIndex = -1;
            seciliKitapID = -1;
        }

        #endregion

        #region Üye İşlemleri

        private void buttonUyeEkle_Click(object sender, EventArgs e)
        {
            try
            {
                // Validasyon
                if (string.IsNullOrWhiteSpace(txtUyeAdi.Text) || string.IsNullOrWhiteSpace(txtUyeSoyadi.Text))
                {
                    MessageBox.Show("Ad ve soyad boş olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtUyeTC.Text) || txtUyeTC.Text.Length != 11)
                {
                    MessageBox.Show("TC Kimlik numarası 11 haneli olmalıdır!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (comboBoxIlce.SelectedIndex == -1)
                {
                    MessageBox.Show("Lütfen ilçe seçiniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // sp_UyeEkle stored procedure'ünü çağır
                string procedureCall = "sp_UyeEkle(@p_Ad, @p_Soyad, @p_TC, @p_Tel, @p_IlceID)";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@p_Ad", txtUyeAdi.Text.Trim()),
                    new NpgsqlParameter("@p_Soyad", txtUyeSoyadi.Text.Trim()),
                    new NpgsqlParameter("@p_TC", txtUyeTC.Text.Trim()),
                    new NpgsqlParameter("@p_Tel", txtUyeTel.Text.Trim()),
                    new NpgsqlParameter("@p_IlceID", (int)comboBoxIlce.SelectedValue)
                };

                VeritabaniBaglantisi.ExecuteProcedure(procedureCall, parameters);

                MessageBox.Show("Üye başarıyla eklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                UyeFormunuTemizle();
                UyeleriYukle();
                UyeleriComboBoxaYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Üye eklenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dgvUyeler_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                if (e.RowIndex >= 0)
                {
                    DataGridViewRow row = dgvUyeler.Rows[e.RowIndex];

                    // Boş satır kontrolü (yeni kayıt satırı)
                    if (row.Cells["uyeid"].Value == null || row.Cells["uyeid"].Value == DBNull.Value)
                    {
                        return; // Boş satıra tıklandıysa işlem yapma
                    }

                    seciliUyeID = Convert.ToInt32(row.Cells["uyeid"].Value);

                    txtUyeAdi.Text = row.Cells["ad"].Value?.ToString() ?? "";
                    txtUyeSoyadi.Text = row.Cells["soyad"].Value?.ToString() ?? "";
                    txtUyeTC.Text = row.Cells["tc"].Value?.ToString() ?? "";
                    txtUyeTel.Text = row.Cells["tel"].Value?.ToString() ?? "";

                    // İlçe seçimi için veritabanından detay al
                    string query = "SELECT IlceID AS ilceid FROM Kisiler WHERE KisiID = @id";
                    DataTable dt = VeritabaniBaglantisi.ExecuteQuery(query, new NpgsqlParameter("@id", seciliUyeID));

                    if (dt.Rows.Count > 0 && dt.Rows[0]["ilceid"] != DBNull.Value)
                    {
                        comboBoxIlce.SelectedValue = dt.Rows[0]["ilceid"];
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Satır seçilirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonUyeGuncelle_Click(object sender, EventArgs e)
        {
            try
            {
                if (seciliUyeID == -1)
                {
                    MessageBox.Show("Lütfen güncellenecek üyeyi seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Validasyon
                if (string.IsNullOrWhiteSpace(txtUyeAdi.Text) || string.IsNullOrWhiteSpace(txtUyeSoyadi.Text))
                {
                    MessageBox.Show("Ad ve soyad boş olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Kisiler tablosunu güncelle (kalıtım yapısı nedeniyle)
                string query = @"
                    UPDATE Kisiler SET 
                        Ad = @ad, 
                        Soyad = @soyad, 
                        TC = @tc,
                        Tel = @tel, 
                        IlceID = @ilceID
                    WHERE KisiID = @kisiID";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@ad", txtUyeAdi.Text.Trim()),
                    new NpgsqlParameter("@soyad", txtUyeSoyadi.Text.Trim()),
                    new NpgsqlParameter("@tc", txtUyeTC.Text.Trim()),
                    new NpgsqlParameter("@tel", txtUyeTel.Text.Trim()),
                    new NpgsqlParameter("@ilceID", comboBoxIlce.SelectedValue ?? (object)DBNull.Value),
                    new NpgsqlParameter("@kisiID", seciliUyeID)
                };

                VeritabaniBaglantisi.ExecuteNonQuery(query, parameters);
                MessageBox.Show("Üye başarıyla güncellendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                UyeFormunuTemizle();
                UyeleriYukle();
                UyeleriComboBoxaYukle();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Üye güncellenirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonUyeSil_Click(object sender, EventArgs e)
        {
            try
            {
                if (seciliUyeID == -1)
                {
                    MessageBox.Show("Lütfen silinecek üyeyi seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult result = MessageBox.Show("Bu üyeyi silmek istediğinizden emin misiniz?",
                    "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Trigger (trg_uye_sil_kontrol) teslim edilmemiş kitap kontrolünü yapacak
                    string query = "DELETE FROM Uyeler WHERE UyeID = @id";
                    VeritabaniBaglantisi.ExecuteNonQuery(query, new NpgsqlParameter("@id", seciliUyeID));

                    // Kisiler tablosundan da sil (CASCADE ile otomatik silinir ama emin olmak için)
                    query = "DELETE FROM Kisiler WHERE KisiID = @id";
                    VeritabaniBaglantisi.ExecuteNonQuery(query, new NpgsqlParameter("@id", seciliUyeID));

                    MessageBox.Show("Üye başarıyla silindi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    UyeFormunuTemizle();
                    UyeleriYukle();
                    UyeleriComboBoxaYukle();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Üye silinirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonUyeAra_Click(object sender, EventArgs e)
        {
            try
            {
                string aramaMetni = txtUyeAra.Text.Trim();

                if (string.IsNullOrWhiteSpace(aramaMetni))
                {
                    UyeleriYukle();
                    return;
                }

                string query = @"
                    SELECT u.UyeID AS uyeid, k.Ad AS ad, k.Soyad AS soyad, k.TC AS tc, k.Tel AS tel, 
                           i.IlceAdi AS ilceadi, s.SehirAdi AS sehiradi, u.UyelikTarihi AS uyeliktarihi, u.Puan AS puan
                    FROM Uyeler u
                    JOIN Kisiler k ON u.UyeID = k.KisiID
                    LEFT JOIN Ilceler i ON k.IlceID = i.IlceID
                    LEFT JOIN Sehirler s ON i.SehirPlaka = s.SehirPlaka
                    WHERE LOWER(k.Ad) LIKE LOWER(@arama) 
                       OR LOWER(k.Soyad) LIKE LOWER(@arama)
                       OR k.TC LIKE @arama
                       OR LOWER(k.Ad || ' ' || k.Soyad) LIKE LOWER(@arama)
                    ORDER BY u.UyeID";

                dgvUyeler.DataSource = VeritabaniBaglantisi.ExecuteQuery(query,
                    new NpgsqlParameter("@arama", "%" + aramaMetni + "%"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Arama sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void UyeFormunuTemizle()
        {
            txtUyeAdi.Clear();
            txtUyeSoyadi.Clear();
            txtUyeTC.Clear();
            txtUyeTel.Clear();
            comboBoxIlce.SelectedIndex = -1;
            seciliUyeID = -1;
        }

        #endregion

        #region Ödünç ve İade İşlemleri

        private void buttonOduncVer_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxUyeAdiOduncIade.SelectedIndex == -1)
                {
                    MessageBox.Show("Lütfen üye seçiniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (comboBoxKitapAdiOduncIade.SelectedIndex == -1)
                {
                    MessageBox.Show("Lütfen kitap seçiniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int uyeID = (int)comboBoxUyeAdiOduncIade.SelectedValue;
                int kitapID = (int)comboBoxKitapAdiOduncIade.SelectedValue;

                // sp_OduncVer stored procedure'ünü çağır
                // Bu procedure kitabın müsait olup olmadığını kontrol eder
                string procedureCall = "sp_OduncVer(@p_UyeID, @p_KitapID)";

                NpgsqlParameter[] parameters = new NpgsqlParameter[]
                {
                    new NpgsqlParameter("@p_UyeID", uyeID),
                    new NpgsqlParameter("@p_KitapID", kitapID)
                };

                VeritabaniBaglantisi.ExecuteProcedure(procedureCall, parameters);

                MessageBox.Show("Kitap başarıyla ödünç verildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                OduncIslemleriniYukle();
                comboBoxUyeAdiOduncIade.SelectedIndex = -1;
                comboBoxKitapAdiOduncIade.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ödünç verme işleminde hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonIadeEt_Click(object sender, EventArgs e)
        {
            try
            {
                if (comboBoxUyeAdiOduncIade.SelectedIndex == -1 || comboBoxKitapAdiOduncIade.SelectedIndex == -1)
                {
                    MessageBox.Show("Lütfen üye ve kitap seçiniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int uyeID = (int)comboBoxUyeAdiOduncIade.SelectedValue;
                int kitapID = (int)comboBoxKitapAdiOduncIade.SelectedValue;

                // Bu kitabın bu üyede olup olmadığını kontrol et
                string checkQuery = @"
                    SELECT IslemID AS islemid FROM OduncIslemleri 
                    WHERE UyeID = @uyeID AND KitapID = @kitapID AND TeslimTarihi IS NULL";

                DataTable dt = VeritabaniBaglantisi.ExecuteQuery(checkQuery,
                    new NpgsqlParameter("@uyeID", uyeID),
                    new NpgsqlParameter("@kitapID", kitapID));

                if (dt.Rows.Count == 0)
                {
                    MessageBox.Show("Bu kitap bu üyede değil veya zaten iade edilmiş!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int islemID = Convert.ToInt32(dt.Rows[0]["islemid"]);

                // TeslimTarihi'ni güncelle - bu trigger'ı tetikleyecek ve ceza hesaplanacak
                string updateQuery = "UPDATE OduncIslemleri SET TeslimTarihi = CURRENT_DATE WHERE IslemID = @islemID";
                VeritabaniBaglantisi.ExecuteNonQuery(updateQuery, new NpgsqlParameter("@islemID", islemID));

                MessageBox.Show("Kitap başarıyla iade edildi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                OduncIslemleriniYukle();
                comboBoxUyeAdiOduncIade.SelectedIndex = -1;
                comboBoxKitapAdiOduncIade.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show("İade işleminde hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonIslemSil_Click(object sender, EventArgs e)
        {
            try
            {
                // DataGridView'den seçili satırı al
                if (dgvOduncListesi.SelectedRows.Count == 0 && dgvOduncListesi.CurrentRow == null)
                {
                    MessageBox.Show("Lütfen silinecek işlemi seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DataGridViewRow row = dgvOduncListesi.CurrentRow;

                // Boş satır kontrolü
                if (row.Cells["islemid"].Value == null || row.Cells["islemid"].Value == DBNull.Value)
                {
                    return;
                }

                int islemID = Convert.ToInt32(row.Cells["islemid"].Value);

                DialogResult result = MessageBox.Show("Bu ödünç işlemini silmek istediğinizden emin misiniz?",
                    "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    // Önce ilişkili ceza kaydını sil (varsa)
                    string deleteCezaQuery = "DELETE FROM Cezalar WHERE IslemID = @islemID";
                    VeritabaniBaglantisi.ExecuteNonQuery(deleteCezaQuery, new NpgsqlParameter("@islemID", islemID));

                    // Sonra ödünç işlemini sil
                    string deleteQuery = "DELETE FROM OduncIslemleri WHERE IslemID = @islemID";
                    VeritabaniBaglantisi.ExecuteNonQuery(deleteQuery, new NpgsqlParameter("@islemID", islemID));

                    MessageBox.Show("Ödünç işlemi başarıyla silindi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    OduncIslemleriniYukle();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("İşlem silinirken hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonIslemAra_Click(object sender, EventArgs e)
        {
            try
            {
                string aramaMetni = txtIslemAra.Text.Trim();

                if (string.IsNullOrWhiteSpace(aramaMetni))
                {
                    OduncIslemleriniYukle();
                    return;
                }

                string query = @"
                    SELECT o.IslemID AS islemid, 
                           k.Ad || ' ' || k.Soyad AS uyeadi,
                           kit.KitapAdi AS kitapadi,
                           o.VerilisTarihi AS verilstarihi, 
                           o.TeslimTarihi AS teslimtarihi,
                           CASE WHEN o.TeslimTarihi IS NULL THEN 'Teslim Edilmedi' ELSE 'Teslim Edildi' END AS durum,
                           COALESCE(c.CezaTutari, 0) AS cezatutari,
                           COALESCE(c.OdendiMi, false) AS odendimi
                    FROM OduncIslemleri o
                    JOIN Uyeler u ON o.UyeID = u.UyeID
                    JOIN Kisiler k ON u.UyeID = k.KisiID
                    JOIN Kitaplar kit ON o.KitapID = kit.KitapID
                    LEFT JOIN Cezalar c ON o.IslemID = c.IslemID
                    WHERE LOWER(k.Ad || ' ' || k.Soyad) LIKE LOWER(@arama)
                       OR LOWER(kit.KitapAdi) LIKE LOWER(@arama)
                    ORDER BY o.IslemID DESC";

                dgvOduncListesi.DataSource = VeritabaniBaglantisi.ExecuteQuery(query,
                    new NpgsqlParameter("@arama", "%" + aramaMetni + "%"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Arama sırasında hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void buttonIslemGuncelle_Click(object sender, EventArgs e)
        {
            // Bu buton şu an için işlevsiz bırakıldı
            // Ödünç işlemlerinde genellikle güncelleme yapılmaz, sadece iade edilir
            MessageBox.Show("Ödünç işlemlerini güncellemek için:\n" +
                "- İade etmek istiyorsanız 'İade Et' butonunu kullanın.\n" +
                "- Yanlış kayıt varsa 'Sil' butonuyla silin ve yeniden ekleyin.",
                "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        #endregion

        #region Yönetim İşlemleri

        // Yönetim DataGridView'larını yükle
        private void YazarlarListesiniYukle()
        {
            string query = "SELECT YazarID AS yazarid, YazarAdi AS yazaradi, YazarSoyadi AS yazarsoyadi FROM Yazarlar ORDER BY YazarAdi";
            dgvYazarlar.DataSource = VeritabaniBaglantisi.ExecuteQuery(query);
        }

        private void KategorilerListesiniYukle()
        {
            string query = "SELECT KategoriID AS kategoriid, KategoriAdi AS kategoriadi FROM Kategoriler ORDER BY KategoriAdi";
            dgvKategoriler.DataSource = VeritabaniBaglantisi.ExecuteQuery(query);
        }

        private void YayinevleriListesiniYukle()
        {
            string query = "SELECT YayineviID AS yayineviid, YayineviAdi AS yayineviadi FROM Yayinevleri ORDER BY YayineviAdi";
            dgvYayinevleri.DataSource = VeritabaniBaglantisi.ExecuteQuery(query);
        }

        private void DillerListesiniYukle()
        {
            string query = "SELECT DilID AS dilid, DilAdi AS diladi FROM Diller ORDER BY DilAdi";
            dgvDiller.DataSource = VeritabaniBaglantisi.ExecuteQuery(query);
        }

        // Yazar Ekle
        private void btnYazarEkle_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtYazarAdi.Text))
                {
                    MessageBox.Show("Yazar adı boş olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string query = "INSERT INTO Yazarlar (YazarAdi, YazarSoyadi) VALUES (@adi, @soyadi)";
                VeritabaniBaglantisi.ExecuteNonQuery(query,
                    new NpgsqlParameter("@adi", txtYazarAdi.Text.Trim()),
                    new NpgsqlParameter("@soyadi", txtYazarSoyadi.Text.Trim()));

                MessageBox.Show("Yazar başarıyla eklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtYazarAdi.Clear();
                txtYazarSoyadi.Clear();
                YazarlarListesiniYukle();
                YazarlariYukle(); // ComboBox'ı da güncelle
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yazar eklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Kategori Ekle
        private void btnKategoriEkle_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtKategoriAdi.Text))
                {
                    MessageBox.Show("Kategori adı boş olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string query = "INSERT INTO Kategoriler (KategoriAdi) VALUES (@adi)";
                VeritabaniBaglantisi.ExecuteNonQuery(query,
                    new NpgsqlParameter("@adi", txtKategoriAdi.Text.Trim()));

                MessageBox.Show("Kategori başarıyla eklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtKategoriAdi.Clear();
                KategorilerListesiniYukle();
                KategorileriYukle(); // ComboBox'ı da güncelle
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kategori eklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Yayınevi Ekle
        private void btnYayineviEkle_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(textYayinevleri.Text))
                {
                    MessageBox.Show("Yayınevi adı boş olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string query = "INSERT INTO Yayinevleri (YayineviAdi) VALUES (@adi)";
                VeritabaniBaglantisi.ExecuteNonQuery(query,
                    new NpgsqlParameter("@adi", textYayinevleri.Text.Trim()));

                MessageBox.Show("Yayınevi başarıyla eklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                textYayinevleri.Clear();
                YayinevleriListesiniYukle();
                YayinevleriniYukle(); // ComboBox'ı da güncelle
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yayınevi eklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Dil Ekle
        private void btnDilEkle_Click(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtDiller.Text))
                {
                    MessageBox.Show("Dil adı boş olamaz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                string query = "INSERT INTO Diller (DilAdi) VALUES (@adi)";
                VeritabaniBaglantisi.ExecuteNonQuery(query,
                    new NpgsqlParameter("@adi", txtDiller.Text.Trim()));

                MessageBox.Show("Dil başarıyla eklendi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);

                txtDiller.Clear();
                DillerListesiniYukle();
                DilleriYukle(); // ComboBox'ı da güncelle
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dil eklenirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        // Yazar Sil
        public void btnYazarSil_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvYazarlar.CurrentRow == null || dgvYazarlar.CurrentRow.Cells["yazarid"].Value == null)
                {
                    MessageBox.Show("Lütfen silinecek yazarı seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int yazarID = Convert.ToInt32(dgvYazarlar.CurrentRow.Cells["yazarid"].Value);

                // Bu yazara ait kitap var mı kontrol et
                string checkQuery = "SELECT COUNT(*) FROM Kitaplar WHERE YazarID = @id";
                DataTable dt = VeritabaniBaglantisi.ExecuteQuery(checkQuery, new NpgsqlParameter("@id", yazarID));

                if (Convert.ToInt32(dt.Rows[0][0]) > 0)
                {
                    MessageBox.Show("Bu yazara ait kitaplar var. Önce kitapları silmeniz gerekiyor!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult result = MessageBox.Show("Bu yazarı silmek istediğinizden emin misiniz?",
                    "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    string query = "DELETE FROM Yazarlar WHERE YazarID = @id";
                    VeritabaniBaglantisi.ExecuteNonQuery(query, new NpgsqlParameter("@id", yazarID));

                    MessageBox.Show("Yazar başarıyla silindi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    YazarlarListesiniYukle();
                    YazarlariYukle();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yazar silinirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Kategori Sil
        public void btnKategoriSil_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvKategoriler.CurrentRow == null || dgvKategoriler.CurrentRow.Cells["kategoriid"].Value == null)
                {
                    MessageBox.Show("Lütfen silinecek kategoriyi seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int kategoriID = Convert.ToInt32(dgvKategoriler.CurrentRow.Cells["kategoriid"].Value);

                // Bu kategoriye ait kitap var mı kontrol et
                string checkQuery = "SELECT COUNT(*) FROM Kitaplar WHERE KategoriID = @id";
                DataTable dt = VeritabaniBaglantisi.ExecuteQuery(checkQuery, new NpgsqlParameter("@id", kategoriID));

                if (Convert.ToInt32(dt.Rows[0][0]) > 0)
                {
                    MessageBox.Show("Bu kategoriye ait kitaplar var. Önce kitapları silmeniz gerekiyor!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult result = MessageBox.Show("Bu kategoriyi silmek istediğinizden emin misiniz?",
                    "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    string query = "DELETE FROM Kategoriler WHERE KategoriID = @id";
                    VeritabaniBaglantisi.ExecuteNonQuery(query, new NpgsqlParameter("@id", kategoriID));

                    MessageBox.Show("Kategori başarıyla silindi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    KategorilerListesiniYukle();
                    KategorileriYukle();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Kategori silinirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Yayınevi Sil
        public void btnYayineviSil_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvYayinevleri.CurrentRow == null || dgvYayinevleri.CurrentRow.Cells["yayineviid"].Value == null)
                {
                    MessageBox.Show("Lütfen silinecek yayınevini seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int yayineviID = Convert.ToInt32(dgvYayinevleri.CurrentRow.Cells["yayineviid"].Value);

                // Bu yayınevine ait kitap var mı kontrol et
                string checkQuery = "SELECT COUNT(*) FROM Kitaplar WHERE YayineviID = @id";
                DataTable dt = VeritabaniBaglantisi.ExecuteQuery(checkQuery, new NpgsqlParameter("@id", yayineviID));

                if (Convert.ToInt32(dt.Rows[0][0]) > 0)
                {
                    MessageBox.Show("Bu yayınevine ait kitaplar var. Önce kitapları silmeniz gerekiyor!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult result = MessageBox.Show("Bu yayınevini silmek istediğinizden emin misiniz?",
                    "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    string query = "DELETE FROM Yayinevleri WHERE YayineviID = @id";
                    VeritabaniBaglantisi.ExecuteNonQuery(query, new NpgsqlParameter("@id", yayineviID));

                    MessageBox.Show("Yayınevi başarıyla silindi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    YayinevleriListesiniYukle();
                    YayinevleriniYukle();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Yayınevi silinirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Dil Sil
        public void btnDilSil_Click(object sender, EventArgs e)
        {
            try
            {
                if (dgvDiller.CurrentRow == null || dgvDiller.CurrentRow.Cells["dilid"].Value == null)
                {
                    MessageBox.Show("Lütfen silinecek dili seçin!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                int dilID = Convert.ToInt32(dgvDiller.CurrentRow.Cells["dilid"].Value);

                // Bu dile ait kitap var mı kontrol et
                string checkQuery = "SELECT COUNT(*) FROM Kitaplar WHERE DilID = @id";
                DataTable dt = VeritabaniBaglantisi.ExecuteQuery(checkQuery, new NpgsqlParameter("@id", dilID));

                if (Convert.ToInt32(dt.Rows[0][0]) > 0)
                {
                    MessageBox.Show("Bu dilde kitaplar var. Önce kitapları silmeniz gerekiyor!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                DialogResult result = MessageBox.Show("Bu dili silmek istediğinizden emin misiniz?",
                    "Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    string query = "DELETE FROM Diller WHERE DilID = @id";
                    VeritabaniBaglantisi.ExecuteNonQuery(query, new NpgsqlParameter("@id", dilID));

                    MessageBox.Show("Dil başarıyla silindi!", "Başarılı", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DillerListesiniYukle();
                    DilleriYukle();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Dil silinirken hata: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion
    }
}