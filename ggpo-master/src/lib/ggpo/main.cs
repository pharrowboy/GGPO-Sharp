using System;

/* -----------------------------------------------------------------------
 * GGPO.net (http://ggpo.net)  -  Copyright 2009 GroundStorm Studios, LLC.
 *
 * Use of this software is governed by the MIT license that can be found
 * in the LICENSE file.
 */

//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define ASSERT(x) do { if (!(x)) { char assert_buf[1024]; snprintf(assert_buf, sizeof(assert_buf) - 1, "Assertion: %s @ %s:%d (pid:%d)", #x, __FILE__, __LINE__, Platform::GetProcessID()); Log("%s\n", assert_buf); Log("\n"); Log("\n"); Log("\n"); Platform::AssertFailed(assert_buf); exit(0); } } while (false)
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define ARRAY_SIZE(a) (sizeof(a) / sizeof((a)[0]))
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define MAX(x, y) (((x) > (y)) ? (x) : (y))
//C++ TO C# CONVERTER NOTE: The following #define macro was replaced in-line:
//ORIGINAL LINE: #define MIN(x, y) (((x) < (y)) ? (x) : (y))

//C++ TO C# CONVERTER TODO TASK: The following method format was not recognized, possibly due to an unrecognized macro:
bool WINAPI DllMain(IntPtr hinstDLL, uint fdwReason, object * lpvReserved)
{
   srand(Platform.GetCurrentTimeMS() + Platform.GetProcessID());
   return true;
}