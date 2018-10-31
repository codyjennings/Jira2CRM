using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using CsvHelper;
using System.Collections;
using System.Dynamic;

namespace Jira_Comment_Extraction
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }




        class DataRecord
        {
            public String SRName { get; set; }
            public String LastComment { get; set; }
            public String CrmId { get; set; }
        }


        private void cmdStart_Click(object sender, EventArgs e)
        {
            List<string> srJiraID = new List<string>();
            List<int> srJiraCommentCount = new List<int>();
            List<string> crmJiraID = new List<string>();
            using (var sr = new StreamReader(srFile.Text))
            {
                var reader = new CsvReader(sr);
                var records = reader.GetRecords<DataRecord>().ToList();
                foreach (var r in records)
                {
                    srJiraID.Add(r.SRName);
                    srJiraCommentCount.Add(Int32.Parse(r.LastComment));
                    crmJiraID.Add(r.CrmId);
                }
            }
            List<int> newSRJiraCommentCount = srJiraCommentCount.ToList();

            using (StreamWriter sw = new StreamWriter(logFile.Text, true))
            {
                for (int a = 0; a < srJiraCommentCount.Count; a++)
                {
                    string srID = srJiraID[a];
                    int srLastCommentID = srJiraCommentCount[a];
                    RESTClient rClient = new RESTClient();
                    rClient.endPoint = endpoint.Text + srID + "/comment/";
                    rClient.authType = authenticationType.Basic;
                    rClient.userName = username.Text;
                    rClient.passWord = password.Text;
                    debugOutput("Rest Client Created");
                    string strResponse = string.Empty;
                    strResponse = rClient.makeRequest();
                    JiraParser.RootObject jira = JsonConvert.DeserializeObject<JiraParser.RootObject>(strResponse);
                    try
                    {
                        var commentTotal = jira.total;
                        string commentTotalString = jira.total.ToString();
                        int commentID = srLastCommentID;
                        while (commentID < commentTotal)
                        {
                            var author = jira.Comments[commentID].author.displayName.ToString();
                            var body = jira.Comments[commentID].body.ToString();
                            var create = jira.Comments[commentID].created.ToLocalTime();
                            sw.WriteLine(create);
                            sw.WriteLine(author);
                            sw.WriteLine(body);
                            sw.WriteLine();
                            using (StreamWriter swComment = new StreamWriter("C:\\Users\\cjennings\\Desktop\\Projects\\Jira Comment Extraction\\Comments\\" + srID + ".comment" + commentID, true))
                            {
                                swComment.WriteLine(create);
                                swComment.WriteLine(author);
                                swComment.WriteLine(body);
                            }
                            commentID++;
                            newSRJiraCommentCount[a] = commentTotal;
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.Write(ex.Message, ToString() + Environment.NewLine);
                    }
                }
                try
                {
                    using (var newSRFile = new StreamWriter("new" + srFile.Text))
                    {
                        using (var nsf = new CsvWriter(newSRFile))
                        {
                            for (int a = 0; a < srJiraID.Count; a++)
                            {
                                var records = new List<DataRecord>
                                {
                                    new DataRecord { SRName = srJiraID[a], LastComment = newSRJiraCommentCount[a].ToString(), CrmId= crmJiraID[a]}
                                };
                                nsf.WriteRecords(records);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.Write(ex.Message, ToString() + Environment.NewLine);
                }
                using (var newSRFile = new StreamWriter(srFile.Text))
                {
                    using (var nsf = new CsvWriter(newSRFile))
                    {
                        for (int a = 0; a < srJiraID.Count; a++)
                        {
                            var records = new List<DataRecord>
                            {
                                new DataRecord { SRName = srJiraID[a], LastComment = newSRJiraCommentCount[a].ToString(),CrmId= crmJiraID[a]}
                            };
                            nsf.WriteRecords(records);
                        }
                    }   
                }
            }
        }

        private void debugOutput(string strDebugText)
        {
            try
            {
                System.Diagnostics.Debug.Write(strDebugText + Environment.NewLine);
                logOutput.Text = logOutput.Text + strDebugText + Environment.NewLine;
                logOutput.SelectionStart = logOutput.TextLength;
                logOutput.ScrollToCaret();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex.Message, ToString() + Environment.NewLine);
            }
        }

        private void cmdChangeSRFile_Click(object sender, EventArgs e)
        {
            try
            {
                var fileDialog = new OpenFileDialog();
                fileDialog.Title = "Jira Log File";
                fileDialog.RestoreDirectory = true;
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    srFile.Text = fileDialog.FileName;
                }
            }
            catch(Exception ex)
            {
                System.Diagnostics.Debug.Write(ex.Message, ToString() + Environment.NewLine);
            }


        }

        private void cmdChangeLogFile_Click(object sender, EventArgs e)
        {
            using (var folderDialog = new FolderBrowserDialog())
            {
                folderDialog.Description = "Select Log Location";
                if (folderDialog.ShowDialog() == DialogResult.OK)
                {
                    logFile.Text = folderDialog.SelectedPath + "\\JiraComments.log";
                }

                folderDialog.Dispose();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            logFile.Text = "C:\\Users\\cjennings\\Desktop\\Jira Comments.log";
            srFile.Text = "C:\\Users\\cjennings\\Desktop\\Projects\\Jira Comment Extraction\\Comments\\CaseSR.jira";
            endpoint.Text = "http://qtest.neptunetg.com:8080/rest/api/2/issue/";

            string srCSV = File.ReadAllText(srFile.Text);

            LoadDataGrid();




        }
        


        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }



        public void LoadDataGrid()
        {
            try
            {
                DataTable dataTable = new DataTable();
                dataTable.Columns.Add("SR ID");
                dataTable.Columns.Add("Last Comment Retrieved");
                dataTable.Columns.Add("Case ID");
                using (var srDT = new StreamReader(srFile.Text))
                {
                    string[] totalData = new string[File.ReadAllLines(srFile.Text).Length];
                    totalData = srDT.ReadLine().Split(',');
                    while (!srDT.EndOfStream)
                    {
                        totalData = srDT.ReadLine().Split(',');
                        dataTable.Rows.Add(totalData[0], totalData[1], totalData[2]);
                    }
                    dataGridView1.DataSource = dataTable;
                }
                    debugOutput("DataGrid successfully loaded.");
                return;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.Write(ex.Message, ToString() + Environment.NewLine);
                debugOutput("DataGrid did not load. No action performed.");
                return;
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {
            


        }

        private void button2_Click(object sender, EventArgs e)
        {
            //var newForm = new EditRow();
            //newForm.Show();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            var newForm = new Form();
            newForm.Show();
        }
    }
}