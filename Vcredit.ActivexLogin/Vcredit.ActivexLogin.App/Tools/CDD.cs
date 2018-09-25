using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.IO;

namespace Vcredit.ActivexLogin.App.Tools
{
    public enum KeyModifiers        //组合键枚举
    {
        None = 0,
        Alt = 1,
        Control = 2,
        Shift = 4,
        Windows = 8
    }
    public enum MouseBtn           //鼠标按键枚举 
    {
        左下 = 1,
        左上 = 2,
        右下 = 4,
        右上 = 8
    }
    public class CDD
    {
        #region "热键设置相关代码"
        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(
         IntPtr hWnd,
         int id,                        // 热键标识
         KeyModifiers modkey,           //  修改键
         Keys vk                        //  虚键码
        );

        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(
         IntPtr hWnd,                   // 窗口句柄 
         int id                         // 热键标识 
        );
        #endregion

        [DllImport("Kernel32")]
        private static extern System.IntPtr LoadLibrary(string dllfile);

        [DllImport("Kernel32")]
        private static extern System.IntPtr GetProcAddress(System.IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll")]
        public static extern bool FreeLibrary(IntPtr hModule);

        public delegate int pDD_btn(int btn);
        public delegate int pDD_whl(int whl);
        public delegate int pDD_key(int ddcode, int flag);
        public delegate int pDD_mov(int x, int y);
        public delegate int pDD_movR(int dx, int dy);
        public delegate int pDD_str(string str);
        public delegate int pDD_todc(int vkcode);

        public pDD_btn btn;          // 鼠标点击
        public pDD_whl whl;          // 鼠标滚轮
        public pDD_mov mov;          // 鼠标绝对移动
        public pDD_movR movR;        // 鼠标相对移动
        public pDD_key key;          // 键盘按键
        public pDD_str str;          // 键盘字符
        public pDD_todc todc;        // 标准虚拟键码转DD码

        //增强版功能
        public delegate Int32 pDD_MouseMove(IntPtr hwnd, Int32 x, Int32 y);
        public delegate Int32 pDD_SnapPic(IntPtr hwnd, Int32 x, Int32 y, Int32 w, Int32 h);
        public delegate Int32 pDD_PickColor(IntPtr hwnd, Int32 x, Int32 y, Int32 mode);
        public delegate IntPtr pDD_GetActiveWindow();

        public pDD_MouseMove MouseMove;                              // 鼠标移动
        public pDD_SnapPic SnapPic;                                  // 抓图
        public pDD_PickColor PickColor;                              // 取色
        public pDD_GetActiveWindow GetActiveWindow;                  // 取激活窗口句柄

        private IntPtr m_hinst;

        ~CDD()
        {
            if (!m_hinst.Equals(IntPtr.Zero))
            {
                bool b = FreeLibrary(m_hinst);
            }
        }

        public void LoadDllFile(string dllfile)
        {
            FileInfo fi = new FileInfo(dllfile);
            if (!fi.Exists)
                throw new FileNotFoundException("dd组件没有找到");

            int ret = this.Load(dllfile);
            if (ret == -2) throw new FileNotFoundException("dd 组件装载库时发生错误");
            if (ret == -1) throw new FileNotFoundException("dd 取函数地址时发生错误");
            if (ret == 0) throw new FileNotFoundException("dd 非增强模块");
            return;
        }
        public int Load(string dllfile)
        {
            m_hinst = LoadLibrary(dllfile);
            if (m_hinst.Equals(IntPtr.Zero))
            {
                return -2;
            }
            else
            {
                return GetDDfunAddress(m_hinst);
            }
        }

        /// <summary>
        /// 取函数地址返回值  -1：取通用函数地址错误 ，  0：仅取通用函数地址正确 ， 1：取通用函数和增强函数地址都正确
        /// </summary>
        /// <param name="hinst"></param>
        /// <returns></returns>
        private int GetDDfunAddress(IntPtr hinst)
        {
            IntPtr ptr;

            ptr = GetProcAddress(hinst, "DD_btn");
            if (ptr.Equals(IntPtr.Zero)) { return -1; }
            btn = Marshal.GetDelegateForFunctionPointer(ptr, typeof(pDD_btn)) as pDD_btn;

            if (ptr.Equals(IntPtr.Zero)) { return -1; }
            ptr = GetProcAddress(hinst, "DD_whl");
            whl = Marshal.GetDelegateForFunctionPointer(ptr, typeof(pDD_whl)) as pDD_whl;

            if (ptr.Equals(IntPtr.Zero)) { return -1; }
            ptr = GetProcAddress(hinst, "DD_mov");
            mov = Marshal.GetDelegateForFunctionPointer(ptr, typeof(pDD_mov)) as pDD_mov;

            if (ptr.Equals(IntPtr.Zero)) { return -1; }
            ptr = GetProcAddress(hinst, "DD_key");
            key = Marshal.GetDelegateForFunctionPointer(ptr, typeof(pDD_key)) as pDD_key;

            if (ptr.Equals(IntPtr.Zero)) { return -1; }
            ptr = GetProcAddress(hinst, "DD_movR");
            movR = Marshal.GetDelegateForFunctionPointer(ptr, typeof(pDD_movR)) as pDD_movR;

            if (ptr.Equals(IntPtr.Zero)) { return -1; }
            ptr = GetProcAddress(hinst, "DD_str");
            str = Marshal.GetDelegateForFunctionPointer(ptr, typeof(pDD_str)) as pDD_str;

            if (ptr.Equals(IntPtr.Zero)) { return -1; }
            ptr = GetProcAddress(hinst, "DD_todc");
            todc = Marshal.GetDelegateForFunctionPointer(ptr, typeof(pDD_todc)) as pDD_todc;

            //下面四个函数，只有在增强版中才可用
            ptr = GetProcAddress(hinst, "DD_MouseMove"); //鼠标移动
            if (!ptr.Equals(IntPtr.Zero)) MouseMove = Marshal.GetDelegateForFunctionPointer(ptr, typeof(pDD_MouseMove)) as pDD_MouseMove;

            ptr = GetProcAddress(hinst, "DD_SnapPic");        //抓取图片
            if (!ptr.Equals(IntPtr.Zero)) SnapPic = Marshal.GetDelegateForFunctionPointer(ptr, typeof(pDD_SnapPic)) as pDD_SnapPic;

            ptr = GetProcAddress(hinst, "DD_PickColor");      //取色
            if (!ptr.Equals(IntPtr.Zero)) PickColor = Marshal.GetDelegateForFunctionPointer(ptr, typeof(pDD_PickColor)) as pDD_PickColor;

            ptr = GetProcAddress(hinst, "DD_GetActiveWindow");    //获取激活窗口句柄
            if (!ptr.Equals(IntPtr.Zero)) GetActiveWindow = Marshal.GetDelegateForFunctionPointer(ptr, typeof(pDD_GetActiveWindow)) as pDD_GetActiveWindow;

            if (MouseMove == null || SnapPic == null || PickColor == null || GetActiveWindow == null)
            {
                return 0;
            }

            return 1;
        }

        /// <summary>
        /// 按下tab键
        /// </summary>
        /// <param name="doCount"></param>
        public void DoTab(int doCount = 1)
        {
            for (int i = 0; i < doCount; i++)
            {
                Thread.Sleep(5);
                this.key(300, 1);
                Thread.Sleep(5);
                this.key(300, 2);
                Thread.Sleep(5);
            }
        }

        /// <summary>
        /// 鼠标点击事件
        /// </summary>
        public void DoMouseClick()
        {
            Thread.Sleep(5);
            this.btn(1);
            Thread.Sleep(5);
            this.btn(2);
            Thread.Sleep(5);
        }

        public void DoMouseDoubleClick()
        {
            Thread.Sleep(5);
            this.btn(1);
            Thread.Sleep(5);
            this.btn(2);
            Thread.Sleep(5);
            this.btn(1);
            Thread.Sleep(5);
            this.btn(2);
            Thread.Sleep(5);
        }



        public void DoBackSpace(int doCount = 1, int sleep = 5)
        {
            int ddcode = this.todc((int)Keys.Back);

            for (int i = 0; i < doCount; i++)
            {
                Thread.Sleep(sleep);
                this.key(ddcode, 1);
                Thread.Sleep(sleep);
                this.key(ddcode, 2);
                Thread.Sleep(sleep);
            }
        }
    }

}
