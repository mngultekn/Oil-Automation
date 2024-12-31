using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Petrol
{
    public partial class Form4 : Form
    {
        private DataBaseHelper dbHelper;
        private string pompaciAd;
        private string pompaciSoyad;
        private string sube;

        public Form4(string ad, string soyad, string sube)
        {
            InitializeComponent();
            dbHelper = new DataBaseHelper();

            this.pompaciAd = ad;
            this.pompaciSoyad = soyad;
            this.sube = sube;

            label1.Text = $"Hoşgeldiniz Sayın {ad} {soyad}";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string plaka = textBox1.Text.Trim();
            string yakitMiktariStr = textBox2.Text.Trim();

            if (string.IsNullOrWhiteSpace(plaka) || string.IsNullOrWhiteSpace(yakitMiktariStr) ||
                !decimal.TryParse(yakitMiktariStr, out decimal yakitMiktari) || yakitMiktari <= 0)
            {
                MessageBox.Show("Lütfen geçerli bir plaka ve yakıt miktarı girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = dbHelper.GetConnection())
                {
                    string query = @"INSERT INTO bekleyen_islemler (pompaciad, pompacisoyad, plaka, yakit_miktari, sube) 
                                     VALUES (@pompaciAd, @pompaciSoyad, @plaka, @yakitMiktari, @sube)";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@pompaciAd", this.pompaciAd);
                        command.Parameters.AddWithValue("@pompaciSoyad", this.pompaciSoyad);
                        command.Parameters.AddWithValue("@plaka", plaka);
                        command.Parameters.AddWithValue("@yakitMiktari", yakitMiktari);
                        command.Parameters.AddWithValue("@sube", this.sube);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Kayıt başarıyla eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            textBox1.Clear();
                            textBox2.Clear();
                        }
                        else
                        {
                            MessageBox.Show("Kayıt eklenirken bir sorun oluştu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = dbHelper.GetConnection())
                {
                    string checkQuery = @"SELECT COUNT(*) FROM is_saatleri 
                                  WHERE sube = @sube AND ad = @ad AND soyad = @soyad AND is_bitis IS NULL";

                    using (SqlCommand checkCommand = new SqlCommand(checkQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@sube", this.sube);
                        checkCommand.Parameters.AddWithValue("@ad", this.pompaciAd);
                        checkCommand.Parameters.AddWithValue("@soyad", this.pompaciSoyad);

                        int activeJobCount = (int)checkCommand.ExecuteScalar();

                        if (activeJobCount > 0)
                        {
                            MessageBox.Show("Zaten aktif bir iş başlangıcınız bulunuyor. Önce mevcut işten çıkış yapmalısınız.",
                                            "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }
                    }

                    string isBaslangic = DateTime.Now.ToString("HH:mm");

                    string insertQuery = @"INSERT INTO is_saatleri (is_baslangic, sube, ad, soyad) 
                                   VALUES (@isBaslangic, @sube, @ad, @soyad)";

                    using (SqlCommand insertCommand = new SqlCommand(insertQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@isBaslangic", isBaslangic);
                        insertCommand.Parameters.AddWithValue("@sube", this.sube);
                        insertCommand.Parameters.AddWithValue("@ad", this.pompaciAd);
                        insertCommand.Parameters.AddWithValue("@soyad", this.pompaciSoyad);

                        int rowsAffected = insertCommand.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("İş başlangıç saati başarıyla kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("İş başlangıç saati eklenirken bir sorun oluştu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = dbHelper.GetConnection())
                {
                    string selectQuery = @"SELECT is_baslangic FROM is_saatleri 
                                   WHERE sube = @sube AND ad = @ad AND soyad = @soyad";

                    using (SqlCommand selectCommand = new SqlCommand(selectQuery, connection))
                    {
                        selectCommand.Parameters.AddWithValue("@sube", this.sube);
                        selectCommand.Parameters.AddWithValue("@ad", this.pompaciAd);
                        selectCommand.Parameters.AddWithValue("@soyad", this.pompaciSoyad);

                        object isBaslangicObj = selectCommand.ExecuteScalar();

                        if (isBaslangicObj != null)
                        {
                            DateTime isBaslangicTime = DateTime.ParseExact(isBaslangicObj.ToString(), "HH:mm", null);
                            DateTime currentTime = DateTime.Now;

                            TimeSpan timeDiff = currentTime - isBaslangicTime;

                            if (timeDiff.TotalHours < 12)
                            {
                                MessageBox.Show($"Çalışma sürenizi henüz doldurmadınız. Kalan süre: {12 - timeDiff.TotalHours:F1} saat.",
                                                "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                return;
                            }

                            string updateQuery = @"UPDATE is_saatleri 
                                           SET is_bitis = @isBitis 
                                           WHERE sube = @sube AND ad = @ad AND soyad = @soyad";

                            using (SqlCommand updateCommand = new SqlCommand(updateQuery, connection))
                            {
                                updateCommand.Parameters.AddWithValue("@isBitis", currentTime.ToString("HH:mm"));
                                updateCommand.Parameters.AddWithValue("@sube", this.sube);
                                updateCommand.Parameters.AddWithValue("@ad", this.pompaciAd);
                                updateCommand.Parameters.AddWithValue("@soyad", this.pompaciSoyad);

                                int rowsAffected = updateCommand.ExecuteNonQuery();

                                if (rowsAffected > 0)
                                {
                                    MessageBox.Show("Çalışma süresi başarıyla tamamlandı ve iş bitiş saati kaydedildi.",
                                                    "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                else
                                {
                                    MessageBox.Show("İş bitiş saati güncellenirken bir sorun oluştu.",
                                                    "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                }
                            }
                        }
                        else
                        {
                            MessageBox.Show("Bu kullanıcı için iş başlangıç saati bulunamadı. Önce iş başlangıcını kaydedin.",
                                            "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
