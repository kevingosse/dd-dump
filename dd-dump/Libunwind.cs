using System.Runtime.InteropServices;

namespace dd_dump
{
    internal static unsafe class Libunwind
    {
        private static nint _libunwindx86;
        private static nint _libunwindPtrace;
        private static delegate* unmanaged<int, IntPtr> __UPT_create;
        private static delegate* unmanaged<IntPtr, IntPtr> __UPT_destroy;
        private static delegate* unmanaged<nint, int, IntPtr> _unw_create_addr_space;
        private static delegate* unmanaged<out unw_cursor_t, IntPtr, IntPtr, int> _unw_init_remote;
        private static delegate* unmanaged<ref unw_cursor_t, Register, out nint, int> _unw_get_reg;
        private static delegate* unmanaged<ref unw_cursor_t, byte*, nint, out nint, int> _unw_get_proc_name;
        private static delegate* unmanaged<ref unw_cursor_t, int> _unw_step;
        private const int RTLD_LAZY = 0x1;
        private const int RTLD_NOW = 0x2;
        private const int RTLD_GLOBAL = 0x100;
        
        public static void Initialize()
        {
            _libunwindx86 = dlopen("libunwind-x86_64.so", RTLD_LAZY | RTLD_GLOBAL);
            _libunwindPtrace = dlopen("libunwind-ptrace.so", RTLD_LAZY | RTLD_GLOBAL);

            _UPT_accessors = dlsym(_libunwindPtrace, "_UPT_accessors");

            __UPT_create = (delegate* unmanaged<int, IntPtr>)dlsym(_libunwindPtrace, "_UPT_create");
            __UPT_destroy = (delegate* unmanaged<IntPtr, IntPtr>)dlsym(_libunwindPtrace, "_UPT_create");
            _unw_create_addr_space = (delegate* unmanaged<nint, int, IntPtr>)dlsym(_libunwindx86, "_Ux86_64_create_addr_space");
            _unw_init_remote = (delegate* unmanaged<out unw_cursor_t, IntPtr, IntPtr, int>)dlsym(_libunwindx86, "_Ux86_64_init_remote");
            _unw_get_reg = (delegate* unmanaged<ref unw_cursor_t, Register, out nint, int>)dlsym(_libunwindx86, "_Ux86_64_get_reg");
            _unw_get_proc_name = (delegate* unmanaged<ref unw_cursor_t, byte*, nint, out nint, int>)dlsym(_libunwindx86, "_Ux86_64_get_proc_name");
            _unw_step = (delegate* unmanaged<ref unw_cursor_t, int>)dlsym(_libunwindx86, "_Ux86_64_step");
        }

        public static nint _UPT_accessors { get; private set; }

        public static IntPtr _UPT_create(int pid) => __UPT_create(pid);

        public static IntPtr _UPT_destroy(IntPtr ptr) => __UPT_destroy(ptr);

        public static IntPtr unw_create_addr_space(nint accessors, int byteOrder) => _unw_create_addr_space(accessors, byteOrder);

        public static int unw_init_remote(out unw_cursor_t cursor, IntPtr addressSpace, IntPtr arg) => _unw_init_remote(out cursor, addressSpace, arg);

        public static int unw_step(ref unw_cursor_t cursor) => _unw_step(ref cursor);

        public static int unw_get_reg(ref unw_cursor_t cursor, Register register, out nint value) => _unw_get_reg(ref cursor, register, out value);

        public static int unw_get_proc_name(ref unw_cursor_t cursor, byte* buffer, nint bufferLength, out nint offset) => _unw_get_proc_name(ref cursor, buffer, bufferLength, out offset);


        [DllImport("libdl.so")]
        private static extern nint dlopen(string filename, int flag);

        [DllImport("libdl.so")]
        private static extern nint dlsym(nint handle, string symbol);

        public enum Register
        {
            UNW_X86_64_RAX,
            UNW_X86_64_RDX,
            UNW_X86_64_RCX,
            UNW_X86_64_RBX,
            UNW_X86_64_RSI,
            UNW_X86_64_RDI,
            UNW_X86_64_RBP,
            UNW_X86_64_RSP,
            UNW_X86_64_R8,
            UNW_X86_64_R9,
            UNW_X86_64_R10,
            UNW_X86_64_R11,
            UNW_X86_64_R12,
            UNW_X86_64_R13,
            UNW_X86_64_R14,
            UNW_X86_64_R15,
            UNW_X86_64_RIP,
            UNW_TDEP_LAST_REG = UNW_X86_64_RIP,

            /* frame info (read-only) */
            UNW_X86_64_CFA,

            UNW_TDEP_IP = UNW_X86_64_RIP,
            UNW_TDEP_SP = UNW_X86_64_RSP,
            UNW_TDEP_BP = UNW_X86_64_RBP,
            UNW_TDEP_EH = UNW_X86_64_RAX
        }

        [StructLayout(LayoutKind.Explicit, Size = UNW_TDEP_CURSOR_LEN * WORD_LENGTH)]
        public struct unw_cursor_t
        {
            private const int UNW_TDEP_CURSOR_LEN = 127; // /!\ Platform specific
            private const int WORD_LENGTH = 8; // /!\ Platform specific
        }
    }
}
