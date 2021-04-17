using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using Microsoft.Office.Core;
using MSE = Microsoft.Office.Interop.Excel;
using System.Reflection;

namespace HandUpTest
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}
		static string Invert(string s,bool fl)
		{
			if (!fl) return s;
			for (int i = s.Length - 1; i >= 0; --i) if (s[i].Equals('_')) return s.Substring(i + 1);
			return s;
		}
		static double Comp(string ans,string ret,bool fl)
		{
			int[] c_ans = new int[26], c_ret = new int[26];
			int allcs = 0, respond = 0;
			foreach (char i in ans) if (c_ans[i - 'A']++ == 0) ++allcs;
			foreach (char i in ret) if (i < 'A' || i > 'Z' || c_ret[i - 'A']++ > 0) return 0;
			for (int i = 0; i < 26; ++i)
			{
				if (c_ret[i] > 0 && c_ans[i] == 0) return 0;
				if (c_ret[i] > 0 && c_ans[i] > 0) ++respond;
			}
			return respond == 0 ? 0 : (respond == allcs ? 1 : (fl ? 0.5 : 0));
		}
		static bool Check(string ans)
		{
			int[] c_ans = new int[26];
			foreach (char i in ans) if (i < 'A' || i > 'Z' || c_ans[i - 'A']++ > 0) return true;
			return false;
		}
		private void button1_Click(object sender, EventArgs e)
		{
			toolStripProgressBar1.Value = 0;
			openFileDialog1.Title = "选择答案文件";
			openFileDialog1.Filter = "电子表格|*.xlsx;*.xls";
			if (openFileDialog1.ShowDialog() != DialogResult.OK)
			{
				MessageBox.Show("选取文件失败！", "⚠");
				return;
			}
			if (!(folderBrowserDialog1.ShowDialog() == DialogResult.OK && !string.IsNullOrEmpty(folderBrowserDialog1.SelectedPath)))
			{
				MessageBox.Show("选取文件夹失败！", "⚠");
				return;
			}
			string path = openFileDialog1.FileName.ToString();
			string[] files = Directory.GetFiles(folderBrowserDialog1.SelectedPath);
			MSE.Application app = new MSE.Application();
			MSE.Workbooks wbks = app.Workbooks;
			MSE._Workbook _wbk = wbks.Add(path);
			MSE.Sheets shs = _wbk.Sheets;
			MSE._Worksheet _wsh = (MSE._Worksheet)shs.get_Item(1);
			string[] answer = new string[General.prob];
			for (int i = 0; i < General.prob; ++i)
			{
				answer[i] = ((MSE.Range)_wsh.Cells[i + 1, 1]).Text.ToString().ToUpper();
				if (Check(answer[i]))
				{
					MessageBox.Show("第 " + (i + 1).ToString() + " 行答案不符合规范", "⚠");
					return;
				}
			}
			//	_wbk.Close(null, null, null);
			//	wbks.Close();
			int sum = 0, index;
			dataGridView1.AllowUserToAddRows = false;
			dataGridView2.AllowUserToAddRows = false;
			double[] anal = new double[General.prob];
			for (int num = 0; num < files.Length; ++num)
			{
				if (!files[num].EndsWith(".xlsx") && !files[num].EndsWith(".xls")) continue;
				++sum;
				double scr = 0;
				_wbk = wbks.Add(files[num]);
				shs = _wbk.Sheets;
				_wsh = (MSE._Worksheet)shs.get_Item(1);
				for (int i = 0; i < General.prob; ++i)
				{
					double tempor = Comp(answer[i], ((MSE.Range)_wsh.Cells[i + 1, 1]).Text.ToString().ToUpper(), (General.ps[i] & 1) > 0);
					scr += tempor * (General.ps[i] >> 1) / 2.0; anal[i] += tempor * (General.ps[i] >> 1) / 2.0;
				}
				index = dataGridView1.Rows.Add();
				dataGridView1.Rows[index].Cells[0].Value = Invert(Path.GetFileNameWithoutExtension(files[num]), checkBox1.Checked);
				dataGridView1.Rows[index].Cells[1].Value = scr;
				dataGridView1.FirstDisplayedScrollingRowIndex = index;
				_wbk.Close(null, null, null);
				toolStripProgressBar1.Value = (int)Math.Round((num + 1) * 100.0 / files.Length);
			//	wbks.Close();
			}
			MessageBox.Show("共" + sum + "个提交", "ℹ");
			app.Quit();
			System.Runtime.InteropServices.Marshal.ReleaseComObject(app);
			double total = 0, full = 0;
			for (int i = 0; i < General.prob; ++i)
			{
				index = dataGridView2.Rows.Add();
				dataGridView2.Rows[index].Cells[0].Value = (i + 1).ToString();
				dataGridView2.Rows[index].Cells[1].Value = anal[i] / sum;
				dataGridView2.Rows[index].Cells[2].Value = 100 * anal[i] / sum / (General.ps[i] >> 1) * 2;
				total += anal[i]; full += (General.ps[i] >> 1) / 2.0;
			}
			index = dataGridView2.Rows.Add();
			dataGridView2.Rows[index].Cells[0].Value = "总平均分";
			dataGridView2.Rows[index].Cells[1].Value = total / sum;
			dataGridView2.Rows[index].Cells[2].Value = 100 * total / sum / full;
		}

		private void Form1_Load(object sender, EventArgs e)
		{
			comboBox1.Text = "单选 / 判断";
			General.prob = 100;
			General.all = 3;
			General.ps.Clear();
			for (int i = 1; i <= 60; ++i) General.ps.Add(6);
			listBox1.Items.Add("单选 / 判断 1.5 × 60"); General.prob_count.Add(60);
			for (int i = 61; i <= 70; ++i) General.ps.Add(12);
			listBox1.Items.Add("多选 - 无部分分 3.0 × 10"); General.prob_count.Add(10);
			for (int i = 71; i <= 100; ++i) General.ps.Add(4);
			listBox1.Items.Add("单选 / 判断 1 × 30"); General.prob_count.Add(30);
		}

		private void button3_Click(object sender, EventArgs e)
		{
			if (General.all == 0) return;
			listBox1.Items.RemoveAt(--General.all);
			General.ps.RemoveRange(General.prob - General.prob_count.Last(), General.prob_count.Last());
			General.prob -= General.prob_count.Last();
			General.prob_count.RemoveAt(General.all);
		}

		private void button2_Click(object sender, EventArgs e)
		{
			decimal tmp = 2 * numericUpDown1.Value, num=numericUpDown2.Value;
			if (!tmp.Equals(Math.Round(tmp)))
			{
				MessageBox.Show("分值最多精确到半分！", "⚠");
				return;
			}
			if (!num.Equals(Math.Round(num)))
			{
				MessageBox.Show(num.ToString() + "题目数量必须为整数！", "⚠");
				return;
			}
			++General.all;
			int ned = (int)(tmp * 2 + (comboBox1.Text == "多选 - 有部分分" ? 1 : 0));
			for (int i = 0; i < num; ++i) General.ps.Add(ned);
			listBox1.Items.Add(comboBox1.Text + " " + numericUpDown1.Value.ToString() + " × " + num.ToString());
			General.prob += (int)num;
			General.prob_count.Add((int)num);
		}

		private void toolStripStatusLabel2_Click(object sender, EventArgs e)
		{
			MessageBox.Show(" · 成绩报告可以选中行、全选进行删除操作\n · 如果用Ctrl+C复制成绩报告到Office中，请注意选择“选择性粘贴”的“Unicode文本”选项以防止乱码", "ℹ");
		}
	}
}
