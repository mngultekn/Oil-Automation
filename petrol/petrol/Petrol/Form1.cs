using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace Petrol
{
    public partial class Form1 : Form
    {
        private DataBaseHelper dbHelper; 

        public Form1()
        {
            InitializeComponent();
            dbHelper = new DataBaseHelper();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            string kullaniciAd = textBox1.Text;
            string sifre = textBox2.Text;

            using (SqlConnection connection = dbHelper.GetConnection())
            {
                try
                {
                    string query = "SELECT ad, soyad, yetki, sube FROM kullanici WHERE kullanici_ad = @kullaniciAd AND sifre = @sifre";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@kullaniciAd", kullaniciAd);
                    command.Parameters.AddWithValue("@sifre", sifre);

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            string ad = reader["ad"].ToString();
                            string soyad = reader["soyad"].ToString();
                            string yetki = reader["yetki"].ToString();
                            string sube = reader["sube"].ToString();
                            switch (yetki)
                            {
                                case "patron":
                                    Form2 form2 = new Form2();
                                    form2.Show();
                                    this.Hide();
                                    break;
                                case "subemüdürü":
                                    Form3 form3 = new Form3(ad, soyad,sube);
                                    form3.Show();
                                    this.Hide();
                                    break;
                                case "pompacı":
                                    Form4 form4 = new Form4(ad, soyad,sube);
                                    form4.Show();
                                    this.Hide();
                                    break;
                                default:
                                    MessageBox.Show("Yetki tanımlı değil!");
                                    break;
                            }
                        }
                        else
                        {
                            MessageBox.Show("Kullanıcı adı veya şifre hatalı!");
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Bir hata oluştu: " + ex.Message);
                }
            }
        }
    }


}
