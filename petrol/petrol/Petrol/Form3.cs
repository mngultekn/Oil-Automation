using System;
using System.Data.SqlClient;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

namespace Petrol
{
    public partial class Form3 : Form
    {
        private DataBaseHelper dbHelper;
        private string ad;
        private string soyad;
        private string sube;

        public Form3(string ad, string soyad, string sube)
        {
            InitializeComponent();
            dbHelper = new DataBaseHelper();
            this.ad = ad;
            this.soyad = soyad;
            this.sube = sube;
            label1.Text = $"Hoşgeldiniz Sayın {ad} {soyad}";
        }

        private void button3_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear(); 
            try
            {
                using (SqlConnection connection = dbHelper.GetConnection())
                {
                    string query = @"SELECT plaka, yakit_miktari FROM bekleyen_islemler WHERE sube = @sube";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sube", this.sube);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string plaka = reader["plaka"].ToString();
                                decimal yakitMiktari = Convert.ToDecimal(reader["yakit_miktari"]);
                                listBox1.Items.Add($"{plaka} - {yakitMiktari} Litre");
                            }
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
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir eleman seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedItem = listBox1.SelectedItem.ToString();
            string plaka = selectedItem.Split('-')[0].Trim();

            try
            {
                using (SqlConnection connection = dbHelper.GetConnection())
                {
                    string deleteQuery = @"DELETE FROM bekleyen_islemler WHERE plaka = @plaka AND sube = @sube";
                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@plaka", plaka);
                        command.Parameters.AddWithValue("@sube", this.sube);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Kayıt başarıyla silindi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            listBox1.Items.Remove(listBox1.SelectedItem); 
                        }
                        else
                        {
                            MessageBox.Show("Silme işlemi sırasında bir hata oluştu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            if (listBox1.SelectedItem == null)
            {
                MessageBox.Show("Lütfen bir eleman seçin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedItem = listBox1.SelectedItem.ToString();
            string plaka = selectedItem.Split('-')[0].Trim();
            decimal yakitMiktari = decimal.Parse(selectedItem.Split('-')[1].Replace("Litre", "").Trim());

            try
            {
                using (SqlConnection connection = dbHelper.GetConnection())
                {
                    string checkSubeQuery = @"SELECT miktar FROM subeler WHERE sube = @sube";
                    decimal mevcutYakit = 0;

                    using (SqlCommand checkCommand = new SqlCommand(checkSubeQuery, connection))
                    {
                        checkCommand.Parameters.AddWithValue("@sube", this.sube);
                        object result = checkCommand.ExecuteScalar();
                        if (result != null)
                        {
                            mevcutYakit = Convert.ToDecimal(result);
                        }
                    }

                    if (mevcutYakit < yakitMiktari / 1000)
                    {
                        MessageBox.Show("Şubenizde yeterli yakıt bulunmamaktadır. Satış işlemi gerçekleştirilemez.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }

                    decimal odeme = yakitMiktari * 40;
                    MessageBox.Show($"{odeme} TL Ödeme alınmıştır.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    decimal satilanYakit = yakitMiktari / 1000;

                    string updateSubeQuery = @"UPDATE subeler 
                                       SET miktar = miktar - @satilanYakit 
                                       WHERE sube = @sube";
                    using (SqlCommand command = new SqlCommand(updateSubeQuery, connection))
                    {
                        command.Parameters.AddWithValue("@satilanYakit", satilanYakit);
                        command.Parameters.AddWithValue("@sube", this.sube);

                        int rowsAffected = command.ExecuteNonQuery();

                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Satılan yakıt miktarı başarıyla güncellendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("Yakıt miktarı güncellenirken bir hata oluştu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    string insertGünsonuQuery = @"INSERT INTO günsonu (sube, plaka, yakit_miktari, odeme, pompacı, tarih)
                                          VALUES (@sube, @plaka, @yakitMiktari, @odeme, @pompcı, @tarih)";
                    using (SqlCommand insertCommand = new SqlCommand(insertGünsonuQuery, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@sube", this.sube);
                        insertCommand.Parameters.AddWithValue("@plaka", plaka);
                        insertCommand.Parameters.AddWithValue("@yakitMiktari", yakitMiktari);
                        insertCommand.Parameters.AddWithValue("@odeme", odeme);
                        insertCommand.Parameters.AddWithValue("@pompcı", this.ad + " " + this.soyad);
                        insertCommand.Parameters.AddWithValue("@tarih", DateTime.Now);

                        int insertRowsAffected = insertCommand.ExecuteNonQuery();

                        if (insertRowsAffected > 0)
                        {
                            MessageBox.Show("İşlem başarıyla kaydedildi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                        {
                            MessageBox.Show("İşlem kaydedilirken bir hata oluştu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }

                    string deleteQuery = "DELETE FROM bekleyen_islemler WHERE plaka = @plaka";
                    using (SqlCommand deleteCommand = new SqlCommand(deleteQuery, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@plaka", plaka);

                        int deleteRowsAffected = deleteCommand.ExecuteNonQuery();

                        if (deleteRowsAffected > 0)
                        {
                            listBox1.Items.Remove(selectedItem);
                        }
                        else
                        {
                            MessageBox.Show("Kayıt silinirken bir hata oluştu.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hata: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

     

        private void button4_Click_1(object sender, EventArgs e)
        {
            listBox2.Items.Clear(); 
            try
            {
                using (SqlConnection connection = dbHelper.GetConnection())
                {
                    string query = @"SELECT ad, soyad FROM is_saatleri 
                             WHERE sube = @sube AND is_bitis IS NULL";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sube", this.sube);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string ad = reader["ad"].ToString();
                                string soyad = reader["soyad"].ToString();
                                listBox2.Items.Add($"{ad} {soyad}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            listBox3.Items.Clear();
            decimal toplamOdeme = 0;
            decimal toplamYakit = 0;

            try
            {
                using (SqlConnection connection = dbHelper.GetConnection())
                {
                    string query = @"SELECT plaka, yakit_miktari, odeme FROM günsonu WHERE sube = @sube AND CAST(tarih AS DATE) = CAST(GETDATE() AS DATE)";
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@sube", this.sube);

                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                string plaka = reader["plaka"].ToString();
                                decimal yakitMiktari = Convert.ToDecimal(reader["yakit_miktari"]);
                                decimal odeme = Convert.ToDecimal(reader["odeme"]);

                                listBox3.Items.Add($"{plaka} - {yakitMiktari} Litre - {odeme} TL");
                                toplamYakit += yakitMiktari;
                                toplamOdeme += odeme;
                            }
                        }
                    }
                }

                textBox1.Text = toplamOdeme.ToString("C2");
                textBox2.Text = toplamYakit.ToString("0.##") + " Litre";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Bir hata oluştu: {ex.Message}", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            try
            {
                using (SqlConnection connection = dbHelper.GetConnection())
                {
                    string deleteQuery = @"DELETE FROM günsonu WHERE sube = @sube AND CAST(tarih AS DATE) = CAST(GETDATE() AS DATE)";
                    using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@sube", this.sube);

                        int rowsAffected = command.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            MessageBox.Show("Günsonu tablosu başarıyla sıfırlandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            listBox3.Items.Clear();
                            textBox1.Clear();
                            textBox2.Clear();
                        }
                        else
                        {
                            MessageBox.Show("Bir hata oluştu. Günsonu tablosu sıfırlanamadı.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
