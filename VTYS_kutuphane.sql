-- tabloları oluşturma 

CREATE TABLE Sehirler (
    SehirPlaka INTEGER NOT NULL,
    SehirAdi VARCHAR(50) NOT NULL,
    CONSTRAINT pk_sehirler PRIMARY KEY (SehirPlaka)
);

CREATE TABLE Dolaplar (
    DolapID SERIAL,
    DolapKodu VARCHAR(20) NOT NULL,
    CONSTRAINT pk_dolaplar PRIMARY KEY (DolapID),
    CONSTRAINT uq_dolap_kodu UNIQUE (DolapKodu)
);

CREATE TABLE Yazarlar (
    YazarID SERIAL,
    YazarAdi VARCHAR(50) NOT NULL,
    YazarSoyadi VARCHAR(50) NOT NULL,
    CONSTRAINT pk_yazarlar PRIMARY KEY (YazarID)
);

CREATE TABLE Kategoriler (
    KategoriID SERIAL,
    KategoriAdi VARCHAR(50) NOT NULL,
    CONSTRAINT pk_kategoriler PRIMARY KEY (KategoriID)
);

CREATE TABLE Yayinevleri (
    YayineviID SERIAL,
    YayineviAdi VARCHAR(100) NOT NULL,
    CONSTRAINT pk_yayinevleri PRIMARY KEY (YayineviID)
);

CREATE TABLE Diller (
    DilID SERIAL,
    DilAdi VARCHAR(50) NOT NULL,
    CONSTRAINT pk_diller PRIMARY KEY (DilID)
);

CREATE TABLE IslemLoglari (
    LogID SERIAL,
    TabloAdi VARCHAR(50),
    IslemTuru VARCHAR(50),
    Tarih TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    Aciklama TEXT,
    CONSTRAINT pk_islemloglari PRIMARY KEY (LogID)
);

CREATE TABLE Ilceler (
    IlceID SERIAL,
    IlceAdi VARCHAR(50) NOT NULL,
    SehirPlaka INTEGER NOT NULL,
    CONSTRAINT pk_ilceler PRIMARY KEY (IlceID),
    CONSTRAINT fk_ilce_sehir FOREIGN KEY (SehirPlaka) REFERENCES Sehirler(SehirPlaka)
);

CREATE TABLE Raflar (
    RafID SERIAL,
    RafNumarasi VARCHAR(20) NOT NULL,
    DolapID INTEGER NOT NULL,
    CONSTRAINT pk_raflar PRIMARY KEY (RafID),
    CONSTRAINT fk_raf_dolap FOREIGN KEY (DolapID) REFERENCES Dolaplar(DolapID)
);

-- kişiler ve kalıtım yapısını oluşturma

-- ana tablo, ebeveyn sınıf
CREATE TABLE Kisiler (
    KisiID SERIAL,
    Ad VARCHAR(50) NOT NULL,
    Soyad VARCHAR(50) NOT NULL,
    TC CHAR(11) NOT NULL, 
    Tel VARCHAR(15),
    IlceID INTEGER,
    CONSTRAINT pk_kisiler PRIMARY KEY (KisiID),
    CONSTRAINT uq_kisi_tc UNIQUE (TC),
    CONSTRAINT chk_tc_uzunluk CHECK (LENGTH(TC) = 11)
);

-- personel tablosu, çocuk sınıf
CREATE TABLE Personel (
    PersonelID INTEGER NOT NULL, -- Serial değil, çünkü Kisiler'den gelecek
    Maas DECIMAL(10, 2),
    Departman VARCHAR(50),
    CONSTRAINT pk_personel PRIMARY KEY (PersonelID)
);

-- üyeler tablosu, çocuk sınıf
CREATE TABLE Uyeler (
    UyeID INTEGER NOT NULL,
    UyelikTarihi DATE DEFAULT CURRENT_DATE,
    Puan INTEGER DEFAULT 0,
    CONSTRAINT pk_uyeler PRIMARY KEY (UyeID)
);

-- FK bağlantılarının kurulması

-- Kisiler ile İlçe bağlantısı
ALTER TABLE Kisiler
    ADD CONSTRAINT fk_kisi_ilce FOREIGN KEY (IlceID)
    REFERENCES Ilceler(IlceID);

-- Kisiler ile Personel arasındaki bağıntı
ALTER TABLE Personel
    ADD CONSTRAINT fk_personel_kisi FOREIGN KEY (PersonelID)
    REFERENCES Kisiler (KisiID)
    ON DELETE CASCADE
    ON UPDATE CASCADE;

-- Kisiler ile Uyeler arasındaki bağıntı
ALTER TABLE Uyeler
    ADD CONSTRAINT fk_uye_kisi FOREIGN KEY (UyeID)
    REFERENCES Kisiler (KisiID)
    ON DELETE CASCADE
    ON UPDATE CASCADE;

-- kitaplar ve işlemler tabloları

CREATE TABLE Kitaplar (
    KitapID SERIAL,
    KitapAdi VARCHAR(100) NOT NULL,
    SayfaSayisi INTEGER,
    ISBN CHAR(13), --veritabanında sadece rakamlar saklanacak tireler değil.
    YazarID INTEGER NOT NULL,
    KategoriID INTEGER NOT NULL,
    YayineviID INTEGER NOT NULL,
    RafID INTEGER NOT NULL,
    DilID INTEGER NOT NULL,
    CONSTRAINT pk_kitaplar PRIMARY KEY (KitapID),
    CONSTRAINT uq_kitap_isbn UNIQUE (ISBN),
    CONSTRAINT fk_kitap_yazar FOREIGN KEY (YazarID) REFERENCES Yazarlar(YazarID),
    CONSTRAINT fk_kitap_kategori FOREIGN KEY (KategoriID) REFERENCES Kategoriler(KategoriID),
    CONSTRAINT fk_kitap_yayinevi FOREIGN KEY (YayineviID) REFERENCES Yayinevleri(YayineviID),
    CONSTRAINT fk_kitap_raf FOREIGN KEY (RafID) REFERENCES Raflar(RafID),
    CONSTRAINT fk_kitap_dil FOREIGN KEY (DilID) REFERENCES Diller(DilID)
);

CREATE TABLE OduncIslemleri (
    IslemID SERIAL,
    UyeID INTEGER NOT NULL,public.cezalar
    KitapID INTEGER NOT NULL,
    VerilisTarihi DATE NOT NULL DEFAULT CURRENT_DATE,
    TeslimTarihi DATE,
    CONSTRAINT pk_oduncislemleri PRIMARY KEY (IslemID),
    CONSTRAINT fk_odunc_uye FOREIGN KEY (UyeID) REFERENCES Uyeler(UyeID),
    CONSTRAINT fk_odunc_kitap FOREIGN KEY (KitapID) REFERENCES Kitaplar(KitapID)
);

CREATE TABLE Cezalar (
    CezaID SERIAL,
    IslemID INTEGER NOT NULL,
    CezaTutari DECIMAL(10, 2) NOT NULL,
    OdendiMi BOOLEAN DEFAULT FALSE,
    CONSTRAINT pk_cezalar PRIMARY KEY (CezaID),
    CONSTRAINT uq_ceza_islem UNIQUE (IslemID),
    CONSTRAINT fk_ceza_islem FOREIGN KEY (IslemID) REFERENCES OduncIslemleri(IslemID)
);



-- saklı yordamlar

-- kitap ekleme 
CREATE OR REPLACE PROCEDURE sp_KitapEkle(
    p_KitapAdi VARCHAR,
    p_SayfaSayisi INTEGER,
    P_ISBN CHAR,
    p_YazarID INTEGER,
    p_KategoriID INTEGER,
    p_YayineviID INTEGER,
    p_RafID INTEGER,
    p_DilID INTEGER
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO Kitaplar (KitapAdi, SayfaSayisi, ISBN, YazarID, KategoriID, YayineviID, RafID, DilID)
    VALUES (p_KitapAdi, p_SayfaSayisi, p_ISBN, p_YazarID, P_KategoriID, p_YayineviID, p_RafID, p_DilID);
END;
$$;

-- üye ekleme
CREATE OR REPLACE PROCEDURE sp_UyeEkle(
    p_Ad VARCHAR,
    p_Soyad VARCHAR,
    p_TC CHAR,
    p_Tel VARCHAR,
    p_IlceID INTEGER
)
LANGUAGE plpgsql
AS $$
DECLARE yeni_kisi_id INTEGER;
BEGIN
    -- Önce Kisiler sınıfına ekleyip ID'yi al
    INSERT INTO Kisiler(Ad, Soyad, TC, Tel, IlceID)
    VALUES (p_Ad, p_Soyad, p_TC, p_Tel, p_IlceID)
    RETURNING KisiID INTO yeni_kisi_id;

    -- O ID'yi kullanarak Uyeler sınıfına ekle
    INSERT INTO Uyeler(UyeID, UyelikTarihi, Puan)
    VALUES (yeni_kisi_id, CURRENT_DATE, 100); -- Her yeni üye 100 puanla başlar
END;
$$;

-- personel ekleme
CREATE OR REPLACE PROCEDURE sp_PersonelEkle(
    p_Ad VARCHAR,
    p_Soyad VARCHAR,
    p_TC CHAR,
    p_Tel VARCHAR,
    p_IlceID INTEGER,
    p_Maas DECIMAL,
    p_Departman VARCHAR
)
LANGUAGE plpgsql
AS $$
DECLARE
    yeni_kisi_id INTEGER;
BEGIN
    INSERT INTO Kisiler (Ad, Soyad, TC, Tel, IlceID)
    VALUES (p_Ad, p_Soyad, p_TC, p_Tel, p_IlceID)
    RETURNING KisiID INTO yeni_kisi_id;

    INSERT INTO Personel (PersonelID, Maas, Departman)
    VALUES (yeni_kisi_id, p_Maas, p_Departman);
END;
$$;

-- ödünç verme
CREATE OR REPLACE PROCEDURE sp_OduncVer(
    p_UyeID INTEGER,
    p_KitapID INTEGER
)
LANGUAGE plpgsql
AS $$
BEGIN
    IF EXISTS (SELECT 1 FROM OduncIslemleri WHERE KitapID = p_KitapID AND TeslimTarihi IS NULL) THEN
        RAISE EXCEPTION 'Bu kitap şu anda başkasında, ödünç verilemez.';
    ELSE
        INSERT INTO OduncIslemleri (UyeID, KitapID, VerilisTarihi, TeslimTarihi)
        VALUES (p_UyeID, p_KitapID, CURRENT_DATE, NULL);
    END IF;
END;
$$;


-- ceza hesaplama fonksiyonu
CREATE OR REPLACE FUNCTION fn_CezaHesapla(p_VerilisTarihi DATE, p_TeslimTarihi DATE)
RETURNS DECIMAL(10,2)
LANGUAGE plpgsql
AS $$
DECLARE
    gecikme_gun INTEGER;
    ceza_tutari DECIMAL(10,2);
BEGIN
    IF p_TeslimTarihi > (p_VerilisTarihi + INTERVAL '15 days') THEN
        gecikme_gun := p_TeslimTarihi - (p_VerilisTarihi + INTERVAL '15 days');
        ceza_tutari := gecikme_gun * 5.00;
    ELSE
        ceza_tutari := 0.00;
    END IF;
    RETURN ceza_tutari;
END;
$$;


-- triggerlar

-- kitap ekleme trg
DROP FUNCTION IF EXISTS fn_trg_log_kitap_ekleme() CASCADE;
CREATE OR REPLACE FUNCTION fn_trg_log_kitap_ekleme()
RETURNS TRIGGER AS $$
BEGIN
    INSERT INTO IslemLoglari (TabloAdi, IslemTuru, Tarih, Aciklama)
    VALUES (
        'Kitaplar',
        'INSERT',
        NOW(),
        'Yeni kitap eklendi. KitapID: ' || NEW.KitapID
    );

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_log_kitap_ekleme ON Kitaplar;

CREATE TRIGGER trg_log_kitap_ekleme
AFTER INSERT ON Kitaplar
FOR EACH ROW
EXECUTE FUNCTION fn_trg_log_kitap_ekleme();

-- ceza oluşturma trg
DROP FUNCTION IF EXISTS fn_trg_ceza_olustur() CASCADE;
CREATE OR REPLACE FUNCTION fn_trg_ceza_olustur()
RETURNS TRIGGER AS $$
DECLARE
    ceza_tutari DECIMAL(10,2);
BEGIN
    IF NEW.TeslimTarihi IS NULL THEN
        RETURN NEW;
    END IF;

    -- fn_CezaHesapla fonksiyonunu kullanacağım
    ceza_tutari := fn_CezaHesapla(NEW.VerilisTarihi, NEW.TeslimTarihi);

    IF ceza_tutari = 0 THEN
        RETURN NEW;
    END IF;

    INSERT INTO Cezalar (IslemID, CezaTutari, OdendiMi)
    VALUES (NEW.IslemID, ceza_tutari, FALSE);

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_ceza_olustur ON OduncIslemleri;
CREATE TRIGGER trg_ceza_olustur
AFTER UPDATE OF TeslimTarihi ON OduncIslemleri
FOR EACH ROW
EXECUTE FUNCTION fn_trg_ceza_olustur();


-- kitap isbn kontrol etme trg
DROP FUNCTION IF EXISTS fn_trg_isbn_kontrol() CASCADE;

CREATE OR REPLACE FUNCTION fn_trg_isbn_kontrol()
RETURNS TRIGGER AS $$
BEGIN
    IF NEW.ISBN IS NULL OR NEW.ISBN = '' THEN
        RAISE EXCEPTION 'ISBN alanı boş bırakılamaz.';
    END IF;

    IF length(NEW.ISBN) <> 13 THEN
        RAISE EXCEPTION 'ISBN 13 haneli olmalıdır. Girilen: %', NEW.ISBN;
    END IF;

    RETURN NEW;
END;
$$ LANGUAGE plpgsql;

DROP TRIGGER IF EXISTS trg_isbn_kontrol ON Kitaplar;

CREATE TRIGGER trg_isbn_kontrol
BEFORE INSERT OR UPDATE ON Kitaplar
FOR EACH ROW
EXECUTE FUNCTION fn_trg_isbn_kontrol();


-- üye silinmesini engelleme trg
DROP FUNCTION IF EXISTS fn_trg_uye_sil_kontrol() CASCADE;

CREATE OR REPLACE FUNCTION fn_trg_uye_sil_kontrol()
RETURNS TRIGGER AS $$
DECLARE
    sayi INT;
BEGIN
    SELECT COUNT(*) INTO sayi
    FROM OduncIslemleri
    WHERE UyeID = OLD.UyeID
      AND TeslimTarihi IS NULL;

    IF sayi > 0 THEN
        RAISE EXCEPTION 'Bu üyenin teslim etmediği kitaplar var. Silme işlemi yapılamaz.';
    END IF;

    RETURN OLD;
END;
$$ LANGUAGE plpgsql;


DROP TRIGGER IF EXISTS trg_uye_sil_kontrol ON Uyeler;

CREATE TRIGGER trg_uye_sil_kontrol
BEFORE DELETE ON Uyeler
FOR EACH ROW
EXECUTE FUNCTION fn_trg_uye_sil_kontrol();


--------------------------------------- TABLOLARA ÖRNEK VERİ EKLEME

INSERT INTO Sehirler (SehirPlaka, SehirAdi) VALUES
(34, 'İstanbul'),
(6, 'Ankara'),
(35, 'İzmir'),
(16, 'Bursa'),
(1, 'Adana');

INSERT INTO Ilceler (IlceAdi, SehirPlaka) VALUES
('Kadıköy', 34),
('Beşiktaş', 34),
('Çankaya', 6),
('Keçiören', 6),
('Konak', 35),
('Karşıyaka', 35),
('Osmangazi', 16),
('Seyhan', 1);

INSERT INTO Dolaplar (DolapKodu) VALUES
('A-001'),
('A-002'),
('B-001'),
('B-002'),
('C-001');

INSERT INTO Raflar (RafNumarasi, DolapID) VALUES
('Raf-1', 1),
('Raf-2', 1),
('Raf-3', 1),
('Raf-1', 2),
('Raf-2', 2),
('Raf-1', 3),
('Raf-2', 3),
('Raf-1', 4),
('Raf-1', 5);

INSERT INTO Yazarlar (YazarAdi, YazarSoyadi) VALUES
('Orhan', 'Pamuk'),
('Sabahattin', 'Ali'),
('Yaşar', 'Kemal'),
('Elif', 'Şafak'),
('Ahmet', 'Ümit'),
('George', 'Orwell'),
('J.K.', 'Rowling'),
('Fyodor', 'Dostoyevski');

INSERT INTO Kategoriler (KategoriAdi) VALUES
('Roman'),
('Hikaye'),
('Şiir'),
('Bilim Kurgu'),
('Polisiye'),
('Tarih'),
('Felsefe'),
('Çocuk Kitapları');

INSERT INTO Yayinevleri (YayineviAdi) VALUES
('İş Bankası Kültür Yayınları'),
('Yapı Kredi Yayınları'),
('Can Yayınları'),
('Doğan Kitap'),
('Everest Yayınları'),
('İletişim Yayınları');

INSERT INTO Diller (DilAdi) VALUES
('Türkçe'),
('İngilizce'),
('Fransızca'),
('Almanca'),
('İspanyolca');

INSERT INTO Kisiler (Ad, Soyad, TC, Tel, IlceID) VALUES
('Zeynep', 'Uludağ', '12345678901', '05321234567', 1),
('Fahriye', 'Demir', '12345678902', '05322345678', 2),
('Mehmet', 'Özkan', '12345678903', '05323456789', 3),
('Fatma', 'Şahin', '12345678904', '05324567890', 4),
('Ali', 'Balcı', '12345678905', '05325678901', 5);

INSERT INTO Uyeler (UyeID, UyelikTarihi, Puan) VALUES
(1, '2024-01-15', 100),
(2, '2024-02-20', 150),
(3, '2024-03-10', 200),
(4, '2024-04-05', 100),
(5, '2024-05-18', 120);

-- bunlar personel olacak
INSERT INTO Kisiler (Ad, Soyad, TC, Tel, IlceID) VALUES
('Mehmet', 'Arslan', '12345678906', '05326789012', 1),
('Can', 'Yıldız', '12345678907', '05327890123', 2),
('Esra', 'Kurt', '12345678908', '05328901234', 3);

INSERT INTO Personel (PersonelID, Maas, Departman) VALUES
(6, 15000.00, 'Kütüphane Görevlisi'),
(7, 18000.00, 'Kütüphane Müdürü'),
(8, 14000.00, 'Arşiv Sorumlusu');