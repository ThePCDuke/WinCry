using Microsoft.Win32;
using System;
using System.Runtime.InteropServices;
using System.Text;

namespace WinCry.Models
{
    public static class SetFTA
    {
		public static void SetFileTypeAssociation(string extension, string progid)
		{
			try
			{
				String regpath;
				if (extension.StartsWith("."))
					regpath = String.Format("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Explorer\\FileExts\\{0}\\UserChoice", extension);
				else
					regpath = String.Format("SOFTWARE\\Microsoft\\Windows\\Shell\\Associations\\UrlAssociations\\{0}\\UserChoice", extension);

				Registry.CurrentUser.DeleteSubKey(regpath, false);
				RegistryKey regnode = Registry.CurrentUser.CreateSubKey(regpath);

				System.Security.Principal.WindowsIdentity user = System.Security.Principal.WindowsIdentity.GetCurrent();
				String sid = user.User.Value;

				long ftLastWriteTime;
				RegQueryInfoKey(regnode.Handle.DangerousGetHandle(), IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, out ftLastWriteTime);
				DateTime time = DateTime.FromFileTime(ftLastWriteTime);
				time = time.AddTicks(-(time.Ticks % 600000000)); //clamp to minute part (1min=600000000*100ns)
				ftLastWriteTime = time.ToFileTime();

				String regdate = ftLastWriteTime.ToString("x16");

				String experience = "user choice set via windows user experience {d18b6dd5-6124-4341-9318-804003bafa0b}";

				//Step1: String (Unicode with 0 terminator) from the following: extension, user sid, progid, last modification time for the registry node clamped to minute part, secret experience string
				//Step2: Lowercase
				byte[] bytes = Encoding.Unicode.GetBytes((extension + sid + progid + regdate + experience + "\0").ToLower());
				System.Security.Cryptography.MD5 md5Hash = System.Security.Cryptography.MD5.Create();
				//Step3: MD5
				byte[] md5 = md5Hash.ComputeHash(bytes);
				//Step4: Microsoft hashes from data and md5, xored together
				byte[] mshash1 = sub_1(bytes, md5);
				byte[] mshash2 = sub_2(bytes, md5);
				byte[] finalraw = xorbytes(mshash1, mshash2);
				//Step5: Base64
				String hash = System.Convert.ToBase64String(finalraw);

				regnode.SetValue("ProgId", progid);
				regnode.SetValue("Hash", hash);

				Console.WriteLine("Success");
			}
			catch (Exception ex)
			{
				Console.WriteLine("Error during operation:");
				Console.WriteLine(ex.Message);
			}
		}

		public static byte[] xorbytes(byte[] data1, byte[] data2)
		{
			byte[] retval = new byte[Math.Max(data1.Length, data2.Length)];
			for (int i = 0; i < data1.Length; i++) retval[i] ^= data1[i];
			for (int i = 0; i < data2.Length; i++) retval[i] ^= data2[i];
			return retval;
		}

		public static byte[] sub_1(byte[] data, byte[] md5)
		{
			byte[] retval = new byte[8];

			UInt32 length = (UInt32)(((((data.Length) >> 2) & 1) < 1 ? 1 : 0) + ((data.Length) >> 2) - 1);// (UInt32)Math.Floor((double)(data.Length/8))*2; //length in dword
			UInt32[] dword_data = new UInt32[length];
			UInt32[] dword_md5 = new UInt32[4];
			for (int i = 0; i < dword_data.Length; i++)
			{
				dword_data[i] = System.BitConverter.ToUInt32(data, i * 4);
			}
			dword_md5[0] = System.BitConverter.ToUInt32(md5, 0);
			dword_md5[1] = System.BitConverter.ToUInt32(md5, 4);
			dword_md5[2] = System.BitConverter.ToUInt32(md5, 8);
			dword_md5[3] = System.BitConverter.ToUInt32(md5, 12);

			if (length <= 1 || (length & 1) == 1)
				return retval;

			UInt32 v5 = 0;
			UInt32 v6 = 0;
			UInt32 v7 = (length - 2) >> 1;
			UInt32 v18 = v7++;
			UInt32 v8 = v7;
			UInt32 v19 = v7;
			UInt32 result = 0;
			UInt32 v9 = (dword_md5[1] | 1) + 0x13DB0000u;
			UInt32 v10 = (dword_md5[0] | 1) + 0x69FB0000u;

			UInt32 v11 = 0;
			UInt32 v12 = 0;
			UInt32 v13 = 0;
			UInt32 v14 = 0;
			UInt32 v15 = 0;
			UInt32 v16 = 0;
			UInt32 v17 = 0;

			do //TODO: based on asm
			{
				v11 = dword_data[v6] + result;
				v6 += 2;
				v12 = 0x79F8A395u * (v10 * v11 - 0x10FA9605u * (v11 >> 16)) + 0x689B6B9Fu * ((v10 * v11 - 0x10FA9605u * (v11 >> 16)) >> 16);
				v13 = (0xEA970001u * v12 - 0x3C101569u * (v12 >> 16));
				v14 = v13 + v5;
				v15 = v9 * (dword_data[v6 - 1] + v13) - 0x3CE8EC25u * ((dword_data[v6 - 1] + v13) >> 16);
				result = 0x1EC90001u * (0x59C3AF2Du * v15 - 0x2232E0F1u * (v15 >> 16)) + 0x35BD1EC9u * ((0x59C3AF2Du * v15 - 0x2232E0F1u * (v15 >> 16)) >> 16);
				v5 = result + v14;
				--v8;
			}
			while (v8 != 0);
			if (length - 2 - 2 * v18 == 1)
			{
				v16 = (dword_data[2 * v19] + result) * v10 - 0x10FA9605u * ((dword_data[2 * v19] + result) >> 16);
				v17 = 0x39646B9Fu * (v16 >> 16) + 0x28DBA395u * v16 - 0x3C101569u * ((0x689B6B9Fu * (v16 >> 16) + 0x79F8A395u * v16) >> 16);
				result = 0x35BD1EC9u * ((0x59C3AF2Du * (v17 * v9 - 0x3CE8EC25u * (v17 >> 16)) - 0x2232E0F1u * ((v17 * v9 - 0x3CE8EC25u * (v17 >> 16)) >> 16)) >> 16) + 0x2A18AF2Du * (v17 * v9 - 0x3CE8EC25u * (v17 >> 16)) - 0xFD6BE0F1u * ((v17 * v9 - 0x3CE8EC25u * (v17 >> 16)) >> 16);
				v5 += result + v17;
			}

			BitConverter.GetBytes(result).CopyTo(retval, 0);
			BitConverter.GetBytes(v5).CopyTo(retval, 4);
			return retval;

		}


		public static byte[] sub_2(byte[] data, byte[] md5)
		{
			byte[] retval = new byte[8];

			UInt32 length = (UInt32)(((((data.Length) >> 2) & 1) < 1 ? 1 : 0) + ((data.Length) >> 2) - 1);// (UInt32)Math.Floor((double)(data.Length/8))*2; //length in dword
			UInt32[] dword_data = new UInt32[length];
			UInt32[] dword_md5 = new UInt32[4];
			for (int i = 0; i < dword_data.Length; i++)
			{
				dword_data[i] = System.BitConverter.ToUInt32(data, i * 4);
			}
			dword_md5[0] = System.BitConverter.ToUInt32(md5, 0);
			dword_md5[1] = System.BitConverter.ToUInt32(md5, 4);
			dword_md5[2] = System.BitConverter.ToUInt32(md5, 8);
			dword_md5[3] = System.BitConverter.ToUInt32(md5, 12);

			if (length <= 1 || (length & 1) == 1)
				return retval;

			UInt32 v5 = 0;
			UInt32 v6 = 0;
			UInt32 v7 = 0;
			UInt32 v25 = (length - 2) >> 1;
			UInt32 v21 = dword_md5[0] | 1;
			UInt32 v22 = dword_md5[1] | 1;
			UInt32 v23 = (UInt32)(0xB1110000u * v21);
			UInt32 v24 = 0x16F50000u * v22;
			UInt32 v8 = v25 + 1;

			UInt32 v9 = 0;
			UInt32 v10 = 0;
			UInt32 v11 = 0;
			UInt32 v12 = 0;
			UInt32 v13 = 0;
			UInt32 v14 = 0;
			UInt32 v15 = 0;
			UInt32 v16 = 0;
			UInt32 v17 = 0;
			UInt32 v18 = 0;
			UInt32 v19 = 0;
			UInt32 v20 = 0;

			do //TODO: based on asm
			{
				v6 += 2;
				v9 = (dword_data[v6 - 2] + v5) * v23 - 0x30674EEFu * (v21 * (dword_data[v6 - 2] + v5) >> 16);
				v10 = v9 >> 16;
				v11 = 0xE9B30000u * v10 + 0x12CEB96Du * ((0x5B9F0000u * v9 - 0x78F7A461u * v10) >> 16);
				v12 = 0x1D830000u * v11 + 0x257E1D83u * (v11 >> 16);
				v13 = ((v12 + dword_data[v6 - 1]) * v24 - 0x5D8BE90Bu * ((v22 * (v12 + dword_data[v6 - 1])) >> 16)) >> 16;
				v14 = 0x96FF0000u * ((v12 + dword_data[v6 - 1]) * v24 - 0x5D8BE90Bu * ((v22 * (v12 + dword_data[v6 - 1])) >> 16)) - 0x2C7C6901u * v13 >> 16;
				v5 = 0xF2310000u * v14 - 0x405B6097u * ((0x7C932B89u * v14 - 0x5C890000u * v13) >> 16);
				v7 += v5 + v12;
				--v8;
			}
			while (v8 != 0);
			if (length - 2 - 2 * v25 == 1)
			{
				v15 = 0xB1110000u * v21 * (dword_data[2 * (v25 + 1)] + v5) - 0x30674EEFu * (v21 * (dword_data[2 * (v25 + 1)] + v5) >> 16);
				v16 = v15 >> 16;
				v17 = (0x5B9F0000u * v15 - 0x78F7A461u * (v15 >> 16)) >> 16;
				v18 = 0x257E1D83u * ((0xE9B30000u * v16 + 0x12CEB96Du * v17) >> 16) + 0x3BC70000u * v17;
				v19 = (0x16F50000u * v18 * v22 - 0x5D8BE90Bu * (v18 * v22 >> 16)) >> 16;
				v20 = (0x96FF0000u * (0x16F50000u * v18 * v22 - 0x5D8BE90Bu * (v18 * v22 >> 16)) - 0x2C7C6901u * v19) >> 16;
				v5 = 0xF2310000u * v20 - 0x405B6097u * ((0x7C932B89u * v20 - 0x5C890000u * v19) >> 16);
				v7 += v5 + v18;
			}

			BitConverter.GetBytes(v5).CopyTo(retval, 0);
			BitConverter.GetBytes(v7).CopyTo(retval, 4);

			return retval;
		}


		[DllImport("advapi32.dll", EntryPoint = "RegQueryInfoKey", CallingConvention = CallingConvention.Winapi, SetLastError = true)]
		extern private static int RegQueryInfoKey(
			IntPtr handle,
			IntPtr /*out StringBuilder*/ lpClass,
			IntPtr /*ref uint*/ lpcbClass,
			IntPtr lpReserved,
			IntPtr /*out uint*/ lpcSubKeys,
			IntPtr /*out uint*/ lpcbMaxSubKeyLen,
			IntPtr /*out uint*/ lpcbMaxClassLen,
			IntPtr /*out uint*/ lpcValues,
			IntPtr /*out uint*/ lpcbMaxValueNameLen,
			IntPtr /*out uint*/ lpcbMaxValueLen,
			IntPtr /*out uint*/ lpcbSecurityDescriptor,
			out long lpftLastWriteTime
		);
	}
}