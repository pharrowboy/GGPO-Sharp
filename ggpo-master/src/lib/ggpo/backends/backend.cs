/* -----------------------------------------------------------------------
 * GGPO.net (http://ggpo.net)  -  Copyright 2009 GroundStorm Studios, LLC.
 *
 * Use of this software is governed by the MIT license that can be found
 * in the LICENSE file.
 */


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

public abstract class GGPOSession : System.IDisposable
{
   public virtual void Dispose()
   {
   }
   public virtual GGPOErrorCode DoPoll(int timeout)
   {
	   return GGPOErrorCode.GGPO_OK;
   }
   public abstract GGPOErrorCode AddPlayer(GGPOPlayer player, GGPOPlayerHandle handle);
   public abstract GGPOErrorCode AddLocalInput(GGPOPlayerHandle player, object values, int size);
   public abstract GGPOErrorCode SyncInput(object values, int size, ref int disconnect_flags);
   public virtual GGPOErrorCode IncrementFrame()
   {
	   return GGPOErrorCode.GGPO_OK;
   }
   public virtual GGPOErrorCode Chat(ref string text)
   {
	   return GGPOErrorCode.GGPO_OK;
   }
   public virtual GGPOErrorCode DisconnectPlayer(GGPOPlayerHandle handle)
   {
	   return GGPOErrorCode.GGPO_OK;
   }
   public virtual GGPOErrorCode GetNetworkStats(GGPONetworkStats stats, GGPOPlayerHandle handle)
   {
	   return GGPOErrorCode.GGPO_OK;
   }
   public virtual GGPOErrorCode Logv(string fmt, va_list list)
   {
	   global::Logv(fmt, list);
	   return GGPOErrorCode.GGPO_OK;
   }

   public virtual GGPOErrorCode SetFrameDelay(GGPOPlayerHandle player, int delay)
   {
	   return GGPOErrorCode.GGPO_ERRORCODE_UNSUPPORTED;
   }
   public virtual GGPOErrorCode SetDisconnectTimeout(int timeout)
   {
	   return GGPOErrorCode.GGPO_ERRORCODE_UNSUPPORTED;
   }
   public virtual GGPOErrorCode SetDisconnectNotifyStart(int timeout)
   {
	   return GGPOErrorCode.GGPO_ERRORCODE_UNSUPPORTED;
   }
}



