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


/*
 * nongamestate.h --
 *
 * These are other pieces of information not related to the state
 * of the game which are useful to carry around.  They are not
 * included in the GameState class because they specifically
 * should not be rolled back.
 */

public enum PlayerConnectState
{
   Connecting = 0,
   Synchronizing,
   Running,
   Disconnected,
   Disconnecting
}

public class PlayerConnectionInfo
{
   public GGPOPlayerType type;
   public GGPOPlayerHandle handle = new GGPOPlayerHandle();
   public PlayerConnectState state;
   public int connect_progress;
   public int disconnect_timeout;
   public int disconnect_start;
}

public class NonGameState
{
   public class ChecksumInfo
   {
	  public int framenumber;
	  public int checksum;
   }

   public void SetConnectState(GGPOPlayerHandle handle, PlayerConnectState state)
   {
	  for (int i = 0; i < num_players; i++)
	  {
		 if (players[i].handle == handle)
		 {
			players[i].connect_progress = 0;
			players[i].state = state;
			break;
		 }
	  }
   }

   public void SetDisconnectTimeout(GGPOPlayerHandle handle, int when, int timeout)
   {
	  for (int i = 0; i < num_players; i++)
	  {
		 if (players[i].handle == handle)
		 {
			players[i].disconnect_start = when;
			players[i].disconnect_timeout = timeout;
			players[i].state = PlayerConnectState.Disconnecting;
			break;
		 }
	  }
   }

   public void SetConnectState(PlayerConnectState state)
   {
	  for (int i = 0; i < num_players; i++)
	  {
		 players[i].state = state;
	  }
   }

   public void UpdateConnectProgress(GGPOPlayerHandle handle, int progress)
   {
	  for (int i = 0; i < num_players; i++)
	  {
		 if (players[i].handle == handle)
		 {
			players[i].connect_progress = progress;
			break;
		 }
	  }
   }

   public GGPOPlayerHandle local_player_handle = new GGPOPlayerHandle();
   public PlayerConnectionInfo[] players = Arrays.InitializeWithDefaultInstances<PlayerConnectionInfo>(DefineConstants.MAX_PLAYERS);
   public int num_players;

   public ChecksumInfo now = new ChecksumInfo();
   public ChecksumInfo periodic = new ChecksumInfo();
}

