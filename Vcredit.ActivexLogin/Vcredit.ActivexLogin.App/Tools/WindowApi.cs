using System;
using System.Runtime.InteropServices;

namespace Vcredit.ActivexLogin.App.Tools
{
	public class WindowApi
    {
        /// <summary>
        /// The FindWindow API
        /// </summary>
        /// <param name="lpClassName">the class name for the window to search for</param>
        /// <param name="lpWindowName">指向一个指定了窗口名（窗口标题）的空结束字符串。
        /// 如果该参数为空，则为所有窗口全匹配。</param>
        /// <returns>如果函数成功，返回值为具有指定类名和窗口名的窗口句柄；如果函数失败，返回值为NULL</returns>
        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "FindWindow", SetLastError = true)]
        public static extern IntPtr FindWindow(String lpClassName, String lpWindowName);

        /// <summary>
        /// 函数功能：该函数获得一个窗口的句柄，该窗口的类名和窗口名与给定的字符串相匹配。
        /// 这个函数查找子窗口，从排在给定的子窗口后面的下一个子窗口开始。在查找时不区分大小写。
        /// </summary>
        /// <param name="hwndParent">要查找子窗口的父窗口句柄。
        ///  如果hwnjParent为NULL，则函数以桌面窗口为父窗口，查找桌面窗口的所有子窗口。
        ///</param>
        /// <param name="hwndChildAfter">子窗口句柄。查找从在Z序中的下一个子窗口开始。
        /// 子窗口必须为hwndPareRt窗口的直接子窗口而非后代窗口。
        /// 如果HwndChildAfter为NULL，查找从hwndParent的第一个子窗口开始。
        /// 如果hwndParent 和 hwndChildAfter同时为NULL，
        /// 则函数查找所有的顶层窗口及消息窗口。</param>
        /// <param name="lpszClass"></param>
        /// <param name="lpszWindow"></param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "FindWindowEx")]
        public static extern IntPtr FindWindowEx(int hwndParent, int hwndChildAfter, string lpszClass, string lpszWindow);

        /// <summary>
        /// 该函数设置指定窗口的显示状态。
        /// </summary>
        /// <param name="hWnd">窗口句柄</param>
        /// <param name="nCmdShow">指定窗口如何显示</param>
        /// <returns></returns>
        [DllImport("user32.dll", EntryPoint = "ShowWindow")]
        public static extern IntPtr ShowWindow(IntPtr hWnd, int nCmdShow);

        /// <summary>
        /// 函数功能：该函数将创建指定窗口的线程设置到前台，
        /// 并且激活该窗口。键盘输入转向该窗口，并为用户改各种可视的记号。
        /// 系统给创建前台窗口的线程分配的权限稍高于其他线程。
        /// </summary>
        /// <param name="hWnd">将被激活并被调入前台的窗口句柄</param>
        /// <returns>如果窗口设入了前台，返回值为非零；如果窗口未被设入前台，返回值为零</returns> 
        [DllImport("user32.dll", EntryPoint = "SetForegroundWindow")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("wininet.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern bool InternetSetOption(int hInternet, int dwOption, IntPtr lpBuffer, int dwBufferLength);

		/// <summary>
		/// 清除页面cookie---需要修改设置允许不安全代码执行
		/// </summary>
		/// <returns>true:success;false:fail</returns>
		public static unsafe bool SuppressWininetBehavior()
		{
			/* SOURCE: http://msdn.microsoft.com/en-us/library/windows/desktop/aa385328%28v=vs.85%29.aspx
			* INTERNET_OPTION_SUPPRESS_BEHAVIOR (81):
			*   A general purpose option that is used to suppress behaviors on a process-wide basis. 
			*   The lpBuffer parameter of the function must be a pointer to a DWORD containing the specific behavior to suppress. 
			*   This option cannot be queried with InternetQueryOption. 
			*   
			* INTERNET_SUPPRESS_COOKIE_PERSIST (3):
			*   Suppresses the persistence of cookies, even if the server has specified them as persistent.
			*   Version: Requires Internet Explorer 8.0 or later.
			*/
			int option = 3/* INTERNET_SUPPRESS_COOKIE_PERSIST*/;
			int* optionPtr = &option;
			bool success = InternetSetOption(0, 81/*INTERNET_OPTION_SUPPRESS_BEHAVIOR*/, new IntPtr(optionPtr), sizeof(int));
			return success;
		}
	}

    //一些常量
    public class APINameHelper
    {
        public const int SW_HIDE = 0; //隐藏窗口，活动状态给令一个窗口 

        /// <summary>
        /// 用原来的大小和位置显示一个窗口，同时令其进入活动状态
        /// </summary>
        public const int SW_SHOWNORMAL = 1;
        public const int SW_NORMAL = 1;
        public const int SW_SHOWMINIMIZED = 2;
        public const int SW_SHOWMAXIMIZED = 3;
        public const int SW_MAXIMIZE = 3;
        public const int SW_SHOWNOACTIVATE = 4; //用最近的大小和位置显示一个窗口，同时不改变活动窗口
        public const int SW_SHOW = 5;//用当前的大小和位置显示一个窗口，同时令其进入活动状态
        public const int SW_MINIMIZE = 6;//最小化窗口，活动状态给令一个窗口
        public const int SW_SHOWMINNOACTIVE = 7;//最小化一个窗口，同时不改变活动窗口
        public const int SW_SHOWNA = 8;//用当前的大小和位置显示一个窗口，不改变活动窗口
        public const int SW_RESTORE = 9; //与 SW_SHOWNORMAL  1 相同
        public const int SW_SHOWDEFAULT = 10;
        public const int SW_FORCEMINIMIZE = 11;
        public const int SW_MAX = 11;


        public const int WM_CHAR = 0x0102;
        public const int WM_KEYDOWN = 0x0100;
        public const int WM_KEYUP = 0x0101;

        public const int WM_PASTE = 0x0302;
        public const int WM_CLEAR = 0x0303;
    }
}
