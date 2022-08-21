using System.Diagnostics;

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



// GAMEINPUT_MAX_BYTES * GAMEINPUT_MAX_PLAYERS * 8 must be less than
// 2^BITVECTOR_NIBBLE_SIZE (see bitvector.h)


public class GameInput
{
   public enum Constants
   {
	  NullFrame = -1
   }
   public int frame;
   public int size; // size in bytes of the entire input for all players
   public string bits = new string(new char[DefineConstants.GAMEINPUT_MAX_BYTES * DefineConstants.GAMEINPUT_MAX_PLAYERS]);

   public bool is_null()
   {
	   return frame == (int)Constants.NullFrame;
   }
   public void init(int iframe, ref string ibits, int isize, int offset)
   {
	  Debug.Assert(isize);
	  Debug.Assert(isize <= DefineConstants.GAMEINPUT_MAX_BYTES);
	  frame = iframe;
	  size = isize;
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
	  memset(bits, 0, sizeof(char));
	  if (ibits != '\0')
	  {
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
		 memcpy(bits + (offset * isize), ibits, isize);
	  }
   }

   public void init(int iframe, ref string ibits, int isize)
   {
	  Debug.Assert(isize);
	  Debug.Assert(isize <= DefineConstants.GAMEINPUT_MAX_BYTES * DefineConstants.GAMEINPUT_MAX_PLAYERS);
	  frame = iframe;
	  size = isize;
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
	  memset(bits, 0, sizeof(char));
	  if (ibits != '\0')
	  {
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memcpy' has no equivalent in C#:
		 memcpy(bits, ibits, isize);
	  }
   }

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: bool value(int i) const
   public bool value(int i)
   {
	   return (bits[i / 8] & (1 << (i % 8))) != 0;
   }
   public void set(int i)
   {
	   bits[i / 8] |= (1 << (i % 8));
   }
   public void clear(int i)
   {
	   bits[i / 8] &= ~(1 << (i % 8));
   }
   public void erase()
   {
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memset' has no equivalent in C#:
	   memset(bits, 0, sizeof(char));
   }
//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: void desc(char *buf, size_t buf_size, bool show_frame = true) const
   public void desc(ref string buf, size_t buf_size, bool show_frame = true)
   {
	  Debug.Assert(size);
	  size_t remaining = new size_t(buf_size);
	  if (show_frame)
	  {
		 remaining -= sprintf_s(buf, buf_size, "(frame:%d size:%d ", frame, size);
	  }
	  else
	  {
		 remaining -= sprintf_s(buf, buf_size, "(size:%d ", size);
	  }

	  for (int i = 0; i < size * 8; i++)
	  {
		 string buf2 = new string(new char[16]);
		 if (value(i))
		 {
			int c = sprintf_s(buf2, ARRAY_SIZE(buf2), "%2d ", i);
			strncat_s(buf, remaining, buf2, ARRAY_SIZE(buf2));
			remaining -= c;
		 }
	  }
	  strncat_s(buf, remaining, ")", 1);
   }

//C++ TO C# CONVERTER WARNING: 'const' methods are not available in C#:
//ORIGINAL LINE: void log(char *prefix, bool show_frame = true) const
   public void log(ref string prefix, bool show_frame = true)
   {
	   string buf = new string(new char[1024]);
	  size_t c = strlen(prefix);
	   strcpy_s(buf, prefix);
	   desc(ref buf + c, ARRAY_SIZE(buf) - c, show_frame);
	  strncat_s(buf, ARRAY_SIZE(buf) - strlen(buf), "\n", 1);
	   Log(buf);
   }

   public bool equal(GameInput other, bool bitsonly = false)
   {
	  if (!bitsonly && frame != other.frame)
	  {
		 Log("frames don't match: %d, %d\n", frame, other.frame);
	  }
	  if (size != other.size)
	  {
		 Log("sizes don't match: %d, %d\n", size, other.size);
	  }
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memcmp' has no equivalent in C#:
	  if (memcmp(bits, other.bits, size))
	  {
		 Log("bits don't match\n");
	  }
	  Debug.Assert(size && other.size);
//C++ TO C# CONVERTER TODO TASK: The memory management function 'memcmp' has no equivalent in C#:
	  return (bitsonly || frame == other.frame) && size == other.size && memcmp(bits, other.bits, size) == 0;
   }
}

