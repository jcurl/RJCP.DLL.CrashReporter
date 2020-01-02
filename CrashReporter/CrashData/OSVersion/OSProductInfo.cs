namespace RJCP.Diagnostics.CrashData.OSVersion
{
    /// <summary>
    /// The Operating System Product Info. Supported in Vista and later.
    /// </summary>
    /// <remarks>
    /// This is obtained from the function call <c>GetProductInfo()</c>.
    /// </remarks>
    internal enum OSProductInfo : long
    {
        /// <summary>
        /// Product unknown.
        /// </summary>
        Undefined = 0x00000000,

        /// <summary>
        /// Product is unlicensed.
        /// </summary>
        Unlicensed = 0xABCDABCD,

        /// <summary>
        /// Ultimate.
        /// </summary>
        Ultimate = 0x00000001,

        /// <summary>
        /// Home Basic.
        /// </summary>
        Home_Basic = 0x00000002,

        /// <summary>
        /// Home Premium.
        /// </summary>
        Home_Premium = 0x00000003,

        /// <summary>
        /// Enterprise.
        /// </summary>
        Enterprise = 0x00000004,

        /// <summary>
        /// Home Basic N.
        /// </summary>
        Home_Basic_N = 0x00000005,

        /// <summary>
        /// Business.
        /// </summary>
        Business = 0x00000006,

        /// <summary>
        /// Server Standard (full installation).
        /// </summary>
        Standard_Server = 0x00000007,

        /// <summary>
        /// Server Datacenter (full installation).
        /// </summary>
        DataCenter_Server = 0x00000008,

        /// <summary>
        /// Windows Small Business Server.
        /// </summary>
        SmallBusiness_Server = 0x00000009,

        /// <summary>
        /// Server Enterprise (full installation).
        /// </summary>
        Enterprise_Server = 0x0000000A,

        /// <summary>
        /// Starter.
        /// </summary>
        Starter = 0x0000000B,

        /// <summary>
        /// Server Datacenter (core installation).
        /// </summary>
        DataCenter_Server_Core = 0x0000000C,

        /// <summary>
        /// Server Standard (core installation).
        /// </summary>
        Standard_Server_Core = 0x0000000D,

        /// <summary>
        /// Server Enterprise (core installation).
        /// </summary>
        Enterprise_Server_Core = 0x0000000E,

        /// <summary>
        /// Server Enterprise for Itanium-based Systems.
        /// </summary>
        Enterprise_Server_IA64 = 0x0000000F,

        /// <summary>
        /// Business N.
        /// </summary>
        Business_N = 0x00000010,

        /// <summary>
        /// Web Server (full installation).
        /// </summary>
        Web_Server = 0x00000011,

        /// <summary>
        /// HPC Edition.
        /// </summary>
        Cluster_Server = 0x00000012,

        /// <summary>
        /// Windows Storage Server 2008 R2 Essentials.
        /// </summary>
        Home_Server = 0x00000013,

        /// <summary>
        /// Storage Server Express.
        /// </summary>
        Storage_Express_Server = 0x00000014,

        /// <summary>
        /// Storage Server Standard.
        /// </summary>
        Storage_Standard_Server = 0x00000015,

        /// <summary>
        /// Storage Server Workgroup.
        /// </summary>
        Storage_Workgroup_Server = 0x00000016,

        /// <summary>
        /// Storage Server Enterprise.
        /// </summary>
        Storage_Enterprise_Server = 0x0000000017,

        /// <summary>
        /// Windows Server 2008 for Windows Essential Server Solutions.
        /// </summary>
        Server_For_SmallBusiness = 0x00000018,

        /// <summary>
        /// Small Business Server Premium.
        /// </summary>
        SmallBusiness_Server_Premium = 0x00000019,

        /// <summary>
        /// Home Premium N.
        /// </summary>
        Home_Premium_N = 0x0000001A,

        /// <summary>
        /// Enterprise N.
        /// </summary>
        Enterprise_N = 0x0000001B,

        /// <summary>
        /// Ultimate N.
        /// </summary>
        Ultimate_N = 0x0000001C,

        /// <summary>
        /// Web Server (core installation).
        /// </summary>
        Web_Server_Core = 0x0000001D,

        /// <summary>
        /// Windows Essential Business Server Management Server.
        /// </summary>
        MediumBusiness_Server_Management = 0x0000001E,

        /// <summary>
        /// Windows Essential Business Server Security Server.
        /// </summary>
        MediumBusiness_Server_Security = 0x0000001F,

        /// <summary>
        /// Windows Essential Business Server Messaging Server.
        /// </summary>
        MediumBusiness_Server_Messaging = 0x00000020,

        /// <summary>
        /// Server Foundation.
        /// </summary>
        Server_Foundation = 0x00000021,

        /// <summary>
        /// Windows Home Server 2011.
        /// </summary>
        Home_Premium_Server = 0x00000022,

        /// <summary>
        /// Windows Server 2008 without Hyper-V for Windows Essential Server Solutions.
        /// </summary>
        Server_For_SmallBusiness_V = 0x00000023,

        /// <summary>
        /// Server Standard without Hyper-V (full installation).
        /// </summary>
        Standard_Server_V = 0x00000024,

        /// <summary>
        /// Server Datacenter without Hyper-V (full installation).
        /// </summary>
        DataCenter_Server_V = 0x00000025,

        /// <summary>
        /// Server Enterprise without Hyper-V (full installation).
        /// </summary>
        Enterprise_Server_V = 0x00000026,

        /// <summary>
        /// Server Datacenter without Hyper-V (core installation).
        /// </summary>
        DataCenter_Server_Core_V = 0x00000027,

        /// <summary>
        /// Server Standard without Hyper-V (core installation).
        /// </summary>
        Standard_Server_Core_V = 0x00000028,

        /// <summary>
        /// Server Enterprise without Hyper-V (core installation).
        /// </summary>
        Enterprise_Server_Core_V = 0x00000029,

        /// <summary>
        /// Hyper-V Server.
        /// </summary>
        HyperV = 0x0000002A,

        /// <summary>
        /// Storage Server Express (core installation).
        /// </summary>
        Storage_Express_Server_Core = 0x0000002B,

        /// <summary>
        /// Storage Server Standard (core installation).
        /// </summary>
        Storage_Standard_Server_Core = 0x0000002C,

        /// <summary>
        /// Storage Server Workgroup (core installation).
        /// </summary>
        Storage_Workgroup_Server_Core = 0x0000002D,

        /// <summary>
        /// Storage Server Enterprise (core installation).
        /// </summary>
        Storage_Enterprise_Core = 0x0000002E,

        /// <summary>
        /// Starter N.
        /// </summary>
        Starter_N = 0x0000002F,

        /// <summary>
        /// Professional.
        /// </summary>
        Professional = 0x00000030,

        /// <summary>
        /// Professional N.
        /// </summary>
        Professional_N = 0x00000031,

        /// <summary>
        /// Windows Small Business Server 2011 Essentials.
        /// </summary>
        SB_Solution_Server = 0x00000032,

        /// <summary>
        /// Server For SB Solutions.
        /// </summary>
        Server_For_SB_Solutions = 0x00000033,

        /// <summary>
        /// Server Solutions Premium.
        /// </summary>
        Standard_Server_Solutions = 0x00000034,

        /// <summary>
        /// Server Solutions Premium (core installation).
        /// </summary>
        Standard_Server_Solutions_Core = 0x00000035,

        /// <summary>
        /// Server For SB Solutions EM.
        /// </summary>
        SB_Solution_Server_Em = 0x00000036,

        /// <summary>
        /// Server For SB Solutions EM.
        /// </summary>
        Server_For_SB_Solutions_EM = 0x00000037,

        /// <summary>
        /// Windows Multipoint Server.
        /// </summary>
        Solution_EmbeddedServer = 0x00000038,

        /// <summary>
        /// Windows Essential Server Solution Management.
        /// </summary>
        EssentialBusiness_Server_Mgmt = 0x0000003B,

        /// <summary>
        /// Windows Essential Server Solution Additional.
        /// </summary>
        EssentialBusiness_Server_Addl = 0x0000003C,

        /// <summary>
        /// Windows Essential Server Solution Management SVC.
        /// </summary>
        EssentialBusiness_Server_MgmtSvc = 0x0000003D,

        /// <summary>
        /// Windows Essential Server Solution Additional SVC.
        /// </summary>
        EssentialBusiness_Server_AddlSvc = 0x0000003E,

        /// <summary>
        /// Small Business Server Premium (core installation).
        /// </summary>
        SmallBusiness_Server_Premium_Core = 0x0000003F,

        /// <summary>
        /// Server Hyper Core V.
        /// </summary>
        Cluster_Server_V = 0x00000040,

        /// <summary>
        /// Starter E (not supported).
        /// </summary>
        Starter_E = 0x00000042,

        /// <summary>
        /// Home Basic E (not supported).
        /// </summary>
        Home_Basic_E = 0x00000043,

        /// <summary>
        /// Home Premium E (not supported).
        /// </summary>
        Home_Premium_E = 0x00000044,

        /// <summary>
        /// Professional E (not supported).
        /// </summary>
        Professional_E = 0x00000045,

        /// <summary>
        /// Enterprise E (Not supported).
        /// </summary>
        Enterprise_E = 0x00000046,

        /// <summary>
        /// Ultimate E (not supported).
        /// </summary>
        Ultimate_E = 0x00000047,

        /// <summary>
        /// Enterprise (Evaluation Edition).
        /// </summary>
        Enterprise_Evaluation = 0x00000048,

        /// <summary>
        /// Windows MultiPoint Server Standard (full installation).
        /// </summary>
        Multipoint_Standard_Server = 0x0000004C,

        /// <summary>
        /// Windows MultiPoint Server Premium (full installation).
        /// </summary>
        Multipoint_Premium_Server = 0x0000004D,

        /// <summary>
        /// Server Standard (evaluation installation).
        /// </summary>
        Standard_Evaluation_Server = 0x0000004F,

        /// <summary>
        /// Server Datacenter (Evaluation Edition).
        /// </summary>
        DataCenter_Evaluation_Server = 0x00000050,

        /// <summary>
        /// Enterprise N (evaluation installation).
        /// </summary>
        Enterprise_N_Evaluation = 0x00000054,

        /// <summary>
        /// Storage Server Workgroup (evaluation installation).
        /// </summary>
        Storage_Workgroup_Evaluation_Server = 0x0000005F,

        /// <summary>
        /// Storage Server Standard (evaluation installation).
        /// </summary>
        Storage_Standard_Evaluation_Server = 0x00000060,

        /// <summary>
        /// Windows 8 N.
        /// </summary>
        Core_N = 0x00000062,

        /// <summary>
        /// Windows 8 China.
        /// </summary>
        Core_CountrySpecific = 0x00000063,

        /// <summary>
        /// Windows 8 Single Language.
        /// </summary>
        Core_SingleLanguage = 0x00000064,

        /// <summary>
        /// Windows 8.
        /// </summary>
        Core = 0x00000065,

        /// <summary>
        /// Professional with Media Center.
        /// </summary>
        Professional_Wmc = 0x00000067,

        /// <summary>
        /// Windows 10 Mobile.
        /// </summary>
        Mobile_Core = 0x00000068,

        /// <summary>
        /// Windows 10 Education.
        /// </summary>
        Education = 0x00000079,

        /// <summary>
        /// Windows 10 Education N.
        /// </summary>
        Education_N = 0x0000007A,

        /// <summary>
        /// Windows 10 IoT Core.
        /// </summary>
        IoT_UAP = 0x0000007B,

        /// <summary>
        /// Windows 10 Enterprise 2015 LTSB.
        /// </summary>
        Enterprise_S = 0x0000007D,

        /// <summary>
        /// Windows 10 Enterprise 2015 LTSB N.
        /// </summary>
        Enterprise_S_N = 0x0000007E,

        /// <summary>
        /// Windows 10 Enterprise 2015 LTSB Evaluation.
        /// </summary>
        Enterprise_S_Evaluation = 0x00000081,

        /// <summary>
        /// Windows 10 Enterprise 2015 LTSB N Evaluation.
        /// </summary>
        Enterprise_S_N_Evaluation = 0x00000082,

        /// <summary>
        /// Windows 10 IoT Core Commercial.
        /// </summary>
        IoT_UAP_Commercial = 0x00000083,

        /// <summary>
        /// Windows 10 Mobile Enterprise.
        /// </summary>
        Mobile_Enterprise = 0x00000085,

        /// <summary>
        /// The data center semi annual core server.
        /// </summary>
        DataCenter_SemiAnnual_Server_Core = 0x00000091,

        /// <summary>
        /// The standard semi annual core server.
        /// </summary>
        Standard_SemiAnnual_ServerCore = 0x00000092,

        /// <summary>
        /// Windows 10 Pro for Workstations.
        /// </summary>
        Professional_Workstation = 0x000000A1,

        /// <summary>
        /// Windows 10 Pro for Workstations.
        /// </summary>
        Professional_Workstation_N = 0x000000A2,

        /// <summary>
        /// Windows XP MediaCenter.
        /// </summary>
        MediaCenter = 0xF8000001,

        /// <summary>
        /// Windows XP Tablet PC.
        /// </summary>
        TabletPc = 0xF8000002
    }
}
