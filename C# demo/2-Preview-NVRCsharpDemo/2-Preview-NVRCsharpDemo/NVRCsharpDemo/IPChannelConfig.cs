using System;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace NVRCsharpDemo
{
    public partial class IPChannelConfig : Form
    {
        public CHCNetSDK.NET_DVR_IPPARACFG_V40 m_struIPParaCfgV40 = new CHCNetSDK.NET_DVR_IPPARACFG_V40();
        private CHCNetSDK.NET_DVR_IPCHANINFO m_struChanInfo = new CHCNetSDK.NET_DVR_IPCHANINFO();
        private CHCNetSDK.NET_DVR_IPCHANINFO_V40 m_struChanInfoV40 = new CHCNetSDK.NET_DVR_IPCHANINFO_V40();
        private CHCNetSDK.NET_DVR_IPC_PROTO_LIST m_struProtoList = new CHCNetSDK.NET_DVR_IPC_PROTO_LIST();
        private CHCNetSDK.NET_DVR_GET_STREAM_UNION m_struStreamUnion = new CHCNetSDK.NET_DVR_GET_STREAM_UNION();
        public int iIPDevIndex = -1;
        public int iChanIndex = 0;
        public uint dwUnionSize = 0;
        public Int32 m_lUserID = -1;

        public CHCNetSDK.NET_DVR_PROTO_TYPE m_struProtoType = new CHCNetSDK.NET_DVR_PROTO_TYPE();

        public IPChannelConfig()
        {
            InitializeComponent();
            dwUnionSize = (uint)Marshal.SizeOf(m_struStreamUnion);
        }

        private void CopyStringToBytes(string src, byte[] dest)
        {
            byte[] bytes = Encoding.ASCII.GetBytes(src ?? "");
            Array.Clear(dest, 0, dest.Length);
            Array.Copy(bytes, dest, Math.Min(bytes.Length, dest.Length));
        }

        private string CopyBytesToString(byte[] src)
        {
            return Encoding.ASCII.GetString(src).Trim('\0');
        }

        private void btnSet_Click(object sender, EventArgs e)
        {
            if (iIPDevIndex < 0)
            {
                for (int i = 0; i < 64; i++)
                {
                    if (m_struIPParaCfgV40.struIPDevInfo[i].byEnable == 0)
                    {
                        iIPDevIndex = i;
                        break;
                    }
                }
            }

            if (iIPDevIndex < 0)
            {
                MessageBox.Show("No free IP device slot found.");
                return;
            }

            m_struIPParaCfgV40.struIPDevInfo[iIPDevIndex].byEnable = 1;
            m_struIPParaCfgV40.struIPDevInfo[iIPDevIndex].byProType =
                (byte)m_struProtoList.struProto[comboBoxFactory.SelectedIndex].dwType;

            CopyStringToBytes(
                textBoxIPAddr.Text,
                m_struIPParaCfgV40.struIPDevInfo[iIPDevIndex].struIP.sIpV4);

            m_struIPParaCfgV40.struIPDevInfo[iIPDevIndex].wDVRPort =
                UInt16.Parse(textBoxPort.Text);

            CopyStringToBytes(
                textBoxUserName.Text,
                m_struIPParaCfgV40.struIPDevInfo[iIPDevIndex].sUserName);

            CopyStringToBytes(
                textBoxPsw.Text,
                m_struIPParaCfgV40.struIPDevInfo[iIPDevIndex].sPassword);

            switch (m_struIPParaCfgV40.struStreamMode[iChanIndex].byGetStreamType)
            {
                case 0:
                    m_struChanInfo.byEnable = 1;
                    m_struChanInfo.byIPID = Convert.ToByte((iIPDevIndex + 1) % 256);
                    m_struChanInfo.byChannel = byte.Parse(textBoxChannel.Text);

                    IntPtr ptrChanInfo = Marshal.AllocHGlobal((Int32)dwUnionSize);
                    Marshal.StructureToPtr(m_struChanInfo, ptrChanInfo, false);
                    m_struIPParaCfgV40.struStreamMode[iChanIndex].uGetStream =
                        (CHCNetSDK.NET_DVR_GET_STREAM_UNION)Marshal.PtrToStructure(
                            ptrChanInfo, typeof(CHCNetSDK.NET_DVR_GET_STREAM_UNION));
                    Marshal.FreeHGlobal(ptrChanInfo);
                    break;

                case 6:
                    m_struChanInfoV40.byEnable = 1;
                    m_struChanInfoV40.wIPID = Convert.ToUInt16(iIPDevIndex + 1);
                    m_struChanInfoV40.dwChannel = Convert.ToUInt32(textBoxChannel.Text);
                    m_struChanInfoV40.byTransProtocol = (byte)comboBoxProtocol.SelectedIndex;
                    m_struChanInfoV40.byTransMode = (byte)comboBoxStream.SelectedIndex;

                    IntPtr ptrChanInfoV40 = Marshal.AllocHGlobal((Int32)dwUnionSize);
                    Marshal.StructureToPtr(m_struChanInfoV40, ptrChanInfoV40, false);
                    m_struIPParaCfgV40.struStreamMode[iChanIndex].uGetStream =
                        (CHCNetSDK.NET_DVR_GET_STREAM_UNION)Marshal.PtrToStructure(
                            ptrChanInfoV40, typeof(CHCNetSDK.NET_DVR_GET_STREAM_UNION));
                    Marshal.FreeHGlobal(ptrChanInfoV40);
                    break;

                default:
                    break;
            }

            int nSize = Marshal.SizeOf(m_struIPParaCfgV40);
            IntPtr ptrIPParaCfgV40 = Marshal.AllocHGlobal(nSize);
            Marshal.StructureToPtr(m_struIPParaCfgV40, ptrIPParaCfgV40, false);

            int iGroupNo = 0;
            if (!CHCNetSDK.NET_DVR_SetDVRConfig(
                m_lUserID,
                CHCNetSDK.NET_DVR_SET_IPPARACFG_V40,
                iGroupNo,
                ptrIPParaCfgV40,
                (UInt32)nSize))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string str = "NET_DVR_SET_IPPARACFG_V40 failed, error code= " + iLastErr;
                MessageBox.Show(str);
            }
            else
            {
                MessageBox.Show("IP通道配置成功!");
            }

            Marshal.FreeHGlobal(ptrIPParaCfgV40);
        }

        private void IPChannelConfig_Load(object sender, EventArgs e)
        {
            int i = 0;

            if (CHCNetSDK.NET_DVR_GetIPCProtoList(m_lUserID, ref m_struProtoList))
            {
                for (i = 0; i < (int)m_struProtoList.dwProtoNum; i++)
                {
                    comboBoxFactory.Items.Add(m_struProtoList.struProto[i].byDescribe);
                }
            }

            comboBoxStreamType.SelectedIndex = 0;
            if (comboBoxFactory.Items.Count > 0) comboBoxFactory.SelectedIndex = 0;
            comboBoxStream.SelectedIndex = 0;
            comboBoxProtocol.SelectedIndex = 0;

            if (iIPDevIndex < 0)
            {
                return;
            }

            comboBoxStreamType.SelectedIndex =
                m_struIPParaCfgV40.struStreamMode[iChanIndex].byGetStreamType;

            textBoxIPAddr.Text = CopyBytesToString(
                m_struIPParaCfgV40.struIPDevInfo[iIPDevIndex].struIP.sIpV4);

            textBoxPort.Text = Convert.ToString(
                m_struIPParaCfgV40.struIPDevInfo[iIPDevIndex].wDVRPort);

            textBoxUserName.Text = CopyBytesToString(
                m_struIPParaCfgV40.struIPDevInfo[iIPDevIndex].sUserName);

            textBoxPsw.Text = CopyBytesToString(
                m_struIPParaCfgV40.struIPDevInfo[iIPDevIndex].sPassword);

            for (i = 0; i < (int)m_struProtoList.dwProtoNum; i++)
            {
                if (m_struProtoList.struProto[i].dwType ==
                    m_struIPParaCfgV40.struIPDevInfo[iIPDevIndex].byProType)
                {
                    comboBoxFactory.SelectedIndex = i;
                }
            }

            switch (m_struIPParaCfgV40.struStreamMode[iChanIndex].byGetStreamType)
            {
                case 0:
                    IntPtr ptrChanInfo = Marshal.AllocHGlobal((Int32)dwUnionSize);
                    Marshal.StructureToPtr(
                        m_struIPParaCfgV40.struStreamMode[iChanIndex].uGetStream,
                        ptrChanInfo,
                        false);
                    m_struChanInfo = (CHCNetSDK.NET_DVR_IPCHANINFO)Marshal.PtrToStructure(
                        ptrChanInfo, typeof(CHCNetSDK.NET_DVR_IPCHANINFO));
                    textBoxChannel.Text = Convert.ToString(m_struChanInfo.byChannel);
                    Marshal.FreeHGlobal(ptrChanInfo);
                    break;

                case 6:
                    IntPtr ptrChanInfoV40 = Marshal.AllocHGlobal((Int32)dwUnionSize);
                    Marshal.StructureToPtr(
                        m_struIPParaCfgV40.struStreamMode[iChanIndex].uGetStream,
                        ptrChanInfoV40,
                        false);
                    m_struChanInfoV40 = (CHCNetSDK.NET_DVR_IPCHANINFO_V40)Marshal.PtrToStructure(
                        ptrChanInfoV40, typeof(CHCNetSDK.NET_DVR_IPCHANINFO_V40));
                    textBoxChannel.Text = Convert.ToString(m_struChanInfoV40.dwChannel);
                    comboBoxStream.SelectedIndex = m_struChanInfoV40.byTransMode;
                    comboBoxProtocol.SelectedIndex = m_struChanInfoV40.byTransProtocol;
                    Marshal.FreeHGlobal(ptrChanInfoV40);
                    break;

                default:
                    break;
            }
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        private void btnDel_Click(object sender, EventArgs e)
        {
            if (iIPDevIndex < 0)
            {
                return;
            }

            m_struIPParaCfgV40.struStreamMode[iChanIndex].uGetStream.Init();
            m_struIPParaCfgV40.struIPDevInfo[iIPDevIndex] = new CHCNetSDK.NET_DVR_IPDEVINFO_V31();

            int nSize = Marshal.SizeOf(m_struIPParaCfgV40);
            IntPtr ptrIPParaCfgV40 = Marshal.AllocHGlobal(nSize);
            Marshal.StructureToPtr(m_struIPParaCfgV40, ptrIPParaCfgV40, false);

            int iGroupNo = 0;
            if (!CHCNetSDK.NET_DVR_SetDVRConfig(
                m_lUserID,
                CHCNetSDK.NET_DVR_SET_IPPARACFG_V40,
                iGroupNo,
                ptrIPParaCfgV40,
                (UInt32)nSize))
            {
                uint iLastErr = CHCNetSDK.NET_DVR_GetLastError();
                string str = "NET_DVR_SET_IPPARACFG_V40 failed, error code= " + iLastErr;
                MessageBox.Show(str);
            }
            else
            {
                MessageBox.Show("IP通道删除成功!");
            }

            Marshal.FreeHGlobal(ptrIPParaCfgV40);
        }
    }
}