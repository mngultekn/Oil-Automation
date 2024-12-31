using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Petrol
{
    public partial class Form2 : Form
    {
        private readonly DataBaseHelper dbHelper = new DataBaseHelper();

        public Form2()
        {
            InitializeComponent();
            listBox1.SelectedIndexChanged += ListBox1_SelectedIndexChanged;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string subeAdi = textBox1.Text;

            if (string.IsNullOrWhiteSpace(subeAdi))
            {
                MessageBox.Show("Lütfen bir şube adı giriniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = dbHelper.GetConnection())
                {
                    string query = "SELECT ad, soyad, yetki FROM kullanici WHERE sube = @sube";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sube", subeAdi);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            listBox1.Items.Clear();
                            listBox2.Items.Clear();

                            while (reader.Read())
                            {
                                string ad = reader["ad"].ToString();
                                string soyad = reader["soyad"].ToString();
                                string yetki = reader["yetki"].ToString();

                                listBox1.Items.Add($"{ad} {soyad}");
                                listBox2.Items.Add(yetki);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ListBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int selectedIndex = listBox1.SelectedIndex;

            if (selectedIndex >= 0 && selectedIndex < listBox2.Items.Count)
            {
                listBox2.SelectedIndex = selectedIndex;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            int selectedIndex = listBox1.SelectedIndex;

            if (selectedIndex < 0 || selectedIndex >= listBox1.Items.Count)
            {
                MessageBox.Show("Lütfen silmek için bir eleman seçiniz!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedName = listBox1.Items[selectedIndex].ToString();
            string selectedYetki = listBox2.Items[selectedIndex].ToString();

            string[] nameParts = selectedName.Split(' ');
            if (nameParts.Length < 2)
            {
                MessageBox.Show("Geçersiz veri formatı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string ad = nameParts[0];
            string soyad = nameParts[1];

            try
            {
                using (SqlConnection connection = dbHelper.GetConnection())
                {
                    string deleteQuery = "DELETE FROM kullanici WHERE ad = @ad AND soyad = @soyad AND yetki = @yetki";
                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ad", ad);
                        command.Parameters.AddWithValue("@soyad", soyad);
                        command.Parameters.AddWithValue("@yetki", selectedYetki);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Kayıt başarıyla silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            listBox1.Items.RemoveAt(selectedIndex);
                            listBox2.Items.RemoveAt(selectedIndex);
                        }
                        else
                        {
                            MessageBox.Show("Kayıt silinirken bir sorun oluştu veya kayıt bulunamadı.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            string ad = textBox2.Text.Trim();
            string soyad = textBox3.Text.Trim();
            string yetki = comboBox1.SelectedItem?.ToString();
            string sube = textBox4.Text.Trim();
            string sifre = textBox5.Text.Trim();
            string kullaniciAd = textBox6.Text.Trim();

            if (string.IsNullOrWhiteSpace(ad) ||
                string.IsNullOrWhiteSpace(soyad) ||
                string.IsNullOrWhiteSpace(yetki) ||
                string.IsNullOrWhiteSpace(sube) ||
                string.IsNullOrWhiteSpace(sifre) ||
                string.IsNullOrWhiteSpace(kullaniciAd))
            {
                MessageBox.Show("Lütfen tüm alanları doldurun!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = dbHelper.GetConnection())
                {
                    string insertQuery = "INSERT INTO kullanici (ad, soyad, yetki, sube, sifre, kullanici_ad) VALUES (@ad, @soyad, @yetki, @sube, @sifre, @kullaniciAd)";
                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@ad", ad);
                        command.Parameters.AddWithValue("@soyad", soyad);
                        command.Parameters.AddWithValue("@yetki", yetki);
                        command.Parameters.AddWithValue("@sube", sube);
                        command.Parameters.AddWithValue("@sifre", sifre);
                        command.Parameters.AddWithValue("@kullaniciAd", kullaniciAd);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Kullanıcı başarıyla eklendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            textBox2.Clear();
                            textBox3.Clear();
                            textBox4.Clear();
                            textBox5.Clear();
                            textBox6.Clear();
                            comboBox1.SelectedIndex = -1;
                        }
                        else
                        {
                            MessageBox.Show("Kullanıcı eklenirken bir sorun oluştu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = dbHelper.GetConnection())
                {
                    string query = "SELECT sube, COUNT(*) AS personelSayisi FROM kullanici GROUP BY sube HAVING COUNT(*) > 0";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        listBox3.Items.Clear();

                        while (reader.Read())
                        {
                            string sube = reader["sube"].ToString();
                            int personelSayisi = Convert.ToInt32(reader["personelSayisi"]);
                            listBox3.Items.Add($"{sube} ({personelSayisi})");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            string sube = textBox7.Text.Trim();

            if (!decimal.TryParse(textBox8.Text.Trim(), out decimal gonderilenMiktar) || gonderilenMiktar <= 0)
            {
                MessageBox.Show("Geçerli bir yakıt miktarı girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            listBox4.Items.Clear();
            listBox5.Items.Clear();

            try
            {
                using (SqlConnection connection = dbHelper.GetConnection())
                {
                    decimal anaDepoMiktar = 0;
                    string getAnaDepoQuery = "SELECT miktar FROM anadepo WHERE id = 1";
                    using (SqlCommand getCommand = new SqlCommand(getAnaDepoQuery, connection))
                    {
                        object result = getCommand.ExecuteScalar();
                        anaDepoMiktar = result != null ? Convert.ToDecimal(result) : 0;
                    }

                    if (anaDepoMiktar < gonderilenMiktar)
                    {
                        MessageBox.Show("Ana depoda yeterli yakıt yok.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    string updateAnaDepoQuery = "UPDATE anadepo SET miktar = miktar - @gonderilenMiktar WHERE id = 1";
                    using (SqlCommand updateCommand = new SqlCommand(updateAnaDepoQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@gonderilenMiktar", gonderilenMiktar);
                        updateCommand.ExecuteNonQuery();
                    }

                    string updateSubeQuery = @"IF EXISTS (SELECT 1 FROM subeler WHERE sube = @sube)
                                     UPDATE subeler SET miktar = miktar + @gonderilenMiktar WHERE sube = @sube
                                     ELSE
                                     INSERT INTO subeler (sube, miktar) VALUES (@sube, @gonderilenMiktar)";
                    using (SqlCommand updateSubeCommand = new SqlCommand(updateSubeQuery, connection))
                    {
                        updateSubeCommand.Parameters.AddWithValue("@sube", sube);
                        updateSubeCommand.Parameters.AddWithValue("@gonderilenMiktar", gonderilenMiktar);
                        updateSubeCommand.ExecuteNonQuery();
                    }

                    using (SqlCommand getAnaDepoCommand = new SqlCommand(getAnaDepoQuery, connection))
                    {
                        object anaDepoMiktarGuncel = getAnaDepoCommand.ExecuteScalar();
                        listBox4.Items.Add($"Ana Depo: {anaDepoMiktarGuncel ?? 0} ton");
                    }

                    string getSubeMiktarQuery = "SELECT miktar FROM subeler WHERE sube = @sube";
                    using (SqlCommand getSubeMiktarCommand = new SqlCommand(getSubeMiktarQuery, connection))
                    {
                        getSubeMiktarCommand.Parameters.AddWithValue("@sube", sube);
                        object subeMiktar = getSubeMiktarCommand.ExecuteScalar();
                        listBox5.Items.Add($"{sube}: {subeMiktar ?? 0} ton");
                    }
                }
            }
            catch (SqlException sqlEx)
            {
                MessageBox.Show($"Veritabanı hatası: {sqlEx.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            string query = "SELECT miktar FROM anadepo";

            try
            {
                using (SqlConnection connection = dbHelper.GetConnection())
                {
                    SqlCommand command = new SqlCommand(query, connection);
                    object result = command.ExecuteScalar();

                    if (result != null)
                    {
                        textBox9.Text = result.ToString();
                    }
                    else
                    {
                        MessageBox.Show("Anadepo tablosunda miktar bilgisi bulunamadı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button7_Click(object sender, EventArgs e)
        {
            if (!decimal.TryParse(textBox10.Text.Trim(), out decimal eklenecekMiktar) || eklenecekMiktar <= 0)
            {
                MessageBox.Show("Geçerli bir miktar girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                using (SqlConnection connection = dbHelper.GetConnection())
                {
                    decimal mevcutMiktar = 0;
                    string getMiktarQuery = "SELECT miktar FROM anadepo WHERE id = 1";
                    using (SqlCommand getCommand = new SqlCommand(getMiktarQuery, connection))
                    {
                        object result = getCommand.ExecuteScalar();
                        mevcutMiktar = result != null ? Convert.ToDecimal(result) : 0;
                    }

                    decimal toplamMiktar = mevcutMiktar + eklenecekMiktar;

                    if (toplamMiktar > 100)
                    {
                        MessageBox.Show("Ana depo büyüklüğü 100 ton'a kadar uygundur.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    string updateMiktarQuery = "UPDATE anadepo SET miktar = @miktar WHERE id = 1";
                    using (SqlCommand updateCommand = new SqlCommand(updateMiktarQuery, connection))
                    {
                        updateCommand.Parameters.AddWithValue("@miktar", toplamMiktar);
                        updateCommand.ExecuteNonQuery();
                    }

                    MessageBox.Show("Miktar başarıyla güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    textBox9.Text = toplamMiktar.ToString();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Bir hata oluştu: " + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

    }
}
