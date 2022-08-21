//C++ TO C# CONVERTER TODO TASK: Check to ensure that the necessary preprocessor flags are defined:
internal static class DefineConstants
{
	public const int GGPO_MAX_PLAYERS = 4;
	public const int GGPO_MAX_PREDICTION_FRAMES = 8;
	public const int GGPO_MAX_SPECTATORS = 32;
	public const int GGPO_SPECTATOR_INPUT_INTERVAL = 4;
	public const int GGPO_INVALID_HANDLE = -1;
	public const int FRAME_DELAY = 2;
	public const int STARTING_HEALTH = 100;
	public const int ROTATE_INCREMENT = 3;
	public const int SHIP_RADIUS = 15;
	public const int SHIP_WIDTH = 8;
	public const int SHIP_TUCK = 3;
	public const double SHIP_THRUST = 0.06;
	public const double SHIP_MAX_THRUST = 4.0;
	public const double SHIP_BREAK_SPEED = 0.6;
	public const int BULLET_SPEED = 5;
	public const int MAX_BULLETS = 30;
	public const int BULLET_COOLDOWN = 8;
	public const int BULLET_DAMAGE = 10;
	public const int MAX_SHIPS = 4;
	public const int MAX_PLAYERS = 64;
	public const int MAX_PLAYERS = 4;
	public const int PROGRESS_BAR_WIDTH = 100;
	public const int PROGRESS_BAR_TOP_OFFSET = 22;
	public const int PROGRESS_BAR_HEIGHT = 8;
	public const int MAX_GRAPH_SIZE = 4096;
	public const int MAX_FAIRNESS = 20;
	public const int MAX_COMPRESSED_BITS = 4096;
	public const int UDP_MSG_MAX_PLAYERS = 4;
	public const int MAX_UDP_ENDPOINTS = 16;
	public const int SPECTATOR_FRAME_BUFFER_SIZE = 64;
#if ! _WINDOWS && __GNUC__
	public const int BITVECTOR_NIBBLE_SIZE = 8;
#endif
#if ! _WINDOWS && __GNUC__
	public const int GAMEINPUT_MAX_BYTES = 9;
#endif
#if ! _WINDOWS && __GNUC__
	public const int GAMEINPUT_MAX_PLAYERS = 2;
#endif
#if ! _WINDOWS && __GNUC__
	public const int INPUT_QUEUE_LENGTH = 128;
#endif
#if ! _WINDOWS && __GNUC__
	public const int DEFAULT_INPUT_SIZE = 4;
#endif
	public const int MAX_INT = 0xEFFFFFF;
	public const int MAX_POLLABLE_HANDLES = 64;
	public const int MAX_PREDICTION_FRAMES = 8;
	public const int FRAME_WINDOW_SIZE = 40;
	public const int MIN_UNIQUE_FRAMES = 10;
	public const int MIN_FRAME_ADVANTAGE = 3;
	public const int MAX_FRAME_ADVANTAGE = 9;
	public const int IDS_APP_TITLE = 103;
	public const int IDR_MAINFRAME = 128;
	public const int IDD_VECTORWAR_DIALOG = 102;
	public const int IDD_ABOUTBOX = 103;
	public const int IDM_ABOUT = 104;
	public const int IDM_EXIT = 105;
	public const int IDI_VECTORWAR = 107;
	public const int IDI_SMALL = 108;
	public const int IDC_VECTORWAR = 109;
	public const int IDC_MYICON = 2;
	public const int IDC_STATIC = -1;
	public const int IDD_PERFMON = 200;
	public const int IDC_CLOSE = 202;
	public const int IDC_FRAME_LAG = 203;
	public const int IDC_LOCAL_AHEAD = 204;
	public const int IDC_REMOTE_AHEAD = 205;
	public const int IDC_NETWORK_GRAPH = 206;
	public const int IDC_FAIRNESS_GRAPH = 207;
	public const int IDC_PID = 208;
	public const int IDC_LOCAL_CPU = 209;
	public const int IDC_REMOTE_CPU = 210;
	public const int IDC_NETWORK_LAG = 211;
	public const int IDC_BANDWIDTH = 212;
	public const int IDC_PACKET_LOSS = 223;
#if APSTUDIO_INVOKED && ! APSTUDIO_READONLY_SYMBOLS
	public const int _APS_NO_MFC = 130;
#endif
#if APSTUDIO_INVOKED && ! APSTUDIO_READONLY_SYMBOLS
	public const int _APS_NEXT_RESOURCE_VALUE = 129;
#endif
#if APSTUDIO_INVOKED && ! APSTUDIO_READONLY_SYMBOLS
	public const int _APS_NEXT_COMMAND_VALUE = 32771;
#endif
#if APSTUDIO_INVOKED && ! APSTUDIO_READONLY_SYMBOLS
	public const int _APS_NEXT_CONTROL_VALUE = 1000;
#endif
#if APSTUDIO_INVOKED && ! APSTUDIO_READONLY_SYMBOLS
	public const int _APS_NEXT_SYMED_VALUE = 110;
#endif
#if ! MAX_MEM_LEVEL && MAXSEG_64K
	public const int MAX_MEM_LEVEL = 8;
#endif
#if ! MAX_MEM_LEVEL && ! MAXSEG_64K
	public const int MAX_MEM_LEVEL = 9;
#endif
	public const int MAX_WBITS = 15; // 32K LZ77 window
	public const int SEEK_SET = 0; // Seek from beginning of file.
	public const int SEEK_CUR = 1; // Seek from current position.
	public const int SEEK_END = 2; // Set file pointer to EOF plus "offset"
#if HAVE_UNISTD_H
	public const string ZLIB_VERSION = "1.1.4";
#endif
#if HAVE_UNISTD_H
	public const int Z_NO_FLUSH = 0;
#endif
#if HAVE_UNISTD_H
	public const int Z_PARTIAL_FLUSH = 1; // will be removed, use Z_SYNC_FLUSH instead
#endif
#if HAVE_UNISTD_H
	public const int Z_SYNC_FLUSH = 2;
#endif
#if HAVE_UNISTD_H
	public const int Z_FULL_FLUSH = 3;
#endif
#if HAVE_UNISTD_H
	public const int Z_FINISH = 4;
#endif
#if HAVE_UNISTD_H
	public const int Z_OK = 0;
#endif
#if HAVE_UNISTD_H
	public const int Z_STREAM_END = 1;
#endif
#if HAVE_UNISTD_H
	public const int Z_NEED_DICT = 2;
#endif
#if HAVE_UNISTD_H
	public const int Z_ERRNO = -1;
#endif
#if HAVE_UNISTD_H
	public const int Z_STREAM_ERROR = -2;
#endif
#if HAVE_UNISTD_H
	public const int Z_DATA_ERROR = -3;
#endif
#if HAVE_UNISTD_H
	public const int Z_MEM_ERROR = -4;
#endif
#if HAVE_UNISTD_H
	public const int Z_BUF_ERROR = -5;
#endif
#if HAVE_UNISTD_H
	public const int Z_VERSION_ERROR = -6;
#endif
#if HAVE_UNISTD_H
	public const int Z_NO_COMPRESSION = 0;
#endif
#if HAVE_UNISTD_H
	public const int Z_BEST_SPEED = 1;
#endif
#if HAVE_UNISTD_H
	public const int Z_BEST_COMPRESSION = 9;
#endif
#if HAVE_UNISTD_H
	public const int Z_DEFAULT_COMPRESSION = -1;
#endif
#if HAVE_UNISTD_H
	public const int Z_FILTERED = 1;
#endif
#if HAVE_UNISTD_H
	public const int Z_HUFFMAN_ONLY = 2;
#endif
#if HAVE_UNISTD_H
	public const int Z_DEFAULT_STRATEGY = 0;
#endif
#if HAVE_UNISTD_H
	public const int Z_BINARY = 0;
#endif
#if HAVE_UNISTD_H
	public const int Z_ASCII = 1;
#endif
#if HAVE_UNISTD_H
	public const int Z_UNKNOWN = 2;
#endif
#if HAVE_UNISTD_H
	public const int Z_DEFLATED = 8;
#endif
#if HAVE_UNISTD_H
	public const int Z_NULL = 0; // for initializing zalloc, zfree, opaque
#endif
}