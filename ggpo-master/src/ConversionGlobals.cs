using System;
using System.Diagnostics;

#define ARRAY_SIZE

public static class Globals
{
//C++ TO C# CONVERTER NOTE: 'extern' variable declarations are not required in C#:
	//extern GGPOSession *ggpo;

	internal static double degtorad(double deg)
	{
	   return ((double)3.1415926) * deg / 180;
	}

	internal static double distance(Position lhs, Position rhs)
	{
	   double x = rhs.x - lhs.x;
	   double y = rhs.y - lhs.y;
	   return Math.Sqrt(x * x + y * y);
	}

	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define GGPO_API __declspec(dllexport)
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define GGPO_API __declspec(dllimport)
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define GGPO_ERRORLIST GGPO_ERRORLIST_ENTRY(GGPO_OK, 0) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_SUCCESS, 0) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_GENERAL_FAILURE, -1) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_INVALID_SESSION, 1) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_INVALID_PLAYER_HANDLE, 2) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_PLAYER_OUT_OF_RANGE, 3) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_PREDICTION_THRESHOLD, 4) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_UNSUPPORTED, 5) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_NOT_SYNCHRONIZED, 6) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_IN_ROLLBACK, 7) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_INPUT_DROPPED, 8) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_PLAYER_DISCONNECTED, 9) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_TOO_MANY_SPECTATORS, 10) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_INVALID_REQUEST, 11)
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define GGPO_ERRORLIST_ENTRY(name, value) name = value,
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define GGPO_SUCCEEDED(result) ((result) == GGPO_ERRORCODE_SUCCESS)


	public static void ggpoutil_perfmon_init(IntPtr hwnd)
	{
	   _hwnd = hwnd;
	   _green_pen = CreatePen(PS_SOLID, 1, RGB(0, 255, 0));
	   _red_pen = CreatePen(PS_SOLID, 1, RGB(255, 64, 64));
	   _blue_pen = CreatePen(PS_SOLID, 1, RGB(64, 64, 255));
	   _yellow_pen = CreatePen(PS_SOLID, 1, RGB(255, 235, 0));
	   _grey_pen = CreatePen(PS_SOLID, 1, RGB(96, 96, 96));
	   _pink_pen = CreatePen(PS_SOLID, 1, RGB(255, 0, 255));
	}

	public static void ggpoutil_perfmon_update(GGPOSession ggpo, GGPOPlayerHandle[] players, int num_players)
	{
	   GGPONetworkStats stats = new GGPONetworkStats();
	   int i;

	   _num_players = num_players;

	   if (_graph_size < DefineConstants.MAX_GRAPH_SIZE)
	   {
		  i = _graph_size++;
	   }
	   else
	   {
		  i = _first_graph_index;
		  _first_graph_index = (_first_graph_index + 1) % DefineConstants.MAX_GRAPH_SIZE;
	   }

	   /*
	    * Random graphs
	    */
	   //_predict_queue_graph[i] = stats.network.predict_queue_len;
	   //_remote_queue_graph[i] = stats.network.recv_queue_len;
	   //_send_queue_graph[i] = stats.network.send_queue_len;


	   for (int j = 0; j < num_players; j++)
	   {
		  ggpo_get_network_stats(ggpo, players[j], stats);

		  /*
		   * Ping
		   */
		  _ping_graph[j][i] = stats.network.ping;

		  /*
		   * Frame Advantage
		   */
		  _local_fairness_graph[j][i] = stats.timesync.local_frames_behind;
		  _remote_fairness_graph[j][i] = stats.timesync.remote_frames_behind;
		  if (stats.timesync.local_frames_behind < 0 && stats.timesync.remote_frames_behind < 0)
		  {
			 /*
			  * Both think it's unfair (which, ironically, is fair).  Scale both and subtrace.
			  */
			 _fairness_graph[i] = Math.Abs(Math.Abs(stats.timesync.local_frames_behind) - Math.Abs(stats.timesync.remote_frames_behind));
		  }
		  else if (stats.timesync.local_frames_behind > 0 && stats.timesync.remote_frames_behind > 0)
		  {
			 /*
			  * Impossible!  Unless the network has negative transmit time.  Odd....
			  */
			 _fairness_graph[i] = 0;
		  }
		  else
		  {
			 /*
			  * They disagree.  Add.
			  */
			 _fairness_graph[i] = Math.Abs(stats.timesync.local_frames_behind) + Math.Abs(stats.timesync.remote_frames_behind);
		  }
	   }

	   int now = timeGetTime();
	   if (_dialog)
	   {
		  InvalidateRect(GetDlgItem(_dialog, IDC_FAIRNESS_GRAPH), null, false);
		  InvalidateRect(GetDlgItem(_dialog, IDC_NETWORK_GRAPH), null, false);

		  if (now > _last_text_update_time + 500)
		  {
			 string fLocal = new string(new char[128]);
			 string fRemote = new string(new char[128]);
			 string fBandwidth = new string(new char[128]);
			 string msLag = new string(new char[128]);
			 string frameLag = new string(new char[128]);

			 sprintf_s(msLag, ARRAYSIZE(msLag), "%d ms", stats.network.ping);
			 sprintf_s(frameLag, ARRAYSIZE(frameLag), "%.1f frames", stats.network.ping ? stats.network.ping * 60.0 / 1000 : 0);
			 sprintf_s(fBandwidth, ARRAYSIZE(fBandwidth), "%.2f kilobytes/sec", stats.network.kbps_sent / 8.0);
			 sprintf_s(fLocal, ARRAYSIZE(fLocal), "%d frames", stats.timesync.local_frames_behind);
			 sprintf_s(fRemote, ARRAYSIZE(fRemote), "%d frames", stats.timesync.remote_frames_behind);
			 SetWindowTextA(GetDlgItem(_dialog, IDC_NETWORK_LAG), msLag);
			 SetWindowTextA(GetDlgItem(_dialog, IDC_FRAME_LAG), frameLag);
			 SetWindowTextA(GetDlgItem(_dialog, IDC_BANDWIDTH), fBandwidth);
			 SetWindowTextA(GetDlgItem(_dialog, IDC_LOCAL_AHEAD), fLocal);
			 SetWindowTextA(GetDlgItem(_dialog, IDC_REMOTE_AHEAD), fRemote);
			 _last_text_update_time = now;
		  }
	   }
	}

	public static void ggpoutil_perfmon_toggle()
	{
	   if (!_dialog)
	   {
		  _dialog = CreateDialog(GetModuleHandle(null), MAKEINTRESOURCE(IDD_PERFMON), _hwnd, ggpo_perfmon_dlgproc);
	   }
	   _shown = !_shown;
	   ShowWindow(_dialog, _shown ? SW_SHOW : SW_HIDE);
	}




	internal static IntPtr _hwnd = null;
	internal static IntPtr _dialog = null;
	internal static IntPtr _green_pen;
	internal static IntPtr _red_pen;
	internal static IntPtr _blue_pen;
	internal static IntPtr _yellow_pen;
	internal static IntPtr _grey_pen;
	internal static IntPtr _pink_pen;
	internal static IntPtr[] _fairness_pens = new IntPtr[DefineConstants.MAX_PLAYERS];
	internal static bool _shown = false;
	internal static int _last_text_update_time = 0;

	public static int _num_players;
	public static int _first_graph_index = 0;
	public static int _graph_size = 0;
	public static int[][] _ping_graph = RectangularArrays.RectangularIntArray(DefineConstants.MAX_PLAYERS, DefineConstants.MAX_GRAPH_SIZE);
	public static int[][] _local_fairness_graph = RectangularArrays.RectangularIntArray(DefineConstants.MAX_PLAYERS, DefineConstants.MAX_GRAPH_SIZE);
	public static int[][] _remote_fairness_graph = RectangularArrays.RectangularIntArray(DefineConstants.MAX_PLAYERS, DefineConstants.MAX_GRAPH_SIZE);
	public static int[] _fairness_graph = new int[DefineConstants.MAX_GRAPH_SIZE];
	public static int[] _predict_queue_graph = new int[DefineConstants.MAX_GRAPH_SIZE];
	public static int[] _remote_queue_graph = new int[DefineConstants.MAX_GRAPH_SIZE];
	public static int[] _send_queue_graph = new int[DefineConstants.MAX_GRAPH_SIZE];

	internal static void draw_graph(LPDRAWITEMSTRUCT di, IntPtr pen, int[] graph, int count, int min, int max)
	{
	   POINT[] pt = Arrays.InitializeWithDefaultInstances<POINT>(DefineConstants.MAX_GRAPH_SIZE);
	   int i;
	   int height = di.rcItem.bottom - di.rcItem.top;
	   int width = di.rcItem.right - di.rcItem.left;
	   int range = max - min;
	   int offset = 0;

	   if (count > width)
	   {
		  offset = count - width;
		  count = width;
	   }
	   for (i = 0; i < count; i++)
	   {
		  int value = graph[(_first_graph_index + offset + i) % DefineConstants.MAX_GRAPH_SIZE] - min;
		  int y = height - (value * height / range);
		  pt[i].x = (width - count) + i;
		  pt[i].y = y;
	   }
	   SelectObject(di.hDC, pen);
	   Polyline(di.hDC, pt, count);
	}

	internal static void draw_grid(LPDRAWITEMSTRUCT di)
	{
	   FillRect(di.hDC, di.rcItem, (IntPtr)GetStockObject(BLACK_BRUSH));
	}

	internal static void draw_network_graph_control(LPDRAWITEMSTRUCT di)
	{
	   draw_grid(new LPDRAWITEMSTRUCT(di));
	   for (int i = 0; i < _num_players; i++)
	   {
		  draw_graph(new LPDRAWITEMSTRUCT(di), _green_pen, _ping_graph[i], _graph_size, 0, 500);
	   }
	   draw_graph(new LPDRAWITEMSTRUCT(di), _pink_pen, _predict_queue_graph, _graph_size, 0, 14);
	   draw_graph(new LPDRAWITEMSTRUCT(di), _red_pen, _remote_queue_graph, _graph_size, 0, 14);
	   draw_graph(new LPDRAWITEMSTRUCT(di), _blue_pen, _send_queue_graph, _graph_size, 0, 14);

	   _fairness_pens[0] = _blue_pen;
	   _fairness_pens[1] = _grey_pen;
	   _fairness_pens[2] = _red_pen;
	   _fairness_pens[3] = _pink_pen;
	}

	internal static void draw_fairness_graph_control(LPDRAWITEMSTRUCT di)
	{
	   int midpoint = (di.rcItem.bottom - di.rcItem.top) / 2;

	   draw_grid(new LPDRAWITEMSTRUCT(di));
	   SelectObject(di.hDC, _grey_pen);

	   MoveToEx(di.hDC, di.rcItem.left, midpoint, null);
	   LineTo(di.hDC, di.rcItem.right, midpoint);

	   for (int i = 0; i < _num_players; i++)
	   {
		  draw_graph(new LPDRAWITEMSTRUCT(di), _fairness_pens[i], _remote_fairness_graph[i], _graph_size, -DefineConstants.MAX_FAIRNESS, DefineConstants.MAX_FAIRNESS);
		  //draw_graph(di, _blue_pen,   _local_fairness_graph,  _graph_size, -MAX_FAIRNESS, MAX_FAIRNESS);
	   }
	   draw_graph(new LPDRAWITEMSTRUCT(di), _yellow_pen, _fairness_graph, _graph_size, -DefineConstants.MAX_FAIRNESS, DefineConstants.MAX_FAIRNESS);
	}

//C++ TO C# CONVERTER NOTE: CALLBACK is not available in C#:
//ORIGINAL LINE: static System.IntPtr CALLBACK ggpo_perfmon_dlgproc(System.IntPtr hwndDlg, uint uMsg, System.IntPtr, System.IntPtr lParam)
	internal static IntPtr ggpo_perfmon_dlgproc(IntPtr hwndDlg, uint uMsg, IntPtr UnnamedParameter, IntPtr lParam)
	{

	   switch (uMsg)
	   {
	   case WM_COMMAND:
	   {
			 ggpoutil_perfmon_toggle();
			 return true;
	   }
		  break;

	   case WM_DRAWITEM:
	   {
			 LPDRAWITEMSTRUCT lpDrawItem = (LPDRAWITEMSTRUCT) lParam;
			 if (lpDrawItem.CtlID == IDC_FAIRNESS_GRAPH)
			 {
				draw_fairness_graph_control(new LPDRAWITEMSTRUCT(lpDrawItem));
			 }
			 else
			 {
				draw_network_graph_control(new LPDRAWITEMSTRUCT(lpDrawItem));
			 }
			 return true;
	   }
	   case WM_INITDIALOG:
	   {
			 string pid = new string(new char[64]);
			 snprintf(pid, ARRAYSIZE(pid), "%d", GetCurrentProcessId());
			 SetWindowTextA(GetDlgItem(hwndDlg, IDC_PID), pid);
			 return true;
	   }
	   }
	   return false;
	}

	public static void ggpoutil_perfmon_exit()
	{
	   DeleteObject(_green_pen);
	   DeleteObject(_red_pen);
	   DeleteObject(_blue_pen);
	   DeleteObject(_yellow_pen);
	   DeleteObject(_grey_pen);
	   DeleteObject(_pink_pen);
	}


	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define GGPO_API __declspec(dllexport)
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define GGPO_API __declspec(dllimport)
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define GGPO_ERRORLIST GGPO_ERRORLIST_ENTRY(GGPO_OK, 0) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_SUCCESS, 0) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_GENERAL_FAILURE, -1) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_INVALID_SESSION, 1) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_INVALID_PLAYER_HANDLE, 2) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_PLAYER_OUT_OF_RANGE, 3) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_PREDICTION_THRESHOLD, 4) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_UNSUPPORTED, 5) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_NOT_SYNCHRONIZED, 6) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_IN_ROLLBACK, 7) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_INPUT_DROPPED, 8) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_PLAYER_DISCONNECTED, 9) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_TOO_MANY_SPECTATORS, 10) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_INVALID_REQUEST, 11)
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define GGPO_ERRORLIST_ENTRY(name, value) name = value,
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define GGPO_SUCCEEDED(result) ((result) == GGPO_ERRORCODE_SUCCESS)
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define ARRAY_SIZE(n) (sizeof(n) / sizeof(n[0]))

//C++ TO C# CONVERTER NOTE: CALLBACK is not available in C#:
//ORIGINAL LINE: int CALLBACK MainWindowProc(System.IntPtr hwnd, uint uMsg, System.IntPtr wParam, System.IntPtr lParam)
	public static int MainWindowProc(IntPtr hwnd, uint uMsg, IntPtr wParam, IntPtr lParam)
	{
	   switch (uMsg)
	   {
	   case WM_ERASEBKGND:
		  return 1;
	   case WM_KEYDOWN:
		  if (wParam == 'P')
		  {
			 ggpoutil_perfmon_toggle();
		  }
		  else if (wParam == VK_ESCAPE)
		  {
			 VectorWar_Exit();
			 PostQuitMessage(0);
		  }
		  else if (wParam >= VK_F1 && wParam <= VK_F12)
		  {
			 VectorWar_DisconnectPlayer((int)(wParam - VK_F1));
		  }
		  return 0;
	   case WM_PAINT:
		  VectorWar_DrawCurrentFrame();
		  ValidateRect(hwnd, null);
		  return 0;
	   case WM_CLOSE:
		  PostQuitMessage(0);
		  break;
	   }
	   return CallWindowProc(DefWindowProc, hwnd, uMsg, wParam, lParam);
	}

	public static IntPtr CreateMainWindow(IntPtr hInstance)
	{
	   IntPtr hwnd;
	   WNDCLASSEX wndclass = new WNDCLASSEX();
	   RECT rc = new RECT();
	   int width = 640;
	   int height = 480;
	   string titlebuf = new string(new char[128]);

	   wsprintf(titlebuf, "(pid:%d) ggpo sdk sample: vector war", GetCurrentProcessId());
	   wndclass.cbSize = sizeof(WNDCLASSEX);
	   wndclass.lpfnWndProc = MainWindowProc;
	   wndclass.lpszClassName = "vwwnd";
	   RegisterClassEx(wndclass);
	   hwnd = CreateWindow("vwwnd", titlebuf, WS_OVERLAPPEDWINDOW | WS_VISIBLE, CW_USEDEFAULT, CW_USEDEFAULT, width, height, null, null, hInstance, null);

	   GetClientRect(hwnd, rc);
	   SetWindowPos(hwnd, null, 0, 0, width + (width - (rc.right - rc.left)), height + (height - (rc.bottom - rc.top)), SWP_NOMOVE);
	   return hwnd;
	}

	public static void RunMainLoop(IntPtr hwnd)
	{
	   MSG msg = new MSG();
	   int start;
	   int next;
	   int now;

	   start = next = now = timeGetTime();
	   while (true)
	   {
		  while (PeekMessage(msg, null, 0, 0, PM_REMOVE))
		  {
			 TranslateMessage(msg);
			 DispatchMessage(msg);
			 if (msg.message == WM_QUIT)
			 {
				return;
			 }
		  }
		  now = timeGetTime();
		  VectorWar_Idle(Math.Max(0, next - now - 1));
		  if (now >= next)
		  {
			 VectorWar_RunFrame(hwnd);
			 next = now + (1000 / 60);
		  }
	   }
	}

	public static void Syntax()
	{
	   MessageBox(null, "Syntax: vectorwar.exe <local port> <num players> ('local' | <remote ip>:<remote port>)*\n", "Could not start", MB_OK);
	}

//C++ TO C# CONVERTER NOTE: APIENTRY is not available in C#:
//ORIGINAL LINE: int APIENTRY wWinMain(_In_ System.IntPtr hInstance, _In_opt_ System.IntPtr, _In_ char*, _In_ int)
	public static int wWinMain(_In_ IntPtr hInstance, _In_opt_ IntPtr, _In_ string UnnamedParameter, _In_ int)
	{
	   IntPtr hwnd = CreateMainWindow(new _In_(hInstance));
	   int offset = 1;
	   int local_player = 0;
	   WSADATA wd = new WSADATA();
	   string wide_ip_buffer = new string(new char[128]);
	   uint wide_ip_buffer_size = (uint)ARRAYSIZE(wide_ip_buffer);

	   WSAStartup(MAKEWORD(2, 2), wd);
	   POINT[] window_offsets =
	   {
		   new POINT(64, 64),
		   new POINT(740, 64),
		   new POINT(64, 600),
		   new POINT(740, 600)
	   };

	#if DEBUG
	   _CrtSetDbgFlag(_CRTDBG_ALLOC_MEM_DF | _CRTDBG_LEAK_CHECK_DF);
	#endif

	   if (__argc < 3)
	   {
		  Syntax();
		  return 1;
	   }
	   ushort local_port = (ushort)_wtoi(__wargv[offset++]);
	   int num_players = _wtoi(__wargv[offset++]);
	   if (num_players < 0 || __argc < offset + num_players)
	   {
		  Syntax();
		  return 1;
	   }
	   if (wcscmp(__wargv[offset], "spectate") == 0)
	   {
		  string host_ip = new string(new char[128]);
		  ushort host_port;
		  if (swscanf_s(__wargv[offset + 1], "%[^:]:%hu", wide_ip_buffer, wide_ip_buffer_size, host_port) != 2)
		  {
			 Syntax();
			 return 1;
		  }
		  wcstombs_s(null, host_ip, ARRAYSIZE(host_ip), wide_ip_buffer, _TRUNCATE);
		  VectorWar_InitSpectator(hwnd, local_port, num_players, ref host_ip, host_port);
	   }
	   else
	   {
		  GGPOPlayer[] players = Arrays.InitializeWithDefaultInstances<GGPOPlayer>(DefineConstants.GGPO_MAX_SPECTATORS + DefineConstants.GGPO_MAX_PLAYERS);

		  int i;
		  for (i = 0; i < num_players; i++)
		  {
			 string arg = __wargv[offset++];

			 players[i].size = sizeof(GGPOPlayer);
			 players[i].player_num = i + 1;
			 if (!_wcsicmp(arg, "local"))
			 {
				players[i].type = (GGPOPlayerType)GGPOPlayerType.GGPO_PLAYERTYPE_LOCAL;
				local_player = i;
				continue;
			 }

			 players[i].type = GGPOPlayerType.GGPO_PLAYERTYPE_REMOTE;
			 if (swscanf_s(arg, "%[^:]:%hd", wide_ip_buffer, wide_ip_buffer_size, players[i].u.remote.port) != 2)
			 {
				Syntax();
				return 1;
			 }
			 wcstombs_s(null, players[i].u.remote.ip_address, ARRAYSIZE(players[i].u.remote.ip_address), wide_ip_buffer, _TRUNCATE);
		  }
		  // these are spectators...
		  int num_spectators = 0;
		  while (offset < __argc)
		  {
			 players[i].type = GGPOPlayerType.GGPO_PLAYERTYPE_SPECTATOR;
			 if (swscanf_s(__wargv[offset++], "%[^:]:%hd", wide_ip_buffer, wide_ip_buffer_size, players[i].u.remote.port) != 2)
			 {
				Syntax();
				return 1;
			 }
			 wcstombs_s(null, players[i].u.remote.ip_address, ARRAYSIZE(players[i].u.remote.ip_address), wide_ip_buffer, _TRUNCATE);
			 i++;
			 num_spectators++;
		  }

//C++ TO C# CONVERTER WARNING: This 'sizeof' ratio was replaced with a direct reference to the array length:
//ORIGINAL LINE: if (local_player < sizeof(window_offsets) / sizeof(window_offsets[0]))
		  if (local_player < window_offsets.Length)
		  {
			 global::SetWindowPos(hwnd, null, window_offsets[local_player].x, window_offsets[local_player].y, 0, 0, SWP_NOSIZE | SWP_NOZORDER);
		  }

		  VectorWar_Init(hwnd, local_port, num_players, players, num_spectators);
	   }
	   RunMainLoop(hwnd);
	   VectorWar_Exit();
	   WSACleanup();
	   DestroyWindow(hwnd);
	   return 0;
	}

/*
 * VectorWar_Init --
 *
 * Initialize the vector war game.  This initializes the game state and
 * the video renderer and creates a new network session.
 */

	public static void VectorWar_Init(IntPtr hwnd, ushort localport, int num_players, GGPOPlayer[] players, int num_spectators)
	{
	   GGPOErrorCode result;
	   renderer = new GDIRenderer(hwnd);

	   // Initialize the game state
	   gs.Init(hwnd, num_players);
	   ngs.num_players = num_players;

	   // Fill in a ggpo callbacks structure to pass to start_session.
	   GGPOSessionCallbacks cb = new GGPOSessionCallbacks();
	   cb.begin_game = vw_begin_game_callback;
	   cb.advance_frame = vw_advance_frame_callback;
	   cb.load_game_state = vw_load_game_state_callback;
	   cb.save_game_state = vw_save_game_state_callback;
	   cb.free_buffer = vw_free_buffer;
	   cb.on_event = vw_on_event_callback;
	   cb.log_game_state = vw_log_game_state;

	#if SYNC_TEST
	   result = ggpo_start_synctest(ggpo, cb, "vectorwar", num_players, sizeof(int), 1);
	#else
	   result = ggpo_start_session(ggpo, cb, "vectorwar", num_players, sizeof(int), localport);
	#endif

	   // automatically disconnect clients after 3000 ms and start our count-down timer
	   // for disconnects after 1000 ms.   To completely disable disconnects, simply use
	   // a value of 0 for ggpo_set_disconnect_timeout.
	   ggpo_set_disconnect_timeout(ggpo, 3000);
	   ggpo_set_disconnect_notify_start(ggpo, 1000);

	   int i;
	   for (i = 0; i < num_players + num_spectators; i++)
	   {
		  GGPOPlayerHandle handle = new GGPOPlayerHandle();
		  result = ggpo_add_player(ggpo, players + i, ref handle);
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: ngs.players[i].handle = handle;
		  ngs.players[i].handle.CopyFrom(handle);
		  ngs.players[i].type = players[i].type;
		  if (players[i].type == (GGPOPlayerType)GGPOPlayerType.GGPO_PLAYERTYPE_LOCAL)
		  {
			 ngs.players[i].connect_progress = 100;
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: ngs.local_player_handle = handle;
			 ngs.local_player_handle.CopyFrom(handle);
			 ngs.SetConnectState(new GGPOPlayerHandle(handle), PlayerConnectState.Connecting);
			 ggpo_set_frame_delay(ggpo, new GGPOPlayerHandle(handle), DefineConstants.FRAME_DELAY);
		  }
		  else
		  {
			 ngs.players[i].connect_progress = 0;
		  }
	   }

	   ggpoutil_perfmon_init(hwnd);
	   renderer.SetStatusText("Connecting to peers.");
	}

/*
 * VectorWar_InitSpectator --
 *
 * Create a new spectator session
 */

	public static void VectorWar_InitSpectator(IntPtr hwnd, ushort localport, int num_players, ref string host_ip, ushort host_port)
	{
	   GGPOErrorCode result;
	   renderer = new GDIRenderer(hwnd);

	   // Initialize the game state
	   gs.Init(hwnd, num_players);
	   ngs.num_players = num_players;

	   // Fill in a ggpo callbacks structure to pass to start_session.
	   GGPOSessionCallbacks cb = new GGPOSessionCallbacks();
	   cb.begin_game = vw_begin_game_callback;
	   cb.advance_frame = vw_advance_frame_callback;
	   cb.load_game_state = vw_load_game_state_callback;
	   cb.save_game_state = vw_save_game_state_callback;
	   cb.free_buffer = vw_free_buffer;
	   cb.on_event = vw_on_event_callback;
	   cb.log_game_state = vw_log_game_state;

	   result = ggpo_start_spectating(ggpo, cb, "vectorwar", num_players, sizeof(int), localport, ref host_ip, host_port);

	   ggpoutil_perfmon_init(hwnd);

	   renderer.SetStatusText("Starting new spectator session");
	}

/*
 * VectorWar_DrawCurrentFrame --
 *
 * Draws the current frame without modifying the game state.
 */

	public static void VectorWar_DrawCurrentFrame()
	{
	   if (renderer != null)
	   {
		  renderer.Draw(gs, ngs);
	   }
	}

/*
 * VectorWar_AdvanceFrame --
 *
 * Advances the game state by exactly 1 frame using the inputs specified
 * for player 1 and player 2.
 */

	public static void VectorWar_AdvanceFrame(int[] inputs, int disconnect_flags)
	{
	   gs.Update(inputs, disconnect_flags);

	   // update the checksums to display in the top of the window.  this
	   // helps to detect desyncs.
	   ngs.now.framenumber = gs._framenumber;
	   ngs.now.checksum = fletcher32_checksum((short) gs, sizeof(gs) / 2);
	   if ((gs._framenumber % 90) == 0)
	   {
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: ngs.periodic = ngs.now;
		  ngs.periodic.CopyFrom(ngs.now);
	   }

	   // Notify ggpo that we've moved forward exactly 1 frame.
	   ggpo_advance_frame(ggpo);

	   // Update the performance monitor display.
	   GGPOPlayerHandle[] handles = Arrays.InitializeWithDefaultInstances<GGPOPlayerHandle>(DefineConstants.MAX_PLAYERS);
	   int count = 0;
	   for (int i = 0; i < ngs.num_players; i++)
	   {
		  if (ngs.players[i].type == GGPOPlayerType.GGPO_PLAYERTYPE_REMOTE)
		  {
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: handles[count++] = ngs.players[i].handle;
			 handles[count++].CopyFrom(ngs.players[i].handle);
		  }
	   }
	   ggpoutil_perfmon_update(ggpo, handles, count);
	}

/*
 * VectorWar_RunFrame --
 *
 * Run a single frame of the game.
 */

	public static void VectorWar_RunFrame(IntPtr hwnd)
	{
	  GGPOErrorCode result = GGPOErrorCode.GGPO_OK;
	  int disconnect_flags;
	  int[] inputs = new int[DefineConstants.MAX_SHIPS];

	  if (ngs.local_player_handle != DefineConstants.GGPO_INVALID_HANDLE)
	  {
		 int input = ReadInputs(hwnd);
	#if SYNC_TEST
		 input = rand(); // test: use random inputs to demonstrate sync testing
	#endif
		 result = ggpo_add_local_input(ggpo, new GGPOPlayerHandle(ngs.local_player_handle), input, sizeof(int));
	  }

	   // synchronize these inputs with ggpo.  If we have enough input to proceed
	   // ggpo will modify the input list with the correct inputs to use and
	   // return 1.
	  if (((result) == GGPOErrorCode.GGPO_ERRORCODE_SUCCESS))
	  {
		 result = ggpo_synchronize_input(ggpo, (object)inputs, sizeof(int) * DefineConstants.MAX_SHIPS, ref disconnect_flags);
		 if (((result) == GGPOErrorCode.GGPO_ERRORCODE_SUCCESS))
		 {
			 // inputs[0] and inputs[1] contain the inputs for p1 and p2.  Advance
			 // the game by 1 frame using those inputs.
			 VectorWar_AdvanceFrame(inputs, disconnect_flags);
		 }
	  }
	  VectorWar_DrawCurrentFrame();
	}

/*
 * VectorWar_Idle --
 *
 * Spend our idle time in ggpo so it can use whatever time we have left over
 * for its internal bookkeeping.
 */

	public static void VectorWar_Idle(int time)
	{
	   ggpo_idle(ggpo, time);
	}

/*
 * VectorWar_DisconnectPlayer --
 *
 * Disconnects a player from this session.
 */


	public static void VectorWar_DisconnectPlayer(int player)
	{
	   if (player < ngs.num_players)
	   {
		  string logbuf = new string(new char[128]);
		  GGPOErrorCode result = ggpo_disconnect_player(ggpo, new GGPOPlayerHandle(ngs.players[player].handle));
		  if (((result) == GGPOErrorCode.GGPO_ERRORCODE_SUCCESS))
		  {
			 sprintf_s(logbuf, ARRAYSIZE(logbuf), "Disconnected player %d.\n", player);
		  }
		  else
		  {
			 sprintf_s(logbuf, ARRAYSIZE(logbuf), "Error while disconnecting player (err:%d).\n", result);
		  }
		  renderer.SetStatusText(logbuf);
	   }
	}

	public static void VectorWar_Exit()
	{
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
	   memset(gs, 0, sizeof(GameState));
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
	   memset(ngs, 0, sizeof(NonGameState));

	   if (ggpo != null)
	   {
		  ggpo_close_session(ggpo);
		  ggpo = null;
	   }
	   if (renderer != null)
	   {
	   renderer.Dispose();
	   }
	   renderer = null;
	}

	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define ARRAY_SIZE(n) (sizeof(n) / sizeof(n[0]))



	//#define SYNC_TEST    // test: turn on synctest

	public static GameState gs = new GameState();
	public static NonGameState ngs = new NonGameState();
	public static Renderer renderer = null;
	public static GGPOSession ggpo = null;

	/* 
	 * Simple checksum function stolen from wikipedia:
	 *
	 *   http://en.wikipedia.org/wiki/Fletcher%27s_checksum
	 */

//C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on the parameter 'data', so pointers on this parameter are left unchanged:
	public static int fletcher32_checksum(short * data, size_t len)
	{
	   int sum1 = 0xffff;
	   int sum2 = 0xffff;

	   while (len != null)
	   {
		  size_t tlen = len > 360 ? 360 : len;
		  len -= tlen;
		  do
		  {
			 sum1 += *data++;
			 sum2 += sum1;
		  } while ((--tlen) != 0);
		  sum1 = (sum1 & 0xffff) + (sum1 >> 16);
		  sum2 = (sum2 & 0xffff) + (sum2 >> 16);
	   }

	   /* Second reduction step to reduce sums to 16 bits */
	   sum1 = (sum1 & 0xffff) + (sum1 >> 16);
	   sum2 = (sum2 & 0xffff) + (sum2 >> 16);
	   return sum2 << 16 | sum1;
	}

	/*
	 * vw_begin_game_callback --
	 *
	 * The begin game callback.  We don't need to do anything special here,
	 * so just return true.
	 */
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: bool __cdecl vw_begin_game_callback(const char *)
	public static bool vw_begin_game_callback(string UnnamedParameter)
	{
	   return true;
	}

	/*
	 * vw_on_event_callback --
	 *
	 * Notification from GGPO that something has happened.  Update the status
	 * text at the bottom of the screen to notify the user.
	 */
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: bool __cdecl vw_on_event_callback(GGPOEvent *info)
	public static bool vw_on_event_callback(GGPOEvent info)
	{
	   int progress;
	   switch (info.code)
	   {
	   case GGPOEventCode.GGPO_EVENTCODE_CONNECTED_TO_PEER:
		  ngs.SetConnectState(info.u.connected.player, PlayerConnectState.Synchronizing);
		  break;
	   case GGPOEventCode.GGPO_EVENTCODE_SYNCHRONIZING_WITH_PEER:
		  progress = (int)100 * info.u.synchronizing.count / info.u.synchronizing.total;
		  ngs.UpdateConnectProgress(info.u.synchronizing.player, progress);
		  break;
	   case GGPOEventCode.GGPO_EVENTCODE_SYNCHRONIZED_WITH_PEER:
		  ngs.UpdateConnectProgress(info.u.synchronized.player, 100);
		  break;
	   case GGPOEventCode.GGPO_EVENTCODE_RUNNING:
		  ngs.SetConnectState(PlayerConnectState.Running);
		  renderer.SetStatusText("");
		  break;
	   case GGPOEventCode.GGPO_EVENTCODE_CONNECTION_INTERRUPTED:
		  ngs.SetDisconnectTimeout(info.u.connection_interrupted.player, timeGetTime(), info.u.connection_interrupted.disconnect_timeout);
		  break;
	   case GGPOEventCode.GGPO_EVENTCODE_CONNECTION_RESUMED:
		  ngs.SetConnectState(info.u.connection_resumed.player, PlayerConnectState.Running);
		  break;
	   case GGPOEventCode.GGPO_EVENTCODE_DISCONNECTED_FROM_PEER:
		  ngs.SetConnectState(info.u.disconnected.player, PlayerConnectState.Disconnected);
		  break;
	   case GGPOEventCode.GGPO_EVENTCODE_TIMESYNC:
		  Sleep(1000 * info.u.timesync.frames_ahead / 60);
		  break;
	   }
	   return true;
	}


	/*
	 * vw_advance_frame_callback --
	 *
	 * Notification from GGPO we should step foward exactly 1 frame
	 * during a rollback.
	 */
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: bool __cdecl vw_advance_frame_callback(int)
	public static bool vw_advance_frame_callback(int UnnamedParameter)
	{
	   int[] inputs = new int[DefineConstants.MAX_SHIPS];
	   int disconnect_flags;

	   // Make sure we fetch new inputs from GGPO and use those to update
	   // the game state instead of reading from the keyboard.
	   ggpo_synchronize_input(ggpo, (object)inputs, sizeof(int) * DefineConstants.MAX_SHIPS, ref disconnect_flags);
	   VectorWar_AdvanceFrame(inputs, disconnect_flags);
	   return true;
	}

	/*
	 * vw_load_game_state_callback --
	 *
	 * Makes our current state match the state passed in by GGPO.
	 */
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: bool __cdecl vw_load_game_state_callback(byte *buffer, int len)
	public static bool vw_load_game_state_callback(ref byte buffer, int len)
	{
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
	   memcpy(gs, buffer, len);
	   return true;
	}

	/*
	 * vw_save_game_state_callback --
	 *
	 * Save the current state to a buffer and return it to GGPO via the
	 * buffer and len parameters.
	 */
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: bool __cdecl vw_save_game_state_callback(byte **buffer, int *len, int *checksum, int)
//C++ TO C# CONVERTER TODO TASK: C# does not have an equivalent to pointers to value types:
	public static bool vw_save_game_state_callback(byte[] buffer, ref int len, ref int checksum, int UnnamedParameter)
	{
	   len = sizeof(GameState);
//C++ TO C# CONVERTER TODO TASK: The memory management function 'malloc' has no equivalent in C#:
	   buffer[0] = (byte)malloc(len);
	   if (buffer[0] == 0)
	   {
		  return false;
	   }
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
	   memcpy(buffer[0], gs, len);
	   checksum = fletcher32_checksum((short)buffer[0], len / 2);
	   return true;
	}

	/*
	 * vw_log_game_state --
	 *
	 * Log the gamestate.  Used by the synctest debugging tool.
	 */
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: bool __cdecl vw_log_game_state(char *filename, byte *buffer, int)
	public static bool vw_log_game_state(ref string filename, ref byte buffer, int UnnamedParameter)
	{
	   FILE fp = null;
	   fopen_s(fp, filename, "w");
	   if (fp != null)
	   {
		  GameState gamestate = (GameState)buffer;
		  fprintf(fp, "GameState object.\n");
		  fprintf(fp, "  bounds: %d,%d x %d,%d.\n", gamestate._bounds.left, gamestate._bounds.top, gamestate._bounds.right, gamestate._bounds.bottom);
		  fprintf(fp, "  num_ships: %d.\n", gamestate._num_ships);
		  for (int i = 0; i < gamestate._num_ships; i++)
		  {
			 Ship ship = gamestate._ships + i;
			 fprintf(fp, "  ship %d position:  %.4f, %.4f\n", i, ship.position.x, ship.position.y);
			 fprintf(fp, "  ship %d velocity:  %.4f, %.4f\n", i, ship.velocity.dx, ship.velocity.dy);
			 fprintf(fp, "  ship %d radius:    %d.\n", i, ship.radius);
			 fprintf(fp, "  ship %d heading:   %d.\n", i, ship.heading);
			 fprintf(fp, "  ship %d health:    %d.\n", i, ship.health);
			 fprintf(fp, "  ship %d speed:     %d.\n", i, ship.speed);
			 fprintf(fp, "  ship %d cooldown:  %d.\n", i, ship.cooldown);
			 fprintf(fp, "  ship %d score:     %d.\n", i, ship.score);
			 for (int j = 0; j < DefineConstants.MAX_BULLETS; j++)
			 {
				Bullet bullet = ship.bullets + j;
				fprintf(fp, "  ship %d bullet %d: %.2f %.2f -> %.2f %.2f.\n", i, j, bullet.position.x, bullet.position.y, bullet.velocity.dx, bullet.velocity.dy);
			 }
		  }
		  fclose(fp);
	   }
	   return true;
	}

	/*
	 * vw_free_buffer --
	 *
	 * Free a save state buffer previously returned in vw_save_game_state_callback.
	 */
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: void __cdecl vw_free_buffer(object* buffer)
	public static void vw_free_buffer(object buffer)
	{
//C++ TO C# CONVERTER TODO TASK: The memory management function 'free' has no equivalent in C#:
	   free(buffer);
	}
	public static int ReadInputs(IntPtr hwnd)
	{
		inputtable[] =
		{
			{VK_UP, VectorWarInputs.INPUT_THRUST},
			{VK_DOWN, VectorWarInputs.INPUT_BREAK},
			{VK_LEFT, VectorWarInputs.INPUT_ROTATE_LEFT},
			{VK_RIGHT, VectorWarInputs.INPUT_ROTATE_RIGHT},
			{'D', VectorWarInputs.INPUT_FIRE},
			{'S', VectorWarInputs.INPUT_BOMB}
		};
	   int i;
	   int inputs = 0;

	   if (GetForegroundWindow() == hwnd)
	   {
//C++ TO C# CONVERTER WARNING: This 'sizeof' ratio was replaced with a direct reference to the array length:
//ORIGINAL LINE: for (i = 0; i < sizeof(inputtable) / sizeof(inputtable[0]); i++)
		  for (i = 0; i < inputtable.Length; i++)
		  {
			 if (GetAsyncKeyState(inputtable[i].key))
			 {
				inputs |= inputtable[i].input;
			 }
		  }
	   }

	   return inputs;
	}




	internal const int RECOMMENDATION_INTERVAL = 240;
	internal const int DEFAULT_DISCONNECT_TIMEOUT = 5000;
	internal const int DEFAULT_DISCONNECT_NOTIFY_START = 750;

	/* -----------------------------------------------------------------------
	 * GGPO.net (http://ggpo.net)  -  Copyright 2009 GroundStorm Studios, LLC.
	 *
	 * Use of this software is governed by the MIT license that can be found
	 * in the LICENSE file.
	 */

	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRTIMP __declspec(dllexport)
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRTIMP __declspec(dllimport)
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_BEGIN_C_HEADER __pragma(pack(push, _CRT_PACKING)) extern "C" {
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_END_C_HEADER } __pragma(pack(pop))
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_BEGIN_C_HEADER cpp_quote("__pragma(pack(push, _CRT_PACKING))") cpp_quote("extern \"C\" {")
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_END_C_HEADER cpp_quote("}") cpp_quote("__pragma(pack(pop))")
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_BEGIN_C_HEADER __pragma(pack(push, _CRT_PACKING))
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_END_C_HEADER __pragma(pack(pop))
	//C++ TO C# CONVERTER WARNING: Statement interrupted by a preprocessor statement:
	//The original statement from the file starts with:
	//    __pragma(pack(push, _CRT_PACKING))
	//Preprocessor-interrupted statements cannot be handled by this converter.
	//The remainder of the header file is ignored.
	/* -----------------------------------------------------------------------
	 * GGPO.net (http://ggpo.net)  -  Copyright 2009 GroundStorm Studios, LLC.
	 *
	 * Use of this software is governed by the MIT license that can be found
	 * in the LICENSE file.
	 */


	//C++ TO C# CONVERTER TODO TASK: The following line could not be converted:
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define GGPO_API __declspec(dllexport)
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define GGPO_API __declspec(dllimport)
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define GGPO_ERRORLIST GGPO_ERRORLIST_ENTRY(GGPO_OK, 0) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_SUCCESS, 0) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_GENERAL_FAILURE, -1) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_INVALID_SESSION, 1) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_INVALID_PLAYER_HANDLE, 2) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_PLAYER_OUT_OF_RANGE, 3) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_PREDICTION_THRESHOLD, 4) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_UNSUPPORTED, 5) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_NOT_SYNCHRONIZED, 6) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_IN_ROLLBACK, 7) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_INPUT_DROPPED, 8) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_PLAYER_DISCONNECTED, 9) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_TOO_MANY_SPECTATORS, 10) GGPO_ERRORLIST_ENTRY(GGPO_ERRORCODE_INVALID_REQUEST, 11)
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define GGPO_ERRORLIST_ENTRY(name, value) name = value,
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define GGPO_SUCCEEDED(result) ((result) == GGPO_ERRORCODE_SUCCESS)
	//C++ TO C# CONVERTER TODO TASK: The following line could not be converted:


	internal const int MAX_UDP_PACKET_SIZE = 4096;



	public static SOCKET CreateSocket(ushort bind_port, int retries)
	{
	   SOCKET s = new SOCKET();
	   sockaddr_in sin = new sockaddr_in();
	   ushort port;
	   int optval = 1;

	   s = socket(AF_INET, SOCK_DGRAM, 0);
	   setsockopt(s, SOL_SOCKET, SO_REUSEADDR, (string) & optval, sizeof (int));
	   setsockopt(s, SOL_SOCKET, SO_DONTLINGER, (string) & optval, sizeof (int));

	   // non-blocking...
	   u_long iMode = 1;
	   ioctlsocket(s, FIONBIO, iMode);

	   sin.sin_family = AF_INET;
	   sin.sin_addr.s_addr = htonl(INADDR_ANY);
	   for (port = bind_port; port <= bind_port + retries; port++)
	   {
		  sin.sin_port = htons(port);
		  if (bind(s, (sockaddr) sin, sizeof (sockaddr_in)) != SOCKET_ERROR)
		  {
			 Log("Udp bound to port: %d.\n", port);
			 return new SOCKET(s);
		  }
	   }
	   closesocket(s);
	   return INVALID_SOCKET;
	}



	//C++ TO C# CONVERTER TODO TASK: The following line could not be converted:

	internal const int UDP_HEADER_SIZE = 28; // Size of IP + UDP headers
	internal const int NUM_SYNC_PACKETS = 5;
	internal const int SYNC_RETRY_INTERVAL = 2000;
	internal const int SYNC_FIRST_RETRY_INTERVAL = 500;
	internal const int RUNNING_RETRY_INTERVAL = 200;
	internal const int KEEP_ALIVE_INTERVAL = 200;
	internal const int QUALITY_REPORT_INTERVAL = 1000;
	internal const int NETWORK_STATS_INTERVAL = 1000;
	internal const int UDP_SHUTDOWN_TIMER = 5000;
	internal static int MAX_SEQ_DISTANCE = (1 << 15);

	/* -----------------------------------------------------------------------
	 * GGPO.net (http://ggpo.net)  -  Copyright 2009 GroundStorm Studios, LLC.
	 *
	 * Use of this software is governed by the MIT license that can be found
	 * in the LICENSE file.
	 */

	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRTIMP __declspec(dllexport)
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRTIMP __declspec(dllimport)
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_BEGIN_C_HEADER __pragma(pack(push, _CRT_PACKING)) extern "C" {
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_END_C_HEADER } __pragma(pack(pop))
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_BEGIN_C_HEADER cpp_quote("__pragma(pack(push, _CRT_PACKING))") cpp_quote("extern \"C\" {")
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_END_C_HEADER cpp_quote("}") cpp_quote("__pragma(pack(pop))")
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_BEGIN_C_HEADER __pragma(pack(push, _CRT_PACKING))
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_END_C_HEADER __pragma(pack(pop))
	//C++ TO C# CONVERTER WARNING: Statement interrupted by a preprocessor statement:
	//The original statement from the file starts with:
	//    __pragma(pack(push, _CRT_PACKING))
	//Preprocessor-interrupted statements cannot be handled by this converter.
	//The remainder of the header file is ignored.
	/* -----------------------------------------------------------------------
	 * GGPO.net (http://ggpo.net)  -  Copyright 2009 GroundStorm Studios, LLC.
	 *
	 * Use of this software is governed by the MIT license that can be found
	 * in the LICENSE file.
	 */



//C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on the parameter 'offset', so pointers on this parameter are left unchanged:
	public static void BitVector_SetBit(byte[] vector, int * offset)
	{
	   vector[(*offset) / 8] |= (byte)(1 << ((*offset) % 8));
	   *offset += 1;
	}

//C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on the parameter 'offset', so pointers on this parameter are left unchanged:
	public static void BitVector_ClearBit(byte[] vector, int * offset)
	{
	   vector[(*offset) / 8] &= (byte)(~(1 << ((*offset) % 8)));
	   *offset += 1;
	}

	public static void BitVector_WriteNibblet(ref byte vector, int nibble, ref int offset)
	{
	   Debug.Assert(nibble < (1 << DefineConstants.BITVECTOR_NIBBLE_SIZE));
	   for (int i = 0; i < DefineConstants.BITVECTOR_NIBBLE_SIZE; i++)
	   {
		  if ((nibble & (1 << i)) != 0)
		  {
			 BitVector_SetBit(vector, offset);
		  }
		  else
		  {
			 BitVector_ClearBit(vector, offset);
		  }
	   }
	}

//C++ TO C# CONVERTER TODO TASK: Pointer arithmetic is detected on the parameter 'offset', so pointers on this parameter are left unchanged:
	public static int BitVector_ReadBit(byte[] vector, int * offset)
	{
	   int retval = (vector[(*offset) / 8] & (1 << ((*offset) % 8)));
	   *offset += 1;
	   return retval;
	}

	public static int BitVector_ReadNibblet(ref byte vector, ref int offset)
	{
	   int nibblet = 0;
	   for (int i = 0; i < DefineConstants.BITVECTOR_NIBBLE_SIZE; i++)
	   {
		  nibblet |= (BitVector_ReadBit(vector, offset) << i);
	   }
	   return nibblet;
	}



	/* -----------------------------------------------------------------------
	 * GGPO.net (http://ggpo.net)  -  Copyright 2009 GroundStorm Studios, LLC.
	 *
	 * Use of this software is governed by the MIT license that can be found
	 * in the LICENSE file.
	 */

	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRTIMP __declspec(dllexport)
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRTIMP __declspec(dllimport)
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_BEGIN_C_HEADER __pragma(pack(push, _CRT_PACKING)) extern "C" {
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_END_C_HEADER } __pragma(pack(pop))
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_BEGIN_C_HEADER cpp_quote("__pragma(pack(push, _CRT_PACKING))") cpp_quote("extern \"C\" {")
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_END_C_HEADER cpp_quote("}") cpp_quote("__pragma(pack(pop))")
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_BEGIN_C_HEADER __pragma(pack(push, _CRT_PACKING))
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_END_C_HEADER __pragma(pack(pop))
	//C++ TO C# CONVERTER WARNING: Statement interrupted by a preprocessor statement:
	//The original statement from the file starts with:
	//    __pragma(pack(push, _CRT_PACKING))
	//Preprocessor-interrupted statements cannot be handled by this converter.
	//The remainder of the header file is ignored.

	internal static FILE logfile = null;

	public static void LogFlush()
	{
	   if (logfile != null)
	   {
		  fflush(logfile);
	   }
	}

	internal static string logbuf = new string(new char[4 * 1024 * 1024]);

	public static void Log(string fmt, params object[] LegacyParamArray)
	{
	//   va_list args;
   int ParamCount = -1;
	//   va_start(args, fmt);
	   Logv(fmt, new va_list(args));
	//   va_end(args);
	}

	public static void Logv(string fmt, va_list args)
	{
	   if (!Platform.GetConfigBool("ggpo.log") || Platform.GetConfigBool("ggpo.log.ignore"))
	   {
		  return;
	   }
	   if (logfile == null)
	   {
		  sprintf_s(logbuf, ARRAY_SIZE(logbuf), "log-%d.log", Platform.GetProcessID());
		  fopen_s(logfile, logbuf, "w");
	   }
	   Logv(logfile, fmt, new va_list(args));
	}

	//C++ TO C# CONVERTER NOTE: This was formerly a static local variable declaration (not allowed in C#):
	internal static int Logv_start = 0;

	public static void Logv(FILE fp, string fmt, va_list args)
	{
	   if (Platform.GetConfigBool("ggpo.log.timestamps"))
	   {
	//C++ TO C# CONVERTER NOTE: This static local variable declaration (not allowed in C#) has been moved just prior to the method:
	//	  static int start = 0;
		  int t = 0;
		  if (Logv_start == 0)
		  {
			 Logv_start = (int)Platform.GetCurrentTimeMS();
		  }
		  else
		  {
			 t = (int)(Platform.GetCurrentTimeMS() - Logv_start);
		  }
		  fprintf(fp, "%d.%03d : ", t / 1000, t % 1000);
	   }

	   vfprintf(fp, fmt, args);
	   fflush(fp);

	   vsprintf_s(logbuf, ARRAY_SIZE(logbuf), fmt, args);
	}



	public static void ggpo_log(GGPOSession ggpo, string fmt, params object[] LegacyParamArray)
	{
	//   va_list args;
   int ParamCount = -1;
	//   va_start(args, fmt);
	   ggpo_logv(ggpo, fmt, new va_list(args));
	//   va_end(args);
	}

	public static void ggpo_logv(GGPOSession ggpo, string fmt, va_list args)
	{
	   if (ggpo != null)
	   {
		  ggpo.Logv(fmt, args);
	   }
	}

	public static GGPOErrorCode ggpo_start_session(GGPOSession[] session, GGPOSessionCallbacks cb, string game, int num_players, int input_size, ushort localport)
	{
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: *session= (GGPOSession *)new Peer2PeerBackend(cb, game, localport, num_players, input_size);
	   session[0].CopyFrom((GGPOSession)new Peer2PeerBackend(cb, game, localport, num_players, input_size));
	   return GGPO_OK;
	}

	public static GGPOErrorCode ggpo_add_player(GGPOSession ggpo, GGPOPlayer player, GGPOPlayerHandle handle)
	{
	   if (ggpo == null)
	   {
		  return GGPO_ERRORCODE_INVALID_SESSION;
	   }
	   return new GGPOErrorCode(ggpo.AddPlayer(player, handle));
	}



	public static GGPOErrorCode ggpo_start_synctest(GGPOSession[] ggpo, GGPOSessionCallbacks cb, ref string game, int num_players, int input_size, int frames)
	{
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: *ggpo = (GGPOSession *)new SyncTestBackend(cb, game, frames, num_players);
	   ggpo[0].CopyFrom((GGPOSession)new SyncTestBackend(cb, ref game, frames, num_players));
	   return GGPO_OK;
	}

	public static GGPOErrorCode ggpo_set_frame_delay(GGPOSession ggpo, GGPOPlayerHandle player, int frame_delay)
	{
	   if (ggpo == null)
	   {
		  return GGPO_ERRORCODE_INVALID_SESSION;
	   }
	   return new GGPOErrorCode(ggpo.SetFrameDelay(new GGPOPlayerHandle(player), frame_delay));
	}

	public static GGPOErrorCode ggpo_idle(GGPOSession ggpo, int timeout)
	{
	   if (ggpo == null)
	   {
		  return GGPO_ERRORCODE_INVALID_SESSION;
	   }
	   return new GGPOErrorCode(ggpo.DoPoll(timeout));
	}

	public static GGPOErrorCode ggpo_add_local_input(GGPOSession ggpo, GGPOPlayerHandle player, object values, int size)
	{
	   if (ggpo == null)
	   {
		  return GGPO_ERRORCODE_INVALID_SESSION;
	   }
	   return new GGPOErrorCode(ggpo.AddLocalInput(new GGPOPlayerHandle(player), values, size));
	}

	public static GGPOErrorCode ggpo_synchronize_input(GGPOSession ggpo, object values, int size, ref int disconnect_flags)
	{
	   if (ggpo == null)
	   {
		  return GGPO_ERRORCODE_INVALID_SESSION;
	   }
	   return new GGPOErrorCode(ggpo.SyncInput(values, size, ref disconnect_flags));
	}

	public static GGPOErrorCode ggpo_disconnect_player(GGPOSession ggpo, GGPOPlayerHandle player)
	{
	   if (ggpo == null)
	   {
		  return GGPO_ERRORCODE_INVALID_SESSION;
	   }
	   return new GGPOErrorCode(ggpo.DisconnectPlayer(new GGPOPlayerHandle(player)));
	}

	public static GGPOErrorCode ggpo_advance_frame(GGPOSession ggpo)
	{
	   if (ggpo == null)
	   {
		  return GGPO_ERRORCODE_INVALID_SESSION;
	   }
	   return new GGPOErrorCode(ggpo.IncrementFrame());
	}

	public static GGPOErrorCode ggpo_client_chat(GGPOSession ggpo, ref string text)
	{
	   if (ggpo == null)
	   {
		  return GGPO_ERRORCODE_INVALID_SESSION;
	   }
	   return new GGPOErrorCode(ggpo.Chat(ref text));
	}

	public static GGPOErrorCode ggpo_get_network_stats(GGPOSession ggpo, GGPOPlayerHandle player, GGPONetworkStats stats)
	{
	   if (ggpo == null)
	   {
		  return GGPO_ERRORCODE_INVALID_SESSION;
	   }
	   return new GGPOErrorCode(ggpo.GetNetworkStats(stats, new GGPOPlayerHandle(player)));
	}


	public static GGPOErrorCode ggpo_close_session(GGPOSession ggpo)
	{
	   if (ggpo == null)
	   {
		  return GGPO_ERRORCODE_INVALID_SESSION;
	   }
	   if (ggpo != null)
	   {
	   ggpo.Dispose();
	   }
	   return GGPO_OK;
	}

	public static GGPOErrorCode ggpo_set_disconnect_timeout(GGPOSession ggpo, int timeout)
	{
	   if (ggpo == null)
	   {
		  return GGPO_ERRORCODE_INVALID_SESSION;
	   }
	   return new GGPOErrorCode(ggpo.SetDisconnectTimeout(timeout));
	}

	public static GGPOErrorCode ggpo_set_disconnect_notify_start(GGPOSession ggpo, int timeout)
	{
	   if (ggpo == null)
	   {
		  return GGPO_ERRORCODE_INVALID_SESSION;
	   }
	   return new GGPOErrorCode(ggpo.SetDisconnectNotifyStart(timeout));
	}

	public static GGPOErrorCode ggpo_start_spectating(GGPOSession[] session, GGPOSessionCallbacks cb, string game, int num_players, int input_size, ushort local_port, ref string host_ip, ushort host_port)
	{
//C++ TO C# CONVERTER TODO TASK: The following line was determined to be a copy assignment (rather than a reference assignment) - this should be verified and a 'CopyFrom' method should be created:
//ORIGINAL LINE: *session= (GGPOSession *)new SpectatorBackend(cb, game, local_port, num_players, input_size, host_ip, host_port);
	   session[0].CopyFrom((GGPOSession)new SpectatorBackend(cb, game, local_port, num_players, input_size, ref host_ip, host_port));
	   return GGPO_OK;
	}



	/*
	 * ggpo_start_session --
	 *
	 * Used to being a new GGPO.net session.  The ggpo object returned by ggpo_start_session
	 * uniquely identifies the state for this session and should be passed to all other
	 * functions.
	 *
	 * session - An out parameter to the new ggpo session object.
	 *
	 * cb - A GGPOSessionCallbacks structure which contains the callbacks you implement
	 * to help GGPO.net synchronize the two games.  You must implement all functions in
	 * cb, even if they do nothing but 'return true';
	 *
	 * game - The name of the game.  This is used internally for GGPO for logging purposes only.
	 *
	 * num_players - The number of players which will be in this game.  The number of players
	 * per session is fixed.  If you need to change the number of players or any player
	 * disconnects, you must start a new session.
	 *
	 * input_size - The size of the game inputs which will be passsed to ggpo_add_local_input.
	 *
	 * local_port - The port GGPO should bind to for UDP traffic.
	 */
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: GGPO_API GGPOErrorCode __cdecl ggpo_start_session(struct GGPOSession **session, GGPOSessionCallbacks *cb, const char *game, int num_players, int input_size, ushort localport);
//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
	//GGPO_API GGPOErrorCode ggpo_start_session(GGPOSession[] session, GGPOSessionCallbacks cb, string game, int num_players, int input_size, ushort localport);


	/*
	 * ggpo_add_player --
	 *
	 * Must be called for each player in the session (e.g. in a 3 player session, must
	 * be called 3 times).
	 *
	 * player - A GGPOPlayer struct used to describe the player.
	 *
	 * handle - An out parameter to a handle used to identify this player in the future.
	 * (e.g. in the on_event callbacks).
	 */
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: GGPO_API GGPOErrorCode __cdecl ggpo_add_player(struct GGPOSession *session, GGPOPlayer *player, int *handle);
//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
	//GGPO_API GGPOErrorCode ggpo_add_player(GGPOSession session, GGPOPlayer player, ref int handle);


	/*
	 * ggpo_start_synctest --
	 *
	 * Used to being a new GGPO.net sync test session.  During a sync test, every
	 * frame of execution is run twice: once in prediction mode and once again to
	 * verify the result of the prediction.  If the checksums of your save states
	 * do not match, the test is aborted.
	 *
	 * cb - A GGPOSessionCallbacks structure which contains the callbacks you implement
	 * to help GGPO.net synchronize the two games.  You must implement all functions in
	 * cb, even if they do nothing but 'return true';
	 *
	 * game - The name of the game.  This is used internally for GGPO for logging purposes only.
	 *
	 * num_players - The number of players which will be in this game.  The number of players
	 * per session is fixed.  If you need to change the number of players or any player
	 * disconnects, you must start a new session.
	 *
	 * input_size - The size of the game inputs which will be passsed to ggpo_add_local_input.
	 *
	 * frames - The number of frames to run before verifying the prediction.  The
	 * recommended value is 1.
	 *
	 */
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: GGPO_API GGPOErrorCode __cdecl ggpo_start_synctest(struct GGPOSession **session, GGPOSessionCallbacks *cb, char *game, int num_players, int input_size, int frames);
//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
	//GGPO_API GGPOErrorCode ggpo_start_synctest(GGPOSession[] session, GGPOSessionCallbacks cb, ref string game, int num_players, int input_size, int frames);


	/*
	 * ggpo_start_spectating --
	 *
	 * Start a spectator session.
	 *
	 * cb - A GGPOSessionCallbacks structure which contains the callbacks you implement
	 * to help GGPO.net synchronize the two games.  You must implement all functions in
	 * cb, even if they do nothing but 'return true';
	 *
	 * game - The name of the game.  This is used internally for GGPO for logging purposes only.
	 *
	 * num_players - The number of players which will be in this game.  The number of players
	 * per session is fixed.  If you need to change the number of players or any player
	 * disconnects, you must start a new session.
	 *
	 * input_size - The size of the game inputs which will be passsed to ggpo_add_local_input.
	 *
	 * local_port - The port GGPO should bind to for UDP traffic.
	 *
	 * host_ip - The IP address of the host who will serve you the inputs for the game.  Any
	 * player partcipating in the session can serve as a host.
	 *
	 * host_port - The port of the session on the host
	 */
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: GGPO_API GGPOErrorCode __cdecl ggpo_start_spectating(struct GGPOSession **session, GGPOSessionCallbacks *cb, const char *game, int num_players, int input_size, ushort local_port, char *host_ip, ushort host_port);
//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
	//GGPO_API GGPOErrorCode ggpo_start_spectating(GGPOSession[] session, GGPOSessionCallbacks cb, string game, int num_players, int input_size, ushort local_port, ref string host_ip, ushort host_port);

	/*
	 * ggpo_close_session --
	 * Used to close a session.  You must call ggpo_close_session to
	 * free the resources allocated in ggpo_start_session.
	 */
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: GGPO_API GGPOErrorCode __cdecl ggpo_close_session(struct GGPOSession *);
//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
	//GGPO_API GGPOErrorCode ggpo_close_session(GGPOSession UnnamedParameter);


	/*
	 * ggpo_set_frame_delay --
	 *
	 * Change the amount of frames ggpo will delay local input.  Must be called
	 * before the first call to ggpo_synchronize_input.
	 */
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: GGPO_API GGPOErrorCode __cdecl ggpo_set_frame_delay(struct GGPOSession *, int player, int frame_delay);
//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
	//GGPO_API GGPOErrorCode ggpo_set_frame_delay(GGPOSession UnnamedParameter, int player, int frame_delay);

	/*
	 * ggpo_idle --
	 * Should be called periodically by your application to give GGPO.net
	 * a chance to do some work.  Most packet transmissions and rollbacks occur
	 * in ggpo_idle.
	 *
	 * timeout - The amount of time GGPO.net is allowed to spend in this function,
	 * in milliseconds.
	 */
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: GGPO_API GGPOErrorCode __cdecl ggpo_idle(struct GGPOSession *, int timeout);
//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
	//GGPO_API GGPOErrorCode ggpo_idle(GGPOSession UnnamedParameter, int timeout);

	/*
	 * ggpo_add_local_input --
	 *
	 * Used to notify GGPO.net of inputs that should be trasmitted to remote
	 * players.  ggpo_add_local_input must be called once every frame for
	 * all player of type GGPO_PLAYERTYPE_LOCAL.
	 *
	 * player - The player handle returned for this player when you called
	 * ggpo_add_local_player.
	 *
	 * values - The controller inputs for this player.
	 *
	 * size - The size of the controller inputs.  This must be exactly equal to the
	 * size passed into ggpo_start_session.
	 */
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: GGPO_API GGPOErrorCode __cdecl ggpo_add_local_input(struct GGPOSession *, int player, object* values, int size);
//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
	//GGPO_API GGPOErrorCode ggpo_add_local_input(GGPOSession UnnamedParameter, int player, object values, int size);

	/*
	 * ggpo_synchronize_input --
	 *
	 * You should call ggpo_synchronize_input before every frame of execution,
	 * including those frames which happen during rollback.
	 *
	 * values - When the function returns, the values parameter will contain
	 * inputs for this frame for all players.  The values array must be at
	 * least (size * players) large.
	 *
	 * size - The size of the values array.
	 *
	 * disconnect_flags - Indicated whether the input in slot (1 << flag) is
	 * valid.  If a player has disconnected, the input in the values array for
	 * that player will be zeroed and the i-th flag will be set.  For example,
	 * if only player 3 has disconnected, disconnect flags will be 8 (i.e. 1 << 3).
	 */
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: GGPO_API GGPOErrorCode __cdecl ggpo_synchronize_input(struct GGPOSession *, object* values, int size, int *disconnect_flags);
//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
	//GGPO_API GGPOErrorCode ggpo_synchronize_input(GGPOSession UnnamedParameter, object values, int size, ref int disconnect_flags);

	/*
	 * ggpo_disconnect_player --
	 *
	 * Disconnects a remote player from a game.  Will return GGPO_ERRORCODE_PLAYER_DISCONNECTED
	 * if you try to disconnect a player who has already been disconnected.
	 */
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: GGPO_API GGPOErrorCode __cdecl ggpo_disconnect_player(struct GGPOSession *, int player);
//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
	//GGPO_API GGPOErrorCode ggpo_disconnect_player(GGPOSession UnnamedParameter, int player);

	/*
	 * ggpo_advance_frame --
	 *
	 * You should call ggpo_advance_frame to notify GGPO.net that you have
	 * advanced your gamestate by a single frame.  You should call this everytime
	 * you advance the gamestate by a frame, even during rollbacks.  GGPO.net
	 * may call your save_state callback before this function returns.
	 */
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: GGPO_API GGPOErrorCode __cdecl ggpo_advance_frame(struct GGPOSession *);
//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
	//GGPO_API GGPOErrorCode ggpo_advance_frame(GGPOSession UnnamedParameter);

	/*
	 * ggpo_get_network_stats --
	 *
	 * Used to fetch some statistics about the quality of the network connection.
	 *
	 * player - The player handle returned from the ggpo_add_player function you used
	 * to add the remote player.
	 *
	 * stats - Out parameter to the network statistics.
	 */
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: GGPO_API GGPOErrorCode __cdecl ggpo_get_network_stats(struct GGPOSession *, int player, GGPONetworkStats *stats);
//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
	//GGPO_API GGPOErrorCode ggpo_get_network_stats(GGPOSession UnnamedParameter, int player, GGPONetworkStats stats);

	/*
	 * ggpo_set_disconnect_timeout --
	 *
	 * Sets the disconnect timeout.  The session will automatically disconnect
	 * from a remote peer if it has not received a packet in the timeout window.
	 * You will be notified of the disconnect via a GGPO_EVENTCODE_DISCONNECTED_FROM_PEER
	 * event.
	 *
	 * Setting a timeout value of 0 will disable automatic disconnects.
	 *
	 * timeout - The time in milliseconds to wait before disconnecting a peer.
	 */
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: GGPO_API GGPOErrorCode __cdecl ggpo_set_disconnect_timeout(struct GGPOSession *, int timeout);
//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
	//GGPO_API GGPOErrorCode ggpo_set_disconnect_timeout(GGPOSession UnnamedParameter, int timeout);

	/*
	 * ggpo_set_disconnect_notify_start --
	 *
	 * The time to wait before the first GGPO_EVENTCODE_NETWORK_INTERRUPTED timeout
	 * will be sent.
	 *
	 * timeout - The amount of time which needs to elapse without receiving a packet
	 *           before the GGPO_EVENTCODE_NETWORK_INTERRUPTED event is sent.
	 */
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: GGPO_API GGPOErrorCode __cdecl ggpo_set_disconnect_notify_start(struct GGPOSession *, int timeout);
//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
	//GGPO_API GGPOErrorCode ggpo_set_disconnect_notify_start(GGPOSession UnnamedParameter, int timeout);

	/*
	 * ggpo_log --
	 *
	 * Used to write to the ggpo.net log.  In the current versions of the
	 * SDK, a log file is only generated if the "quark.log" environment
	 * variable is set to 1.  This will change in future versions of the
	 * SDK.
	 */
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: GGPO_API void __cdecl ggpo_log(struct GGPOSession *, const char *fmt, ...);
//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
	//GGPO_API void ggpo_log(GGPOSession UnnamedParameter, string fmt, params object[] LegacyParamArray);
	/*
	 * ggpo_logv --
	 *
	 * A varargs compatible version of ggpo_log.  See ggpo_log for
	 * more details.
	 */
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER TODO TASK: The #define macro 'GGPO_API' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
//C++ TO C# CONVERTER NOTE: __cdecl is not available in C#:
//ORIGINAL LINE: GGPO_API void __cdecl ggpo_logv(struct GGPOSession *, const char *fmt, va_list args);
//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
	//GGPO_API void ggpo_logv(GGPOSession UnnamedParameter, string fmt, va_list args);

	#if __cplusplus
	#endif


	/* -----------------------------------------------------------------------
	 * GGPO.net (http://ggpo.net)  -  Copyright 2009 GroundStorm Studios, LLC.
	 *
	 * Use of this software is governed by the MIT license that can be found
	 * in the LICENSE file.
	 */

	/*
	 * Keep the compiler happy
	 */

	/*
	 * Disable specific compiler warnings
	 *   4018 - '<' : signed/unsigned mismatch
	 *   4100 - 'xxx' : unreferenced formal parameter
	 *   4127 - conditional expression is constant
	 *   4201 - nonstandard extension used : nameless struct/union
	 *   4389 - '!=' : signed/unsigned mismatch
	 *   4800 - 'int' : forcing value to bool 'true' or 'false' (performance warning)
	 */
	//C++ TO C# CONVERTER TODO TASK: There is no equivalent to most C++ 'pragma' directives in C#:
	//#pragma warning(disable: 4018 4100 4127 4201 4389 4800)

	/*
	 * Simple types
	 */

	/*
	 * Additional headers
	 */
	#if _WINDOWS
	/* -----------------------------------------------------------------------
	 * GGPO.net (http://ggpo.net)  -  Copyright 2009 GroundStorm Studios, LLC.
	 *
	 * Use of this software is governed by the MIT license that can be found
	 * in the LICENSE file.
	 */

	/*
	 * Keep the compiler happy
	 */

	/*
	 * Disable specific compiler warnings
	 *   4018 - '<' : signed/unsigned mismatch
	 *   4100 - 'xxx' : unreferenced formal parameter
	 *   4127 - conditional expression is constant
	 *   4201 - nonstandard extension used : nameless struct/union
	 *   4389 - '!=' : signed/unsigned mismatch
	 *   4800 - 'int' : forcing value to bool 'true' or 'false' (performance warning)
	 */
	//C++ TO C# CONVERTER TODO TASK: There is no equivalent to most C++ 'pragma' directives in C#:
	//#pragma warning(disable: 4018 4100 4127 4201 4389 4800)

	/*
	 * Simple types
	 */
	//C++ TO C# CONVERTER TODO TASK: Typedefs defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//typedef byte byte;

	/*
	 * Additional headers
	 */
	#if _WINDOWS
	#elif __GNUC__
	//
	// sys/types.h
	//
	//      Copyright (c) Microsoft Corporation. All rights reserved.
	//
	// Types used for returning file status and time information.
	//

		#define _VCRT_DEFINED_CRTIMP
	//C++ TO C# CONVERTER TODO TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRTIMP __declspec(dllexport)
			#define _CRTIMP
	//C++ TO C# CONVERTER TODO TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRTIMP __declspec(dllimport)
				#define _CRTIMP
				#define _CRTIMP
	//C++ TO C# CONVERTER TODO TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_BEGIN_C_HEADER __pragma(pack(push, _CRT_PACKING)) extern "C" {
		#define _CRT_BEGIN_C_HEADER
	//C++ TO C# CONVERTER TODO TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_END_C_HEADER } __pragma(pack(pop))
		#define _CRT_END_C_HEADER
	//C++ TO C# CONVERTER TODO TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_BEGIN_C_HEADER cpp_quote("__pragma(pack(push, _CRT_PACKING))") cpp_quote("extern \"C\" {")
		#define _CRT_BEGIN_C_HEADER
	//C++ TO C# CONVERTER TODO TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_END_C_HEADER cpp_quote("}") cpp_quote("__pragma(pack(pop))")
		#define _CRT_END_C_HEADER
	//C++ TO C# CONVERTER TODO TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_BEGIN_C_HEADER __pragma(pack(push, _CRT_PACKING))
		#define _CRT_BEGIN_C_HEADER
	//C++ TO C# CONVERTER TODO TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define _CRT_END_C_HEADER __pragma(pack(pop))
		#define _CRT_END_C_HEADER
	//C++ TO C# CONVERTER WARNING: Statement interrupted by a preprocessor statement:
	//The original statement from the file starts with:
	//    __pragma(pack(push, _CRT_PACKING))
	//Preprocessor-interrupted statements cannot be handled by this converter.
	//The remainder of the header file is ignored.

	//C++ TO C# CONVERTER TODO TASK: There is no equivalent to most C++ 'pragma' directives in C#:
	//#pragma warning(push)
	//C++ TO C# CONVERTER TODO TASK: There is no equivalent to most C++ 'pragma' directives in C#:
	//#pragma warning(disable: _UCRT_DISABLED_WARNINGS)
	public static _UCRT_DISABLE_CLANG_WARNINGS typedef ushort _ino_t = new _UCRT_DISABLE_CLANG_WARNINGS(); // inode number (unused on Windows)

		#if ( _CRT_DECLARE_NONSTDC_NAMES && _CRT_DECLARE_NONSTDC_NAMES) || (! _CRT_DECLARE_NONSTDC_NAMES && !__STDC__)
	//C++ TO C# CONVERTER TODO TASK: Typedefs defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//		typedef _ino_t ino_t;
		#endif




	//C++ TO C# CONVERTER TODO TASK: Typedefs defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//	typedef uint _dev_t;

		#if ( _CRT_DECLARE_NONSTDC_NAMES && _CRT_DECLARE_NONSTDC_NAMES) || (! _CRT_DECLARE_NONSTDC_NAMES && !__STDC__)
	//C++ TO C# CONVERTER TODO TASK: Typedefs defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//		typedef _dev_t dev_t;
		#endif




	//C++ TO C# CONVERTER TODO TASK: Typedefs defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//	typedef int _off_t;

		#if ( _CRT_DECLARE_NONSTDC_NAMES && _CRT_DECLARE_NONSTDC_NAMES) || (! _CRT_DECLARE_NONSTDC_NAMES && !__STDC__)
	//C++ TO C# CONVERTER TODO TASK: Typedefs defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//		typedef _off_t off_t;
		#endif

	//C++ TO C# CONVERTER NOTE: This was formerly a static local variable declaration (not allowed in C#):
//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
	//static uint warning_GetCurrentTimeMS();

	public static _UCRT_RESTORE_CLANG_WARNINGS #pragma warning(pop UnnamedParameter) Platform
	{
	//C++ TO C# CONVERTER TODO TASK: Typedefs defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//   typedef pid_t ProcessID;

	   static pid_t GetProcessID()
	   {
		   return getpid();
	   }
	   static void AssertFailed(char * msg)
	   {
	   }
	//C++ TO C# CONVERTER TODO TASK: The typedef 'uint32' was defined in multiple preprocessor conditionals and cannot be replaced in-line:
	//C++ TO C# CONVERTER NOTE: This static local variable declaration (not allowed in C#) has been moved just prior to the method:
	//   static uint GetCurrentTimeMS();
	}


	//Tangible Process Only End
	#else
	#error Unsupported platform
	#endif




	/*
	 * Macros
	 */
	//C++ TO C# CONVERTER TODO TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define ASSERT(x) do { if (!(x)) { char assert_buf[1024]; snprintf(assert_buf, sizeof(assert_buf) - 1, "Assertion: %s @ %s:%d (pid:%d)", #x, __FILE__, __LINE__, Platform::GetProcessID()); Log("%s\n", assert_buf); Log("\n"); Log("\n"); Log("\n"); Platform::AssertFailed(assert_buf); exit(0); } } while (false)
	#define ASSERT

	#if ! ARRAY_SIZE
	//C++ TO C# CONVERTER TODO TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define ARRAY_SIZE(a) (sizeof(a) / sizeof((a)[0]))
	#define ARRAY_SIZE
	#endif

	#define MAX_INT

	#if ! MAX
	//C++ TO C# CONVERTER TODO TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define MAX(x, y) (((x) > (y)) ? (x) : (y))
	#define MAX
	#endif

	#if ! MIN
	//C++ TO C# CONVERTER TODO TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define MIN(x, y) (((x) < (y)) ? (x) : (y))
	#define MIN
	#endif


	#elif __GNUC__
	#else
	#error Unsupported platform
	#endif




	/*
	 * Macros
	 */
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define ASSERT(x) do { if (!(x)) { char assert_buf[1024]; snprintf(assert_buf, sizeof(assert_buf) - 1, "Assertion: %s @ %s:%d (pid:%d)", #x, __FILE__, __LINE__, Platform::GetProcessID()); Log("%s\n", assert_buf); Log("\n"); Log("\n"); Log("\n"); Platform::AssertFailed(assert_buf); exit(0); } } while (false)

	#if ! ARRAY_SIZE
	//C++ TO C# CONVERTER TODO TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define ARRAY_SIZE(a) (sizeof(a) / sizeof((a)[0]))
	#define ARRAY_SIZE
	#endif


	#if ! MAX
	//C++ TO C# CONVERTER TODO TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define MAX(x, y) (((x) > (y)) ? (x) : (y))
	#define MAX
	#endif

	#if ! MIN
	//C++ TO C# CONVERTER TODO TASK: #define macros defined in multiple preprocessor conditionals can only be replaced within the scope of the preprocessor conditional:
	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define MIN(x, y) (((x) < (y)) ? (x) : (y))
	#define MIN
	#endif




	/*
	   The application must update next_in and avail_in when avail_in has
	   dropped to zero. It must update next_out and avail_out when avail_out
	   has dropped to zero. The application must initialize zalloc, zfree and
	   opaque before calling the init function. All other fields are set by the
	   compression library and must not be updated by the application.
	
	   The opaque value provided by the application will be passed as the first
	   parameter for calls of zalloc and zfree. This can be useful for custom
	   memory management. The compression library attaches no meaning to the
	   opaque value.
	
	   zalloc must return Z_NULL if there is not enough memory for the object.
	   If zlib is used in a multi-threaded application, zalloc and zfree must be
	   thread safe.
	
	   On 16-bit systems, the functions zalloc and zfree must be able to allocate
	   exactly 65536 bytes, but will not be required to allocate more than this
	   if the symbol MAXSEG_64K is defined (see zconf.h). WARNING: On MSDOS,
	   pointers returned by zalloc for objects of exactly 65536 bytes *must*
	   have their offset normalized to zero. The default allocation function
	   provided by this library ensures this (see zutil.c). To reduce memory
	   requirements and avoid any allocation of 64K objects, at the expense of
	   compression ratio, compile the library with -DMAX_WBITS=14 (see zconf.h).
	
	   The fields total_in and total_out can be used for statistics or
	   progress reports. After compression, total_in holds the total size of
	   the uncompressed data and may be saved for use in the decompressor
	   (particularly if the decompressor wants to decompress everything in
	   a single step).
	*/

							/* constants */

	/* Allowed flush values; see deflate() below for details */

	/* Return codes for the compression/decompression functions. Negative
	 * values are errors, positive values are used for special but normal events.
	 */

	/* compression levels */

	/* compression strategy; see deflateInit2() below for details */

	/* Possible values of the data_type field */

	/* The deflate compression method (the only one supported in this version) */


	//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
	//ORIGINAL LINE: #define zlib_version zlibVersion()
	/* for compatibility with versions < 1.0.2 */

							/* basic functions */

	extern const char * _export zlibVersion(args)()();
	/* The application can compare zlibVersion and ZLIB_VERSION for consistency.
	   If the first character differs, the library code actually used is
	   not compatible with the zlib.h header file used by the application.
	   This check is automatically made by deflateInit and inflateInit.
	 */

	/* 
	ZEXTERN int ZEXPORT deflateInit OF((z_streamp strm, int level));
	
	     Initializes the internal stream state for compression. The fields
	   zalloc, zfree and opaque must be initialized before by the caller.
	   If zalloc and zfree are set to Z_NULL, deflateInit updates them to
	   use default allocation functions.
	
	     The compression level must be Z_DEFAULT_COMPRESSION, or between 0 and 9:
	   1 gives best speed, 9 gives best compression, 0 gives no compression at
	   all (the input data is simply copied a block at a time).
	   Z_DEFAULT_COMPRESSION requests a default compromise between speed and
	   compression (currently equivalent to level 6).
	
	     deflateInit returns Z_OK if success, Z_MEM_ERROR if there was not
	   enough memory, Z_STREAM_ERROR if level is not a valid compression level,
	   Z_VERSION_ERROR if the zlib library version (zlib_version) is incompatible
	   with the version assumed by the caller (ZLIB_VERSION).
	   msg is set to null if there is no error message.  deflateInit does not
	   perform any compression: this will be done by deflate().
	*/


	extern int _export z_deflate(args)()((z_stream_s _far * strm, int flush));
	/*
	    deflate compresses as much data as possible, and stops when the input
	  buffer becomes empty or the output buffer becomes full. It may introduce some
	  output latency (reading input without producing any output) except when
	  forced to flush.
	
	    The detailed semantics are as follows. deflate performs one or both of the
	  following actions:
	
	  - Compress more input starting at next_in and update next_in and avail_in
	    accordingly. If not all input can be processed (because there is not
	    enough room in the output buffer), next_in and avail_in are updated and
	    processing will resume at this point for the next call of deflate().
	
	  - Provide more output starting at next_out and update next_out and avail_out
	    accordingly. This action is forced if the parameter flush is non zero.
	    Forcing flush frequently degrades the compression ratio, so this parameter
	    should be set only when necessary (in interactive applications).
	    Some output may be provided even if flush is not set.
	
	  Before the call of deflate(), the application should ensure that at least
	  one of the actions is possible, by providing more input and/or consuming
	  more output, and updating avail_in or avail_out accordingly; avail_out
	  should never be zero before the call. The application can consume the
	  compressed output when it wants, for example when the output buffer is full
	  (avail_out == 0), or after each call of deflate(). If deflate returns Z_OK
	  and with zero avail_out, it must be called again after making room in the
	  output buffer because there might be more output pending.
	
	    If the parameter flush is set to Z_SYNC_FLUSH, all pending output is
	  flushed to the output buffer and the output is aligned on a byte boundary, so
	  that the decompressor can get all input data available so far. (In particular
	  avail_in is zero after the call if enough output space has been provided
	  before the call.)  Flushing may degrade compression for some compression
	  algorithms and so it should be used only when necessary.
	
	    If flush is set to Z_FULL_FLUSH, all output is flushed as with
	  Z_SYNC_FLUSH, and the compression state is reset so that decompression can
	  restart from this point if previous compressed data has been damaged or if
	  random access is desired. Using Z_FULL_FLUSH too often can seriously degrade
	  the compression.
	
	    If deflate returns with avail_out == 0, this function must be called again
	  with the same value of the flush parameter and more output space (updated
	  avail_out), until the flush is complete (deflate returns with non-zero
	  avail_out).
	
	    If the parameter flush is set to Z_FINISH, pending input is processed,
	  pending output is flushed and deflate returns with Z_STREAM_END if there
	  was enough output space; if deflate returns with Z_OK, this function must be
	  called again with Z_FINISH and more output space (updated avail_out) but no
	  more input data, until it returns with Z_STREAM_END or an error. After
	  deflate has returned Z_STREAM_END, the only possible operations on the
	  stream are deflateReset or deflateEnd.
	  
	    Z_FINISH can be used immediately after deflateInit if all the compression
	  is to be done in a single step. In this case, avail_out must be at least
	  0.1% larger than avail_in plus 12 bytes.  If deflate does not return
	  Z_STREAM_END, then it must be called again as described above.
	
	    deflate() sets strm->adler to the adler32 checksum of all input read
	  so far (that is, total_in bytes).
	
	    deflate() may update data_type if it can make a good guess about
	  the input data type (Z_ASCII or Z_BINARY). In doubt, the data is considered
	  binary. This field is only for information purposes and does not affect
	  the compression algorithm in any manner.
	
	    deflate() returns Z_OK if some progress has been made (more input
	  processed or more output produced), Z_STREAM_END if all input has been
	  consumed and all output has been produced (only when flush is set to
	  Z_FINISH), Z_STREAM_ERROR if the stream state was inconsistent (for example
	  if next_in or next_out was NULL), Z_BUF_ERROR if no progress is possible
	  (for example avail_in or avail_out was zero).
	*/


	extern int _export z_deflateEnd(args)()((z_stream_s _far * strm));
	/*
	     All dynamically allocated data structures for this stream are freed.
	   This function discards any unprocessed input and does not flush any
	   pending output.
	
	     deflateEnd returns Z_OK if success, Z_STREAM_ERROR if the
	   stream state was inconsistent, Z_DATA_ERROR if the stream was freed
	   prematurely (some input or output was discarded). In the error case,
	   msg may be set but then points to a static string (which must not be
	   deallocated).
	*/


	/* 
	ZEXTERN int ZEXPORT inflateInit OF((z_streamp strm));
	
	     Initializes the internal stream state for decompression. The fields
	   next_in, avail_in, zalloc, zfree and opaque must be initialized before by
	   the caller. If next_in is not Z_NULL and avail_in is large enough (the exact
	   value depends on the compression method), inflateInit determines the
	   compression method from the zlib header and allocates all data structures
	   accordingly; otherwise the allocation will be deferred to the first call of
	   inflate.  If zalloc and zfree are set to Z_NULL, inflateInit updates them to
	   use default allocation functions.
	
	     inflateInit returns Z_OK if success, Z_MEM_ERROR if there was not enough
	   memory, Z_VERSION_ERROR if the zlib library version is incompatible with the
	   version assumed by the caller.  msg is set to null if there is no error
	   message. inflateInit does not perform any decompression apart from reading
	   the zlib header if present: this will be done by inflate().  (So next_in and
	   avail_in may be modified, but next_out and avail_out are unchanged.)
	*/


	extern int _export z_inflate(args)()((z_stream_s _far * strm, int flush));
	/*
	    inflate decompresses as much data as possible, and stops when the input
	  buffer becomes empty or the output buffer becomes full. It may some
	  introduce some output latency (reading input without producing any output)
	  except when forced to flush.
	
	  The detailed semantics are as follows. inflate performs one or both of the
	  following actions:
	
	  - Decompress more input starting at next_in and update next_in and avail_in
	    accordingly. If not all input can be processed (because there is not
	    enough room in the output buffer), next_in is updated and processing
	    will resume at this point for the next call of inflate().
	
	  - Provide more output starting at next_out and update next_out and avail_out
	    accordingly.  inflate() provides as much output as possible, until there
	    is no more input data or no more space in the output buffer (see below
	    about the flush parameter).
	
	  Before the call of inflate(), the application should ensure that at least
	  one of the actions is possible, by providing more input and/or consuming
	  more output, and updating the next_* and avail_* values accordingly.
	  The application can consume the uncompressed output when it wants, for
	  example when the output buffer is full (avail_out == 0), or after each
	  call of inflate(). If inflate returns Z_OK and with zero avail_out, it
	  must be called again after making room in the output buffer because there
	  might be more output pending.
	
	    If the parameter flush is set to Z_SYNC_FLUSH, inflate flushes as much
	  output as possible to the output buffer. The flushing behavior of inflate is
	  not specified for values of the flush parameter other than Z_SYNC_FLUSH
	  and Z_FINISH, but the current implementation actually flushes as much output
	  as possible anyway.
	
	    inflate() should normally be called until it returns Z_STREAM_END or an
	  error. However if all decompression is to be performed in a single step
	  (a single call of inflate), the parameter flush should be set to
	  Z_FINISH. In this case all pending input is processed and all pending
	  output is flushed; avail_out must be large enough to hold all the
	  uncompressed data. (The size of the uncompressed data may have been saved
	  by the compressor for this purpose.) The next operation on this stream must
	  be inflateEnd to deallocate the decompression state. The use of Z_FINISH
	  is never required, but can be used to inform inflate that a faster routine
	  may be used for the single inflate() call.
	
	     If a preset dictionary is needed at this point (see inflateSetDictionary
	  below), inflate sets strm-adler to the adler32 checksum of the
	  dictionary chosen by the compressor and returns Z_NEED_DICT; otherwise 
	  it sets strm->adler to the adler32 checksum of all output produced
	  so far (that is, total_out bytes) and returns Z_OK, Z_STREAM_END or
	  an error code as described below. At the end of the stream, inflate()
	  checks that its computed adler32 checksum is equal to that saved by the
	  compressor and returns Z_STREAM_END only if the checksum is correct.
	
	    inflate() returns Z_OK if some progress has been made (more input processed
	  or more output produced), Z_STREAM_END if the end of the compressed data has
	  been reached and all uncompressed output has been produced, Z_NEED_DICT if a
	  preset dictionary is needed at this point, Z_DATA_ERROR if the input data was
	  corrupted (input stream not conforming to the zlib format or incorrect
	  adler32 checksum), Z_STREAM_ERROR if the stream structure was inconsistent
	  (for example if next_in or next_out was NULL), Z_MEM_ERROR if there was not
	  enough memory, Z_BUF_ERROR if no progress is possible or if there was not
	  enough room in the output buffer when Z_FINISH is used. In the Z_DATA_ERROR
	  case, the application may then call inflateSync to look for a good
	  compression block.
	*/


	extern int _export z_inflateEnd(args)()((z_stream_s _far * strm));
	/*
	     All dynamically allocated data structures for this stream are freed.
	   This function discards any unprocessed input and does not flush any
	   pending output.
	
	     inflateEnd returns Z_OK if success, Z_STREAM_ERROR if the stream state
	   was inconsistent. In the error case, msg may be set but then points to a
	   static string (which must not be deallocated).
	*/

							/* Advanced functions */

	/*
	    The following functions are needed only in some special applications.
	*/

	/*   
	ZEXTERN int ZEXPORT deflateInit2 OF((z_streamp strm,
	                                     int  level,
	                                     int  method,
	                                     int  windowBits,
	                                     int  memLevel,
	                                     int  strategy));
	
	     This is another version of deflateInit with more compression options. The
	   fields next_in, zalloc, zfree and opaque must be initialized before by
	   the caller.
	
	     The method parameter is the compression method. It must be Z_DEFLATED in
	   this version of the library.
	
	     The windowBits parameter is the base two logarithm of the window size
	   (the size of the history buffer).  It should be in the range 8..15 for this
	   version of the library. Larger values of this parameter result in better
	   compression at the expense of memory usage. The default value is 15 if
	   deflateInit is used instead.
	
	     The memLevel parameter specifies how much memory should be allocated
	   for the internal compression state. memLevel=1 uses minimum memory but
	   is slow and reduces compression ratio; memLevel=9 uses maximum memory
	   for optimal speed. The default value is 8. See zconf.h for total memory
	   usage as a function of windowBits and memLevel.
	
	     The strategy parameter is used to tune the compression algorithm. Use the
	   value Z_DEFAULT_STRATEGY for normal data, Z_FILTERED for data produced by a
	   filter (or predictor), or Z_HUFFMAN_ONLY to force Huffman encoding only (no
	   string match).  Filtered data consists mostly of small values with a
	   somewhat random distribution. In this case, the compression algorithm is
	   tuned to compress them better. The effect of Z_FILTERED is to force more
	   Huffman coding and less string matching; it is somewhat intermediate
	   between Z_DEFAULT and Z_HUFFMAN_ONLY. The strategy parameter only affects
	   the compression ratio but not the correctness of the compressed output even
	   if it is not set appropriately.
	
	      deflateInit2 returns Z_OK if success, Z_MEM_ERROR if there was not enough
	   memory, Z_STREAM_ERROR if a parameter is invalid (such as an invalid
	   method). msg is set to null if there is no error message.  deflateInit2 does
	   not perform any compression: this will be done by deflate().
	*/

	extern int _export z_deflateSetDictionary(args)()((z_stream_s _far * strm, const byte _far * dictionary, uint dictLength));
	/*
	     Initializes the compression dictionary from the given byte sequence
	   without producing any compressed output. This function must be called
	   immediately after deflateInit, deflateInit2 or deflateReset, before any
	   call of deflate. The compressor and decompressor must use exactly the same
	   dictionary (see inflateSetDictionary).
	
	     The dictionary should consist of strings (byte sequences) that are likely
	   to be encountered later in the data to be compressed, with the most commonly
	   used strings preferably put towards the end of the dictionary. Using a
	   dictionary is most useful when the data to be compressed is short and can be
	   predicted with good accuracy; the data can then be compressed better than
	   with the default empty dictionary.
	
	     Depending on the size of the compression data structures selected by
	   deflateInit or deflateInit2, a part of the dictionary may in effect be
	   discarded, for example if the dictionary is larger than the window size in
	   deflate or deflate2. Thus the strings most likely to be useful should be
	   put at the end of the dictionary, not at the front.
	
	     Upon return of this function, strm->adler is set to the Adler32 value
	   of the dictionary; the decompressor may later use this value to determine
	   which dictionary has been used by the compressor. (The Adler32 value
	   applies to the whole dictionary even if only a subset of the dictionary is
	   actually used by the compressor.)
	
	     deflateSetDictionary returns Z_OK if success, or Z_STREAM_ERROR if a
	   parameter is invalid (such as NULL dictionary) or the stream state is
	   inconsistent (for example if deflate has already been called for this stream
	   or if the compression method is bsort). deflateSetDictionary does not
	   perform any compression: this will be done by deflate().
	*/

	extern int _export z_deflateCopy(args)()((z_stream_s _far * dest, z_stream_s _far * source));
	/*
	     Sets the destination stream as a complete copy of the source stream.
	
	     This function can be useful when several compression strategies will be
	   tried, for example when there are several ways of pre-processing the input
	   data with a filter. The streams that will be discarded should then be freed
	   by calling deflateEnd.  Note that deflateCopy duplicates the internal
	   compression state which can be quite large, so this strategy is slow and
	   can consume lots of memory.
	
	     deflateCopy returns Z_OK if success, Z_MEM_ERROR if there was not
	   enough memory, Z_STREAM_ERROR if the source stream state was inconsistent
	   (such as zalloc being NULL). msg is left unchanged in both source and
	   destination.
	*/

	extern int _export z_deflateReset(args)()((z_stream_s _far * strm));
	/*
	     This function is equivalent to deflateEnd followed by deflateInit,
	   but does not free and reallocate all the internal compression state.
	   The stream will keep the same compression level and any other attributes
	   that may have been set by deflateInit2.
	
	      deflateReset returns Z_OK if success, or Z_STREAM_ERROR if the source
	   stream state was inconsistent (such as zalloc or state being NULL).
	*/

	extern int _export z_deflateParams(args)()((z_stream_s _far * strm, int level, int strategy));
	/*
	     Dynamically update the compression level and compression strategy.  The
	   interpretation of level and strategy is as in deflateInit2.  This can be
	   used to switch between compression and straight copy of the input data, or
	   to switch to a different kind of input data requiring a different
	   strategy. If the compression level is changed, the input available so far
	   is compressed with the old level (and may be flushed); the new level will
	   take effect only at the next call of deflate().
	
	     Before the call of deflateParams, the stream state must be set as for
	   a call of deflate(), since the currently available input may have to
	   be compressed and flushed. In particular, strm->avail_out must be non-zero.
	
	     deflateParams returns Z_OK if success, Z_STREAM_ERROR if the source
	   stream state was inconsistent or if a parameter was invalid, Z_BUF_ERROR
	   if strm->avail_out was zero.
	*/

	/*   
	ZEXTERN int ZEXPORT inflateInit2 OF((z_streamp strm,
	                                     int  windowBits));
	
	     This is another version of inflateInit with an extra parameter. The
	   fields next_in, avail_in, zalloc, zfree and opaque must be initialized
	   before by the caller.
	
	     The windowBits parameter is the base two logarithm of the maximum window
	   size (the size of the history buffer).  It should be in the range 8..15 for
	   this version of the library. The default value is 15 if inflateInit is used
	   instead. If a compressed stream with a larger window size is given as
	   input, inflate() will return with the error code Z_DATA_ERROR instead of
	   trying to allocate a larger window.
	
	      inflateInit2 returns Z_OK if success, Z_MEM_ERROR if there was not enough
	   memory, Z_STREAM_ERROR if a parameter is invalid (such as a negative
	   memLevel). msg is set to null if there is no error message.  inflateInit2
	   does not perform any decompression apart from reading the zlib header if
	   present: this will be done by inflate(). (So next_in and avail_in may be
	   modified, but next_out and avail_out are unchanged.)
	*/

	extern int _export z_inflateSetDictionary(args)()((z_stream_s _far * strm, const byte _far * dictionary, uint dictLength));
	/*
	     Initializes the decompression dictionary from the given uncompressed byte
	   sequence. This function must be called immediately after a call of inflate
	   if this call returned Z_NEED_DICT. The dictionary chosen by the compressor
	   can be determined from the Adler32 value returned by this call of
	   inflate. The compressor and decompressor must use exactly the same
	   dictionary (see deflateSetDictionary).
	
	     inflateSetDictionary returns Z_OK if success, Z_STREAM_ERROR if a
	   parameter is invalid (such as NULL dictionary) or the stream state is
	   inconsistent, Z_DATA_ERROR if the given dictionary doesn't match the
	   expected one (incorrect Adler32 value). inflateSetDictionary does not
	   perform any decompression: this will be done by subsequent calls of
	   inflate().
	*/

	extern int _export z_inflateSync(args)()((z_stream_s _far * strm));
	/* 
	    Skips invalid compressed data until a full flush point (see above the
	  description of deflate with Z_FULL_FLUSH) can be found, or until all
	  available input is skipped. No output is provided.
	
	    inflateSync returns Z_OK if a full flush point has been found, Z_BUF_ERROR
	  if no more input was provided, Z_DATA_ERROR if no flush point has been found,
	  or Z_STREAM_ERROR if the stream structure was inconsistent. In the success
	  case, the application may save the current current value of total_in which
	  indicates where valid compressed data was found. In the error case, the
	  application may repeatedly call inflateSync, providing more input each time,
	  until success or end of the input data.
	*/

	extern int _export z_inflateReset(args)()((z_stream_s _far * strm));
	/*
	     This function is equivalent to inflateEnd followed by inflateInit,
	   but does not free and reallocate all the internal decompression state.
	   The stream will keep attributes that may have been set by inflateInit2.
	
	      inflateReset returns Z_OK if success, or Z_STREAM_ERROR if the source
	   stream state was inconsistent (such as zalloc or state being NULL).
	*/


							/* utility functions */

	/*
	     The following utility functions are implemented on top of the
	   basic stream-oriented functions. To simplify the interface, some
	   default options are assumed (compression level and memory usage,
	   standard memory allocation functions). The source code of these
	   utility functions can easily be modified if you need special options.
	*/

	extern int _export z_compress(args)()((byte _far * dest, uint _far * destLen, const byte _far * source, uint sourceLen));
	/*
	     Compresses the source buffer into the destination buffer.  sourceLen is
	   the byte length of the source buffer. Upon entry, destLen is the total
	   size of the destination buffer, which must be at least 0.1% larger than
	   sourceLen plus 12 bytes. Upon exit, destLen is the actual size of the
	   compressed buffer.
	     This function can be used to compress a whole file at once if the
	   input file is mmap'ed.
	     compress returns Z_OK if success, Z_MEM_ERROR if there was not
	   enough memory, Z_BUF_ERROR if there was not enough room in the output
	   buffer.
	*/

	extern int _export z_compress2(args)()((byte _far * dest, uint _far * destLen, const byte _far * source, uint sourceLen, int level));
	/*
	     Compresses the source buffer into the destination buffer. The level
	   parameter has the same meaning as in deflateInit.  sourceLen is the byte
	   length of the source buffer. Upon entry, destLen is the total size of the
	   destination buffer, which must be at least 0.1% larger than sourceLen plus
	   12 bytes. Upon exit, destLen is the actual size of the compressed buffer.
	
	     compress2 returns Z_OK if success, Z_MEM_ERROR if there was not enough
	   memory, Z_BUF_ERROR if there was not enough room in the output buffer,
	   Z_STREAM_ERROR if the level parameter is invalid.
	*/

	extern int _export z_uncompress(args)()((byte _far * dest, uint _far * destLen, const byte _far * source, uint sourceLen));
	/*
	     Decompresses the source buffer into the destination buffer.  sourceLen is
	   the byte length of the source buffer. Upon entry, destLen is the total
	   size of the destination buffer, which must be large enough to hold the
	   entire uncompressed data. (The size of the uncompressed data must have
	   been saved previously by the compressor and transmitted to the decompressor
	   by some mechanism outside the scope of this compression library.)
	   Upon exit, destLen is the actual size of the compressed buffer.
	     This function can be used to decompress a whole file at once if the
	   input file is mmap'ed.
	
	     uncompress returns Z_OK if success, Z_MEM_ERROR if there was not
	   enough memory, Z_BUF_ERROR if there was not enough room in the output
	   buffer, or Z_DATA_ERROR if the input data was corrupted.
	*/



	extern z_voidp _export gzopen(args)()((const char * path, const char * mode));
	/*
	     Opens a gzip (.gz) file for reading or writing. The mode parameter
	   is as in fopen ("rb" or "wb") but can also include a compression level
	   ("wb9") or a strategy: 'f' for filtered data as in "wb6f", 'h' for
	   Huffman only compression as in "wb1h". (See the description
	   of deflateInit2 for more information about the strategy parameter.)
	
	     gzopen can be used to read a file which is not in gzip format; in this
	   case gzread will directly read from the file without decompression.
	
	     gzopen returns NULL if the file could not be opened or if there was
	   insufficient memory to allocate the (de)compression state; errno
	   can be checked to distinguish the two cases (if errno is zero, the
	   zlib error is Z_MEM_ERROR).  */

	extern z_voidp _export gzdopen(args)()((int fd, const char * mode));
	/*
	     gzdopen() associates a gzFile with the file descriptor fd.  File
	   descriptors are obtained from calls like open, dup, creat, pipe or
	   fileno (in the file has been previously opened with fopen).
	   The mode parameter is as in gzopen.
	     The next call of gzclose on the returned gzFile will also close the
	   file descriptor fd, just like fclose(fdopen(fd), mode) closes the file
	   descriptor fd. If you want to keep fd open, use gzdopen(dup(fd), mode).
	     gzdopen returns NULL if there was insufficient memory to allocate
	   the (de)compression state.
	*/

	extern int _export gzsetparams(args)()((z_voidp file, int level, int strategy));
	/*
	     Dynamically update the compression level or strategy. See the description
	   of deflateInit2 for the meaning of these parameters.
	     gzsetparams returns Z_OK if success, or Z_STREAM_ERROR if the file was not
	   opened for writing.
	*/

	extern int _export gzread(args)()((z_voidp file, z_voidp buf, uint len));
	/*
	     Reads the given number of uncompressed bytes from the compressed file.
	   If the input file was not in gzip format, gzread copies the given number
	   of bytes into the buffer.
	     gzread returns the number of uncompressed bytes actually read (0 for
	   end of file, -1 for error). */

	extern int _export gzwrite(args)()((z_voidp file, const z_voidp buf, uint len));
	/*
	     Writes the given number of uncompressed bytes into the compressed file.
	   gzwrite returns the number of uncompressed bytes actually written
	   (0 in case of error).
	*/

	extern int _export gzprintf(args)()((z_voidp file, const char * format, ...));
	/*
	     Converts, formats, and writes the args to the compressed file under
	   control of the format string, as in fprintf. gzprintf returns the number of
	   uncompressed bytes actually written (0 in case of error).
	*/

	extern int _export gzputs(args)()((z_voidp file, const char * s));
	/*
	      Writes the given null-terminated string to the compressed file, excluding
	   the terminating null character.
	      gzputs returns the number of characters written, or -1 in case of error.
	*/

	extern char * _export gzgets(args)()((z_voidp file, char * buf, int len));
	/*
	      Reads bytes from the compressed file until len-1 characters are read, or
	   a newline character is read and transferred to buf, or an end-of-file
	   condition is encountered.  The string is then terminated with a null
	   character.
	      gzgets returns buf, or Z_NULL in case of error.
	*/

	extern int _export gzputc(args)()((z_voidp file, int c));
	/*
	      Writes c, converted to an unsigned char, into the compressed file.
	   gzputc returns the value that was written, or -1 in case of error.
	*/

	extern int _export gzgetc(args)()((z_voidp file));
	/*
	      Reads one byte from the compressed file. gzgetc returns this byte
	   or -1 in case of end of file or error.
	*/

	extern int _export gzflush(args)()((z_voidp file, int flush));
	/*
	     Flushes all pending output into the compressed file. The parameter
	   flush is as in the deflate() function. The return value is the zlib
	   error number (see function gzerror below). gzflush returns Z_OK if
	   the flush parameter is Z_FINISH and all output could be flushed.
	     gzflush should be called only when strictly necessary because it can
	   degrade compression.
	*/

	extern z_off_t _export gzseek(args)()((z_voidp file, z_off_t offset, int whence));
	/* 
	      Sets the starting position for the next gzread or gzwrite on the
	   given compressed file. The offset represents a number of bytes in the
	   uncompressed data stream. The whence parameter is defined as in lseek(2);
	   the value SEEK_END is not supported.
	     If the file is opened for reading, this function is emulated but can be
	   extremely slow. If the file is opened for writing, only forward seeks are
	   supported; gzseek then compresses a sequence of zeroes up to the new
	   starting position.
	
	      gzseek returns the resulting offset location as measured in bytes from
	   the beginning of the uncompressed stream, or -1 in case of error, in
	   particular if the file is opened for writing and the new starting position
	   would be before the current position.
	*/

	extern int _export gzrewind(args)()((z_voidp file));
	/*
	     Rewinds the given file. This function is supported only for reading.
	
	   gzrewind(file) is equivalent to (int)gzseek(file, 0L, SEEK_SET)
	*/

	extern z_off_t _export gztell(args)()((z_voidp file));
	/*
	     Returns the starting position for the next gzread or gzwrite on the
	   given compressed file. This position represents a number of bytes in the
	   uncompressed data stream.
	
	   gztell(file) is equivalent to gzseek(file, 0L, SEEK_CUR)
	*/

	extern int _export gzeof(args)()((z_voidp file));
	/*
	     Returns 1 when EOF has previously been detected reading the given
	   input stream, otherwise zero.
	*/

	extern int _export gzclose(args)()((z_voidp file));
	/*
	     Flushes all pending output if necessary, closes the compressed file
	   and deallocates all the (de)compression state. The return value is the zlib
	   error number (see function gzerror below).
	*/

	extern const char * _export gzerror(args)()((z_voidp file, int * errnum));
	/*
	     Returns the error message for the last error which occurred on the
	   given compressed file. errnum is set to zlib error number. If an
	   error occurred in the file system and not in the compression library,
	   errnum is set to Z_ERRNO and the application may consult errno
	   to get the exact error code.
	*/

							/* checksum functions */

	/*
	     These functions are not related to compression but are exported
	   anyway because they might be useful in applications using the
	   compression library.
	*/

	extern uint _export z_adler32(args)()((uint adler, const byte _far * buf, uint len));

	/*
	     Update a running Adler-32 checksum with the bytes buf[0..len-1] and
	   return the updated checksum. If buf is NULL, this function returns
	   the required initial value for the checksum.
	   An Adler-32 checksum is almost as reliable as a CRC32 but can be computed
	   much faster. Usage example:
	
	     uLong adler = adler32(0L, Z_NULL, 0);
	
	     while (read_buffer(buffer, length) != EOF) {
	       adler = adler32(adler, buffer, length);
	     }
	     if (adler != original_adler) error();
	*/

	extern uint _export z_crc32(args)()((uint crc, const byte _far * buf, uint len));
	/*
	     Update a running crc with the bytes buf[0..len-1] and return the updated
	   crc. If buf is NULL, this function returns the required initial value
	   for the crc. Pre- and post-conditioning (one's complement) is performed
	   within this function so it shouldn't be done by the application.
	   Usage example:
	
	     uLong crc = crc32(0L, Z_NULL, 0);
	
	     while (read_buffer(buffer, length) != EOF) {
	       crc = crc32(crc, buffer, length);
	     }
	     if (crc != original_crc) error();
	*/


							/* various hacks, don't look :) */

	/* deflateInit and inflateInit are macros to allow checking the zlib version
	 * and the compiler's view of z_stream:
	 */
	extern int _export z_deflateInit_(args)()((z_stream_s _far * strm, int level, const char * version, int stream_size));
	extern int _export z_inflateInit_(args)()((z_stream_s _far * strm, const char * version, int stream_size));
	extern int _export z_deflateInit2_(args)()((z_stream_s _far * strm, int level, int method, int windowBits, int memLevel, int strategy, const char * version, int stream_size));
	extern int _export z_inflateInit2_(args)()((z_stream_s _far * strm, int windowBits, const char * version, int stream_size));

	extern const char * _export zError(args)()((int err));
	extern int _export z_inflateSyncPoint(args)()((z_stream_s _far * z));
	extern const uint _far * _export z_get_crc_table(args)()();

	#if __cplusplus
	#endif


}