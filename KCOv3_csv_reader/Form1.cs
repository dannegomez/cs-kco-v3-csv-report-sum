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

namespace KCOv3_csv_reader
{
    public partial class Form1 : Form
    {
        //Holds the data from csv file
        private IDictionary<string, int> csv_columns = new Dictionary<string, int>();
        private IDictionary<int, string> csv_data = new Dictionary<int, string>();
        
        //delimiter in csv
        char delimiter = ';';

        public Form1()
        {
            InitializeComponent();
            comboBoxDelimiter.Items.Add(';');
            comboBoxDelimiter.Items.Add('\t');
            comboBoxDelimiter.SelectedIndex = 0;
        }

        private void BtnSelectFile_Click(object sender, EventArgs e)
        {
            StreamReader reader;
            string row = "";
            string[] rowArray;
            csv_columns = new Dictionary<string, int>();
            csv_data = new Dictionary<int, string>();

            if (comboBoxDelimiter.SelectedIndex != 0)
            {
                delimiter = comboBoxDelimiter.SelectedItem.ToString().ToCharArray()[0];
            }
            
            openFileDialog1.FileName = string.Empty;
            openFileDialog1.Filter = "KCO v3 custom export|*.csv";
            openFileDialog1.ShowDialog(this);

            //load file
            if (openFileDialog1.FileName.Length > 0)
            {
                txtBoxInputFile.Text = openFileDialog1.FileName;

                //Load columns
                reader = new StreamReader(txtBoxInputFile.Text, System.Text.Encoding.GetEncoding("Windows-1252"));
                row = reader.ReadLine();
                if (row == null)
                {
                    row = "";
                }

                if (row.Length > 0)
                {
                    rowArray = row.Split(delimiter);
                    //add columns
                    for (int i = 0; i < rowArray.Length; i++)
                    {
                        csv_columns.Add(rowArray[i], i);
                    }
                }

                //read all values in csv
                int num_data_rows = 0;
                while (!reader.EndOfStream)
                {
                    row = reader.ReadLine();
                    if (row.Length > 0)
                    {
                        rowArray = row.Split(delimiter);

                        //save currencies
                        addCurrencyInSelect(rowArray[csv_columns["posting_currency"]]);

                        //save row data
                        csv_data.Add(num_data_rows, row);
                        num_data_rows++;
                    }
                }

            }
        }

        void addCurrencyInSelect(string currency)
        {
            currency = removeQuote(currency);

            bool currencyExist = false;
            for (int i = 0; i < comboBoxCurrency.Items.Count; i++)
            {
                if (comboBoxCurrency.Items[i].ToString().Equals(currency))
                {
                    currencyExist = true;
                    break;
                }
            }

            if (!currencyExist)
            {
                comboBoxCurrency.Items.Add(currency);
                comboBoxCurrency.SelectedIndex = 0;
            }
        }

        private void BtnSum_Click(object sender, EventArgs e)
        {
            string[] row_array;
            string selected_currency = comboBoxCurrency.SelectedItem.ToString();

            IDictionary<string, double> sum_total = new Dictionary<string, double>();
            IDictionary<string, double> sum_purchase = new Dictionary<string, double>();
            IDictionary<string, double> sum_fee = new Dictionary<string, double>();
            IDictionary<string, double> sum_return = new Dictionary<string, double>();

            sum_total["ORDERS"] = 0.0;
            sum_total["ORDERS_RETURN"] = 0.0;

            sum_purchase["PURCHASE"] = 0.0;
            sum_fee["FEE_PERCENTAGE"] = 0.0;
            sum_fee["FEE_FIXED"] = 0.0;
            sum_return["RETURN"] = 0.0;


            //sum all data
            for (int i = 0; i < csv_data.Count; i++)
            {
                row_array = csv_data[i].Split(delimiter);

                if (removeQuote(row_array[csv_columns["posting_currency"]]).Equals(selected_currency))
                {
                    sum_total["ORDERS"] += 1.0;
                    bool row_summed = false;

                    //FEE
                    if (removeQuote(row_array[csv_columns["type"]]).Equals("FEE"))
                    {
                        if (removeQuote(row_array[csv_columns["detailed_type"]]).Equals("PURCHASE_FEE_FIXED"))
                        {
                            sum_fee["FEE_FIXED"] += toDouble(row_array[csv_columns["amount"]]);
                            row_summed = true;
                        }

                        else if (removeQuote(row_array[csv_columns["detailed_type"]]).Equals("PURCHASE_FEE_PERCENTAGE"))
                        {
                            sum_fee["FEE_PERCENTAGE"] += toDouble(row_array[csv_columns["amount"]]);
                            row_summed = true;
                        }

                        else
                        {
                            if (!sum_fee.Keys.Contains(removeQuote(row_array[csv_columns["detailed_type"]])))
                            {
                                sum_fee[removeQuote(row_array[csv_columns["detailed_type"]])] = 0.0;
                            }
                            sum_fee[removeQuote(row_array[csv_columns["detailed_type"]])] += toDouble(row_array[csv_columns["amount"]]);
                            row_summed = true;
                        }
                    }

                    //SALE
                    if (removeQuote(row_array[csv_columns["type"]]).Equals("SALE"))
                    {
                        if (removeQuote(row_array[csv_columns["detailed_type"]]).Equals("PURCHASE"))
                        {
                            sum_purchase["PURCHASE"] += toDouble(row_array[csv_columns["amount"]]);
                            row_summed = true;
                        }
                        else
                        {
                            if (!sum_purchase.Keys.Contains(removeQuote(row_array[csv_columns["detailed_type"]])))
                            {
                                sum_purchase[removeQuote(row_array[csv_columns["detailed_type"]])] = 0.0;
                            }
                            sum_purchase[removeQuote(row_array[csv_columns["detailed_type"]])] += toDouble(row_array[csv_columns["amount"]]);
                            row_summed = true;
                        }
                    }

                    //RETURN
                    if (removeQuote(row_array[csv_columns["type"]]).Equals("RETURN"))
                    {
                        if (removeQuote(row_array[csv_columns["detailed_type"]]).Equals("PURCHASE_RETURN"))
                        {
                            sum_return["RETURN"] += toDouble(row_array[csv_columns["amount"]]);
                            sum_total["ORDERS_RETURN"] += 1.0;
                            row_summed = true;
                        }

                        else
                        {
                            if (!sum_return.Keys.Contains(removeQuote(row_array[csv_columns["detailed_type"]])))
                            {
                                sum_return[removeQuote(row_array[csv_columns["detailed_type"]])] = 0.0;
                            }
                            sum_return[removeQuote(row_array[csv_columns["detailed_type"]])] += toDouble(row_array[csv_columns["amount"]]);
                            row_summed = true;
                        }
                    }


                    if (!row_summed)
                    {
                        MessageBox.Show("Error, unknown row type: " + removeQuote(row_array[csv_columns["type"]]) + " : " + removeQuote(row_array[csv_columns["detailed_type"]]));
                    }
                }
            }

            DataTable dt = new DataTable();
            dt.Columns.Add("Sort");
            dt.Columns.Add("Type");
            dt.Columns.Add("Type Detail");
            dt.Columns.Add("Sum");
            dt.Columns.Add("Currency");


            dt.Rows.Add("1", "TOTAL", "ORDERS", Int16.Parse(sum_total["ORDERS"].ToString()), "");
            dt.Rows.Add("1", "TOTAL", "RETURN", Int16.Parse(sum_total["ORDERS_RETURN"].ToString()), "");

            foreach (var pair in sum_purchase)
            {
                dt.Rows.Add("2", "SALE", pair.Key, pair.Value, comboBoxCurrency.SelectedItem.ToString());
            }

            foreach (var pair in sum_return)
            {
                dt.Rows.Add("3", "RETURN", pair.Key, pair.Value, comboBoxCurrency.SelectedItem.ToString());
            }

            foreach (var pair in sum_fee)
            {
                dt.Rows.Add("4", "FEE", pair.Key, pair.Value, comboBoxCurrency.SelectedItem.ToString());
            }

            dataGridView1.DataSource = dt;
            dataGridView1.Columns[0].Width = 30;
            dataGridView1.Columns[1].Width = 60;
            dataGridView1.Columns[2].Width = 180;

        }


        string removeQuote(string text)
        {
            text = text.Replace("\"", "");
            return text;
        }

        double toDouble(string text)
        {
            text = text.Replace("\"", "");
            text = text.Replace(".", ",");
            return Convert.ToDouble(text);
        }
    }
}
