using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static System.Collections.Specialized.BitVector32;

namespace dd_dump.Symbols
{
    //internal class ElfFile
    //{
    //    public ELFHeader Header { get; }

    //    public byte[] ReadBuildId()
    //    {
    //        byte[] buildId = null;

    //        if (Header.ProgramHeaderOffset > 0 && Header.ProgramHeaderEntrySize > 0 && Header.ProgramHeaderCount > 0)
    //        {
    //            foreach (ELFProgramSegment segment in Segments)
    //            {
    //                if (segment.Header.Type == ELFProgramHeaderType.Note)
    //                {
    //                    buildId = ReadBuildIdNote(segment.Contents);
    //                    if (buildId != null)
    //                    {
    //                        break;
    //                    }
    //                }
    //            }
    //        }

    //        if (buildId == null)
    //        {
    //            // Use sections to find build id if there isn't any program headers (i.e. some FreeBSD .dbg files)
    //            try
    //            {
    //                foreach (ELFSection section in Sections)
    //                {
    //                    if (section.Header.Type == ELFSectionHeaderType.Note)
    //                    {
    //                        buildId = ReadBuildIdNote(section.Contents);
    //                        if (buildId != null)
    //                        {
    //                            break;
    //                        }
    //                    }
    //                }
    //            }
    //            catch (Exception ex) when (ex is InvalidVirtualAddressException || ex is BadInputFormatException || ex is OverflowException)
    //            {
    //            }
    //        }

    //        return buildId;
    //    }
    //}
}
