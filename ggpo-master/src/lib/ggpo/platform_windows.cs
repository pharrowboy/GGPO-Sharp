/* -----------------------------------------------------------------------
 * GGPO.net (http://ggpo.net)  -  Copyright 2009 GroundStorm Studios, LLC.
 *
 * Use of this software is governed by the MIT license that can be found
 * in the LICENSE file.
 */

/* -----------------------------------------------------------------------
 * GGPO.net (http://ggpo.net)  -  Copyright 2009 GroundStorm Studios, LLC.
 *
 * Use of this software is governed by the MIT license that can be found
 * in the LICENSE file.
 */


//C++ TO C# CONVERTER TODO TASK: The following line could not be converted:
//C++ TO C# CONVERTER TODO TASK: The following line could not be converted:
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

public class Platform
{

   public static uint GetProcessID()
   {
	   return GetCurrentProcessId();
   }
   public static void AssertFailed(ref string msg)
   {
	   MessageBoxA(null, msg, "GGPO Assertion Failed", MB_OK | MB_ICONEXCLAMATION);
   }
   public static uint GetCurrentTimeMS()
   {
	   return timeGetTime();
   }
   public static int GetConfigInt(string name)
   {
	  string buf = new string(new char[1024]);
	  if (GetEnvironmentVariable(name, buf, ARRAY_SIZE(buf)) == 0)
	  {
		 return 0;
	  }
	  return atoi(buf);
   }

   public static bool GetConfigBool(string name)
   {
	  string buf = new string(new char[1024]);
	  if (GetEnvironmentVariable(name, buf, ARRAY_SIZE(buf)) == 0)
	  {
		 return false;
	  }
	  return atoi(buf) != 0 || _stricmp(buf, "true") == 0;
   }
}
