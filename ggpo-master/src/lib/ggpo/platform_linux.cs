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
//C++ TO C# CONVERTER TODO TASK: The following line could not be converted:

public class Platform
{

   public static pid_t GetProcessID()
   {
	   return getpid();
   }
   public static void AssertFailed(ref string msg)
   {
   }
//C++ TO C# CONVERTER TODO TASK: The implementation of the following method could not be found:
//   static uint GetCurrentTimeMS();
}



//C++ TO C# CONVERTER TODO TASK: The following method format was not recognized, possibly due to an unrecognized macro:
struct timespec start = {0} uint Platform.GetCurrentTimeMS()
{
	if (start.tv_sec == 0 && start.tv_nsec == 0)
	{
		clock_gettime(CLOCK_MONOTONIC, start);
		return 0
	}
	timespec current = new timespec();
	clock_gettime(CLOCK_MONOTONIC, current);

	return ((current.tv_sec - start.tv_sec) * 1000) + ((current.tv_nsec - start.tv_nsec) / 1000000) +
}

