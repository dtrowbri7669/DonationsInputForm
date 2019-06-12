using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Text.RegularExpressions;


namespace DonationsForm
{
    public partial class Form1 : Form
    {
        private FileStream file;
        private FileStream HazFile;
        private StreamReader ReadingRainbow;
        private StreamWriter stream;
        private DateTime date = DateTime.Now;
        private string filename;
        private string hazFileName = "HazType.txt";
        private int size = 96;
        private string[] profile; 
        private double[] disposal;
        private bool[] pounds; 
        private const string Delim = ",";
        private int fileCount = 1;
        public Form1()
        {
            InitializeComponent();
            profile = new string[size];
            disposal = new double[size];
            pounds = new bool[size];
            OpenHazFile();
            ReadHaz();
            filename = "Donations_" + date.Month + "_" + date.Day + "_" + date.Year + ".csv";
            if (!File.Exists(filename))
            {
                CreateFile();
            }
            else
            {
                 OpenFile();
            }
        }
        //Clears configs
        private void btnClear_Click(object sender, EventArgs e)
        {
            clear();
        }

        private void clear()
        {
            txtAsin.Clear();
            txtCase.Clear();
            txtPallet.Clear();
            txtTitle.Clear();
            txtValue.Clear();
            txtWeight.Clear();
            txtProduct.Clear();
            nudQty.Value = 1;
        }

        //Validates logins
        private bool ValidateLogin()
        {
            string pattern = @"^[a-zA-Z]+$";
            Regex regex = new Regex(pattern);
            bool match = regex.IsMatch(txtLogin.Text);
            if (!regex.IsMatch(txtLogin.Text))
            {
                InvalidInput(txtLogin);
            }
            return match;
        }
        //Validates price
        private bool ValidateValue()
        {
            string pattern = @"^\d+[.]\d{1,2}$|^\d+$";
            Regex regex = new Regex(pattern);
            bool match = regex.IsMatch(txtValue.Text);
            if (!match)
            {
                InvalidInput(txtValue);
            }
            return match;
        }
        //Validates weight of object
        private bool ValidateWeight()
        {
            string pattern = @"^\d{1,3}\.\d{1,2}$|^\d{1,3}$";
            Regex regex = new Regex(pattern);
            bool match = regex.IsMatch(txtWeight.Text);
            if (!match)
            {
                InvalidInput(txtWeight);
            }
            return match;
        }
        //checks if remaining fields are null
        private bool IsFieldsNull()
        {
            bool asin = txtAsin.TextLength == 0;
            bool title = txtTitle.TextLength == 0;
            bool pallet = txtPallet.TextLength == 0;
            bool lcase = txtCase.TextLength == 0;
            bool ptype = txtProduct.TextLength == 0;
            bool valid = true;
            if (asin || title || pallet || lcase || ptype)
                valid = false;
            return valid;
        }

        //changes text back after input has been correctedl
        private void txtChange(object sender, EventArgs e)
        {
            TextBox current = sender as TextBox;
            current.ForeColor = Color.Black;
            current.BackColor = Color.White;
        }
        //Changes text when incorrect
        private void InvalidInput(TextBox t)
        {
            t.ForeColor = Color.Red;
            t.BackColor = Color.LightSalmon;
        }


        //checks submit criteria
        private bool CheckInput()
        {
            bool value =ValidateValue();
            bool weight = ValidateWeight();
            bool login = ValidateLogin();
            bool fields = IsFieldsNull();
            bool valid = true;
            if (!value || !weight || !login || !fields)
               valid = false;
            return valid;
        }
        //submits input
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            //runs validation
            if (CheckInput())
                writeToFile();
        }

        //Create File
        private void CreateFile()
        {
            try
            {
                file = new FileStream(filename, FileMode.Create, FileAccess.Write);
                stream = new StreamWriter(file);
                string x = "Asin" + Delim + "Title" + Delim + "QTY" + Delim + "Weight/Unit" +
                        Delim + "Price/Unit" + Delim + "Total Weight" + Delim + "Total Value" +
                        Delim + "Date" + Delim + "Pallet" + Delim + "Case" + Delim + "Product Type" +
                        Delim + "Login" + Delim + "Profile Type" + Delim + "Disposal Price";
                stream.WriteLine(x);
                stream.Flush();
            }catch(IOException)
            {
                filename = "Donations_" + date.Month + "_" + date.Day + "_" + date.Year + "(" + fileCount.ToString() + ").csv";
                fileCount++;
                CreateFile();
            }
        }
        
        //open file
        private void OpenFile()
        {

            try
            {
                file = new FileStream(filename, FileMode.Append, FileAccess.Write);
                stream = new StreamWriter(file);
            }
            catch(IOException)
            {
                filename = "Donations_" + date.Month + "_" + date.Day + "_" + date.Year + "(" + fileCount.ToString() + ").csv";
                fileCount++;
                CreateFile();
            }
        }

        //gets current input, puts it into a string
        private void writeToFile()
        {
            double price;
            double weight;
            try
            {
                price = Convert.ToDouble(txtValue.Text) * Convert.ToDouble(nudQty.Value);
                weight = Convert.ToDouble(txtWeight.Text) * Convert.ToDouble(nudQty.Value);
                double disposalPrice = 0;
                int sel = cmbProfile.SelectedIndex;
                if(pounds[sel])
                {
                    disposalPrice = disposal[sel] * weight;
                }
                else
                {
                    disposalPrice = disposal[sel] * Convert.ToDouble(nudQty.Value);
                }
                string title = txtTitle.Text;
                title = title.Replace(Delim, "");
                string x = "";
                x += txtAsin.Text + Delim + title + Delim + nudQty.Value + Delim;
                x += txtWeight.Text + Delim + "$" + txtValue.Text + Delim + weight.ToString() + Delim;
                x += price.ToString() + Delim;
                x += date.Month + "/" + date.Day + "/" + date.Year + "/" + " " + date.Hour + ":" + date.Minute + ":" + date.Second;
                x += Delim + txtPallet.Text + Delim;
                x += txtCase.Text + Delim + txtProduct.Text + Delim + txtLogin.Text + Delim + profile[sel].ToString();
                x += Delim + "$" + disposalPrice.ToString();
                stream.WriteLine(x);
                stream.Flush();
                clear();
            }
            catch(InvalidCastException)
            {
                MessageBox.Show("The values in the Price or Weight field are invalid. Please correct and try again.");
            }  
        }


        //Close File
        private void CloseFile()
        {
            stream.Flush();
            stream.Close();
            file.Close();
        }

        private void OpenHazFile()
        {
            try
            {
                HazFile = new FileStream(hazFileName, FileMode.Open, FileAccess.Read);
                ReadingRainbow = new StreamReader(HazFile);
            }
            catch(FileNotFoundException)
            {
                MessageBox.Show("The file \"HazType.txt\" is missing or renamed.");
            }
        }

        private void ReadHaz()
        {
            int count = 0;
            while(!ReadingRainbow.EndOfStream)
            {
                string line = ReadingRainbow.ReadLine();
                string[] input = new string[3];
                input = line.Split(',');
                profile[count] = input[0];
                disposal[count] = Convert.ToDouble(input[1]);
                if (input[2].Equals("lb"))
                {
                    pounds[count] = true;
                }
                else
                {
                    pounds[count] = false;
                }
                count++;
            }
            setCmb();
        }
        
        private void setCmb()
        {
            cmbProfile.DataSource = profile;
        }

        //closes file when form closes
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseFile();
        }
    }
}
