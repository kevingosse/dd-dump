using Microsoft.Diagnostics.Runtime;
using System.Globalization;
using System.Runtime.InteropServices;

namespace dd_dump;

partial class Program
{
    private const int PTRACE_ATTACH = 16;
    private const int PTRACE_DETACH = 17;

    static unsafe void Main(string[] args)
    {
        // Disable QUIC support. It requires reflection (and thus increases the final size of the binary)
        AppContext.SetSwitch("System.Net.SocketsHttpHandler.Http3Support", false);

        var pid = int.Parse(args[0]);

        Libunwind.Initialize();

        var addressSpace = Libunwind.unw_create_addr_space(Libunwind._UPT_accessors, 0);

        //if (ptrace(PTRACE_ATTACH, pid, IntPtr.Zero, IntPtr.Zero) != 0)
        //{
        //    Console.WriteLine("ptrace failed");
        //    return;
        //}

        using var dataTarget = DataTarget.AttachToProcess(pid, true);
        var runtime = dataTarget.ClrVersions[0].CreateRuntime();

        var threads = Directory.GetDirectories($"/proc/{pid}/task/")
            .Select(f => int.Parse(Path.GetFileName(f)))
            .ToList();

        var modules = ReadModules(pid);

        try
        {
            using var client = new HttpClient();
            var content = client.GetStringAsync("http://datadoghq.com").Result;
        }
        catch (Exception ex)
        {
            Console.WriteLine("Exception: " + ex);
        }
        
        const int bufferLength = 4096;
        Span<byte> sym = stackalloc byte[bufferLength];

        foreach (var thread in threads)
        {
            Console.WriteLine($"Thread {thread}");

            var context = Libunwind._UPT_create(thread);

            int status;

            if ((status = Libunwind.unw_init_remote(out var cursor, addressSpace, context)) != 0)
            {
                Console.WriteLine("Cannot initialize cursor for remote unwinding: " + status);
                return;
            }

            do
            {
                if (Libunwind.unw_get_reg(ref cursor, Libunwind.Register.UNW_X86_64_RIP, out var pc) != 0)
                {
                    Console.WriteLine("Cannot read program counter");
                    return;
                }

                Console.Write(pc.ToString("x2"));

                var method = runtime.GetMethodByInstructionPointer((ulong) pc);

                if (method == null)
                {
                    var module = modules.FirstOrDefault(m => m.Start <= pc && m.End >= pc);

                    var moduleName = "{unknown}";

                    if (module.Path != null)
                    {
                        moduleName = Path.GetFileName(module.Path);
                    }

                    fixed (byte* buffer = sym)
                    {
                        if (Libunwind.unw_get_proc_name(ref cursor, buffer, bufferLength, out var offset) == 0)
                        {
                            var str = Marshal.PtrToStringAnsi((nint) buffer);
                            Console.WriteLine($" -- {moduleName}!{str} +{offset:x2}");
                        }
                        else
                        {
                            Console.WriteLine($" -- {moduleName}!{{unknown}}");
                        }
                    }
                }
                else
                {
                    Console.WriteLine($" -- {Path.GetFileName(method.Type.Module.AssemblyName)}!{method.Type}.{method.Name} +{method.GetILOffset((ulong) pc):x2}");
                }
            } while (Libunwind.unw_step(ref cursor) > 0);

            Libunwind._UPT_destroy(context);

            Console.WriteLine();
            Console.WriteLine();
        }
    }


    internal static List<Module> ReadModules(int pid)
    {
        /*
            /proc/[pid]/maps
            A file containing the currently mapped memory regions and their access permissions.
            The format is:

            address           perms offset  dev   inode   pathname
            08048000-08056000 r-xp 00000000 03:0c 64593   /usr/sbin/gpm
            08056000-08058000 rw-p 0000d000 03:0c 64593   /usr/sbin/gpm
            08058000-0805b000 rwxp 00000000 00:00 0
            40000000-40013000 r-xp 00000000 03:0c 4165    /lib/ld-2.2.4.so
            40013000-40015000 rw-p 00012000 03:0c 4165    /lib/ld-2.2.4.so
            4001f000-40135000 r-xp 00000000 03:0c 45494   /lib/libc-2.2.4.so
            40135000-4013e000 rw-p 00115000 03:0c 45494   /lib/libc-2.2.4.so
            4013e000-40142000 rw-p 00000000 00:00 0
            bffff000-c0000000 rwxp 00000000 00:00 0
            where "address" is the address space in the process that it occupies, "perms" is a set of permissions:
            r = read
            w = write
            x = execute
            s = shared
            p = private (copy on write)
            "offset" is the offset into the file/whatever, "dev" is the device (major:minor), and "inode" is the inode on that device. 0 indicates that no inode is associated with the memory region, as the case would be with BSS (uninitialized data).
            Under Linux 2.0 there is no field giving pathname.
         */

        var path = $"/proc/{pid}/maps";

        var modules = new List<Module>();

        foreach (var line in File.ReadAllLines(path))
        {
            var values = line.Split(' ', 6, System.StringSplitOptions.RemoveEmptyEntries);

            if (values.Length != 6)
            {
                continue;
            }

            var range = values[0].Split('-');

            modules.Add(new Module(
                start: long.Parse(range[0], NumberStyles.HexNumber),
                end: long.Parse(range[1], NumberStyles.HexNumber),
                offset: long.Parse(values[2], NumberStyles.HexNumber),
                path: values[5]));
        }

        return modules;
    }

    public readonly struct Module
    {
        public Module(long start, long end, long offset, string path)
        {
            Start = start;
            End = end;
            Offset = offset;
            Path = path;
        }

        public readonly long Start;
        public readonly long End;
        public readonly long Offset;
        public readonly string Path;
    }

    //[LibraryImport("libc")]
    //private static partial nint ptrace(int request, int pid, IntPtr addr, IntPtr data);

}
