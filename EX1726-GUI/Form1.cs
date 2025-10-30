using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Management;
using System.Threading;
using System.IO.Ports;
using System.Xml.Linq;
using System.Runtime.Remoting.Contexts;

namespace EX1726_GUI
{
    public partial class EX1726_GUI : Form
    {
        public EX1726_GUI()
        {
            if (Properties.Settings.Default.Language != "")
            {
                InitLanguage(Properties.Settings.Default.Language);
                if (Properties.Settings.Default.Language == "en")
                    englishToolStripMenuItem.Checked = true;
                else
                    chineseToolStripMenuItem.Checked = true;
            }
            else
            {
                InitializeComponent();
                InitUserChannelDataGrid();
                InitITU_SimpChannelDataGrid();
                InitFreqRangeDataGrid();

                tabs.Width = this.Size.Width - 40;
                tabs.Height = this.Size.Height - 90;

            }
        }


        DataTable dtUserCH = null;
        DataTable dtITUSimp = null;
        DataTable dtFreqRange = null;

        ToolStripMenuItem menuFile1 = null;
        ToolStripMenuItem menuFile2 = null;
        ToolStripMenuItem menuFile3 = null;
        private void RefreshFileHistory()
        {
            if (menuFile1 != null) MenuFile.DropDownItems.Remove(menuFile1);
            if (menuFile2 != null) MenuFile.DropDownItems.Remove(menuFile2);
            if (menuFile3 != null) MenuFile.DropDownItems.Remove(menuFile3);

            if (Properties.Settings.Default.LastFile1 != null && Properties.Settings.Default.LastFile1 != "")
            {
                menuFile1 = new ToolStripMenuItem();
                menuFile1.Name = "LastFile1";
                menuFile1.Size = new System.Drawing.Size(121, 22);
                menuFile1.Text = Properties.Settings.Default.LastFile1;
                menuFile1.Click += new System.EventHandler(this.MenuHistortFileLoad_Click);
                MenuFile.DropDownItems.Add(menuFile1);
            }

            if (Properties.Settings.Default.LastFile2 != null && Properties.Settings.Default.LastFile2 != "")
            {
                menuFile2 = new ToolStripMenuItem();
                menuFile2.Name = "LastFile2";
                menuFile2.Size = new System.Drawing.Size(121, 22);
                menuFile2.Text = Properties.Settings.Default.LastFile2;
                menuFile2.Click += new System.EventHandler(this.MenuHistortFileLoad_Click);
                MenuFile.DropDownItems.Add(menuFile2);
            }


            if (Properties.Settings.Default.LastFile3 != null && Properties.Settings.Default.LastFile3 != "")
            {
                menuFile3 = new ToolStripMenuItem();
                menuFile3.Name = "LastFile3";
                menuFile3.Size = new System.Drawing.Size(121, 22);
                menuFile3.Text = Properties.Settings.Default.LastFile3;
                menuFile3.Click += new System.EventHandler(this.MenuHistortFileLoad_Click);
                MenuFile.DropDownItems.Add(menuFile3);
            }

        }
        string SelectedSerialName = "";
        private void MenuComPortSelected_Click(object sender, EventArgs e)
        {
            foreach (ToolStripMenuItem com in MenuClonePort.DropDownItems) {com.Checked = false;}
            ((ToolStripMenuItem)sender).Checked = true;

            Properties.Settings.Default.ComName = ((ToolStripMenuItem)sender).Text;
            Properties.Settings.Default.Save();
            SelectedSerialName = Properties.Settings.Default.ComName.Split(' ')[0];

        }

        private void ScanCommPort()
        {
            using (var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_PnPEntity"))
            {
                var devices = searcher.Get();
                SortedDictionary<int, string> Ports = new SortedDictionary<int, string>();

                foreach (ManagementObject device in devices)
                {
                    string name = device["name"] as string;
                    if (name != null && name.Contains("(COM"))
                    {
                        var comname = name.Split(new string[] { "(COM" }, 2, StringSplitOptions.None); //CP2102 (COM3 ->COM22)
                        Match match = Regex.Match(comname[1], @"^\d{1,3}");
                        if (match.Success)
                        {
                            Ports.Add(int.Parse(match.Value), "COM" + match.Value + " " + comname[0]);
                        }
                    }
                }

                MenuClonePort.DropDownItems.Clear();
                foreach (var port in Ports)
                { 
                    var comMenu = new ToolStripMenuItem();
                    comMenu.Text = port.Value;
                    comMenu.Click += new EventHandler(MenuComPortSelected_Click);
                    if (port.Value == Properties.Settings.Default.ComName)
                    { 
                        comMenu.Checked = true;
                        SelectedSerialName = port.Value.Split(' ')[0];
                    }

                    MenuClonePort.DropDownItems.Add(comMenu);

                }
            }
        }

        private void frmEX1726_Load(object sender, EventArgs e)
        {
            menuUnitMHz.Checked = Properties.Settings.Default.UnitMHz;
            menuUnitkHz.Checked = !menuUnitMHz.Checked;
            ScanCommPort();
            RefreshFileHistory();

            
            cbJ3E.SelectedIndex = 1;
            cbR3E.SelectedIndex = 0;
            cbH3E.SelectedIndex = 1;
            cbLSB.SelectedIndex = 0;
            cbJ2B.SelectedIndex = 0;
            cbFSK.SelectedIndex = 0;
            cbA1A.SelectedIndex = 1;

            cbTxMeter.SelectedIndex = 0;
            cbTxPwrSel.SelectedIndex = 2;
            cbModeSel2182.SelectedIndex = 1;
            cbITU_FSKch.SelectedIndex = 1;
            cbITU_Direction.SelectedIndex = 0;
            cbATU.SelectedIndex = 0;
            cbAutoTuning_use.SelectedIndex = 0;
            cbAutoTuningType.SelectedIndex = 0;
            cb2182sel_atALM.SelectedIndex = 0;
            cbScanType.SelectedIndex = 1;
            cbIndType.SelectedIndex = 0;
            cb_NarrowFilter.SelectedIndex = 0;
            cbFSK_Shift.SelectedIndex = 0;
            cbFSK_Pol.SelectedIndex = 0;
            cbCW_BreakIn.SelectedIndex = 2;
            cbACCMode_input.SelectedIndex = 0;
            cbMicAudio_input.SelectedIndex = 0;
            cbNMEA_Jack.SelectedIndex = 0;
            cbCrossChOper.SelectedIndex = 0;
            cbITUchUSE.SelectedIndex = 1;
            cbFreqProg.SelectedIndex = 1;


        }

        private void SwapUnit()
        {
            decimal factor = 1000;
            string Format = "0.000";
            if (Properties.Settings.Default.UnitMHz)
            {
                factor = (decimal)0.001;
                Format = "0.000000";
            }

            for (int i = 0; i < 161; i++)
            {
                if (dtUserCH.Rows[i][1].ToString() != "") dtUserCH.Rows[i][1] = (decimal.Parse(dtUserCH.Rows[i][1].ToString()) * factor).ToString(Format);
                if (dtUserCH.Rows[i][2].ToString() != "") dtUserCH.Rows[i][2] = (decimal.Parse(dtUserCH.Rows[i][2].ToString()) * factor).ToString(Format);
            }
            for (int i = 0; i < 72; i++)
            {
                if (dtITUSimp.Rows[i][1].ToString() != "") dtITUSimp.Rows[i][1] = (decimal.Parse(dtITUSimp.Rows[i][1].ToString()) * factor).ToString(Format);
            }
            for (int i = 0; i < 20; i++)
            {
                if (dtFreqRange.Rows[i][1].ToString() != "") dtFreqRange.Rows[i][1] = (decimal.Parse(dtFreqRange.Rows[i][1].ToString()) * factor).ToString(Format);
                if (dtFreqRange.Rows[i][2].ToString() != "") dtFreqRange.Rows[i][2] = (decimal.Parse(dtFreqRange.Rows[i][2].ToString()) * factor).ToString(Format);
            }
        }

        private void InitUserChannelDataGrid()
        {
            dgUserCh.Columns.Clear();
            if (dtUserCH == null)
            {
                dtUserCH = new DataTable();

                dtUserCH.Columns.Add("ID");
                dtUserCH.Columns.Add("LowLimit");
                dtUserCH.Columns.Add("HighLimit");
                dtUserCH.Columns.Add("MODE");
                dtUserCH.Columns.Add("COMMENT");

                dtUserCH.Columns[0].ColumnName = "CH No.";
                dtUserCH.Columns[1].ColumnName = "Rx FREQ. [kHz]";
                dtUserCH.Columns[2].ColumnName = "Tx FREQ. [kHz]";
                dtUserCH.Columns[3].ColumnName = "MODE";
                dtUserCH.Columns[4].ColumnName = "COMMENT";



                if (Properties.Settings.Default.UnitMHz)
                {
                    dtUserCH.Rows.Add("0", "2.18200", "2.18200", "AM", "EMERGEN");
                }
                else
                {
                    dtUserCH.Rows.Add("0", "2182.00", "2182.00", "AM", "EMERGEN");
                }
                for (int i = 1; i < 161; i++)
                {
                    dtUserCH.Rows.Add(i.ToString(), "", "", "", "");
                }

            }
            dgUserCh.AutoGenerateColumns = false;


            DataGridViewColumn colID = new DataGridViewTextBoxColumn();
            colID.Name = "ID";
            colID.DataPropertyName = "CH No.";
            colID.HeaderText = "CH No.";

            DataGridViewColumn colRxFreq = new DataGridViewTextBoxColumn();
            colRxFreq.DataPropertyName = "Rx FREQ. [kHz]";
            colRxFreq.HeaderText = "Rx FREQ. [kHz]";

            DataGridViewColumn colTxFreq = new DataGridViewTextBoxColumn();
            colTxFreq.DataPropertyName = "Tx FREQ. [kHz]";
            colTxFreq.HeaderText = "Tx FREQ. [kHz]";


            DataGridViewColumn colComment = new DataGridViewTextBoxColumn();
            colComment.DataPropertyName = "COMMENT";
            colComment.HeaderText = "COMMENT";

            DataGridViewComboBoxColumn colMode = new DataGridViewComboBoxColumn();
            colMode.DataPropertyName = "MODE";
            colMode.HeaderText = "Mode";
            colMode.Items.AddRange("USB", "R3E", "AM", "LSB", "J2B", "FSK", "CW");

            if (Properties.Settings.Default.UnitMHz)
            {
                colRxFreq.HeaderText = "Rx FREQ. [MHz]";
                colTxFreq.HeaderText = "Tx FREQ. [MHz]";
            }

            dgUserCh.Columns.Add(colID);
            dgUserCh.Columns.Add(colRxFreq);
            dgUserCh.Columns.Add(colTxFreq);
            dgUserCh.Columns.Add(colMode);
            dgUserCh.Columns.Add(colComment);
            dgUserCh.DataSource = dtUserCH;
            dgUserCh.Columns[0].Width = 65;
            dgUserCh.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }

        private void InitITU_SimpChannelDataGrid()
        {
            string[] Prefix = { "4-", "6-", "8-", "12-", "16-", "18-", "22-", "25-" };

            dgITUSimp.Columns.Clear();
            if (dtITUSimp == null)
            {
                dtITUSimp = new DataTable();

                dtITUSimp.Columns.Add("CH");
                dtITUSimp.Columns.Add("Freq");
                dtITUSimp.Columns.Add("MODE");
                dtITUSimp.Columns.Add("COMMENT");

                dtITUSimp.Columns[0].ColumnName = "CH No.";
                dtITUSimp.Columns[1].ColumnName = "FREQ. [kHz]";
                dtITUSimp.Columns[2].ColumnName = "MODE";
                dtITUSimp.Columns[3].ColumnName = "COMMENT";


                for (int i = 0; i < 72; i++)
                {
                    dtITUSimp.Rows.Add(Prefix[i / 9] + (i % 9 + 1).ToString(), "", "", "");
                }
            }
            dgITUSimp.AutoGenerateColumns = false;


            DataGridViewColumn colCH = new DataGridViewTextBoxColumn();
            colCH.Name = "CH";
            colCH.DataPropertyName = "CH No.";
            colCH.HeaderText = "CH No.";

            DataGridViewColumn colFreq = new DataGridViewTextBoxColumn();
            colFreq.DataPropertyName = "FREQ. [kHz]";
            colFreq.HeaderText = "FREQ. [kHz]";


            DataGridViewColumn colComment = new DataGridViewTextBoxColumn();
            colComment.DataPropertyName = "COMMENT";
            colComment.HeaderText = "COMMENT";

            DataGridViewComboBoxColumn colMode = new DataGridViewComboBoxColumn();
            colMode.DataPropertyName = "MODE";
            colMode.HeaderText = "Mode";
            colMode.Items.AddRange("USB", "R3E", "AM", "LSB", "J2B", "FSK", "CW");


            if (Properties.Settings.Default.UnitMHz)
            {
                colFreq.HeaderText = "FREQ. [MHz]";
            }

            dgITUSimp.Columns.Add(colCH);
            dgITUSimp.Columns.Add(colFreq);
            dgITUSimp.Columns.Add(colMode);
            dgITUSimp.Columns.Add(colComment);

            dgITUSimp.DataSource = dtITUSimp;
            dgITUSimp.Columns[0].Width = 65;
            dgITUSimp.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;

        }
        private void InitFreqRangeDataGrid()
        {

            dgFreqRange.Columns.Clear();
            if (dtFreqRange == null)
            {
                dtFreqRange = new DataTable();

                dtFreqRange.Columns.Add("ITEM");
                dtFreqRange.Columns.Add("LowerEdge");
                dtFreqRange.Columns.Add("HigherEdge");

                dtFreqRange.Columns[0].ColumnName = "ITEM";
                dtFreqRange.Columns[1].ColumnName = "Lower Edge [kHz]";
                dtFreqRange.Columns[2].ColumnName = "Higher Edge [kHz]";




                for (int i = 0; i < 20; i++)
                {
                    dtFreqRange.Rows.Add(i.ToString(), "", "");
                }
            }
            dgFreqRange.AutoGenerateColumns = false;


            DataGridViewColumn colITEM = new DataGridViewTextBoxColumn();
            colITEM.DataPropertyName = "ITEM";
            colITEM.HeaderText = "ITEM";

            DataGridViewColumn colLowFreq = new DataGridViewTextBoxColumn();
            colLowFreq.DataPropertyName = "Lower Edge [kHz]";
            colLowFreq.HeaderText = "Lower Edge [kHz]";


            DataGridViewColumn colHighFreq = new DataGridViewTextBoxColumn();
            colHighFreq.DataPropertyName = "Higher Edge [kHz]";
            colHighFreq.HeaderText = "Higher Edge [kHz]";


            if (Properties.Settings.Default.UnitMHz)
            {
                colLowFreq.HeaderText = "Lower Edge [MHz]";
                colHighFreq.HeaderText = "Higher Edge [MHz]";
            }



            dgFreqRange.Columns.Add(colITEM);
            dgFreqRange.Columns.Add(colLowFreq);
            dgFreqRange.Columns.Add(colHighFreq);

            dgFreqRange.DataSource = dtFreqRange;
            dgFreqRange.Columns[0].Width = 65;
            dgFreqRange.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void frmEX1726_SizeChanged(object sender, EventArgs e)
        {
            //560, 400
            //600, 490
            tabs.Width = this.Size.Width - 40;
            tabs.Height = this.Size.Height - 90;
        }

        private void tabs_SizeChanged(object sender, EventArgs e)
        {
            //560, 400
            //540, 390
            dgFreqRange.Width = tabs.Size.Width - 20;
            dgFreqRange.Height = tabs.Size.Height - 10;


            dgITUSimp.Width = tabs.Size.Width - 20;
            dgITUSimp.Height = tabs.Size.Height - 10;


            dgUserCh.Width = tabs.Size.Width - 20;
            dgUserCh.Height = tabs.Size.Height - 10;
        }

        public static string BytesToHEXString(byte[] bytes, bool split = true)
        {
            if (bytes == null || bytes.Length == 0) return "";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                if (split)
                    sb.AppendFormat("{0:X2} ", bytes[i]);
                else
                    sb.AppendFormat("{0:X2}", bytes[i]);
            }
            return sb.ToString().TrimEnd();
        }


        public static string BytesToASCIIString(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return "";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] >= 32 && bytes[i] <= 126)
                    sb.Append((char)bytes[i]);
                else
                    sb.Append('.');
            }
            return sb.ToString();
        }
        private static byte[] HexString2Bytes(string str)
        {
            byte[] bytes = new byte[str.Length / 2];
            for (int i = 0; i < str.Length; i += 2)
            {
                string byteStr = str.Substring(i, 2);
                bytes[i / 2] = Convert.ToByte(byteStr, 16);
            }
            return bytes;
        }

        private static bool GetAddrAndLength(string head, ref int Addr, ref int Length)
        {
            byte[] bytes = HexString2Bytes(head);
            if (bytes != null)
            {
                Addr = bytes[0] << 8 | bytes[1];
                Length = bytes[2];
                return true;
            }
            return false;
        }
        List<string> ModeDefs = new List<string>() { "USB", "R3E", "AM", "LSB", "J2B", "FSK", "CW" };

        //2. User Channel Config - 2576 Bytes
        //3. ITU Channel Config - 1152 Bytes
        private byte[] getUser_n_ITU_ChannelBytes()
        {
            List<byte> bytes = new List<byte>();

            for (int i = 0; i < 161 + 72; i++)
            {
                string sRxFreq, sTxFreq, Mode, Comment;
                if (i <= 160)
                {
                    sRxFreq = (string)dtUserCH.Rows[i][1];
                    sTxFreq = (string)dtUserCH.Rows[i][2];
                    Mode = (string)dtUserCH.Rows[i][3];
                    Comment = ((string)dtUserCH.Rows[i][4]).PadRight(7, ' ');
                }
                else
                {
                    sRxFreq = (string)dtITUSimp.Rows[i - 161][1];
                    sTxFreq = "";
                    Mode = (string)dtITUSimp.Rows[i - 161][2];
                    Comment = ((string)dtITUSimp.Rows[i - 161][3]).PadRight(7, ' ');
                }

                decimal RxFreq, TxFreq;
                bool RxFreqValid = decimal.TryParse(sRxFreq, out RxFreq);
                bool TxFreqValid = decimal.TryParse(sTxFreq, out TxFreq);
                if (RxFreqValid)
                {
                    if (Properties.Settings.Default.UnitMHz)
                        RxFreq *= 1000000;
                    else
                        RxFreq *= 1000;

                    sRxFreq = string.Format("{0:D8}", (int)RxFreq);
                }
                else
                {
                    sRxFreq = "FFFFFFFF";
                }
                bytes.AddRange(HexString2Bytes(sRxFreq));
                bytes.Add((byte)ModeDefs.IndexOf(Mode));

                bytes.AddRange(Encoding.ASCII.GetBytes(Comment));

                if (TxFreqValid)
                {
                    if (Properties.Settings.Default.UnitMHz)
                        TxFreq *= 1000000;
                    else
                        TxFreq *= 1000;

                    sTxFreq = string.Format("{0:D8}", (int)TxFreq);
                }
                else
                {
                    sTxFreq = "FFFFFFFF";
                }
                bytes.AddRange(HexString2Bytes(sTxFreq));

            }


            return bytes.ToArray();
        }

        //4. Freq. Range Config - 160 Bytes
        private byte[] getFreqRangeBytes()
        {

            List<byte> bytes = new List<byte>();

            for (int i = 0; i < 20; i++)
            {
                string sLowLimit, sHighLimit;

                sLowLimit = (string)dtFreqRange.Rows[i][1];
                sHighLimit = (string)dtFreqRange.Rows[i][2];


                decimal LowLimit, HighLimit;
                bool LowLimitValid = decimal.TryParse(sLowLimit, out LowLimit);
                bool HighLimitValid = decimal.TryParse(sHighLimit, out HighLimit);
                if (LowLimitValid)
                {
                    if (Properties.Settings.Default.UnitMHz)
                        LowLimit *= 1000000;
                    else
                        LowLimit *= 1000;

                    sLowLimit = string.Format("{0:D8}", (int)LowLimit);
                }
                else
                {
                    sLowLimit = "FFFFFFFF";
                }
                bytes.AddRange(HexString2Bytes(sLowLimit));

                if (HighLimitValid)
                {
                    if (Properties.Settings.Default.UnitMHz)
                        HighLimit *= 1000000;
                    else
                        HighLimit *= 1000;

                    sHighLimit = string.Format("{0:D8}", (int)HighLimit);
                }
                else
                {
                    sHighLimit = "FFFFFFFF";
                }
                bytes.AddRange(HexString2Bytes(sHighLimit));
            }
            return bytes.ToArray();
        }

        //5. Mode Name Config - 32 Bytes
        private byte[] getModeNameBytes()
        {
            List<byte> bytes = new List<byte>();

            string ModeText = cbJ3E.Text.PadLeft(3, ' ') +
                cbR3E.Text.PadLeft(3, ' ') + 
                cbH3E.Text.PadLeft(3, ' ') +
                cbLSB.Text.PadLeft(3, ' ') + 
                cbJ2B.Text.PadLeft(3, ' ') + 
                cbFSK.Text.PadLeft(3, ' ') + 
                cbA1A.Text.PadLeft(3, ' ');

            bytes.AddRange(Encoding.ASCII.GetBytes(ModeText));
            //Unused 11 Bytes
            bytes.AddRange(Enumerable.Repeat((byte)0xFF, 11));

            return bytes.ToArray();
        }

        //6. Misc. Configs - 64 Bytes
        private byte[] getMiscBytes()
        {
            List<byte> bytes = new List<byte>();

            bytes.AddRange(Enumerable.Repeat((byte)0, 19));



            //00 01 02 03 04 05 06 07 08 09 10 11 12 13 14 15 16 17 18
            //A0 27 27 00 03 01 60 50 13 4E 00 00 01 04 00 02 07 01 A0 

            //00:Cross CH/ITU CH/Freq.Prog
            if (cbFreqProg.SelectedIndex == 2)
                bytes[0] = 0x82;
            else if (cbFreqProg.SelectedIndex == 1)
                bytes[0] = 0x80;

            if (cbITUchUSE.SelectedIndex == 1)
                bytes[0] |= 0x20;

            if (cbCrossChOper.SelectedIndex == 1)
                bytes[0] |= 0x10;

            //01:ModeRxEn
            if (cbJ3E_Rx.Checked)
                bytes[1] |= 0x01;
            if (cbR3E_Rx.Checked)
                bytes[1] |= 0x02;
            if (cbH3E_Rx.Checked)
                bytes[1] |= 0x04;
            if (cbLSB_Rx.Checked)
                bytes[1] |= 0x08;
            if (cbJ2B_Rx.Checked)
                bytes[1] |= 0x10;
            if (cbFSK_Rx.Checked)
                bytes[1] |= 0x20;
            if (cbA1A_Rx.Checked)
                bytes[1] |= 0x40;

            //02: ModeTxEn
            if (cbJ3E_Tx.Checked)
                bytes[2] |= 0x01;
            if (cbR3E_Tx.Checked)
                bytes[2] |= 0x02;
            if (cbH3E_Tx.Checked)
                bytes[2] |= 0x04;
            if (cbLSB_Tx.Checked)
                bytes[2] |= 0x08;
            if (cbJ2B_Tx.Checked)
                bytes[2] |= 0x10;
            if (cbFSK_Tx.Checked)
                bytes[2] |= 0x20;
            if (cbA1A_Tx.Checked)
                bytes[2] |= 0x40;

            //03:00 ???
            bytes[3] = 0;

            //04:Power Select 1/2/3 -> H/HM/HML
            bytes[4] = (byte)(cbTxPwrSel.SelectedIndex + 1);

            //05-06:MAX USER CH
            var maxCH = HexString2Bytes(cbMaxUserCH.Text.PadLeft(4, '0'));
            bytes[5] = maxCH[0];
            bytes[6] = maxCH[1];

            //07:ALM TIME Sec
            bytes[7] = HexString2Bytes(cbAlmTim.Text.PadLeft(2, '0'))[0];

            //08:Scan ref.
            bytes[8] = HexString2Bytes(cbScanRef.Text.Substring(0, 2))[0];


            //09 Many Configs...
            if (cbTxMeter.SelectedIndex == 1)
            {
                bytes[9] |= 0x80;
            }
            if (cbModeSel2182.SelectedIndex == 1)
            {
                bytes[9] |= 0x40;
            }
            if (cbITU_Direction.SelectedIndex == 1)
            {
                bytes[9] |= 0x20;
            }
            if (cbAutoTuningType.SelectedIndex == 1)
            {
                bytes[9] |= 0x10;
            }
            if (cb2182sel_atALM.SelectedIndex == 1)
            {
                bytes[9] |= 0x08;
            }
            if (cbACCMode_input.SelectedIndex == 1)
            {
                bytes[9] |= 0x01;
            }

            //10 Mic Audio input
            if (cbMicAudio_input.SelectedIndex == 1)
                bytes[10] = 0x80;

            //11 ATU Model
            bytes[11] = (byte)cbATU.SelectedIndex;

            //12 Scan Type(Combined with 09)
            if (cbScanType.SelectedIndex == 0)
            {
                bytes[9] = (byte)(bytes[9] & ~0x04);
                bytes[12] = 1;
            }
            else if (cbScanType.SelectedIndex == 1)
            {
                bytes[9] |= 0x04;
                bytes[12] = 1;
            }
            else if (cbScanType.SelectedIndex == 2)
            {
                bytes[9] |= 0x04;
                bytes[12] = 2;
            }
            else //cbScanType.SelectedIndex = 3;
            {
                bytes[9] |= 0x04;
                bytes[12] = 0;
            }

            //13 Scan Speed[1~10]
            bytes[13] = HexString2Bytes(cbScanSpeed.Text.PadLeft(2, '0'))[0];

            //14 FSK shift Hz
            bytes[14] = (byte)cbFSK_Shift.SelectedIndex;

            //15 CW Break-in
            bytes[15] = (byte)cbCW_BreakIn.SelectedIndex;

            //16 LCD contrast [1~10]
            bytes[16] = HexString2Bytes(cbLCD_Contrast.Text.PadLeft(2, '0'))[0];

            //17 NMEA ID [1~100]
            bytes[17] = HexString2Bytes(cbNMEA_ID.Text.PadLeft(2, '0'))[0];

            //18 NMEA Jack/FSK Pol/NarrowFilter/ATU/ITU-FSK-Enable
            if (cbITU_FSKch.SelectedIndex == 1)
                bytes[18] = 0x80;

            if (cbAutoTuning_use.SelectedIndex == 1)
                bytes[18] |= 0x40;

            if (cbFSK_Pol.SelectedIndex == 1)
                bytes[18] |= 0x08;

            if (cbNMEA_Jack.SelectedIndex == 1)
                bytes[18] |= 0x04;

            if (cb_NarrowFilter.SelectedIndex == 1)
                bytes[18] |= 0x10;

            //   09                           18
            //   02:ind.type - COMMENT [end = 20]
            //   00:ind.type - FRQ ONLY[end = 20]
            //   02:ind.type - FRQUENCY[end = 00]
            if (cbIndType.SelectedIndex == 0)
            {
                bytes[9] |= 0x02;
                bytes[18] |= 0x20;
            }
            else if (cbIndType.SelectedIndex == 1)
            {
                bytes[9] = (byte)(bytes[9] & ~0x02);
                bytes[18] |= 0x20;
            }
            else // cbIndType.SelectedIndex = 2;
            {
                bytes[9] |= 0x02;
                bytes[18] = (byte)(bytes[18] & ~0x20);
            }

            //Unused 45 Bytes
            bytes.AddRange(Enumerable.Repeat((byte)0xFF, 45));

            return bytes.ToArray();
        }
        private List<string> MakeConfigStrings()
        {
            List<string> lines = new List<string>();

            //Make 4032 Data Bytes
            List<byte> configBytes = new List<byte>();
            var note = Encoding.ASCII.GetBytes(tbInfo.Text);

            //1. Note Infomation 32 Bytes
            configBytes.AddRange(note.ToList());
            if (note.Length < 32)
            {
                configBytes.AddRange(Enumerable.Repeat((byte)0x20, 32 - note.Length));
            }
            //2. User Channel Config - 2576 Bytes
            //3. ITU Channel Config - 1152 Bytes
            configBytes.AddRange(getUser_n_ITU_ChannelBytes());

            //4. Freq. Range Config - 160 Bytes
            configBytes.AddRange(getFreqRangeBytes());

            //5. Mode Name Config - 32 Bytes
            configBytes.AddRange(getModeNameBytes());

            //6. Misc. Configs - 64 Bytes
            configBytes.AddRange(getMiscBytes());

            //7. End Info - 16 Bytes
            note = Encoding.ASCII.GetBytes(tbInfo2.Text);
            configBytes.AddRange(note.ToList());
            if (note.Length < 16)
            {
                configBytes.AddRange(Enumerable.Repeat((byte)0x20, 16 - note.Length));
            }

            //First Line
            lines.Add("16320001");
            int StartAddr = 0xE000;

            foreach (var chunk in Enumerable.Range(0, 4032 / 32).Select(i => configBytes.Skip(i * 32).Take(32).ToArray()))
            {
                StringBuilder sb = new StringBuilder();
                sb.AppendFormat("{0:X2}{1:X2}{2}", (byte)(StartAddr >> 8), (byte)(StartAddr & 0xFF), "20");
                var line = sb.ToString() + BytesToHEXString(chunk, false);
                lines.Add(line);
                StartAddr += 0x20;
            }

            return lines;

        }
        private bool SaveFile(string path)
        {
            FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write);

            StreamWriter sw = new StreamWriter(fileStream);

            var lines = MakeConfigStrings();
            foreach (var line in lines)
            {
                sw.WriteLine(line);
            }
           
            sw.Flush();
            sw.Close();
            fileStream.Close();
            return true;

        }
        static string CurrentFile = "";

        private bool ReadFile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;

            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            StreamReader sr = new StreamReader(fileStream);
            List<string> icfFile = new List<string>();
            string ParsedString = "";
            while (true)
            {
                var line = sr.ReadLine();
                if (line == null)
                    break;
                else if (line == "")
                    continue;
                else
                {
                    icfFile.Add(line);
                }
            }
            if (icfFile[0] != "16320001")
            {
                MessageBox.Show("Invalid File!");
                return false;
            }
            icfFile.RemoveAt(0);
            int startAddr = 0xE000 - 0x20;

            foreach (var icf in icfFile)
            {
                int currentAddr = 0;
                int length = 0;

                GetAddrAndLength(icf.Substring(0, 6), ref currentAddr, ref length);
                if (((currentAddr - startAddr) != 0x20) || (length != 32))
                {
                    MessageBox.Show("Invalid File!");
                    return false;
                }
                startAddr = currentAddr;
                ParsedString += icf.Substring(6, 64);
            }

            ICF_to_Form(ParsedString);
            sr.Close();
            fileStream.Close();

            tsFileName.Text = filePath.Split('\\').Last();
            CurrentFile = filePath;

            return true;
        }
        string getFreqkHz(string freq)
        {
            decimal Freq;
            bool succeed = decimal.TryParse(freq, out Freq);
            if (succeed)
            {
                if (Properties.Settings.Default.UnitMHz)
                {
                    return (Freq / (decimal)1e6).ToString("0.000000");
                }
                else
                {
                    return (Freq / (decimal)1e3).ToString("0.000");
                }
            }
            else
            {
                return "";
            }
        }
        private void ParseUserChannel(int id, string userCH)
        {
            // Rx8     M2   COMMENT-14      TX8
            //02182000 02 454D455247454E 02182000

            dtUserCH.Rows[id][0] = id;
            dtUserCH.Rows[id][1] = getFreqkHz(userCH.Substring(0, 8));
            dtUserCH.Rows[id][2] = getFreqkHz(userCH.Substring(24, 8));
            if (userCH[9] == '2')
                dtUserCH.Rows[id][3] = "AM";
            else if (userCH[9] == '0')
                dtUserCH.Rows[id][3] = "USB";
            dtUserCH.Rows[id][4] = BytesToASCIIString(HexString2Bytes(userCH.Substring(10, 14))).TrimEnd();
        }

        private void ParseITUSimpChannel(int id, string ITU_CH)
        {
            // Rx8     M2   COMMENT-14      TX8
            //02182000 02 454D455247454E 02182000
            string[] Prefix = { "4-", "6-", "8-", "12-", "16-", "18-", "22-", "25-" };

            dtITUSimp.Rows[id][0] = Prefix[id / 9] + (id % 9 + 1).ToString();
            dtITUSimp.Rows[id][1] = getFreqkHz(ITU_CH.Substring(0, 8));

            if (ITU_CH[9] == '2')
                dtITUSimp.Rows[id][2] = "AM";
            else if (ITU_CH[9] == '0')
                dtITUSimp.Rows[id][2] = "USB";
            dtITUSimp.Rows[id][3] = BytesToASCIIString(HexString2Bytes(ITU_CH.Substring(10, 14))).TrimEnd();
        }

        private void ParseFreqRange(int id, string freq)
        {
            // LowRange   HighRange
            //00030000 - 29999900[RX         30.000 - 29999.900]
            string[] Prefix = { "RX", "Tx  1st", "Tx  2nd", "Tx  3rd", "Tx  4th", "Tx  5th", "Tx  6th", "Tx  7th", "Tx  8th", "Tx  9th", "Tx 10th", "Tx 11th", "ITU 4M", "ITU 6M", "ITU 8M", "ITU 12M", "ITU 16M", "ITU 18M", "ITU 22M", "ITU 25M" };

            dtFreqRange.Rows[id][0] = Prefix[id];
            dtFreqRange.Rows[id][1] = getFreqkHz(freq.Substring(0, 8));
            dtFreqRange.Rows[id][2] = getFreqkHz(freq.Substring(8, 8));
        }

        private void ParseConfig(byte[] bytes)
        {
            //Console.WriteLine(BytesToHEXString(bytes));
            //00 01 02 03 04 05 06 07 08 09 10 11 12 13 14 15 16 17 18
            //A0 27 27 00 03 01 60 50 13 4E 00 00 01 04 00 02 07 01 A0 

            //00:Cross CH/ITU CH/Freq.Prog
            if ((bytes[0] & 0x82) == 0x82)
                cbFreqProg.SelectedIndex = 2;
            else if ((bytes[0] & 0x82) == 0x80)
                cbFreqProg.SelectedIndex = 1;
            else
                cbFreqProg.SelectedIndex = 0;

            if ((bytes[0] & 0x20) == 0x20)
                cbITUchUSE.SelectedIndex = 1;
            else
                cbITUchUSE.SelectedIndex = 0;

            if ((bytes[0] & 0x10) == 0x10)
                cbCrossChOper.SelectedIndex = 1;
            else
                cbCrossChOper.SelectedIndex = 0;

            //01:ModeRxEn
            cbJ3E_Rx.Checked = ((bytes[1] & 0x01) != 0);
            cbR3E_Rx.Checked = ((bytes[1] & 0x02) != 0);
            cbH3E_Rx.Checked = ((bytes[1] & 0x04) != 0);
            cbLSB_Rx.Checked = ((bytes[1] & 0x08) != 0);
            cbJ2B_Rx.Checked = ((bytes[1] & 0x10) != 0);
            cbFSK_Rx.Checked = ((bytes[1] & 0x20) != 0);
            cbA1A_Rx.Checked = ((bytes[1] & 0x40) != 0);

            //02: ModeTxEn
            cbJ3E_Tx.Checked = ((bytes[2] & 0x01) != 0);
            cbR3E_Tx.Checked = ((bytes[2] & 0x02) != 0);
            cbH3E_Tx.Checked = ((bytes[2] & 0x04) != 0);
            cbLSB_Tx.Checked = ((bytes[2] & 0x08) != 0);
            cbJ2B_Tx.Checked = ((bytes[2] & 0x10) != 0);
            cbFSK_Tx.Checked = ((bytes[2] & 0x20) != 0);
            cbA1A_Tx.Checked = ((bytes[2] & 0x40) != 0);

            //03:00 ???

            //04:Power Select 1/2/3 -> H/HM/HML
            if ((bytes[4] == 0) || (bytes[4] > 3))
                bytes[4] = 3;
            cbTxPwrSel.SelectedIndex = bytes[4] - 1;

            //05-06:MAX USER CH
            int maxCH = (bytes[5] >> 4) * 1000 + (bytes[5] & 0x0F) * 100 + (bytes[6] >> 4) * 10 + (bytes[6] & 0x0F);
            cbMaxUserCH.Text = maxCH.ToString();


            //07:ALM TIME Sec
            int AlarmTimeSecs = (bytes[7] >> 4) * 10 + (bytes[7] & 0x0F);
            cbAlmTim.Text = AlarmTimeSecs.ToString();

            //08:Scan ref.
            int scanRef = ((bytes[8] >> 4) * 10 + (bytes[8] & 0x0F)) * 10;
            if ((scanRef < 100) || (scanRef > 200))
                bytes[8] = 130;
            cbScanRef.Text = scanRef.ToString();

            //09 Many COnfigs...
            if ((bytes[9] & 0x80) == 0x80)
                cbTxMeter.SelectedIndex = 1;
            else
                cbTxMeter.SelectedIndex = 0;

            if ((bytes[9] & 0x40) == 0x40)
                cbModeSel2182.SelectedIndex = 1;
            else
                cbModeSel2182.SelectedIndex = 0;

            if ((bytes[9] & 0x20) == 0x20)
                cbITU_Direction.SelectedIndex = 1;
            else
                cbITU_Direction.SelectedIndex = 0;

            if ((bytes[9] & 0x10) == 0x10)
                cbAutoTuningType.SelectedIndex = 1;
            else
                cbAutoTuningType.SelectedIndex = 0;


            if ((bytes[9] & 0x08) == 0x08)
                cb2182sel_atALM.SelectedIndex = 1;
            else
                cb2182sel_atALM.SelectedIndex = 0;


            if ((bytes[9] & 0x01) == 0x01)
                cbACCMode_input.SelectedIndex = 1;
            else
                cbACCMode_input.SelectedIndex = 0;

            //10 Mic Audio input
            if ((bytes[10] & 0x80) == 0x80)
                cbMicAudio_input.SelectedIndex = 1;
            else
                cbMicAudio_input.SelectedIndex = 0;


            //11 ATU Model
            if (bytes[11] > 2)
                bytes[11] = 0;
            cbATU.SelectedIndex = bytes[11];

            //12 Scan Type(Combined with 09)
            if (((bytes[9] & 0x04) == 0) && (bytes[12] == 1))
                cbScanType.SelectedIndex = 0;
            else if (((bytes[9] & 0x04) == 0x04) && (bytes[12] == 1))
                cbScanType.SelectedIndex = 1;
            else if (((bytes[9] & 0x04) == 0x04) && (bytes[12] == 2))
                cbScanType.SelectedIndex = 2;
            else if (((bytes[9] & 0x04) == 0x04) && (bytes[12] == 0))
                cbScanType.SelectedIndex = 3;
            else cbScanType.SelectedIndex = 1;


            //13 Scan Speed[1~10]
            if (bytes[13] == 0x10) bytes[13] = 10;
            if ((bytes[13] <= 0) || (bytes[13] > 10))
            {
                bytes[13] = 4;
            }
            cbScanSpeed.Text = bytes[13].ToString();

            //14 FSK shift Hz
            if (bytes[14] > 2)
                bytes[14] = 0;
            cbFSK_Shift.SelectedIndex = bytes[14];


            //15 CW Break-in
            if (bytes[15] > 2)
                bytes[15] = 0;
            cbCW_BreakIn.SelectedIndex = bytes[15];

            //16 LCD contrast [1~10]
            if (bytes[16] == 0x10) bytes[16] = 10;
            if ((bytes[16] <= 0) || (bytes[16] > 10))
            {
                bytes[16] = 7;
            }
            cbLCD_Contrast.Text = bytes[16].ToString();


            //17 NMEA ID [1~100]
            int nmeaID = ((bytes[17] >> 4) * 10 + (bytes[17] & 0x0F));
            if ((nmeaID <= 0) || (nmeaID > 99))
            {
                nmeaID = 1;
            }
            cbNMEA_ID.Text = nmeaID.ToString();

            //18 NMEA Jack/FSK Pol/NarrowFilter/ATU/ITU-FSK-Enable
            if ((bytes[18] & 0x80) == 0x80)
                cbITU_FSKch.SelectedIndex = 1;
            else
                cbITU_FSKch.SelectedIndex = 0;

            if ((bytes[18] & 0x40) == 0x40)
                cbAutoTuning_use.SelectedIndex = 1;
            else
                cbAutoTuning_use.SelectedIndex = 0;

            if ((bytes[18] & 0x08) == 0x08)
                cbFSK_Pol.SelectedIndex = 1;
            else
                cbFSK_Pol.SelectedIndex = 0;

            if ((bytes[18] & 0x04) == 0x04)
                cbNMEA_Jack.SelectedIndex = 1;
            else
                cbNMEA_Jack.SelectedIndex = 0;

            if ((bytes[18] & 0x10) == 0x10)
                cb_NarrowFilter.SelectedIndex = 1;
            else
                cb_NarrowFilter.SelectedIndex = 0;

            //   09                           18
            //   02:ind.type - COMMENT [end = 20]
            //   00:ind.type - FRQ ONLY[end = 20]
            //   02:ind.type - FRQUENCY[end = 00]
            if (((bytes[18] & 0x20) == 0x20) && ((bytes[9] & 0x02) == 0x02))
                cbIndType.SelectedIndex = 0;
            else if (((bytes[18] & 0x20) == 0x20) && ((bytes[9] & 0x02) == 0x00))
                cbIndType.SelectedIndex = 1;
            else
                cbIndType.SelectedIndex = 2;
        }


        private void ICF_to_Form(string ICF)
        {
            tbInfo.Text = BytesToASCIIString(HexString2Bytes(ICF.Substring(0, 64))).TrimEnd();
            for (int i = 0; i < 161; i++)
            {
                ParseUserChannel(i, ICF.Substring(64 + i * 32, 32));
            }

            for (int i = 0; i < 72; i++)
            {
                ParseITUSimpChannel(i, ICF.Substring(64 + 32 * 161 + i * 32, 32));
            }

            for (int i = 0; i < 20; i++)
            {
                ParseFreqRange(i, ICF.Substring(64 + 32 * (161 + 72) + i * 16, 16));
            }

            //Remove Parsed part.
            ICF = ICF.Substring(7840, ICF.Length - 7840);

            var Modes = Enumerable.Range(0, 7).Select(i => BytesToASCIIString(HexString2Bytes(ICF.Substring(0, 42))).Substring(i * 3, 3)).ToList();

            cbJ3E.Text = Modes[0];
            cbR3E.Text = Modes[1];
            cbH3E.Text = Modes[2].Trim();
            cbLSB.Text = Modes[3];
            cbJ2B.Text = Modes[4];
            cbFSK.Text = Modes[5];
            cbA1A.Text = Modes[6].Trim();

            //Remove Parsed & unused(0xFF...) part.42+22=64
            ICF = ICF.Substring(64, ICF.Length - 64);

            //Next 38 Chars Are Bit-Defined Parameters
            ParseConfig(HexString2Bytes(ICF.Substring(0, 38)));



            tbInfo2.Text = BytesToASCIIString(HexString2Bytes(ICF.Substring(128, 32))).TrimEnd();









        }
        private void AppendFileHistort(string path)
        {
            if (path == Properties.Settings.Default.LastFile1 ||
                path == Properties.Settings.Default.LastFile2 ||
                path == Properties.Settings.Default.LastFile3)
                return;

            Properties.Settings.Default.LastFile3 = Properties.Settings.Default.LastFile2;
            Properties.Settings.Default.LastFile2 = Properties.Settings.Default.LastFile1;
            Properties.Settings.Default.LastFile1 = path;
            Properties.Settings.Default.Save();

            RefreshFileHistory();
        }

        private void MenuHistortFileLoad_Click(object sender, EventArgs e)
        {
            string path = ((ToolStripMenuItem)sender).Text;
            if (!ReadFile(path))
            {
                if (path == Properties.Settings.Default.LastFile1)
                    Properties.Settings.Default.LastFile1 = "";
                else if (path == Properties.Settings.Default.LastFile2)
                    Properties.Settings.Default.LastFile2 = "";
                else if (path == Properties.Settings.Default.LastFile3)
                    Properties.Settings.Default.LastFile3 = "";

                Properties.Settings.Default.Save();
                RefreshFileHistory();
                MessageBox.Show("File[" + path + "] invalid. Please select another file！");
            }
        }
        private void MenuFileLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select Config File";
            openFileDialog.Filter = "Supported File(*.ini,*.icf)|*.ini;*.icf|INI(*.ini)|*.ini|ICF(*.icf)|*.icf|Any File(*.*)|*.*";
            openFileDialog.InitialDirectory = Application.StartupPath;
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (ReadFile(openFileDialog.FileName))
                {
                    AppendFileHistort(openFileDialog.FileName);
                }
            }

        }


        private void menuUnitMHz_Click(object sender, EventArgs e)
        {
            if (!Properties.Settings.Default.UnitMHz)
            {
                Properties.Settings.Default.UnitMHz = true;
                menuUnitkHz.Checked = false;
                menuUnitMHz.Checked = true;
                Properties.Settings.Default.Save();
                InitUserChannelDataGrid();
                InitITU_SimpChannelDataGrid();
                InitFreqRangeDataGrid();
                SwapUnit();
            }

        }

        private void menuUnitkHz_Click(object sender, EventArgs e)
        {
            if (Properties.Settings.Default.UnitMHz)
            {
                Properties.Settings.Default.UnitMHz = false;
                menuUnitkHz.Checked = true;
                menuUnitMHz.Checked = false;
                Properties.Settings.Default.Save();
                InitUserChannelDataGrid();
                InitITU_SimpChannelDataGrid();
                InitFreqRangeDataGrid();
                SwapUnit();
            }
        }

        private void MenuExit_Click(object sender, EventArgs e)
        {
            Environment.Exit(0);
        }

        private void MenuFileSave_Click(object sender, EventArgs e)
        {
            if (CurrentFile == "")
            {
                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.Title = "Config File Save to...";
                saveFileDialog.Filter = "Supported File(*.ini,*.icf)|*.ini;*.icf|INI(*.ini)|*.ini|ICF(*.icf)|*.icf|Any File(*.*)|*.*";
                saveFileDialog.InitialDirectory = Application.StartupPath;
                saveFileDialog.FileName = tsFileName.Text;
                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    CurrentFile = saveFileDialog.FileName;
                    tsFileName.Text = CurrentFile.Split('\\').Last();
                }
            }
            SaveFile(CurrentFile);
        }

        private void MenuFileSaveAs_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = "Config File Save As...";
            saveFileDialog.Filter = "Supported File(*.ini,*.icf)|*.ini;*.icf|INI(*.ini)|*.ini|ICF(*.icf)|*.icf|Any File(*.*)|*.*";
            saveFileDialog.InitialDirectory = Application.StartupPath;
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                if (!SaveFile(saveFileDialog.FileName))
                {
                    MessageBox.Show("Config Saved to：" + saveFileDialog.FileName + "Failed!!!");
                }
            }
        }

        private void debugToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ReadFile(Properties.Settings.Default.LastFile1))
            {
                MessageBox.Show("ERR");
            }
            if (!SaveFile("E:\\调试软件\\EX1726-GUI\\EX1726-GUI\\bin\\Debug\\test.ini"))
            {
                MessageBox.Show("Config Saved to：E:\\调试软件\\EX1726-GUI\\EX1726-GUI\\bin\\Debug\\test.ini Failed!!!");
            }

        }

        private void cbR3E_Rx_CheckedChanged(object sender, EventArgs e)
        {
            if (!cbR3E_Rx.Checked)
            {
                cbR3E_Tx.Checked = false;
                cbR3E_Tx.Enabled = false;
            }
            else
            {
                cbR3E_Tx.Checked = false;
                cbR3E_Tx.Enabled = true;
            }
        }

        private void cbH3E_Rx_CheckedChanged(object sender, EventArgs e)
        {
            if (!cbH3E_Rx.Checked)
            {
                cbH3E_Tx.Checked = false;
                cbH3E_Tx.Enabled = false;
            }
            else
            {
                cbH3E_Tx.Checked = false;
                cbH3E_Tx.Enabled = true;
            }
        }

        private void cbLSB_Rx_CheckedChanged(object sender, EventArgs e)
        {
            if (!cbLSB_Rx.Checked)
            {
                cbLSB_Tx.Checked = false;
                cbLSB_Tx.Enabled = false;
            }
            else
            {
                cbLSB_Tx.Checked = false;
                cbLSB_Tx.Enabled = true;
            }
        }

        private void cbJ2B_Rx_CheckedChanged(object sender, EventArgs e)
        {
            if (!cbJ2B_Rx.Checked)
            {
                cbJ2B_Tx.Checked = false;
                cbJ2B_Tx.Enabled = false;
            }
            else
            {
                cbJ2B_Tx.Checked = false;
                cbJ2B_Tx.Enabled = true;
            }
        }

        private void cbFSK_Rx_CheckedChanged(object sender, EventArgs e)
        {
            if (!cbFSK_Rx.Checked)
            {
                cbFSK_Tx.Checked = false;
                cbFSK_Tx.Enabled = false;
            }
            else
            {
                cbFSK_Tx.Checked = false;
                cbFSK_Tx.Enabled = true;
            }
        }

        private void cbA1A_Rx_CheckedChanged(object sender, EventArgs e)
        {
            if (!cbA1A_Rx.Checked)
            {
                cbA1A_Tx.Checked = false;
                cbA1A_Tx.Enabled = false;
            }
            else
            {
                cbA1A_Tx.Checked = false;
                cbA1A_Tx.Enabled = true;
            }
        }

        private void tabCommon_Click(object sender, EventArgs e)
        {

        }

        private void InitLanguage(string lan)
        {

            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(lan);
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(lan);
            Controls.Clear();
            InitializeComponent();
            InitUserChannelDataGrid();
            InitITU_SimpChannelDataGrid();
            InitFreqRangeDataGrid();
            tabs.Width = this.Size.Width - 40;
            tabs.Height = this.Size.Height - 90;

        }


        private void englishToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(Thread.CurrentThread.CurrentUICulture.Name != "en")
            { 
                InitLanguage("en");
                Properties.Settings.Default.Language = "en";
                Properties.Settings.Default.Save();
                ScanCommPort();
            }
            if (englishToolStripMenuItem.Checked)
            {
                Properties.Settings.Default.Language = "";
                Properties.Settings.Default.Save();
            }
            englishToolStripMenuItem.Checked = !englishToolStripMenuItem.Checked;
        }

        private void chineseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (Thread.CurrentThread.CurrentUICulture.Name != "zh-CN")
            {
                InitLanguage("zh-CN");
                Properties.Settings.Default.Language = "zh-CN";
                Properties.Settings.Default.Save();
                ScanCommPort();
            }
            if (chineseToolStripMenuItem.Checked)
            {
                Properties.Settings.Default.Language = "";
                Properties.Settings.Default.Save();
            }
            chineseToolStripMenuItem.Checked = !chineseToolStripMenuItem.Checked;
        }
        SerialPort serialport = new SerialPort();
        List<byte> RxData = new List<byte>();
        List<byte[]> TxBytes = new List<byte[]>();
        bool ReadoutConfigs = false;
        bool ProgramConfigs=false;
        string stringFromTR = "";
        int startAddr = 0xE000 - 0x20;
        private void MenuClone2PC_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialport.IsOpen)
                {
                    serialport.Close();
                }

                serialport.PortName = SelectedSerialName;
                serialport.BaudRate = 4800;
                serialport.Parity = Parity.None;

                serialport.DataReceived += Serialport_DataReceived;
                serialport.Open();
                prgbar.Visible = true;
                prgbar.Value = 0;
                MenuClone2PC.Enabled = false;
                MenuClone2TR.Enabled = false;
                ReadoutConfigs = true;
                stringFromTR = "";
                startAddr = 0xE000 - 0x20;

                byte[] Readout_Cmd = { 0xFE, 0xFE, 0xEE, 0xEF, 0xE2, 0x16, 0x32, 0x00, 0x01, 0xFD };
                CurrentFile = "";
                tsFileName.Text = "NONAME.ICF";
                RxData.Clear();
                serialport.DiscardInBuffer();
                serialport.Write(Readout_Cmd, 0, Readout_Cmd.Length);
                serialTimer.Interval = 100;
                serialTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void MenuClone2TR_Click(object sender, EventArgs e)
        {
            try
            {
                if (serialport.IsOpen)
                {
                    serialport.Close();
                }

                serialport.PortName = SelectedSerialName;
                serialport.BaudRate = 4800;
                serialport.Parity = Parity.None;

                serialport.DataReceived += Serialport_DataReceived;
                serialport.Open();
                prgbar.Visible = true;
                prgbar.Value = 0;
                MenuClone2PC.Enabled = false;
                MenuClone2TR.Enabled = false;
                ProgramConfigs = true;

                var lines = MakeConfigStrings();
                TxBytes.Clear();
                TxBytes.Add(new byte[] { 0xFE, 0xFE, 0xEE, 0xEF, 0xE3, 0x16, 0x32, 0x00, 0x01, 0xFD });

                

                foreach (var line in lines.Skip(1) ) {
                    byte[] content = new byte[78];// { 0xFE, 0xFE, 0xEE, 0xEF, 0xE4 };
                    content[0] = content[1] = 0xFE;
                    content[2] = 0xEE;
                    content[3] = 0xEF;
                    content[4] = 0xE4;
                    byte[] lineBytes = Encoding.ASCII.GetBytes(line);
                    Array.Copy(lineBytes, 0, content, 5, lineBytes.Length);

                    byte[] bytes= HexString2Bytes(line);
                    int sum = 0;
                    foreach (var b in bytes) {
                        sum += b;
                    }
                    sum = 0 - sum;
                    var sumStr = Encoding.ASCII.GetBytes(string.Format("{0:X2}",sum&0xFF));

                    content[75] = sumStr[0];
                    content[76] = sumStr[1];
                    content[77] = 0xFD;
                    TxBytes.Add(content);
                }

                TxBytes.Add(new byte[] { 0xFE, 0xFE, 0xEE, 0xEF, 0xE5, 0xFD});





               // byte[] Readout_Cmd = { 0xFE, 0xFE, 0xEE, 0xEF, 0xE2, 0x16, 0x32, 0x00, 0x01, 0xFD };
                
                serialport.DiscardInBuffer();
                //serialport.Write(Readout_Cmd, 0, Readout_Cmd.Length);
                serialTimer.Interval = 100;
                serialTimer.Start();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void Serialport_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            int buf_len = serialport.BytesToRead;
            byte[] buf = new byte[buf_len];
            if (serialport.Read(buf, 0, buf_len) > 0)
            {
                RxData.AddRange(buf);
            }

        }
        

        private void serialTimer_Tick(object sender, EventArgs e)
        {
            if (ReadoutConfigs)
            {
                if (RxData.Count == 0)
                {
                    MessageBox.Show("未收到数据，请检查串口！");
                    serialTimer.Stop();
                }
                else if (RxData.Count >= 78)
                {
                    //Remove echo bytes
                    /*
                     * FE FE EE EF E2 16 32 00 01 FD [ECHO]
                     * FE FE EF EE E4 45 30 30 30 32 30 35 35 35 33 34 31 32 30 34 37 34 35 34 45 34 35 32 30 33 30 33 31 32 30 32 30 32 30 32 30 32 30 32 30 32 30 32 30 32 30 32 30 32 30 32 30 32 30 32 30 32 30 32 30 32 30 32 30 32 30 32 30 32 30 42 37 FD [DATA]
                     * ....
                     * FE FE EE EF E5 FD [END]
                     */
                    if (RxData[0] != 0xFE || RxData[1] != 0xFE)
                        RxData.RemoveAt(0);
                    else if (RxData[2] == 0xEE && RxData[3] == 0xEF && RxData[4] == 0xE2)//ECHO bytes,remove
                    {
                            RxData.RemoveRange(0, 10);
                            return;
                    }
                    else if (RxData[2] == 0xEF && RxData[3] == 0xEE && RxData[4] == 0xE4 && RxData[77] ==0xFD)
                    {
                        byte[] buf = new byte [72];
                        var str = BytesToASCIIString(RxData.Skip(5).Take(72).ToArray());
                        var bytes = HexString2Bytes(str);
                        byte sum = 0;
                        for (int i = 0; i < bytes.Length; i++)
                        {
                            sum += bytes[i];
                        }
                        if (sum == 0)
                        {
                            int currentAddr = 0;
                            int length = 0;

                            GetAddrAndLength(str.Substring(0, 6), ref currentAddr, ref length);
                            if (((currentAddr - startAddr) != 0x20) || (length != 32))
                            {
                                MessageBox.Show("Invalid Data!"); 
                                serialTimer.Stop();
                                prgbar.Visible = false;
                                MenuClone2PC.Enabled = true;
                                MenuClone2TR.Enabled = true;

                                return;
                            }
                            startAddr = currentAddr;
                            stringFromTR += str.Substring(6, 64);
                            prgbar.Value = 100 - (0xEFA0 - currentAddr) * 100 / 4000;
                        }
                        Console.WriteLine(str);
                        RxData.RemoveRange(0, 78);
                    }
                }
                else if (RxData.Count >= 6)//结束符
                {
                    if (RxData[2] == 0xEF && RxData[3] == 0xEE && RxData[4] == 0xE5)//END bytes,finish
                    {
                        prgbar.Value = 100;
                        //Parse config bytes

                        serialTimer.Stop();
                        ReadoutConfigs = false;
                        prgbar.Visible = false;
                        MenuClone2PC.Enabled = true;
                        MenuClone2TR.Enabled = true;
                        ICF_to_Form(stringFromTR);

                        MessageBox.Show("PC <- TR Finished!");
                    }


                }
            }
            else if(ProgramConfigs)
            {
                if (TxBytes.Count > 0)
                {
                    prgbar.Value = (127-TxBytes.Count)*100/127;
                    serialport.Write(TxBytes[0], 0, TxBytes[0].Length);
                    Console.WriteLine(BytesToHEXString(TxBytes[0]));
                    TxBytes.RemoveAt(0);
                }
                else
                {
                    serialTimer.Stop();
                    prgbar.Visible = false;
                    MenuClone2PC.Enabled = true;
                    MenuClone2TR.Enabled = true;
                    ProgramConfigs = false;

                    MessageBox.Show("PC -> TR Finished!");

                }


            }
        }
    }
}
