using System;

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

//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define PI ((double)3.1415926)

/*
 * renderer.h --
 *
 * A simple C++ renderer that uses GDI to render the game state.
 *
 */

public class GDIRenderer : Renderer
{
   public GDIRenderer(IntPtr hwnd)
   {
	   this._hwnd = hwnd;
	  IntPtr hdc = GetDC(_hwnd);
	  _status = StringFunctions.ChangeCharacter(_status, 0, '\0');
	  GetClientRect(hwnd, _rc);
	  CreateGDIFont(hdc);
	  ReleaseDC(_hwnd, hdc);

	  _shipColors[0] = RGB(255, 0, 0);
	  _shipColors[1] = RGB(0, 255, 0);
	  _shipColors[2] = RGB(0, 0, 255);
	  _shipColors[3] = RGB(128, 128, 128);

	  for (int i = 0; i < 4; i++)
	  {
		 _shipPens[i] = CreatePen(PS_SOLID, 1, _shipColors[i]);
	  }
	  _redBrush = CreateSolidBrush(RGB(255, 0, 0));
	  _bulletBrush = CreateSolidBrush(RGB(255, 192, 0));
   }

   public new void Dispose()
   {
	  DeleteObject(_font);
	   base.Dispose();
   }

   public override void Draw(GameState gs, NonGameState ngs)
   {
	  IntPtr hdc = GetDC(_hwnd);

	  FillRect(hdc, _rc, (IntPtr)GetStockObject(BLACK_BRUSH));
	  FrameRect(hdc, gs._bounds, (IntPtr)GetStockObject(WHITE_BRUSH));

	  SetBkMode(hdc, TRANSPARENT);
	  SelectObject(hdc, _font);

	  for (int i = 0; i < gs._num_ships; i++)
	  {
		 SetTextColor(hdc, _shipColors[i]);
		 SelectObject(hdc, _shipPens[i]);
		 DrawShip(hdc, i, gs);
		 DrawConnectState(hdc, gs._ships[i], ngs.players[i]);
	  }

	  SetTextAlign(hdc, TA_BOTTOM | TA_CENTER);
	  TextOutA(hdc, (_rc.left + _rc.right) / 2, _rc.bottom - 32, _status, (int)strlen(_status));

	  SetTextColor(hdc, RGB(192, 192, 192));
	  RenderChecksum(hdc, 40, ngs.periodic);
	  SetTextColor(hdc, RGB(128, 128, 128));
	  RenderChecksum(hdc, 56, ngs.now);

	  //SwapBuffers(hdc);
	  ReleaseDC(_hwnd, hdc);
   }

   public override void SetStatusText(string text)
   {
	  strcpy_s(_status, text);
   }

   protected void RenderChecksum(IntPtr hdc, int y, NonGameState.ChecksumInfo info)
   {
	  string checksum = new string(new char[128]);
	  sprintf_s(checksum, ARRAYSIZE(checksum), "Frame: %04d  Checksum: %08x", info.framenumber, info.checksum);
	  TextOutA(hdc, (_rc.left + _rc.right) / 2, _rc.top + y, checksum, (int)strlen(checksum));
   }

   protected void DrawShip(IntPtr hdc, int which, GameState gs)
   {
	  Ship ship = gs._ships + which;
	  RECT bullet = new RECT();
	  POINT[] shape =
	  {
		  new POINT(DefineConstants.SHIP_RADIUS, 0),
		  new POINT(-DefineConstants.SHIP_RADIUS, DefineConstants.SHIP_WIDTH),
		  new POINT(DefineConstants.SHIP_TUCK - DefineConstants.SHIP_RADIUS, 0),
		  new POINT(-DefineConstants.SHIP_RADIUS, -DefineConstants.SHIP_WIDTH),
		  new POINT(DefineConstants.SHIP_RADIUS, 0)
	  };
	  int[] alignments = {TA_TOP | TA_LEFT, TA_TOP | TA_RIGHT, TA_BOTTOM | TA_LEFT, TA_BOTTOM | TA_RIGHT};
	  POINT[] text_offsets =
	  {
		  new POINT(gs._bounds.left + 2, gs._bounds.top + 2),
		  new POINT(gs._bounds.right - 2, gs._bounds.top + 2),
		  new POINT(gs._bounds.left + 2, gs._bounds.bottom - 2),
		  new POINT(gs._bounds.right - 2, gs._bounds.bottom - 2)
	  };
	  string buf = new string(new char[32]);
	  int i;

//C++ TO C# CONVERTER WARNING: This 'sizeof' ratio was replaced with a direct reference to the array length:
//ORIGINAL LINE: for (i = 0; i < (sizeof(shape) / sizeof(shape[0])); i++)
	  for (i = 0; i < (shape.Length); i++)
	  {
		 double newx;
		 double newy;
		 double cost;
		 double sint;
		 double theta;

		 theta = (double)ship.heading * ((double)3.1415926) / 180;
		 cost = global::cos(theta);
		 sint = global::sin(theta);

		 newx = shape[i].x * cost - shape[i].y * sint;
		 newy = shape[i].x * sint + shape[i].y * cost;

		 shape[i].x = (int)(newx + ship.position.x);
		 shape[i].y = (int)(newy + ship.position.y);
	  }
//C++ TO C# CONVERTER WARNING: This 'sizeof' ratio was replaced with a direct reference to the array length:
//ORIGINAL LINE: Polyline(hdc, shape, (sizeof(shape) / sizeof(shape[0])));
	  Polyline(hdc, shape, (shape.Length));

	  for (i = 0; i < DefineConstants.MAX_BULLETS; i++)
	  {
		 if (ship.bullets[i].active)
		 {
			bullet.left = (int)ship.bullets[i].position.x - 1;
			bullet.right = (int)ship.bullets[i].position.x + 1;
			bullet.top = (int)ship.bullets[i].position.y - 1;
			bullet.bottom = (int)ship.bullets[i].position.y + 1;
			FillRect(hdc, bullet, _bulletBrush);
		 }
	  }
	  SetTextAlign(hdc, alignments[which]);
	  sprintf_s(buf, ARRAYSIZE(buf), "Hits: %d", ship.score);
	  TextOutA(hdc, text_offsets[which].x, text_offsets[which].y, buf, (int)strlen(buf));
   }

   protected void DrawConnectState(IntPtr hdc, Ship ship, PlayerConnectionInfo info)
   {
	  string status = new string(new char[64]);
	  string[] statusStrings = {"Connecting...", "Synchronizing...", "", "Disconnected."};
	  int progress = -1;

	  status = StringFunctions.ChangeCharacter(status, 0, '\0');
	  switch (info.state)
	  {
		 case PlayerConnectState.Connecting:
			sprintf_s(status, ARRAYSIZE(status), (info.type == (GGPOPlayerType)GGPOPlayerType.GGPO_PLAYERTYPE_LOCAL) ? "Local Player" : "Connecting...");
			break;

		 case PlayerConnectState.Synchronizing:
			progress = info.connect_progress;
			sprintf_s(status, ARRAYSIZE(status), (info.type == (GGPOPlayerType)GGPOPlayerType.GGPO_PLAYERTYPE_LOCAL) ? "Local Player" : "Synchronizing...");
			break;

		 case PlayerConnectState.Disconnected:
			sprintf_s(status, ARRAYSIZE(status), "Disconnected");
			break;

		 case PlayerConnectState.Disconnecting:
			sprintf_s(status, ARRAYSIZE(status), "Waiting for player...");
			progress = (timeGetTime() - info.disconnect_start) * 100 / info.disconnect_timeout;
			break;
	  }

	  if (status[0] != '\0')
	  {
		 SetTextAlign(hdc, TA_TOP | TA_CENTER);
		 TextOutA(hdc, (int)ship.position.x, (int)ship.position.y + (DefineConstants.PROGRESS_BAR_TOP_OFFSET + DefineConstants.PROGRESS_BAR_HEIGHT + 4), status, (int)strlen(status));
	  }
	  if (progress >= 0)
	  {
		 IntPtr bar = (IntPtr)(info.state == PlayerConnectState.Synchronizing ? GetStockObject(WHITE_BRUSH) : _redBrush);
		 RECT rc = new RECT((int)(ship.position.x - (DefineConstants.PROGRESS_BAR_WIDTH / 2)), (int)(ship.position.y + DefineConstants.PROGRESS_BAR_TOP_OFFSET), (int)(ship.position.x + (DefineConstants.PROGRESS_BAR_WIDTH / 2)), (int)(ship.position.y + DefineConstants.PROGRESS_BAR_TOP_OFFSET + DefineConstants.PROGRESS_BAR_HEIGHT));

		 FrameRect(hdc, rc, (IntPtr)GetStockObject(GRAY_BRUSH));
		 rc.right = rc.left + Math.Min(100, progress) * DefineConstants.PROGRESS_BAR_WIDTH / 100;
		 InflateRect(rc, -1, -1);
		 FillRect(hdc, rc, bar);
	  }
   }

   protected void CreateGDIFont(IntPtr hdc)
   {
	  _font = CreateFont(-12, 0, 0, 0, 0, false, false, false, ANSI_CHARSET, OUT_TT_PRECIS, CLIP_DEFAULT_PRECIS, ANTIALIASED_QUALITY, FF_DONTCARE | DEFAULT_PITCH, "Tahoma"); // Font Name

   }

   protected IntPtr _font;
   protected IntPtr _hwnd;
   protected RECT _rc = new RECT();
   protected HGLRC _hrc = new HGLRC();
   protected string _status = new string(new char[1024]);
   protected uint[] _shipColors = new uint[4];
   protected IntPtr[] _shipPens = new IntPtr[4];
   protected IntPtr _bulletBrush;
   protected IntPtr _redBrush;
}



//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define PROGRESS_TEXT_OFFSET (PROGRESS_BAR_TOP_OFFSET + PROGRESS_BAR_HEIGHT + 4)
